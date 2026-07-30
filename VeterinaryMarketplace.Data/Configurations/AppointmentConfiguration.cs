using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VeterinaryMarketplace.Core.Entities;

namespace VeterinaryMarketplace.Data.Configurations
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            
            builder.HasOne(a => a.Pet)
                   .WithMany(p => p.Appointments)
                   .HasForeignKey(a => a.PetId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Veterenarian)
                   .WithMany(v => v.Appointments)
                   .HasForeignKey(a => a.VeterinarianDetailId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}