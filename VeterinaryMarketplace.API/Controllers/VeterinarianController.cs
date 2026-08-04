using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VeterinaryMarketplace.Core.DTOs;
using VeterinaryMarketplace.Core.Entities;
using VeterinaryMarketplace.Core.Services;

namespace VeterinaryMarketplace.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VeterinariansController : ControllerBase
    {
        private readonly IVeterinarianDetailService _veterinarianService;
        private readonly IService<Clinic> _clinicService;
        private readonly IMapper _mapper;
        private readonly IValidator<VeterinarianCreateDto> _createValidator;

        public VeterinariansController(
            IVeterinarianDetailService veterinarianService,
            IService<Clinic> clinicService,
            IMapper mapper,
            IValidator<VeterinarianCreateDto> createValidator)
        {
            _veterinarianService = veterinarianService;
            _clinicService = clinicService;
            _mapper = mapper;
            _createValidator = createValidator;
        }

        [HttpPost]
        [Authorize] 
        public async Task<IActionResult> CreateProfile([FromBody] VeterinarianCreateDto createDto)
        {
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingProfile = await _veterinarianService.GetByUserIdAsync(userId);
            if (existingProfile != null)
            {
                return BadRequest(new { Message = "Sizin zaten sisteme kayıtlı bir veteriner profiliniz bulunuyor!" });
            }

            var clinic = await _clinicService.GetByIdAsync(createDto.ClinicId);
            if (clinic == null)
            {
                return NotFound(new { Message = "Seçilen klinik sistemde bulunamadı." });
            }

            var newVeterinarian = _mapper.Map<VeterinarianDetail>(createDto);
            newVeterinarian.Id = Guid.NewGuid();
            newVeterinarian.UserId = userId;
            newVeterinarian.ISAproved = false;

            await _veterinarianService.AddAsync(newVeterinarian);

            return Ok(new { Message = "Veteriner profili başarıyla oluşturuldu. Profiliniz admin/yönetici onayından sonra aktif edilecektir." });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var veterinarians = await _veterinarianService.GetAllWithClinicAsync();

            var vetDtos = _mapper.Map<List<VeterinarianDto>>(veterinarians);
            return Ok(vetDtos);
        }

        [HttpPatch("{id}/approve")]
        [Authorize]
        public async Task<IActionResult> ApproveVeterinarian(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var veterinarian = await _veterinarianService.GetByIdAsync(id);

            if (veterinarian == null)
            {
                return NotFound(new { Message = "Onaylanacak Veteriner yok" });
            }

            var clinic = await _clinicService.GetByIdAsync(veterinarian.ClinicId);

            if (veterinarian.ISAproved)
            {
                return BadRequest(new { Message = "Bu veteriner zaten onaylandı" });
            }
            if (clinic.ManagerId.ToLower() != userId.ToLower())
            {
                return BadRequest(new { Message = "Bu kliniğin yöneticisi siz değilsiniz!" });
            }

            veterinarian.ISAproved = true;
            await _veterinarianService.UpdateAsync(veterinarian);
            return Ok(new { Message = "Veteriner Onaylandı." });
        }

        [HttpPatch("toggle-status")]
        [Authorize(Roles = "Veterinarian")]
        public async Task<IActionResult> ToggleStatus()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var veterinarian = await _veterinarianService.GetByUserIdAsync(userId);
            if (veterinarian == null)
            {
                return NotFound(new { Message = "Profil bulunamadı." });
            }

            veterinarian.IsEmergencyClosed = !veterinarian.IsEmergencyClosed;
            await _veterinarianService.UpdateAsync(veterinarian);

            return Ok(new { Message = veterinarian.IsEmergencyClosed ? "Klinik geçici olarak kapatıldı." : "Klinik tekrar açıldı.", IsEmergencyClosed = veterinarian.IsEmergencyClosed });
        }

        [HttpGet("approved")]
        public async Task<IActionResult> GetApprovedVeterinarians([FromQuery] Guid? clinicId)
        {
            var veterinarians = await _veterinarianService.GetApprovedWithClinicAsync(clinicId);

            if (!veterinarians.Any())
            {
                return NotFound(new { Message = "Aradığınız kritere uygun onaylı veteriner bulunamadı." });
            }

            var vetDtos = _mapper.Map<List<VeterinarianDto>>(veterinarians);
            return Ok(vetDtos);
        }
    }
}