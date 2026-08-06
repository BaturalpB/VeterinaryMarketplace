using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VeterinaryMarketplace.Core.Services;
using VeterinaryMarketplace.Core.Entities;
using VeterinaryMarketplace.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace VeterinaryMarketplace.API.Services
{
    public class AppointmentTimeoutBackgroundService : BackgroundService
    {
        private readonly ILogger<AppointmentTimeoutBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public AppointmentTimeoutBackgroundService(ILogger<AppointmentTimeoutBackgroundService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Appointment Timeout Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckForExpiredAppointmentsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while checking for expired appointments.");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }

            _logger.LogInformation("Appointment Timeout Background Service is stopping.");
        }

        private async Task CheckForExpiredAppointmentsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var appointmentService = scope.ServiceProvider.GetRequiredService<IAppointmentService>();

            var thirtyMinutesAgo = DateTime.Now.AddMinutes(-30);

            var expiredAppointments = await dbContext.Appointments
                .Where(a => a.Status == Appointment.AppointmentStatus.Pending && a.CreatedAt <= thirtyMinutesAgo)
                .ToListAsync(stoppingToken);

            if (expiredAppointments.Any())
            {
                _logger.LogInformation($"Found {expiredAppointments.Count} expired pending appointments to cancel.");

                foreach (var apt in expiredAppointments)
                {
                    
                    var result = await appointmentService.CancelAppointmentAsync(apt.Id);
                    
                    if (result.IsSuccess)
                    {
                        _logger.LogInformation($"Successfully auto-cancelled appointment {apt.Id}. Refund processed.");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to auto-cancel appointment {apt.Id}: {result.ErrorMessage}");
                    }
                }
            }
        }
    }
}
