
using System;
using System.Collections.Generic;

namespace VeterinaryMarketplace.Core.DTOs.Appointment
{
    public class AppointmentCreateDto
    {
        public Guid VeterinarianDetailId { get; set; } 
        public Guid PetId { get; set; }
        public DateTime AppointmentTime { get; set; }
        public List<Guid> TreatmentIds { get; set; }     
    }
}