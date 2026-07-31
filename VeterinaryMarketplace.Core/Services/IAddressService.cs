using VeterinaryMarketplace.Core.DTOs.Address;

namespace VeterinaryMarketplace.Core.Services
{
    public interface IAddressService
    {
        Task<(bool IsSuccess, string? ErrorMessage)> CreateAddressAsync(AddressCreateDto dto,string userId);
        Task<(bool IsSuccess, string? ErrorMessage)> UpdateAddressAsync(AddressUpdateDto dto, string userId);
        Task<(bool IsSuccess, string? ErrorMessage)> DeleteAddressAsync(Guid addressId, string userId);
        Task<bool> IsAddressExistsAsync(Guid addressId,string userId);
        Task<List<AddressDto>> GetMyAddressesAsync(string userId);
        

    }
}
