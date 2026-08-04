using System.Threading.Tasks;
using VeterinaryMarketplace.Core.Entities;

namespace VeterinaryMarketplace.Core.Services
{
    public interface IIyzicoOnboardingService
    {
        Task<(bool IsSuccess, string? SubMerchantKey, string? ErrorMessage)> CreateSubMerchantAsync(Clinic clinic, AppUser managerUser);
    }
}
