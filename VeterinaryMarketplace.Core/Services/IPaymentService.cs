using VeterinaryMarketplace.Core.DTOs.Payment;

namespace VeterinaryMarketplace.Core.Services
{
    public interface IPaymentService
    {
        Task<(bool IsSuccess, string? ErrorMessage)> ProcessPaymentAsync(PaymentRequestDto requestDto, string userId);
        Task<(bool IsSuccess, string? ErrorMessage)> CancelPaymentAsync(Guid appointmentId);
        Task<(bool IsSuccess, string? ErrorMessage)> ApprovePaymentAsync(Guid appointmentId);
    }
}