using AutoMapper;
using VeterinaryMarketplace.Core.DTOs;
using VeterinaryMarketplace.Core.DTOs.Appointment;
using VeterinaryMarketplace.Core.DTOs.Auth;
using VeterinaryMarketplace.Core.DTOs.Treatment;
using VeterinaryMarketplace.Core.Entities;

namespace VeterinaryMarketplace.Service.Mappings
{
    public class MapProfile:Profile
    {
        public MapProfile() 
        {
            CreateMap<AppointmentCreateDto, Appointment>();
            CreateMap<Appointment, AppointmentDto>()
            .ForMember(dest => dest.PetName, opt => opt.MapFrom(src => src.Pet.Name))
            .ForMember(dest => dest.VeterinarianInfo, opt => opt.MapFrom(src => src.Veterenarian.Uzmanlik))
            .ForMember(dest => dest.ClinicName, opt => opt.MapFrom(src => src.Veterenarian.Clinic.Name))
            .ReverseMap();

            CreateMap<AppointmentItem, AppointmentItemDto>()
            .ForMember(dest => dest.TreatmentName, opt => opt.MapFrom(src => src.Treatment.Title))
            .ReverseMap();

            CreateMap<RegisterDto, AppUser>();

            CreateMap<Clinic, ClinicDto>().ReverseMap();
            CreateMap<ClinicCreateDto, Clinic>();

            CreateMap<Pet, PetDto>().ReverseMap();
            CreateMap<PetCreateDto, Pet>();
            CreateMap<PetUpdateDto, Pet>();

            CreateMap<VeterinarianDetail, VeterinarianDto>();
            CreateMap<VeterinarianCreateDto, VeterinarianDetail>();

            CreateMap<Treatment, TreatmentDto>().ReverseMap();
            CreateMap<TreatmentCreateDto, Treatment>();
            CreateMap<TreatmentUpdateDto, Treatment>();

        }

    }
}
