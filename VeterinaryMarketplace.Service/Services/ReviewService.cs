using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using System.Threading.Tasks;
using VeterinaryMarketplace.Core.DTOs.Review;
using VeterinaryMarketplace.Core.Entities;
using VeterinaryMarketplace.Core.Repositories;
using VeterinaryMarketplace.Core.Services;


namespace VeterinaryMarketplace.Service.Services
{
    public class ReviewService : Service<Review>, IReviewService
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IMapper _mapper;
        private readonly IService<Pet> _petService;
        public ReviewService(IGenericRepository<Review> repository, IUnitOfWork unitOfWork,
                             IAppointmentService appointmentService, IMapper mapper, IService<Pet> petService)
            : base(repository, unitOfWork)
        {
            _appointmentService = appointmentService;
            _mapper = mapper;
            _petService = petService;
        }
        public async Task<(bool IsSuccess, string? ErrorMessage)> CreateReviewAsync(ReviewCreateDto dto, string userId)
        {

            var appointment = await _appointmentService.GetByIdAsync(dto.AppointmentId);
            if (appointment == null)
            {
                return (false, "Değerlendirmek istediğiniz randevu sistemde bulunamadı.");
            }
            
            if(appointment.Status!=Appointment.AppointmentStatus.Completed)
            {
                return (false, "Sadece geçmiş ve tamamlanmış randevulara yorum yapabilirsiniz.");
            }
            bool isReviewExist = await AnyAsync(x => x.AppointmentId == dto.AppointmentId);
            if (isReviewExist)
            {
                return (false, "Bu randevu için zaten bir değerlendirme yapmışsınız.");
            }
            var review = _mapper.Map<Review>(dto); 

            await AddAsync(review);
            return (true, null); 
        }

        public async Task<(bool IsSuccess, string? ErrorMessage)> UpdateReviewAsync(ReviewUpdateDto dto, string userId)
        {
            var review = await GetByIdAsync(dto.ReviewId);
            if (review == null)
            {
                return (false, "Güncellenmek istenen değerlendirme bulunamadı.");
            }

            var appointment = await _appointmentService.GetByIdAsync(review.AppointmentId);
            if (appointment == null)
            {
                return (false, "Bu yoruma ait randevu sistemde bulunamadı.");
            }

            var pet = await _petService.GetByIdAsync(appointment.PetId);

            if (pet == null || pet.OwnerId.ToString() != userId)
            {
                return (false, "Bu değerlendirmeyi güncelleme yetkiniz yok.");
            }
            _mapper.Map(dto, review);

            await UpdateAsync(review);

            return (true, null);
        }

        public async Task<(bool IsSuccess, string? ErrorMessage)> DeleteReviewAsync(Guid reviewId, string userId)
        {
            var review = await GetByIdAsync(reviewId);
            if (review == null)
            {
                return (false, "Silinmek istenen değerlendirme bulunamadı.");
            }

            var appointment = await _appointmentService.GetByIdAsync(review.AppointmentId);
            if (appointment == null)
            {
                return (false, "Bu yoruma ait randevu sistemde bulunamadı.");
            }

            var pet = await _petService.GetByIdAsync(appointment.PetId);
            if (pet == null || pet.OwnerId.ToString() != userId)
            {
                return (false, "Bu değerlendirmeyi silme yetkiniz yok.");
            }

            await RemoveAsync(review);

            return (true, null);
        }
    }
}