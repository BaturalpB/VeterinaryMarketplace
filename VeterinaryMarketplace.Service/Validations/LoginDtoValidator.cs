using FluentValidation;
using VeterinaryMarketplace.Core.DTOs.Auth;

namespace VeterinaryMarketplace.Service.Validations
{
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-posta adresi girmelisiniz.")
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi girmelisiniz.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifrenizi girmelisiniz.");
        }
    }
}