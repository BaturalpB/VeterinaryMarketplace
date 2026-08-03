using FluentValidation;
using VeterinaryMarketplace.Core.DTOs.WorkingHour;

namespace VeterinaryMarketplace.Service.Validations
{
    public class WorkingHourUpdateDtoValidator:AbstractValidator<WorkingHourUpdateDto>
    {
        public WorkingHourUpdateDtoValidator()
        {
            RuleFor(x=>x.Id).NotEmpty();
            RuleFor(x => x.StartTime).NotEmpty().WithMessage("Başlangıç Saati Seçin");
                
            RuleFor(x => x.EndTime).NotEmpty().WithMessage("Bitiş Saati Seçiniz")
                .GreaterThan(x => x.StartTime).WithMessage("Geçerli Bir saat Seçiniz.");
        }
    }
}
