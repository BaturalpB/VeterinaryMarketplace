using FluentValidation;
using VeterinaryMarketplace.Core.DTOs.Address;
namespace VeterinaryMarketplace.Service.Validations
{
    public class AddressUpdateDtoValidator:AbstractValidator<AddressUpdateDto>
    {
        public AddressUpdateDtoValidator() 
        {
            RuleFor(x=>x.Id).NotEmpty().WithMessage("Adres Seçiniz.");
            RuleFor(x => x.City).NotEmpty().WithMessage("Şehir Seçiniz.");
            RuleFor(x => x.Title).NotEmpty().WithMessage("Bir Başlık Girin").MaximumLength(30).WithMessage("30 karakterden fazla olamaz.");
            RuleFor(x => x.District).NotEmpty().WithMessage("İlçe Seçiniz.");
            RuleFor(x => x.FullAddress).NotEmpty().WithMessage("Tam Adres Girin.").MaximumLength(100).WithMessage("100 karakterden fazla olamaz");
        }
    }
}
