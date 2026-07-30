using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VeterinaryMarketplace.Core.Entities;

namespace VeterinaryMarketplace.Data.Configurations
{
    public class PetConfiguration : IEntityTypeConfiguration<Pet>
    {
        public void Configure(EntityTypeBuilder<Pet> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Species).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Breed).HasMaxLength(100);
            builder.Property(x => x.ImageURL).HasMaxLength(500);

            builder.HasOne(p => p.Owner)
                   .WithMany(u => u.Pets)
                   .HasForeignKey(p => p.OwnerId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}