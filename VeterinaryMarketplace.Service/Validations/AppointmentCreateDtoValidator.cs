using FluentValidation;
using System;
using VeterinaryMarketplace.Core.DTOs.Appointment;

namespace VeterinaryMarketplace.Service.Validations
{
    public class AppointmentCreateDtoValidator : AbstractValidator<AppointmentCreateDto>
    {
        public AppointmentCreateDtoValidator()
        {
            RuleFor(x => x.PetId)
                .NotEmpty().WithMessage("Lütfen randevu almak istediğiniz evcil hayvanınızı seçin.");

            RuleFor(x => x.VeterinarianDetailId)
                .NotEmpty().WithMessage("Lütfen bir veteriner hekim seçin.");

            RuleFor(x => x.AppointmentTime)
                .NotEmpty().WithMessage("Randevu tarihi boş bırakılamaz.")
                .GreaterThan(DateTime.Now).WithMessage("Randevu tarihi geçmiş bir zaman dilimi olamaz!");
        }
    }
}