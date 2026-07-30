namespace VeterinaryMarketplace.Core.DTOs.Review
{
    public class ReviewCreateDto
    {
        public byte Rating { get; set; }
        public string Comment { get; set; }
        public Guid AppointmentId { get; set; }
    }
}
