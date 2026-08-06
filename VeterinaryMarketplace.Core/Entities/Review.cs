using System;

namespace VeterinaryMarketplace.Core.Entities
{
    public class Review : ISoftDeletable
    {
        public bool IsDeleted { get; set; } = false;
        public Guid Id { get; set; }

       
        public Guid AppointmentId { get; set; }
        public virtual Appointment Appointment { get; set; }

        public byte Rating { get; set; }

        public string Comment { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}