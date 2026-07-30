using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeterinaryMarketplace.Core.Entities
{
    public class AppUser:IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName {  get; set; }
        public string? IdentityNumber {  get; set; }
        public virtual ICollection<Address> Addresses { get; set; }
        public string? City { get; set; }
        public virtual ICollection<Pet> Pets { get; set; }
        public DateTime? RegisteredAt { get; set; }
        public virtual VeterinarianDetail VeterenarianDetail { get; set; }
        public virtual ICollection<Treatment> Treatments { get; set; }  
        public virtual ICollection<Review> Reviews { get; set; }
        public ICollection<WorkingHour> WorkingHours { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    }
}
