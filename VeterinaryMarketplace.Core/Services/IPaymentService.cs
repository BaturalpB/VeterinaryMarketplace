using VeterinaryMarketplace.Core.DTOs.Payment;

namespace VeterinaryMarketplace.Core.Services
{
    public interface IPaymentService
    {
        Task<(bool IsSuccess, string? ErrorMessage)> ProcessPaymentAsync(PaymentRequestDto requestDto, string userId);
    }
}