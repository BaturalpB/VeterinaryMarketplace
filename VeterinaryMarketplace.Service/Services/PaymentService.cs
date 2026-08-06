using Iyzipay.Model;
using VeterinaryMarketplace.Core.Repositories;
using Iyzipay.Request;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using VeterinaryMarketplace.Core.DTOs.Payment;
using VeterinaryMarketplace.Core.Entities;
using VeterinaryMarketplace.Core.Options;
using VeterinaryMarketplace.Core.Services;

namespace VeterinaryMarketplace.Service.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IyzicoOptions _iyzicoOptions;
        private readonly IGenericRepository<Appointment> _appointmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<AppUser> _userRepository;

        public PaymentService(
            IOptions<IyzicoOptions> iyzicoOptions,
            IGenericRepository<Appointment> appointmentRepository,
            IUnitOfWork unitOfWork,
            IGenericRepository<AppUser> userRepository)
        {
            _iyzicoOptions = iyzicoOptions.Value;
            _appointmentRepository = appointmentRepository;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
        }

        public async Task<(bool IsSuccess, string? ErrorMessage)> CancelPaymentAsync(Guid appointmentId)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);

            if (appointment == null)
                return (false, "Randevu bulunamadı.");

            if (!appointment.IsPaid || string.IsNullOrEmpty(appointment.PaymentTransactionId))
                return (false, "Bu randevu için yapılmış geçerli bir ödeme bulunmamaktadır.");

            Iyzipay.Options options = new Iyzipay.Options
            {
                ApiKey = _iyzicoOptions.ApiKey,
                SecretKey = _iyzicoOptions.SecretKey,
                BaseUrl = _iyzicoOptions.BaseUrl
            };

            CreateCancelRequest request = new CreateCancelRequest
            {
                PaymentId = appointment.TransactionID ?? appointment.PaymentTransactionId, // Geriye dönük uyumluluk için, PaymentId'yi kullan.
                Ip = "85.34.78.112",
                Locale = Locale.TR.ToString()
            };

            Cancel cancel = await Task.Run(() => Cancel.Create(request, options));

            if (cancel.Status == "success")
            {
                appointment.IsPaid = false;

                await _unitOfWork.CommitAsync();
                return (true, null);
            }

            return (false, cancel.ErrorMessage);
        }

        public async Task<(bool IsSuccess, string? ErrorMessage)> ProcessPaymentAsync(PaymentRequestDto requestDto, string userId)
        {
            
            var appointment = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                _appointmentRepository.Where(a => a.Id == requestDto.AppointmentId)
                .Include(a => a.Veterinarian)
                .ThenInclude(v => v.Clinic)
            );

            if (appointment == null)
                return (false, "Ödeme yapılmak istenen randevu bulunamadı.");

            if (appointment.IsPaid)
                return (false, "Bu randevu için zaten ödeme yapılmış.");

            Iyzipay.Options options = new Iyzipay.Options
            {
                ApiKey = _iyzicoOptions.ApiKey,
                SecretKey = _iyzicoOptions.SecretKey,
                BaseUrl = _iyzicoOptions.BaseUrl
            };

            
            string formattedPrice = appointment.Price.ToString(new CultureInfo("en-US"));

            CreatePaymentRequest request = new CreatePaymentRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = Guid.NewGuid().ToString(),
                Price = formattedPrice,
                PaidPrice = formattedPrice,
                Currency = Currency.TRY.ToString(),
                Installment = 1,
                BasketId = "VET-" + Guid.NewGuid().ToString().Substring(0, 6),
                PaymentChannel = PaymentChannel.WEB.ToString(),
                PaymentGroup = PaymentGroup.PRODUCT.ToString()
            };

            PaymentCard paymentCard = new PaymentCard
            {
                CardHolderName = requestDto.CardHolderName,
                CardNumber = requestDto.CardNumber,
                ExpireMonth = requestDto.ExpireMonth,
                ExpireYear = requestDto.ExpireYear,
                Cvc = requestDto.Cvc,
                RegisterCard = 0
            };
            request.PaymentCard = paymentCard;

            var user = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(_userRepository.Where(u => u.Id == userId));
            if (user == null) return (false, "Kullanıcı bulunamadı.");

            Buyer buyer = new Buyer
            {
                Id = userId,
                Name = user.FirstName ?? "Müşteri",
                Surname = user.LastName ?? "Soyadı",
                GsmNumber = "+905350000000",
                Email = user.Email ?? "test@veterinarymarketplace.com",
                IdentityNumber = user.IdentityNumber ?? "74300864791",
                RegistrationAddress = "Sistem Kayıtlı Adres",
                Ip = "85.34.78.112",
                City = user.City ?? "Bilinmiyor",
                Country = "Turkey",
                ZipCode = "34732"
            };
            request.Buyer = buyer;

            Iyzipay.Model.Address address = new Iyzipay.Model.Address
            {
                ContactName = "Baturalp Kullanıcı",
                City = "Istanbul",
                Country = "Turkey",
                Description = "Teknokent Bilişim Vadisi",
                ZipCode = "34732"
            };
            request.ShippingAddress = address;
            request.BillingAddress = address;

            List<BasketItem> basketItems = new List<BasketItem>();
            BasketItem item = new BasketItem
            {
                Id = "TREATMENT-1",
                Name = "Veteriner Randevu Ücreti",
                Category1 = "Sağlık",
                ItemType = BasketItemType.VIRTUAL.ToString(),
                Price = formattedPrice
            };

            // Eğer kliniğin İyzico SubMerchantKey'i varsa Pazaryeri Dağıtımı Yap
            if (appointment.Veterinarian?.Clinic?.SubMerchantKey != null)
            {
                item.SubMerchantKey = appointment.Veterinarian.Clinic.SubMerchantKey;
                // %10 Komisyon kesintisi hesaplanıyor
                decimal subMerchantShare = appointment.Price * 0.90m;
                item.SubMerchantPrice = subMerchantShare.ToString(new CultureInfo("en-US"));
            }

            basketItems.Add(item);
            request.BasketItems = basketItems;

            Payment payment = await Task.Run(() => Payment.Create(request, options));

            if (payment.Status == "success")
            {
                appointment.IsPaid = true;
                appointment.TransactionID = payment.PaymentId; // Tüm işlemi iptal etmek için gereken Parent ID
                appointment.PaymentTransactionId = payment.PaymentItems[0].PaymentTransactionId; // Alt üye işyerine para aktarımı (Approval) için gereken Item ID

                _appointmentRepository.Update(appointment);
                await _unitOfWork.CommitAsync();

                return (true, null);
            }

            return (false, payment.ErrorMessage);
        }
        public async Task<(bool IsSuccess, string? ErrorMessage)> ApprovePaymentAsync(Guid appointmentId)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);

            if (appointment == null)
                return (false, "Randevu bulunamadı.");

            if (!appointment.IsPaid || string.IsNullOrEmpty(appointment.PaymentTransactionId))
                return (false, "Bu randevu için yapılmış geçerli bir ödeme bulunmamaktadır.");

            Iyzipay.Options options = new Iyzipay.Options
            {
                ApiKey = _iyzicoOptions.ApiKey,
                SecretKey = _iyzicoOptions.SecretKey,
                BaseUrl = _iyzicoOptions.BaseUrl
            };

            CreateApprovalRequest request = new CreateApprovalRequest
            {
                PaymentTransactionId = appointment.PaymentTransactionId
            };

            Approval approval = await Task.Run(() => Approval.Create(request, options));

            if (approval.Status == "success")
            {
                return (true, null);
            }

            return (false, approval.ErrorMessage);
        }
    }
}