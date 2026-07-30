using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VeterinaryMarketplace.Core.Entities;

namespace VeterinaryMarketplace.Data.Configurations
{
    public class TreatmentConfiguration : IEntityTypeConfiguration<Treatment>
    {
        public void Configure(EntityTypeBuilder<Treatment> builder)
        {
           
            builder.HasKey(x => x.Id);

           
            builder.Property(x => x.Title).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Description).HasMaxLength(500);

            
            builder.Property(x => x.Price).HasColumnType("decimal(18,2)");

            
            builder.HasOne(t => t.User)
                   .WithMany(u => u.Treatments)
                   .HasForeignKey(t => t.UserID)
              
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}