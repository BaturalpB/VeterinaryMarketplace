using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using VeterinaryMarketplace.Core.DTOs.Address;
using VeterinaryMarketplace.Core.DTOs.Treatment;
using VeterinaryMarketplace.Core.Services;
using VeterinaryMarketplace.Service.Services;

namespace VeterinaryMarketplace.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TreatmentController : ControllerBase
    {
        private readonly ITreatmentService _treatmentService;
        private readonly IValidator<TreatmentCreateDto> _createValidator;
        private readonly IValidator<TreatmentUpdateDto> _updateValidator;
        public TreatmentController(ITreatmentService treatmentService, IValidator<TreatmentCreateDto> createValidator, IValidator<TreatmentUpdateDto> updateValidator)
        {
            _treatmentService = treatmentService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetTreatments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var treatments = await _treatmentService.GetMyTreatmentsAsync(userId);

            return Ok(treatments);
        }

        [HttpGet("user/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTreatmentsByUser(string userId)
        {
            var treatments = await _treatmentService.GetMyTreatmentsAsync(userId);
            return Ok(treatments);
        }

        [HttpPost]
        public async Task<IActionResult> AddTreatment([FromBody] TreatmentCreateDto treatmentCreateDto)
        {
            var validationResult = await _createValidator.ValidateAsync(treatmentCreateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _treatmentService.CreateTreatmentAsync(treatmentCreateDto, userId);

            if (!result.IsSuccess)
            {
                return BadRequest(new { Message = result.ErrorMessage });
            }

            return Ok(new { Message = "Tedavi Eklendi." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTreatment(Guid id, [FromBody] TreatmentUpdateDto treatmentUpdateDto)
        {
            treatmentUpdateDto.Id = id;
            var validationResult = await _updateValidator.ValidateAsync(treatmentUpdateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _treatmentService.UpdateTreatmentAsync(treatmentUpdateDto, userId);

            if (!result.IsSuccess)
            {
                return BadRequest(new { Message = result.ErrorMessage });
            }

            return Ok(new { Message = "Tedavi Güncellendi." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTreatment(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _treatmentService.DeleteTreatmentAsync(id, userId);

            if (!result.IsSuccess)
            {
                return BadRequest(new { Message = result.ErrorMessage });
            }

            return Ok(new { Message = "Tedavi Silindi." });
        }
    }
}
