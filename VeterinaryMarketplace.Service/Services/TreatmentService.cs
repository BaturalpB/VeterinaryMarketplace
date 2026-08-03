using AutoMapper;
using VeterinaryMarketplace.Core.Entities;
using VeterinaryMarketplace.Core.DTOs.Treatment;
using VeterinaryMarketplace.Core.Repositories;
using VeterinaryMarketplace.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace VeterinaryMarketplace.Service.Services
{
    public class TreatmentService : Service<Treatment>, ITreatmentService
    {
        private readonly IMapper _mapper;
        public TreatmentService(IGenericRepository<Treatment> repository, IUnitOfWork unitOfWork, IMapper mapper) : base(repository, unitOfWork)
        {
            _mapper = mapper;
        }
        public async Task<(bool IsSuccess, string? ErrorMessage)> CreateTreatmentAsync(TreatmentCreateDto dto, string userId)
        {
            var treatment = _mapper.Map<Treatment>(dto);
            treatment.UserID = userId;
            await AddAsync(treatment);
            return (true, null);

        }
        public async Task<(bool IsSuccess, string? ErrorMessage)> UpdateTreatmentAsync(TreatmentUpdateDto dto, string userId)
        {
            var treatment = await GetByIdAsync(dto.Id);
            if (treatment == null)
            {
                return (false, "Tedavi Bulunamadı.");
            }
            if (treatment.UserID != userId)
            {
                return (false, "Tedavi size ait değil.");

            }
            _mapper.Map(dto, treatment);
            await UpdateAsync(treatment);
            return (true, null);

        }
        public async Task<(bool IsSuccess, string? ErrorMessage)> DeleteTreatmentAsync(Guid treatmentId, string userId)
        {
            var treatment = await GetByIdAsync(treatmentId);
            if (treatment == null)
            {
                return (false, "Tedavi Bulunamadı.");
            }
            if (treatment.UserID != userId)
            {
                return (false, "Tedavi size ait değil.");

            }
            await RemoveAsync(treatment);
            return (true, null);
        }
        public async Task<bool> IsTreatmentExistsAsync(Guid treatmentId, string userId)
        {
            return await AnyAsync(x=>x.Id==treatmentId&& x.UserID==userId);
        }
        public async Task<List<TreatmentDto>> GetMyTreatmentsAsync(string userId)
        {
            var treatments=await Where(x=>x.UserID==userId).ToListAsync();
            return _mapper.Map<List<TreatmentDto>>(treatments);
        }
    }
}
