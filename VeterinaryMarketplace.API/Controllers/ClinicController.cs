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
        private readonly ICacheService _cacheService;

        public ClinicsController(
            IService<Clinic> clinicService, 
            IMapper mapper, 
            IValidator<ClinicCreateDto> createValidator, 
            IIyzicoOnboardingService iyzicoOnboardingService, 
            UserManager<AppUser> userManager, 
            IVeterinarianDetailService veterinarianDetailService,
            IAppointmentService appointmentService,
            IService<Treatment> treatmentService,
            IService<WorkingHour> workingHourService,
            ICacheService cacheService)
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
            _cacheService = cacheService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] VeterinaryMarketplace.Core.DTOs.Common.PaginationFilter filter)
        {
            var isAdmin = User.Identity?.IsAuthenticated == true && User.IsInRole("Admin");
            
            var cacheKey = $"clinics_{(isAdmin ? "admin" : "user")}_page_{filter.PageNumber}_size_{filter.PageSize}_search_{filter.SearchTerm}_city_{filter.City}";
            var cachedData = await _cacheService.GetAsync<VeterinaryMarketplace.Core.DTOs.Common.PagedResult<ClinicDto>>(cacheKey);
            
            if (cachedData != null)
            {
                return Ok(cachedData);
            }

            var query = _clinicService.Where(x => isAdmin || x.IsApproved == true);

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(x => x.Name.Contains(filter.SearchTerm) || x.Address.Contains(filter.SearchTerm));
            }

            if (!string.IsNullOrEmpty(filter.City))
            {
                query = query.Where(x => x.City == filter.City);
            }

            var totalCount = await query.CountAsync();

            var clinics = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var clinicDtos = _mapper.Map<List<ClinicDto>>(clinics);

            var pagedResult = new VeterinaryMarketplace.Core.DTOs.Common.PagedResult<ClinicDto>(
                clinicDtos, totalCount, filter.PageNumber, filter.PageSize);

            await _cacheService.SetAsync(cacheKey, pagedResult, TimeSpan.FromMinutes(15));

            return Ok(pagedResult);
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
            newClinic.IsApproved = null; 

            var iyzicoResult = await _iyzicoOnboardingService.CreateSubMerchantAsync(newClinic, user);
            if (!iyzicoResult.IsSuccess)
            {
                return BadRequest(new { Message = "İyzico pazaryeri kaydı başarısız oldu: " + iyzicoResult.ErrorMessage });
            }
            newClinic.SubMerchantKey = iyzicoResult.SubMerchantKey;

            await _clinicService.AddAsync(newClinic);
            await _cacheService.RemoveByPrefixAsync("clinics_");

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

            await _cacheService.RemoveByPrefixAsync("clinics_");
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

            var clinicVets = await _veterinarianDetailService.Where(v => v.ClinicId == id).ToListAsync();
            foreach (var vetProfile in clinicVets)
            {
                var userId = vetProfile.UserId;

                var vetAppointments = await _appointmentService.Where(a => a.VeterinarianDetailId == vetProfile.Id).ToListAsync();
                foreach (var apt in vetAppointments)
                {
                    if (apt.Status == Appointment.AppointmentStatus.Pending || apt.Status == Appointment.AppointmentStatus.Approved)
                    {
                        await _appointmentService.CancelAppointmentAsync(apt.Id);
                    }
                }

                var userTreatments = await _treatmentService.Where(t => t.UserID == userId).ToListAsync();
                if (userTreatments.Any())
                {
                    await _treatmentService.RemoveRangeAsync(userTreatments);
                }

                var userWorkingHours = await _workingHourService.Where(w => w.UserId == userId).ToListAsync();
                if (userWorkingHours.Any())
                {
                    await _workingHourService.RemoveRangeAsync(userWorkingHours);
                }
                await _veterinarianDetailService.RemoveAsync(vetProfile);
            }

            await _cacheService.RemoveByPrefixAsync("clinics_");
            return Ok(new { Message = "Klinik reddedildi ve ilgili veterinerlerin klinikle ilişiği kesildi." });
        }
    }
}