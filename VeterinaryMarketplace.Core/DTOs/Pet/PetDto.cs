namespace VeterinaryMarketplace.Core.DTOs
{
    public class PetDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Species { get; set; }
        public string Breed { get; set; }
        public int Age { get; set; }
        public string ImageURL { get; set; }
        public string OwnerId { get; set; }
    }
}