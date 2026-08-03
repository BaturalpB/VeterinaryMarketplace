using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeterinaryMarketplace.Core.Entities;

namespace VeterinaryMarketplace.Core.Services
{
    public interface IAppointmentService:IService<Appointment>
    {
        Task<bool> ApproveAppointmentAsync(Guid id);
        Task<(bool IsSuccess, string? ErrorMessage)> CancelAppointmentAsync(Guid id);
    }
}
