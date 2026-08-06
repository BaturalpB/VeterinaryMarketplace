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
        private readonly IService<Treatment> _treatmentDbService;
        private readonly IMapper _mapper;
        private readonly IValidator<AppointmentCreateDto> _createValidator;
        private readonly IPaymentService _paymentService;

        public AppointmentController(
            IAppointmentService appointmentService,
            IService<Pet> petService,
            IService<VeterinarianDetail> veterinarianDetailService,
            IService<Clinic> clinicService,
            IService<Treatment> treatmentDbService,
            IMapper mapper,
            IValidator<AppointmentCreateDto> createValidator,
            IPaymentService paymentService)
        {
            _appointmentService = appointmentService;
            _petService = petService;
            _veterinarianDetailService = veterinarianDetailService;
            _clinicService = clinicService;
            _treatmentDbService = treatmentDbService;
            _mapper = mapper;
            _createValidator = createValidator;
            _paymentService = paymentService;
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

            if (createDto.AppointmentTime < DateTime.Now.AddHours(1))
            {
                return BadRequest(new { Message = "Randevu saati en az 1 saat sonrasına alınabilir." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var pet = await _petService.Where(x => x.Id == createDto.PetId && x.OwnerId == userId).FirstOrDefaultAsync();
            if (pet == null)
            {
                return NotFound(new { Message = "Evcil Hayvan Bulunamadı" });
            }

            var appointmentStart = createDto.AppointmentTime.AddMinutes(-30);
            var appointmentEnd = createDto.AppointmentTime.AddMinutes(30);

            var isConflict = await _appointmentService.Where(a =>
                a.VeterinarianDetailId == createDto.VeterinarianDetailId &&
                a.Status != Appointment.AppointmentStatus.Cancelled &&
                a.AppointmentTime > appointmentStart && 
                a.AppointmentTime < appointmentEnd).AnyAsync();

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
                    var treatment = await _treatmentDbService.Where(t => t.Id == treatmentId).FirstOrDefaultAsync();
                    if (treatment == null)
                    {
                        return NotFound(new { Message = "Seçilen tedavi bulunamadı." });
                    }

                    decimal itemPrice = treatment.Price;
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

            return Ok(new { Message = "Randevu ve seçilen tedaviler başarıyla oluşturuldu!", AppointmentId = newAppointment.Id });
        }

        [HttpGet("all-appointments")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAppointments([FromQuery] VeterinaryMarketplace.Core.DTOs.Common.PaginationFilter filter)
        {
            var query = _appointmentService.Where(a => a.IsPaid);

            if (!string.IsNullOrEmpty(filter.Status) && Enum.TryParse<Appointment.AppointmentStatus>(filter.Status, out var parsedStatus))
            {
                query = query.Where(a => a.Status == parsedStatus);
            }

            var totalCount = await query.CountAsync();

            query = query
                .Include(a => a.Pet).ThenInclude(p => p.Owner)
                .Include(a => a.Veterinarian).ThenInclude(v => v.Clinic)
                .Include(a => a.Veterinarian).ThenInclude(v => v.User)
                .Include(a => a.AppointmentItems).ThenInclude(ai => ai.Treatment)
                .OrderByDescending(a => a.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize);
            
            var allAppointments = await query.ToListAsync();

            if (allAppointments == null || !allAppointments.Any())
            {
                return NotFound(new { Message = "Sistemde kayıtlı herhangi bir randevu bulunamadı." });
            }

            var allAppointmentsDto = _mapper.Map<List<AppointmentDto>>(allAppointments);
            var pagedResult = new VeterinaryMarketplace.Core.DTOs.Common.PagedResult<AppointmentDto>(allAppointmentsDto, totalCount, filter.PageNumber, filter.PageSize);
            return Ok(pagedResult);
        }

        [HttpGet("my-appointments")]
        [Authorize]
        public async Task<IActionResult> GetMyAppointments([FromQuery] VeterinaryMarketplace.Core.DTOs.Common.PaginationFilter filter)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _appointmentService.Where(a => a.Pet.OwnerId == userId && a.IsPaid);

            if (!string.IsNullOrEmpty(filter.Status) && Enum.TryParse<Appointment.AppointmentStatus>(filter.Status, out var parsedStatus))
            {
                query = query.Where(a => a.Status == parsedStatus);
            }

            var totalCount = await query.CountAsync();

            var appointments = await query
                .Include(a => a.Pet)
                    .ThenInclude(p => p.Owner)
                .Include(a => a.Veterinarian)
                    .ThenInclude(v => v.Clinic)
                .Include(a => a.Veterinarian)
                    .ThenInclude(v => v.User)
                .Include(a => a.AppointmentItems)
                    .ThenInclude(ai => ai.Treatment)
                .Include(a => a.Review)
                .OrderByDescending(a => a.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            if (appointments == null || !appointments.Any())
            {
                return NotFound(new { Message = "Size ait herhangi bir randevu bulunamadı." });
            }

            var myAppointmentsDto = _mapper.Map<List<AppointmentDto>>(appointments);
            var pagedResult = new VeterinaryMarketplace.Core.DTOs.Common.PagedResult<AppointmentDto>(myAppointmentsDto, totalCount, filter.PageNumber, filter.PageSize);
            return Ok(pagedResult);
        }

        [HttpGet("vet-appointments")]
        [Authorize]
        public async Task<IActionResult> GetVetAppointments([FromQuery] VeterinaryMarketplace.Core.DTOs.Common.PaginationFilter filter)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var query = _appointmentService.Where(a => a.Veterinarian.UserId == userId && a.IsPaid);

            if (!string.IsNullOrEmpty(filter.Status) && Enum.TryParse<Appointment.AppointmentStatus>(filter.Status, out var parsedStatus))
            {
                query = query.Where(a => a.Status == parsedStatus);
            }

            var totalCount = await query.CountAsync();

            var appointments = await query
                .Include(a => a.Pet)
                    .ThenInclude(p => p.Owner)
                .Include(a => a.Veterinarian)
                    .ThenInclude(v => v.Clinic)
                .Include(a => a.Veterinarian)
                    .ThenInclude(v => v.User)
                .Include(a => a.AppointmentItems)
                    .ThenInclude(ai => ai.Treatment)
                .OrderByDescending(a => a.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            if (appointments == null || !appointments.Any())
            {
                return NotFound(new { Message = "Size atanmış herhangi bir randevu bulunamadı." });
            }

            var vetAppointmentsDto = _mapper.Map<List<AppointmentDto>>(appointments);
            var pagedResult = new VeterinaryMarketplace.Core.DTOs.Common.PagedResult<AppointmentDto>(vetAppointmentsDto, totalCount, filter.PageNumber, filter.PageSize);
            return Ok(pagedResult);
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


            if (newStatus == Core.Entities.Appointment.AppointmentStatus.Completed && Appointment.IsPaid && !string.IsNullOrEmpty(Appointment.PaymentTransactionId))
            {
                var paymentResult = await _paymentService.ApprovePaymentAsync(Appointment.Id);
                if (paymentResult.IsSuccess)
                {
                    return Ok(new { Message = "Randevu başarıyla tamamlandı ve ödeme kliniğe aktarıldı." });
                }
                else
                {
                    return Ok(new { Message = $"Randevu tamamlandı ancak ödeme aktarımı başarısız oldu: {paymentResult.ErrorMessage}" });
                }
            }
            else if (newStatus == Core.Entities.Appointment.AppointmentStatus.Cancelled && Appointment.IsPaid)
            {
                var refundResult = await _paymentService.CancelPaymentAsync(Appointment.Id);
                if (refundResult.IsSuccess)
                {
                    return Ok(new { Message = "Randevu başarıyla reddedildi ve ücret iadesi yapıldı." });
                }
                else
                {
                    return Ok(new { Message = $"Randevu reddedildi ancak ücret iadesi sırasında bir hata oluştu: {refundResult.ErrorMessage}" });
                }
            }

            return Ok(new { Message = "Randevu Başarıyla güncellendi" });
        }

        [HttpPut("{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelAppointment(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var appointment = await _appointmentService.Where(a => a.Pet.OwnerId == userId && a.Id == id).FirstOrDefaultAsync();

            if (appointment == null)
            {
                return NotFound(new { Message = "Randevu bulunamadı veya bu işlem için yetkiniz yok." });
            }

            var timeDifference = appointment.AppointmentTime - DateTime.Now;
            if (timeDifference.TotalHours < 24)
            {
                return BadRequest(new { Message = "Randevuya 24 saatten az kaldığı için iptal işlemi yapılamaz." });
            }

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