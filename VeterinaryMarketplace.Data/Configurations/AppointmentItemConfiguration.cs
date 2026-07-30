using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VeterinaryMarketplace.Core.Entities;

namespace VeterinaryMarketplace.Data.Configurations
{
    public class AppointmentItemConfiguration : IEntityTypeConfiguration<AppointmentItem>
    {
        public void Configure(EntityTypeBuilder<AppointmentItem> builder)
        {
            builder.HasKey(x => x.Id);

            
            builder.Property(x => x.Price).HasColumnType("decimal(18,2)");

            
            builder.HasOne(ai => ai.Appointment)
                   .WithMany(a => a.AppointmentItems)
                   .HasForeignKey(ai => ai.AppointmentId);

            builder.HasOne(ai => ai.Treatment)
                   .WithMany() 
                   .HasForeignKey(ai => ai.TreatmentId)
                   
                   .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}