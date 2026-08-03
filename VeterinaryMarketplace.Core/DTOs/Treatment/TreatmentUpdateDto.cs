namespace VeterinaryMarketplace.Core.DTOs.Treatment
{
    public class TreatmentUpdateDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int DurationInMinutes { get; set; }
    }
}
