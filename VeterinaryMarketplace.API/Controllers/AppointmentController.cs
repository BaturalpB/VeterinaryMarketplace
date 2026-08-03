using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VeterinaryMarketplace.Core.DTOs;
using VeterinaryMarketplace.Core.DTOs.Appointment;
using VeterinaryMarketplace.Core.Entities;
using VeterinaryMarketplace.Core.Services;

namespace VeterinaryMarketplace.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IService<Pet> _petService;
        private readonly IService<VeterinarianDetail> _veterinarianDetailService;
        private readonly IService<Clinic> _clinicService;
        private readonly IMapper _mapper;
        private readonly IValidator<AppointmentCreateDto> _createValidator;

        public AppointmentController(
            IAppointmentService appointmentService,
            IService<Pet> petService,
            IService<VeterinarianDetail> veterinarianDetailService,
            IService<Clinic> clinicService,
            IMapper mapper,
            IValidator<AppointmentCreateDto> createValidator)
        {
            _appointmentService = appointmentService;
            _petService = petService;
            _veterinarianDetailService = veterinarianDetailService;
            _clinicService = clinicService;
            _mapper = mapper;
            _createValidator = createValidator;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentCreateDto createDto)
        {
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var pet = await _petService.Where(x => x.Id == createDto.PetId && x.OwnerId == userId).FirstOrDefaultAsync();
            if (pet == null)
            {
                return NotFound(new { Message = "Evcil Hayvan Bulunamadı" });
            }

            var isConflict = await _appointmentService.Where(a =>
                a.VeterinarianDetailId == createDto.VeterinarianDetailId &&
                a.AppointmentTime == createDto.AppointmentTime &&
                a.Status != Appointment.AppointmentStatus.Cancelled).AnyAsync();

            if (isConflict)
            {
                return BadRequest(new { Message = "Seçilen tarih ve saatte bu doktorun başka bir randevusu bulunmaktadır." });
            }

            decimal totalPrice = 0;
            var appointmentItems = new List<AppointmentItem>();

            if (createDto.TreatmentIds != null && createDto.TreatmentIds.Any())
            {
                foreach (var treatmentId in createDto.TreatmentIds)
                {
                    decimal itemPrice = 150m;
                    totalPrice += itemPrice;

                    appointmentItems.Add(new AppointmentItem
                    {
                        Id = Guid.NewGuid(),
                        TreatmentId = treatmentId,
                        Price = itemPrice
                    });
                }
            }

            var newAppointment = _mapper.Map<Appointment>(createDto);

            newAppointment.Id = Guid.NewGuid();
            newAppointment.Status = Appointment.AppointmentStatus.Pending;
            newAppointment.Price = totalPrice;
            newAppointment.AppointmentItems = appointmentItems;

            await _appointmentService.AddAsync(newAppointment);

            return Ok(new { Message = "Randevu ve seçilen tedaviler başarıyla oluşturuldu!" });
        }

        [HttpGet("my-appointments")]
        [Authorize]
        public async Task<IActionResult> GetMyAppointments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var appointments = await _appointmentService.Where(a => a.Pet.OwnerId == userId)
                .Include(a => a.Pet)
                .Include(a => a.Veterinarian)
                    .ThenInclude(v => v.Clinic)
                .Include(a => a.AppointmentItems)
                    .ThenInclude(ai => ai.Treatment)
                .ToListAsync();

            if (appointments == null || !appointments.Any())
            {
                return NotFound(new { Message = "Size ait herhangi bir randevu bulunamadı." });
            }

            var myAppointmentsDto = _mapper.Map<List<AppointmentDto>>(appointments);

            return Ok(myAppointmentsDto);
        }

        [HttpGet("vet-appointments")]
        [Authorize]
        public async Task<IActionResult> GetVetAppointments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appointments = await _appointmentService.Where(a => a.Veterinarian.UserId == userId)
                .Include(a => a.Pet)
                .Include(a => a.Veterinarian)
                    .ThenInclude(v => v.Clinic)
                .Include(a => a.AppointmentItems)
                    .ThenInclude(ai => ai.Treatment)
                .ToListAsync();

            if (appointments == null || !appointments.Any())
            {
                return NotFound(new { Message = "Size atanmış herhangi bir randevu bulunamadı." });
            }

            var vetAppointmentsDto = _mapper.Map<List<AppointmentDto>>(appointments);

            return Ok(vetAppointmentsDto);
        }

        [HttpPut("{id}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateAppointmentStatus(Guid id, [FromBody] Appointment.AppointmentStatus newStatus)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var Appointment = await _appointmentService.Where(a => a.Veterinarian.UserId == userId && a.Id == id).FirstOrDefaultAsync();
            if (Appointment == null)
            {
                return NotFound(new { Message = "Randevu Bulunamadı" });
            }
            Appointment.Status = newStatus;
            await _appointmentService.UpdateAsync(Appointment);
            return Ok(new { Message = "Randevu Başarıyla güncellendi" });
        }

        [HttpPut("{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelAppointment(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // 1. Güvenlik: Kullanıcının kendi randevusu mu kontrolü
            var appointment = await _appointmentService.Where(a => a.Pet.OwnerId == userId && a.Id == id).FirstOrDefaultAsync();

            if (appointment == null)
            {
                return NotFound(new { Message = "Randevu bulunamadı veya bu işlem için yetkiniz yok." });
            }

            // 2. İş Kuralı: 24 saat kontrolü
            var timeDifference = appointment.AppointmentTime - DateTime.Now;
            if (timeDifference.TotalHours < 24)
            {
                return BadRequest(new { Message = "Randevuya 24 saatten az kaldığı için iptal işlemi yapılamaz." });
            }

            // 3. İptal ve İade Motorunu (PaymentService ile entegre) tetikle!
            var result = await _appointmentService.CancelAppointmentAsync(id);

            if (result.IsSuccess)
            {
                return Ok(new { Message = "Randevunuz başarıyla iptal edildi. Varsa ücret iadesi yapılmıştır." });
            }

            return BadRequest(new { Message = "İptal başarısız.", Error = result.ErrorMessage });
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Veterinarian")]
        public async Task<IActionResult> ApproveAppointment(Guid id)
        {
            var isSuccess = await _appointmentService.ApproveAppointmentAsync(id);

            if (!isSuccess)
            {
                return NotFound(new { Message = "Onaylanacak randevu bulunamadı." });
            }

            return Ok(new { Message = "Randevu başarıyla onaylandı." });
        }

        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Veterinarian")]
        public async Task<IActionResult> RejectAppointment(Guid id)
        {
            
            var result = await _appointmentService.CancelAppointmentAsync(id);

            if (!result.IsSuccess)
            {
                return BadRequest(new { Message = "Randevu reddedilemedi/iptal edilemedi.", Error = result.ErrorMessage });
            }

            return Ok(new { Message = "Randevu veteriner tarafından reddedildi ve varsa ücret iade edildi." });
        }
    }
}