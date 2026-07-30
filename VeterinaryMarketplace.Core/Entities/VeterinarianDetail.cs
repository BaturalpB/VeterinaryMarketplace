using System;
using System.Collections.Generic;

namespace VeterinaryMarketplace.Core.Entities
{
    public class VeterinarianDetail
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public Guid ClinicId { get; set; }

        public string Uzmanlik { get; set; }
        public TimeSpan Baslangic { get; set; }
        public TimeSpan Bitis { get; set; }
        public string IBAN { get; set; }
        public string SubMerchantKey { get; set; }
        public decimal CommissionRate { get; set; }
        public bool ISAproved { get; set; } = false;

        public virtual AppUser User { get; set; }
        public virtual Clinic Clinic { get; set; } 
        public virtual ICollection<Appointment> Appointments { get; set; }
    }
}