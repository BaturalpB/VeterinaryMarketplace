using FluentValidation;
using VeterinaryMarketplace.Core.DTOs.Auth; 
namespace VeterinaryMarketplace.Service.Validations
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
           
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Ad alanı boş bırakılamaz.")
                .MaximumLength(50).WithMessage("Ad en fazla 50 karakter olabilir.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Soyad alanı boş bırakılamaz.")
                .MaximumLength(50).WithMessage("Soyad en fazla 50 karakter olabilir.");

           
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-posta adresi boş bırakılamaz.")
                .EmailAddress().WithMessage("Lütfen geçerli bir e-posta adresi giriniz.");

            
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre boş bırakılamaz.")
                .MinimumLength(6).WithMessage("Şifreniz en az 6 karakter uzunluğunda olmalıdır.");

            
            RuleFor(x => x.IdentityNumber)
                .NotEmpty().WithMessage("TC Kimlik numarası boş bırakılamaz.")
                .Length(11).WithMessage("TC Kimlik numarası tam 11 haneli olmalıdır.")
                .Matches("^[0-9]*$").WithMessage("TC Kimlik numarası sadece rakamlardan oluşmalıdır.");
        }
    }
}