using System;
using System.Collections.Generic;

namespace VeterinaryMarketplace.Core.Entities
{
    public class Appointment
    {
        public Guid Id { get; set; }
        public Guid PetId { get; set; }
        public Guid VeterinarianDetailId { get; set; }
        public DateTime AppointmentTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public decimal Price { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
        public bool IsCancelled { get; set; } = false;
        public enum AppointmentStatus
        {
            Pending = 1,
            Approved = 2,
            Completed = 3,
            Cancelled = 4
        }

        public string? TransactionID { get; set; }
        public bool IsPaid { get; set; } = false;
        public string? PaymentTransactionId { get; set; }

        
        public string? VeterinarianNote { get; set; }

        public virtual Pet Pet { get; set; }
        public virtual VeterinarianDetail Veterinarian { get; set; }

        public virtual ICollection<AppointmentItem> AppointmentItems { get; set; }

       
        public virtual Review? Review { get; set; }
    }
}