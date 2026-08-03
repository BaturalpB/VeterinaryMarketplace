using AutoMapper;
using VeterinaryMarketplace.Core.Entities;
using VeterinaryMarketplace.Core.DTOs.Address;
using VeterinaryMarketplace.Core.Repositories;
using VeterinaryMarketplace.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace VeterinaryMarketplace.Service.Services
{
    public class AddressService : Service<Address>, IAddressService
    {
        private readonly IMapper _mapper;
        public AddressService(IGenericRepository<Address> repository, IUnitOfWork unitOfWork, IMapper mapper) : base(repository, unitOfWork)
        {
            _mapper = mapper;
        }
        public async Task<(bool IsSuccess, string? ErrorMessage)> CreateAddressAsync(AddressCreateDto dto, string userId)
        {
            var address = _mapper.Map<Address>(dto);
            address.UserId = userId;
            await AddAsync(address);
            return (true, null);
        }
        public async Task<(bool IsSuccess, string? ErrorMessage)> UpdateAddressAsync(AddressUpdateDto dto, string userId)
        {
            var address=await GetByIdAsync(dto.Id);
            if (address == null)
            {
                return (false,"Güncellemek istediğiniz adres bulunamadı.");
            }
            if (address.UserId != userId)
            {
                return (false, "Bu adresi güncellemek için yetkiniz yok.");
            }
            _mapper.Map(dto, address);
            await UpdateAsync(address);
            return (true, null);
        }
        public async Task<(bool IsSuccess, string? ErrorMessage)> DeleteAddressAsync(Guid addressId, string userId)
        {
            var address = await GetByIdAsync(addressId);
            if (address == null)
            {
                return (false, "Adres Bulunamadı.");
            }
            if (address.UserId != userId)
            {
                return (false, "Bu adresi silmek için yetkiniz yok.");
            }

            await RemoveAsync(address);
            return (true, null);
        }
        public async Task<bool> IsAddressExistsAsync(Guid addressId, string userId)
        {
            
            return await AnyAsync(x => x.Id == addressId && x.UserId == userId);
        }
        public async Task<List<AddressDto>> GetMyAddressesAsync(string userId)
        {
            var my_addresses=await Where(x=> x.UserId == userId).ToListAsync();
            
            return _mapper.Map<List<AddressDto>>(my_addresses);
        }
    }

}
