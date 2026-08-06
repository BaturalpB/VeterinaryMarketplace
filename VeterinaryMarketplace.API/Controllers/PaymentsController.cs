using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VeterinaryMarketplace.Core.DTOs.Payment;
using VeterinaryMarketplace.Core.Services;

namespace VeterinaryMarketplace.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestDto requestDto)
        {
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Message = "Kullanıcı kimliği doğrulanamadı. Lütfen tekrar giriş yapın." });
            }

            
            var result = await _paymentService.ProcessPaymentAsync(requestDto, userId);

            if (result.IsSuccess)
            {
                return Ok(new { Message = "Ödeme işlemi başarıyla tamamlandı!" });
            }

            
            return BadRequest(new { Message = "Ödeme başarısız.", Error = result.ErrorMessage });
        }

        [HttpPost("cancel/{appointmentId}")]
        public async Task<IActionResult> CancelPayment(Guid appointmentId)
        {
            var result = await _paymentService.CancelPaymentAsync(appointmentId);

            if (result.IsSuccess)
            {
                return Ok(new { Message = "Ödeme başarıyla iptal edildi ve ücret müşterinin kartına iade edildi." });
            }

            return BadRequest(new { Message = "İptal işlemi başarısız.", Error = result.ErrorMessage });
        }
        [HttpPost("approve/{appointmentId}")]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> ApprovePayment(Guid appointmentId)
        {
            var result = await _paymentService.ApprovePaymentAsync(appointmentId);

            if (result.IsSuccess)
            {
                return Ok(new { Message = "Ödeme onaylandı ve para kliniğin hesabına başarıyla aktarıldı." });
            }

            return BadRequest(new { Message = "Onay işlemi başarısız.", Error = result.ErrorMessage });
        }
    }
}