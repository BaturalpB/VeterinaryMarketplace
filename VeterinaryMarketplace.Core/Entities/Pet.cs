using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeterinaryMarketplace.Core.Entities
{
    public class Pet : ISoftDeletable
    {
        public bool IsDeleted { get; set; } = false;
        public Guid Id { get; set; }
        public string OwnerId { get; set; }
        public string Name { get; set; }
        public string Species { get; set; }
        public string Breed { get; set; }
        public int Age {  get; set; }
        public string ImageURL {  get; set; }
        public virtual AppUser Owner { get; set;}
        public virtual ICollection<Appointment> Appointments { get; set; }


    }
}
