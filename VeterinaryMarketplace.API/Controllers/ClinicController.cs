using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
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

        public ClinicsController(IService<Clinic> clinicService, IMapper mapper, IValidator<ClinicCreateDto> createValidator)
        {
            _clinicService = clinicService;
            _mapper = mapper;
            _createValidator = createValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var clinics = await _clinicService.GetAllAsync();
            var clinicDtos = _mapper.Map<List<ClinicDto>>(clinics);
            return Ok(clinicDtos);
        }

        [HttpGet("filterByCity")]
        public async Task<IActionResult> GetByCity([FromQuery] string city)
        {
            var clinics = await _clinicService.Where(x => x.City.ToLower() == city.ToLower()).ToListAsync();
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

            var newClinic = _mapper.Map<Clinic>(clinicCreateDto);
            newClinic.Id = Guid.NewGuid();
            newClinic.ManagerId = userId;

            await _clinicService.AddAsync(newClinic);

            return Ok(new { Message = "Klinik başarıyla sisteme kaydedildi.", ClinicId = newClinic.Id });
        }
    }
}