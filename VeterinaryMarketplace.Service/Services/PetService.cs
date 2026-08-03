using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VeterinaryMarketplace.Core.Entities;
using VeterinaryMarketplace.Core.Repositories;
using VeterinaryMarketplace.Core.Services;

namespace VeterinaryMarketplace.Service.Services
{
    public class PetService : Service<Pet>, IPetService
    {
        private readonly IGenericRepository<Pet> _repository;

        public PetService(IGenericRepository<Pet> repository, IUnitOfWork unitOfWork) : base(repository, unitOfWork)
        {
            _repository = repository;
        }

        public async Task<List<Pet>> GetPetsByUserIdAsync(string userId)
        {
            return await _repository.Where(x => x.OwnerId == userId).ToListAsync();
        }
    }
}
