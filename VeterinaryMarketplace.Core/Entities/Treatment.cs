using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeterinaryMarketplace.Core.Entities
{
    public class Treatment
    {
        public Guid Id { get; set; }
        public string UserID { get; set; }
        public virtual AppUser User { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int DurationInMinutes { get; set; }

    }
}
