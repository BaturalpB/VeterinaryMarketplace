using System;

namespace VeterinaryMarketplace.Core.DTOs
{
    public class VeterinarianCreateDto
    {
        
        public Guid ClinicId { get; set; }

        public string Uzmanlik { get; set; }
        public TimeSpan Baslangic { get; set; }
        public TimeSpan Bitis { get; set; }

        
        public string IBAN { get; set; }
        public string SubMerchantKey { get; set; }
        public decimal CommissionRate { get; set; }

        
    }
}