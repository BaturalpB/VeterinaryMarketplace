using System.Reflection;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

            builder.Entity<Clinic>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Pet>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Appointment>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Address>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Treatment>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Review>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<WorkingHour>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<VeterinarianDetail>().HasQueryFilter(e => !e.IsDeleted);
        }

        public override int SaveChanges()
        {
            HandleSoftDelete();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            HandleSoftDelete();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void HandleSoftDelete()
        {
            var entries = ChangeTracker.Entries<ISoftDeletable>().Where(e => e.State == EntityState.Deleted);
            foreach (var entry in entries)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
            }
        }
    }
}