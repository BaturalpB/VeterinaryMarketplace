using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VeterinaryMarketplace.Core.Entities;

namespace VeterinaryMarketplace.Data.Configurations
{
    public class VeterinarianDetailConfiguration : IEntityTypeConfiguration<VeterinarianDetail>
    {
        public void Configure(EntityTypeBuilder<VeterinarianDetail> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Uzmanlik).IsRequired().HasMaxLength(100);
            builder.Property(x => x.IBAN).HasMaxLength(32);
            builder.Property(x => x.SubMerchantKey).HasMaxLength(200);

           
            builder.Property(x => x.CommissionRate).HasColumnType("decimal(18,2)");

            builder.HasOne(x => x.User)
            .WithOne(u => u.VeterinarianDetail)
            .HasForeignKey<VeterinarianDetail>(x => x.UserId) 
            .OnDelete(DeleteBehavior.Restrict);
        }
    }
}