using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using VeterinaryMarketplace.Core.DTOs;
using VeterinaryMarketplace.Core.Entities;
using VeterinaryMarketplace.Core.Services;

namespace VeterinaryMarketplace.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClinicsController : ControllerBase
    {
        private readonly IService<Clinic> _clinicService;
        private readonly IMapper _mapper;
        private readonly IValidator<ClinicCreateDto> _createValidator;
        private readonly IIyzicoOnboardingService _iyzicoOnboardingService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IVeterinarianDetailService _veterinarianDetailService;
        private readonly IAppointmentService _appointmentService;
        private readonly IService<Treatment> _treatmentService;
        private readonly IService<WorkingHour> _workingHourService;

        public ClinicsController(
            IService<Clinic> clinicService, 
            IMapper mapper, 
            IValidator<ClinicCreateDto> createValidator, 
            IIyzicoOnboardingService iyzicoOnboardingService, 
            UserManager<AppUser> userManager, 
            IVeterinarianDetailService veterinarianDetailService,
            IAppointmentService appointmentService,
            IService<Treatment> treatmentService,
            IService<WorkingHour> workingHourService)
        {
            _clinicService = clinicService;
            _mapper = mapper;
            _createValidator = createValidator;
            _iyzicoOnboardingService = iyzicoOnboardingService;
            _userManager = userManager;
            _veterinarianDetailService = veterinarianDetailService;
            _appointmentService = appointmentService;
            _treatmentService = treatmentService;
            _workingHourService = workingHourService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var isAdmin = User.Identity?.IsAuthenticated == true && User.IsInRole("Admin");
            var clinics = isAdmin 
                ? await _clinicService.GetAllAsync() 
                : await _clinicService.Where(x => x.IsApproved == true).ToListAsync();

            var clinicDtos = _mapper.Map<List<ClinicDto>>(clinics);
            return Ok(clinicDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var clinic = await _clinicService.GetByIdAsync(id);
            if (clinic == null)
            {
                return NotFound(new { Message = "Klinik bulunamadı." });
            }

            var isAdmin = User.Identity?.IsAuthenticated == true && User.IsInRole("Admin");
            if (clinic.IsApproved != true && !isAdmin)
            {
                return NotFound(new { Message = "Klinik onaylı değil veya bulunamadı." });
            }

            var clinicDto = _mapper.Map<ClinicDto>(clinic);
            return Ok(clinicDto);
        }

        [HttpGet("filterByCity")]
        public async Task<IActionResult> GetByCity([FromQuery] string city)
        {
            var isAdmin = User.Identity?.IsAuthenticated == true && User.IsInRole("Admin");
            var clinics = await _clinicService.Where(x => x.City.ToLower() == city.ToLower() && (isAdmin || x.IsApproved == true)).ToListAsync();
            var clinicDtos = _mapper.Map<List<ClinicDto>>(clinics);
            return Ok(clinicDtos);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddClinic([FromBody] ClinicCreateDto clinicCreateDto)
        {
            var validationResult = await _createValidator.ValidateAsync(clinicCreateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            var newClinic = _mapper.Map<Clinic>(clinicCreateDto);
            newClinic.Id = Guid.NewGuid();
            newClinic.ManagerId = userId;
            newClinic.IsApproved = null; // Herkesin açtığı klinik onaya düşmeli ki onaylanırken veteriner profili yaratılabilsin.

            // İyzico'da SubMerchant Oluşturma
            var iyzicoResult = await _iyzicoOnboardingService.CreateSubMerchantAsync(newClinic, user);
            if (!iyzicoResult.IsSuccess)
            {
                return BadRequest(new { Message = "İyzico pazaryeri kaydı başarısız oldu: " + iyzicoResult.ErrorMessage });
            }
            newClinic.SubMerchantKey = iyzicoResult.SubMerchantKey;

            await _clinicService.AddAsync(newClinic);

            return Ok(new { Message = "Klinik başarıyla sisteme kaydedildi.", ClinicId = newClinic.Id });
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveClinic(Guid id)
        {
            var clinic = await _clinicService.GetByIdAsync(id);
            if (clinic == null) return NotFound(new { Message = "Klinik bulunamadı." });

            clinic.IsApproved = true;
            await _clinicService.UpdateAsync(clinic);

            if (!string.IsNullOrEmpty(clinic.ManagerId))
            {
                var existingVet = await _veterinarianDetailService.GetByUserIdAsync(clinic.ManagerId);
                if (existingVet == null)
                {
                    var newVet = new VeterinarianDetail
                    {
                        UserId = clinic.ManagerId,
                        ClinicId = clinic.Id,
                        Uzmanlik = "Klinik Yöneticisi",
                        Baslangic = new TimeSpan(9, 0, 0),
                        Bitis = new TimeSpan(18, 0, 0),
                        IBAN = clinic.Iban ?? "",
                        SubMerchantKey = clinic.SubMerchantKey ?? "",
                        CommissionRate = 10m,
                        ISAproved = true
                    };
                    await _veterinarianDetailService.AddAsync(newVet);
                }
                else
                {
                    existingVet.ClinicId = clinic.Id;
                    existingVet.ISAproved = true;
                    await _veterinarianDetailService.UpdateAsync(existingVet);
                }
            }

            return Ok(new { Message = "Klinik başarıyla onaylandı." });
        }

        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectClinic(Guid id)
        {
            var clinic = await _clinicService.GetByIdAsync(id);
            if (clinic == null) return NotFound(new { Message = "Klinik bulunamadı." });

            clinic.IsApproved = false;
            await _clinicService.UpdateAsync(clinic);

            // Kliniğe ait tüm veteriner profillerini bul ve sil (veya rollerini al)
            var clinicVets = await _veterinarianDetailService.Where(v => v.ClinicId == id).ToListAsync();
            foreach (var vetProfile in clinicVets)
            {
                var userId = vetProfile.UserId;

                // 1. Veterinerin Bekleyen/Onaylanmış Randevularını iptal et (İptal edilirse iade sağlanır)
                var vetAppointments = await _appointmentService.Where(a => a.VeterinarianDetailId == vetProfile.Id).ToListAsync();
                foreach (var apt in vetAppointments)
                {
                    if (apt.Status == Appointment.AppointmentStatus.Pending || apt.Status == Appointment.AppointmentStatus.Approved)
                    {
                        await _appointmentService.CancelAppointmentAsync(apt.Id);
                    }
                }

                // 2. Veterinerin Tedavilerini temizle
                var userTreatments = await _treatmentService.Where(t => t.UserID == userId).ToListAsync();
                if (userTreatments.Any())
                {
                    await _treatmentService.RemoveRangeAsync(userTreatments);
                }

                // 3. Veterinerin Çalışma Saatlerini temizle
                var userWorkingHours = await _workingHourService.Where(w => w.UserId == userId).ToListAsync();
                if (userWorkingHours.Any())
                {
                    await _workingHourService.RemoveRangeAsync(userWorkingHours);
                }

                // 4. Veteriner Profilini sil (Kullanıcı veteriner rolünde kalmaya devam eder, ancak profili ve kliniği olmaz)
                await _veterinarianDetailService.RemoveAsync(vetProfile);
            }

            return Ok(new { Message = "Klinik reddedildi ve ilgili veterinerlerin klinikle ilişiği kesildi." });
        }
    }
}