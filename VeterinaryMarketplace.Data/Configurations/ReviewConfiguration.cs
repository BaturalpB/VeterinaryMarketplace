using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VeterinaryMarketplace.Core.Entities;

namespace VeterinaryMarketplace.Data.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(x => x.Id);

          
            builder.Property(x => x.Comment).HasMaxLength(1000);

            
            builder.HasOne(r => r.Appointment)
                   .WithOne(a => a.Review)
                  
                   .HasForeignKey<Review>(r => r.AppointmentId);
        }
    }
}