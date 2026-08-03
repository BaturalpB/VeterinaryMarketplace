using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VeterinaryMarketplace.Core.Entities;
using VeterinaryMarketplace.Core.Repositories;
using VeterinaryMarketplace.Core.Services;
using VeterinaryMarketplace.Data.UnitOfWorks;

namespace VeterinaryMarketplace.Service.Services
{
    public class AppointmentService : Service<Appointment>, IAppointmentService
    {
        private readonly IPaymentService _paymentService; 

        public AppointmentService(
            IGenericRepository<Appointment> repository,
            IUnitOfWork unitOfWork,
            IPaymentService paymentService) 
            : base(repository, unitOfWork)
        {
            _paymentService = paymentService;
        }

        public async Task<bool> ApproveAppointmentAsync(Guid id)
        {
            var appointment = await GetByIdAsync(id);
            if (appointment == null)
            {
                return false;
            }
            appointment.Status = Appointment.AppointmentStatus.Approved;
            await UpdateAsync(appointment);
            return true;
        }

        
        public async Task<(bool IsSuccess, string? ErrorMessage)> CancelAppointmentAsync(Guid id)
        {
            var appointment = await GetByIdAsync(id);

            if (appointment == null)
            {
                return (false, "Randevu bulunamadı.");
            }

            if (appointment.Status == Appointment.AppointmentStatus.Cancelled)
            {
                return (false, "Bu randevu zaten iptal edilmiş.");
            }

            
            if (appointment.IsPaid)
            {
                var refundResult = await _paymentService.CancelPaymentAsync(id);

                if (!refundResult.IsSuccess)
                {
                    
                    return (false, $"Randevu iptal edilemedi, iade hatası: {refundResult.ErrorMessage}");
                }
            }

            appointment.Status = Appointment.AppointmentStatus.Cancelled;

            await UpdateAsync(appointment);

            return (true, null);
        }
    }
}