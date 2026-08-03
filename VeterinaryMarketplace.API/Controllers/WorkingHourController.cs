using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using VeterinaryMarketplace.Core.DTOs.WorkingHour;
using VeterinaryMarketplace.Core.Services;

namespace VeterinaryMarketplace.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkingHourController : ControllerBase
    {
        private readonly IWorkingHourService _workinghourService;
        private readonly IValidator<WorkingHourCreateDto> _createValidator;
        private readonly IValidator<WorkingHourUpdateDto> _updateValidator;
        public WorkingHourController(IWorkingHourService workinghourService, IValidator<WorkingHourCreateDto> createValidator, IValidator<WorkingHourUpdateDto> updateValidator)
        {
            _workinghourService = workinghourService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }


        [HttpGet]
        public async Task<IActionResult> GetWorkingHour()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var workinghours = await _workinghourService.GetMyWorkingHourAsync(userId);

            return Ok(workinghours);
        }

        [HttpPost]
        public async Task<IActionResult> AddWorkingHour([FromBody] WorkingHourCreateDto workinghourCreateDto)
        {
            var validationResult = await _createValidator.ValidateAsync(workinghourCreateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _workinghourService.CreateWorkingHourAsync(workinghourCreateDto, userId);

            if (!result.IsSuccess)
            {
                return BadRequest(new { Message = result.ErrorMessage });
            }

            return Ok(new { Message = "Çalışma Saati Eklendi." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorkingHour(Guid id, [FromBody] WorkingHourUpdateDto workinghourUpdateDto)
        {
            workinghourUpdateDto.Id = id;
            var validationResult = await _updateValidator.ValidateAsync(workinghourUpdateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _workinghourService.UpdateWorkingHourAsync(workinghourUpdateDto, userId);

            if (!result.IsSuccess)
            {
                return BadRequest(new { Message = result.ErrorMessage });
            }

            return Ok(new { Message = "Çalışma Saati Güncellendi." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkingHour(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _workinghourService.DeleteWorkingHourAsync(id, userId);

            if (!result.IsSuccess)
            {
                return BadRequest(new { Message = result.ErrorMessage });
            }

            return Ok(new { Message = "Çalışma Saati Silindi." });
        }
    }
}
