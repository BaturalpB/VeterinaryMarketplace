using VeterinaryMarketplace.Core.Entities;
using VeterinaryMarketplace.Core.DTOs.Review;

namespace VeterinaryMarketplace.Core.Services
{
    public interface IReviewService : IService<Review>
    {
        Task<(bool IsSuccess, string? ErrorMessage)> CreateReviewAsync(ReviewCreateDto dto, string userId);
        Task<(bool IsSuccess, string? ErrorMessage)> UpdateReviewAsync(ReviewUpdateDto dto, string userId);
        Task<(bool IsSuccess, string? ErrorMessage)> DeleteReviewAsync(Guid reviewId, string userId);
    }
}