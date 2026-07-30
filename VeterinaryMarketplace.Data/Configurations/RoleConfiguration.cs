using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VeterinaryMarketplace.Data.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
    {
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            builder.HasData(
                new IdentityRole
                {
                    Id = "a1b2c3d4-e5f6-7a8b-9c0d-1234567890ab",
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = "stamp-admin-12345" 
                },
                new IdentityRole
                {
                    Id = "b2c3d4e5-f6a7-8b9c-0d1e-234567890abc",
                    Name = "Veterinarian",
                    NormalizedName = "VETERINARIAN",
                    ConcurrencyStamp = "stamp-vet-12345" 
                },
                new IdentityRole
                {
                    Id = "c3d4e5f6-a7b8-9c0d-1e2f-34567890abcd",
                    Name = "StandardUser",
                    NormalizedName = "STANDARDUSER",
                    ConcurrencyStamp = "stamp-user-12345" 
                }
            );
        }
    }
}