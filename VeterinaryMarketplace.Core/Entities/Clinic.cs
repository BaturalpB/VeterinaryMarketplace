using System;
using System.Collections.Generic;

namespace VeterinaryMarketplace.Core.Entities
{
    public class Clinic : ISoftDeletable
    {
        public bool IsDeleted { get; set; } = false;
        public Guid Id { get; set; }
        public string Name { get; set; }

        public string ManagerId { get; set; }
        public string City { get; set; }     
        public string District { get; set; } 
        public string Address { get; set; }   
        public string PhoneNumber { get; set; }
        
        public string? Iban { get; set; }
        public string? CompanyTitle { get; set; }
        public string? TaxOffice { get; set; }
        public string? TaxNumber { get; set; }
        public string? SubMerchantKey { get; set; }

        public bool? IsApproved { get; set; } = null;

        public virtual ICollection<VeterinarianDetail> Veterinarians { get; set; }
    }
}