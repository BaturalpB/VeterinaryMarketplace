using System;

namespace VeterinaryMarketplace.Core.DTOs.Payment
{
    public class PaymentRequestDto
    {
        public string CardHolderName { get; set; } = null!;
        public string CardNumber { get; set; } = null!;
        public string ExpireMonth { get; set; } = null!;
        public string ExpireYear { get; set; } = null!;
        public string Cvc { get; set; } = null!;
        public decimal Price { get; set; }
        public Guid AppointmentId { get; set; }
    }
}