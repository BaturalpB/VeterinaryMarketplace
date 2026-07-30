using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VeterinaryMarketplace.Core.Entities;

namespace VeterinaryMarketplace.Data.Contexts
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<VeterinarianDetail> VeterinarianDetails { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Address> Adresses { get; set; }
        public DbSet<Treatment>Treatments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<WorkingHour> WorkHours { get; set; }
        public DbSet<AppointmentItem> AppointmentItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}