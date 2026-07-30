using FluentValidation;
using System;
using VeterinaryMarketplace.Core.DTOs;

namespace VeterinaryMarketplace.Service.Validations
{
    public class VeterinarianCreateDtoValidator : AbstractValidator<VeterinarianCreateDto>
    {
        public VeterinarianCreateDtoValidator()
        {
            RuleFor(x => x.ClinicId)
                .NotEmpty().WithMessage("Lütfen çalışacağınız kliniği seçin.");

            RuleFor(x => x.Uzmanlik)
                .NotEmpty().WithMessage("Uzmanlık alanı boş bırakılamaz.")
                .MaximumLength(100).WithMessage("Uzmanlık alanı en fazla 100 karakter olabilir.");

            RuleFor(x => x.IBAN)
                .NotEmpty().WithMessage("IBAN adresi boş bırakılamaz.")
                .Must(iban => iban != null && iban.StartsWith("TR")).WithMessage("Lütfen geçerli bir TR IBAN numarası girin.")
                 .Length(26).WithMessage("IBAN numarası 'TR' dahil tam 26 karakter olmalıdır.");

            RuleFor(x => x.SubMerchantKey)
                .NotEmpty().WithMessage("Alt üye işyeri (İyzico vb.) anahtarı zorunludur.");

            RuleFor(x => x.CommissionRate)
                .GreaterThanOrEqualTo(0).WithMessage("Komisyon oranı 0'dan küçük olamaz.")
                .LessThanOrEqualTo(100).WithMessage("Komisyon oranı %100'den büyük olamaz.");
            RuleFor(x => x.Bitis)
                .GreaterThan(x => x.Baslangic).WithMessage("Mesai bitiş saati, başlangıç saatinden daha erken olamaz!");
        }
    }
}