using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VeterinaryMarketplace.Core.Entities;
using VeterinaryMarketplace.Core.Repositories;
using VeterinaryMarketplace.Core.Services;

namespace VeterinaryMarketplace.Service.Services
{
    public class VeterinarianDetailService : Service<VeterinarianDetail>, IVeterinarianDetailService
    {
        private readonly IGenericRepository<VeterinarianDetail> _repository;

        public VeterinarianDetailService(IGenericRepository<VeterinarianDetail> repository, IUnitOfWork unitOfWork) : base(repository, unitOfWork)
        {
            _repository = repository;
        }

        public async Task<List<VeterinarianDetail>> GetAllWithClinicAsync()
        {
            return await _repository.Where(x => true)
                .AsNoTracking()
                .AsSplitQuery()
                .Include(v => v.Clinic)
                .Include(v => v.User)
                .ToListAsync();
        }

        public async Task<List<VeterinarianDetail>> GetApprovedWithClinicAsync(Guid? clinicId)
        {
            var query = _repository.Where(v => v.ISAproved == true);
            if (clinicId.HasValue)
            {
                query = query.Where(v => v.ClinicId == clinicId.Value);
            }
            return await query
                .AsNoTracking()
                .AsSplitQuery()
                .Include(v => v.Clinic)
                .Include(v => v.User)
                .ToListAsync();
        }

        public async Task<VeterinarianDetail?> GetByUserIdAsync(string userId)
        {
            return await _repository.Where(x => x.UserId == userId)
                .AsNoTracking()
                .AsSplitQuery()
                .Include(v => v.User)
                .Include(v => v.Clinic)
                .FirstOrDefaultAsync();
        }
    }
}
