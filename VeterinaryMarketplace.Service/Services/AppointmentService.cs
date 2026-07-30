using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeterinaryMarketplace.Core.Entities;
using VeterinaryMarketplace.Core.Repositories;
using VeterinaryMarketplace.Core.Services;

namespace VeterinaryMarketplace.Service.Services
{
    public class AppointmentService : Service<Appointment>, IAppointmentService
    {
        
        public AppointmentService(IGenericRepository<Appointment> repository, IUnitOfWork unitOfWork) : base(repository, unitOfWork)
        {
           
        }

       
        public async Task<bool> ApproveAppointmentAsync(Guid id)
        {
            var appointment = await GetByIdAsync(id);
            if (appointment == null)
            {
                return false;
            }
            appointment.Status=Appointment.AppointmentStatus.Approved;
            await UpdateAsync(appointment);
            return true;
        }

        public async Task<bool> CancelAppointmentAsync(Guid id)
        {
            var appointment= await GetByIdAsync(id);
            if (appointment == null)
            {
                return false;
            }
            appointment.Status=Appointment.AppointmentStatus.Cancelled;
            return true;
        }
    }
}