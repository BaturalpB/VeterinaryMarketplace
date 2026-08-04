using System;

namespace VeterinaryMarketplace.Core.DTOs
{
    public class ClinicDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public bool? IsApproved { get; set; }
        
        public string Iban { get; set; }
        public string CompanyTitle { get; set; }
        public string TaxOffice { get; set; }
        public string TaxNumber { get; set; }
    }
}