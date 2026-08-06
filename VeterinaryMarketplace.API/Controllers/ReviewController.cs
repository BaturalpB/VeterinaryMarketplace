using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System;
using System.Threading.Tasks;
using VeterinaryMarketplace.Core.DTOs;
using VeterinaryMarketplace.Core.DTOs.Review;
using VeterinaryMarketplace.Core.Services;

namespace VeterinaryMarketplace.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddReview([FromBody] ReviewCreateDto reviewCreateDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _reviewService.CreateReviewAsync(reviewCreateDto, userId);

            if (result.IsSuccess) { return Ok("Değerlendirme başarıyla kaydedildi."); }
            return BadRequest(result.ErrorMessage);
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateReview([FromBody] ReviewUpdateDto reviewUpdateDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _reviewService.UpdateReviewAsync(reviewUpdateDto, userId);

            if (result.IsSuccess)
            {
                return Ok("Değerlendirme başarıyla güncellendi.");
            }
            return BadRequest(result.ErrorMessage);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _reviewService.DeleteReviewAsync(id, userId);

            if (result.IsSuccess)
            {
                return Ok("Değerlendirme başarıyla silindi.");
            }
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("clinic/{clinicId}")]
        public async Task<IActionResult> GetClinicReviews(Guid clinicId)
        {
            var reviews = await _reviewService.GetClinicReviewsAsync(clinicId);
            return Ok(reviews);
        }
    }
}