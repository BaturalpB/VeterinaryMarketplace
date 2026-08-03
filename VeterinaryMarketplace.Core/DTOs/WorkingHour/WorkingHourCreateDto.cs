namespace VeterinaryMarketplace.Core.DTOs.WorkingHour
{
    public class WorkingHourCreateDto
    {
        public Guid Id { get; set; }
        public DayOfWeek DayOfWeek { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
