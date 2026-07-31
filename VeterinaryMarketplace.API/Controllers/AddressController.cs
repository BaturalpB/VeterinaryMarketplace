using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using VeterinaryMarketplace.Core.DTOs.Address;
using VeterinaryMarketplace.Core.Services;

namespace VeterinaryMarketplace.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;
        private readonly IValidator<AddressCreateDto> _createValidator;
        private readonly IValidator<AddressUpdateDto> _updateValidator;

        public AddressController(IAddressService addressService, IValidator<AddressCreateDto> createValidator, IValidator<AddressUpdateDto> updateValidator)
        {
            _addressService = addressService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAddresses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var addresses = await _addressService.GetMyAddressesAsync(userId);

            return Ok(addresses);
        }

        [HttpPost]
        public async Task<IActionResult> AddAddress([FromBody] AddressCreateDto addressCreateDto)
        {
            var validationResult = await _createValidator.ValidateAsync(addressCreateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _addressService.CreateAddressAsync(addressCreateDto, userId);

            if (!result.IsSuccess)
            {
                return BadRequest(new { Message = result.ErrorMessage });
            }

            return Ok(new { Message = "Adres Eklendi." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] AddressUpdateDto addressUpdateDto)
        {
            addressUpdateDto.Id = id;
            var validationResult = await _updateValidator.ValidateAsync(addressUpdateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _addressService.UpdateAddressAsync(addressUpdateDto, userId);

            if (!result.IsSuccess)
            {
                return BadRequest(new { Message = result.ErrorMessage });
            }

            return Ok(new { Message = "Adres Güncellendi." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _addressService.DeleteAddressAsync(id, userId);

            if (!result.IsSuccess)
            {
                return BadRequest(new { Message = result.ErrorMessage });
            }

            return Ok(new { Message = "Adres Silindi." });
        }
    }
}