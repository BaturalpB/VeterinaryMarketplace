using System;
using System.Collections.Generic;

namespace VeterinaryMarketplace.Core.Entities
{
    public class Clinic
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public string ManagerId { get; set; }
        public string City { get; set; }     
        public string District { get; set; } 
        public string Address { get; set; }   
        public string PhoneNumber { get; set; }

        public virtual ICollection<VeterinarianDetail> Veterinarians { get; set; }
    }
}