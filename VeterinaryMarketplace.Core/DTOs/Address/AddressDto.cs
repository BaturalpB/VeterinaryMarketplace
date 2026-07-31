using VeterinaryMarketplace.Core.Entities;

namespace VeterinaryMarketplace.Core.DTOs.Address
{
    public class AddressDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string FullAddress { get; set; }
        public string User { get; set; }
    }
}
