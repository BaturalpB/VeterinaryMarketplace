using System;
using VeterinaryMarketplace.Core.Entities;

namespace VeterinaryMarketplace.Core.Services
{
    public interface IPetService : IService<Pet>
    {
        Task<List<Pet>> GetPetsByUserIdAsync(string userId);
    }
}
