using System;

namespace VeterinaryMarketplace.Core.Entities
{
    public class WorkingHour : ISoftDeletable
    {
        public bool IsDeleted { get; set; } = false;
        public Guid Id { get; set; }

        public string UserId { get; set; }
        public virtual AppUser User { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}