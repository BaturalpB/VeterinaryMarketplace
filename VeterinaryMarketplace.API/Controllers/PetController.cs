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
    [Authorize]
    public class PetsController : ControllerBase
    {
        private readonly IService<Pet> _petService;
        private readonly IMapper _mapper;
        private readonly IValidator<PetCreateDto> _createValidator;
        private readonly IValidator<PetUpdateDto> _updateValidator;
        public PetsController(IService<Pet> petService, IMapper mapper, IValidator<PetCreateDto> createValidator, IValidator<PetUpdateDto> updateValidator)
        {
            _petService = petService;
            _mapper = mapper;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyPets()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var pets = await _petService.Where(x => x.OwnerId == userId).ToListAsync();

            var petDtos = _mapper.Map<List<PetDto>>(pets);

            return Ok(petDtos);
        }

        [HttpPost]
        public async Task<IActionResult> AddPet([FromBody] PetCreateDto petCreateDto)
        {
            var validationResult = await _createValidator.ValidateAsync(petCreateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var newPet = _mapper.Map<Pet>(petCreateDto);
            newPet.Id = Guid.NewGuid();
            newPet.OwnerId = userId;

            await _petService.AddAsync(newPet);

            return Ok(new { Message = "Evcil hayvan başarıyla sisteme kaydedildi.", PetId = newPet.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePet(Guid id, [FromBody] PetUpdateDto petUpdateDto)
        {
            var validationResult = await _updateValidator.ValidateAsync(petUpdateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var pet = await _petService.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (pet == null)
            {
                return NotFound(new { Message = "Evcil hayvan bulunamadı." });
            }

            if (pet.OwnerId != userId)
            {
                return StatusCode(403, new { Message = "Sadece kendi evcil hayvanınızın bilgilerini güncelleyebilirsiniz!" });
            }

            _mapper.Map(petUpdateDto, pet);

            await _petService.UpdateAsync(pet);

            return Ok(new { Message = "Evcil hayvan bilgileri başarıyla güncellendi." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePet(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var pet = await _petService.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (pet == null)
            {
                return NotFound(new { Message = "Evcil hayvan bulunamadı." });
            }

            if (pet.OwnerId != userId)
            {
                return StatusCode(403, new { Message = "Sadece kendi evcil hayvanınızı silebilirsiniz!" });
            }

            await _petService.RemoveAsync(pet);

            return Ok(new { Message = "Evcil hayvan sistemden başarıyla silindi." });
        }
    }
}