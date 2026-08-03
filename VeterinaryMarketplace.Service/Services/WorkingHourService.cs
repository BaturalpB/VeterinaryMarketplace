using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VeterinaryMarketplace.Core.DTOs.Treatment;
using VeterinaryMarketplace.Core.DTOs.WorkingHour;
using VeterinaryMarketplace.Core.Entities;
using VeterinaryMarketplace.Core.Repositories;
using VeterinaryMarketplace.Core.Services;
namespace VeterinaryMarketplace.Service.Services
{
    public class WorkingHourService: Service<WorkingHour>, IWorkingHourService
    {
        private readonly IMapper _mapper;
        public WorkingHourService(IGenericRepository<WorkingHour> repository, IUnitOfWork unitOfWork, IMapper mapper) : base(repository, unitOfWork)
        {
            _mapper = mapper;
        }
        public async Task<(bool IsSuccess, string? ErrorMessage)> CreateWorkingHourAsync(WorkingHourCreateDto dto, string userId)
        {
            var workinghour = _mapper.Map<WorkingHour>(dto);
            workinghour.UserId = userId;
            await AddAsync(workinghour);
            return (true, null);

        }
        public async Task<(bool IsSuccess, string? ErrorMessage)> UpdateWorkingHourAsync(WorkingHourUpdateDto dto, string userId)
        {
            var workinghour = await GetByIdAsync(dto.Id);
            if (workinghour == null)
            {
                return (false, "Çalışma Saati Bulunamadı.");
            }
            if (workinghour.UserId != userId)
            {
                return (false, "Çalışma Saati size ait değil.");

            }
            _mapper.Map(dto, workinghour);
            await UpdateAsync(workinghour);
            return (true, null);

        }
        public async Task<(bool IsSuccess, string? ErrorMessage)> DeleteWorkingHourAsync(Guid workinghourId, string userId)
        {
            var treatment = await GetByIdAsync(workinghourId);
            if (treatment == null)
            {
                return (false, "Çalışma Saati Bulunamadı.");
            }
            if (treatment.UserId != userId)
            {
                return (false, "Çalışma Saati size ait değil.");

            }
            await RemoveAsync(treatment);
            return (true, null);
        }
        public async Task<bool> IsWorkingHourExistsAsync(Guid workinghourId, string userId)
        {
            return await AnyAsync(x => x.Id == workinghourId && x.UserId == userId);
        }
        public async Task<List<WorkingHourDto>> GetMyWorkingHourAsync(string userId)
        {
            var treatments = await Where(x => x.UserId == userId).ToListAsync();
            return _mapper.Map<List<WorkingHourDto>>(treatments);
        }
    }
}
