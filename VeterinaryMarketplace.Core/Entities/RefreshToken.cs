using System;

namespace VeterinaryMarketplace.Core.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresTime { get; set; }
        public bool IsRevoked { get; set; }
        public virtual AppUser User { get; set; }
    }
}