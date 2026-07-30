namespace VeterinaryMarketplace.Core.DTOs.Review
{
    public class ReviewUpdateDto
    {
        public byte Rating { get; set; }
        public string Comment { get; set; }
        public Guid ReviewId { get; set; }
    }
}
