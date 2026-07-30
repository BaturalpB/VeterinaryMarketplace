using FluentValidation;
using VeterinaryMarketplace.Core.DTOs;

namespace VeterinaryMarketplace.Service.Validations
{
    public class ClinicCreateDtoValidator : AbstractValidator<ClinicCreateDto>
    {
        public ClinicCreateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Klinik adı boş bırakılamaz.")
                .MaximumLength(150).WithMessage("Klinik adı en fazla 150 karakter olabilir.");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("Şehir bilgisi boş bırakılamaz.");

            RuleFor(x => x.District)
                .NotEmpty().WithMessage("İlçe bilgisi boş bırakılamaz.");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Açık adres boş bırakılamaz.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Telefon numarası boş bırakılamaz.")
                .MaximumLength(15).WithMessage("Telefon numarası çok uzun.")
                .Matches(@"^[0-9\-\+\s\(\)]+$").WithMessage("Lütfen geçerli bir telefon numarası formatı giriniz.");
        }
    }
}