using System;

namespace VeterinaryMarketplace.Core.DTOs.Review
{
    public class ReviewDto
    {
        public Guid Id { get; set; }
        public Guid AppointmentId { get; set; }
        public byte Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ReviewerName { get; set; }
        public string PetName { get; set; }
    }
}
