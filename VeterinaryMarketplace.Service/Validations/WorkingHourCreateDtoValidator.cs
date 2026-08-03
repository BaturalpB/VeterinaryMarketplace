using FluentValidation;
using VeterinaryMarketplace.Core.DTOs.WorkingHour;

namespace VeterinaryMarketplace.Service.Validations
{
    public class WorkingHourCreateDtoValidator:AbstractValidator<WorkingHourCreateDto>
    {
        public WorkingHourCreateDtoValidator()
        {
            RuleFor(x => x.StartTime).NotEmpty().WithMessage("Başlangıç Saati Seçin");
                
            RuleFor(x=>x.EndTime).NotEmpty().WithMessage("Bitiş Saati Seçiniz")
                .GreaterThan(x=>x.StartTime).WithMessage("Geçerli Bir saat Seçiniz.");
        }
    }
}
