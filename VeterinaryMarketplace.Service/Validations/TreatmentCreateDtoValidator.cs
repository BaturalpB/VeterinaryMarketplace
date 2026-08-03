using FluentValidation;
using VeterinaryMarketplace.Core.DTOs.Treatment;

namespace VeterinaryMarketplace.Service.Validations
{
    public class TreatmentCreateDtoValidator:AbstractValidator<TreatmentCreateDto>
    {
        public TreatmentCreateDtoValidator()
        {
            RuleFor(x=>x.Title).NotEmpty().WithMessage("Tedavinin Bir İsmi Olmalı.")
                .MaximumLength(100).WithMessage("Tedavi İsmi Çok Uzun.");
            RuleFor(x => x.Description).MaximumLength(500).WithMessage("Açıklama Çok Uzun.");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Lütfen Geçerli Bir Tutar Giriniz. ");
            RuleFor(x => x.DurationInMinutes).GreaterThan(0).WithMessage("Lütfen Tedavi Süresini Doğru Girin. ")
                .NotEmpty().WithMessage("Lütfen Tedavi Süresini Doğru Girin. ");
        }
    }
}
