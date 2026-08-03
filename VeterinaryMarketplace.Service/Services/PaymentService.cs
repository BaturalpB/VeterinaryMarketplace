using Iyzipay.Model;
using VeterinaryMarketplace.Core.Repositories;
using Iyzipay.Request;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
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

        public async Task<(bool IsSuccess, string? ErrorMessage)> ProcessPaymentAsync(PaymentRequestDto requestDto, string userId)
        {
            
            Iyzipay.Options options = new Iyzipay.Options
            {
                ApiKey = _iyzicoOptions.ApiKey,
                SecretKey = _iyzicoOptions.SecretKey,
                BaseUrl = _iyzicoOptions.BaseUrl
            };

            
            string formattedPrice = requestDto.Price.ToString(new CultureInfo("en-US"));

          
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
            basketItems.Add(item);
            request.BasketItems = basketItems;
            Payment payment = await Task.Run(() => Payment.Create(request, options));

            if (payment.Status == "success")
            {
                var appointment = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(_appointmentRepository.Where(a => a.Id == requestDto.AppointmentId));
                if (appointment != null)
                {
                    appointment.IsPaid = true;
                    appointment.PaymentTransactionId = payment.PaymentId;
                    _appointmentRepository.Update(appointment);
                    await _unitOfWork.CommitAsync();

                    string emailBody = $@"
                        <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ddd; border-radius: 10px; max-width: 600px;'>
                            <h2 style='color: #4CAF50; text-align: center;'>Ödemeniz Başarıyla Alındı!</h2>
                            <p>Sayın <b>{user.FirstName} {user.LastName}</b>,</p>
                            <p>VeterinaryMarketplace üzerinden yapmış olduğunuz <b>{requestDto.Price} TL</b> tutarındaki ödemeniz başarıyla gerçekleşmiştir.</p>
                            <hr style='border-top: 1px solid #eee;' />
                            <p><b>İşlem ID (Iyzico):</b> {payment.PaymentId}</p>
                            <p><b>Randevu Saati:</b> {appointment.AppointmentTime.ToString("dd.MM.yyyy HH:mm")}</p>
                            <br/>
                            <p>Bizi tercih ettiğiniz için teşekkür ederiz. Minik dostunuza acil şifalar dileriz!</p>
                            <p style='text-align:center; font-size: 12px; color:#888;'>Veterinary Marketplace Ekibi</p>
                        </div>
                    ";

                    
                }

                return (true, null);
            }

            return (false, payment.ErrorMessage);
        }
    }
}