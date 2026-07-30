using System;

namespace VeterinaryMarketplace.Core.DTOs
{
    public class VeterinarianDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }

        
        public Guid ClinicId { get; set; }
        public string ClinicName { get; set; }

        public string Uzmanlik { get; set; }
        public TimeSpan Baslangic { get; set; }
        public TimeSpan Bitis { get; set; }
        public decimal CommissionRate { get; set; }
        public bool ISAproved { get; set; }
    }
}