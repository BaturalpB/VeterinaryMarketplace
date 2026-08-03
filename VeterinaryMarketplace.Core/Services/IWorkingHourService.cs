
using VeterinaryMarketplace.Core.DTOs.WorkingHour;

namespace VeterinaryMarketplace.Core.Services
{
    public interface IWorkingHourService
    {
        Task<(bool IsSuccess, string? ErrorMessage)> CreateWorkingHourAsync(WorkingHourCreateDto dto, string userId);
        Task<(bool IsSuccess, string? ErrorMessage)> UpdateWorkingHourAsync(WorkingHourUpdateDto dto, string userId);
        Task<(bool IsSuccess, string? ErrorMessage)> DeleteWorkingHourAsync(Guid workinghourId, string userId);
        Task<bool> IsWorkingHourExistsAsync(Guid treatmentId, string userId);
        Task<List<WorkingHourDto>> GetMyWorkingHourAsync(string userId);
    }
}
