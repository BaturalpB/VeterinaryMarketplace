public class AppointmentDto
{
    public Guid Id { get; set; }
    public DateTime AppointmentTime { get; set; }
    public decimal Price { get; set; }
    public string Status { get; set; }
    public Guid PetId { get; set; }
    public string PetName { get; set; } 
    public string VeterinarianInfo { get; set; }
    public string ClinicName { get; set; }
    public string? VeterinarianNote { get; set; }
    public List<AppointmentItemDto> AppointmentItems { get; set; }
    public VeterinaryMarketplace.Core.DTOs.Review.ReviewDto? Review { get; set; }
}

public class AppointmentItemDto
{
    public string TreatmentName { get; set; }
    public decimal Price { get; set; }
}