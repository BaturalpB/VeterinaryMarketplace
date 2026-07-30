using FluentValidation;
using VeterinaryMarketplace.Core.DTOs.Auth;

namespace VeterinaryMarketplace.Service.Validations
{
    public class RefreshTokenRequestDtoValidator : AbstractValidator<RefreshTokenRequestDto>
    {
        public RefreshTokenRequestDtoValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token değeri boş bırakılamaz veya gönderilemez.");
        }
    }
}