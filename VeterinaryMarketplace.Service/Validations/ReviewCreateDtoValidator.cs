using FluentValidation;
using VeterinaryMarketplace.Core.DTOs.Review;

namespace VeterinaryMarketplace.Service.Validations
{
    public class ReviewCreateDtoValidator : AbstractValidator<ReviewCreateDto>
    {
        public ReviewCreateDtoValidator()
        {
            RuleFor(x => x.AppointmentId)
                .NotEmpty().WithMessage("Randevu seçilmedi.");

            RuleFor(x => x.Rating)
                .InclusiveBetween((byte)1, (byte)5).WithMessage("Lütfen 1 ile 5 arasında bir puan verin.");

            RuleFor(x => x.Comment)
                .MaximumLength(500).WithMessage("Yorumunuz en fazla 500 karakter olabilir.");
        }
    }
}