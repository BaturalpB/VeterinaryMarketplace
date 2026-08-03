using VeterinaryMarketplace.Core.DTOs.Treatment;

namespace VeterinaryMarketplace.Core.Services
{
    public interface ITreatmentService
    {
        Task<(bool IsSuccess, string? ErrorMessage)> CreateTreatmentAsync(TreatmentCreateDto dto, string userId);
        Task<(bool IsSuccess, string? ErrorMessage)> UpdateTreatmentAsync(TreatmentUpdateDto dto, string userId);
        Task<(bool IsSuccess, string? ErrorMessage)> DeleteTreatmentAsync(Guid treatmentId ,string userId);
        Task<bool> IsTreatmentExistsAsync(Guid treatmentId, string userId);
        Task<List<TreatmentDto>> GetMyTreatmentsAsync(string userId);
    }
}
