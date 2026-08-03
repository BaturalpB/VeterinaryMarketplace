namespace VeterinaryMarketplace.Core.DTOs.Treatment
{
    public class TreatmentCreateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int DurationInMinutes { get; set; }
    }
}
