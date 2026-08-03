using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VeterinaryMarketplace.Core.Entities;

namespace VeterinaryMarketplace.Core.Services
{
    public interface IVeterinarianDetailService : IService<VeterinarianDetail>
    {
        Task<List<VeterinarianDetail>> GetAllWithClinicAsync();
        Task<List<VeterinarianDetail>> GetApprovedWithClinicAsync(Guid? clinicId);
        Task<VeterinarianDetail?> GetByUserIdAsync(string userId);
    }
}
