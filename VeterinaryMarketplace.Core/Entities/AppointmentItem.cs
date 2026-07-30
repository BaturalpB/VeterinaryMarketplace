using System;

namespace VeterinaryMarketplace.Core.Entities
{
    public class AppointmentItem
    {
      
        public Guid Id { get; set; }
        public Guid AppointmentId { get; set; }
        public virtual Appointment Appointment { get; set; }
        public Guid TreatmentId { get; set; }
        public virtual Treatment Treatment { get; set; }

        public decimal Price { get; set; }
    }
}