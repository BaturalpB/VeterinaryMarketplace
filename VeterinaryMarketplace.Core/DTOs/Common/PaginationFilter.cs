namespace VeterinaryMarketplace.Core.DTOs.Common
{
    public class PaginationFilter
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        
        // Optional search/filter fields
        public string? SearchTerm { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? Status { get; set; } // Used for appointments (Pending, Approved, Completed, Cancelled)
    }
}
