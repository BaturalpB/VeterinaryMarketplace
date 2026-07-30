using System;

namespace VeterinaryMarketplace.Core.Entities
{
    public class Address 
    {
        public Guid Id { get; set; } 

       
        public string UserId { get; set; } 
        public AppUser User { get; set; }  

        
        public string Title { get; set; } 
        public string City { get; set; }
        public string District { get; set; } 
        public string FullAddress { get; set; } 
    }
}