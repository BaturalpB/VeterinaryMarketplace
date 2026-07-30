using FluentValidation;
using VeterinaryMarketplace.Core.DTOs;

namespace VeterinaryMarketplace.Service.Validations
{
    public class PetUpdateDtoValidator : AbstractValidator<PetUpdateDto>
    {
        public PetUpdateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Evcil hayvan ismi boş bırakılamaz.")
                .MaximumLength(20).WithMessage("Evcil hayvan ismi en fazla 20 karakter olabilir.");

            RuleFor(x => x.Species)
                .NotEmpty().WithMessage("Tür bilgisi (Kedi, Köpek vb.) girilmelidir.");

            RuleFor(x => x.Breed)
                .NotEmpty().WithMessage("Cins bilgisi boş bırakılamaz.");

            RuleFor(x => x.Age)
                .GreaterThanOrEqualTo(0).WithMessage("Evcil hayvanın yaşı 0'dan küçük olamaz.");

            RuleFor(x => x.ImageURL)
                .NotEmpty().WithMessage("Lütfen bir görsel URL'si ekleyin.");
        }
    }
}