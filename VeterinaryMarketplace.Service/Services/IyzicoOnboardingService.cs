using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using VeterinaryMarketplace.Core.Entities;
using VeterinaryMarketplace.Core.Options;
using VeterinaryMarketplace.Core.Services;

namespace VeterinaryMarketplace.Service.Services
{
    public class IyzicoOnboardingService : IIyzicoOnboardingService
    {
        private readonly IyzicoOptions _iyzicoOptions;

        public IyzicoOnboardingService(IOptions<IyzicoOptions> iyzicoOptions)
        {
            _iyzicoOptions = iyzicoOptions.Value;
        }

        public async Task<(bool IsSuccess, string? SubMerchantKey, string? ErrorMessage)> CreateSubMerchantAsync(Clinic clinic, AppUser managerUser)
        {
            Iyzipay.Options options = new Iyzipay.Options
            {
                ApiKey = _iyzicoOptions.ApiKey,
                SecretKey = _iyzicoOptions.SecretKey,
                BaseUrl = _iyzicoOptions.BaseUrl
            };

            CreateSubMerchantRequest request = new CreateSubMerchantRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = clinic.Id.ToString(),
                SubMerchantExternalId = clinic.Id.ToString(), // Kendi sistemimizdeki ID
                SubMerchantType = SubMerchantType.PERSONAL.ToString(), // Basitlik için PERSONAL (Şahıs Şirketi) varsayıyoruz veya PRIVATE_COMPANY
                Name = clinic.Name,
                Email = managerUser.Email ?? "test@veterinarymarketplace.com",
                GsmNumber = clinic.PhoneNumber,
                Address = clinic.Address,
                Iban = clinic.Iban,
                TaxOffice = clinic.TaxOffice,
                ContactName = managerUser.FirstName ?? "Müşteri",
                ContactSurname = managerUser.LastName ?? "Soyadı",
                LegalCompanyTitle = clinic.CompanyTitle,
                Currency = Currency.TRY.ToString(),
                IdentityNumber = clinic.TaxNumber // Vergi No veya TC Kimlik
            };

            SubMerchant subMerchant = await Task.Run(() => SubMerchant.Create(request, options));

            if (subMerchant.Status == "success")
            {
                return (true, subMerchant.SubMerchantKey, null);
            }

            return (false, null, subMerchant.ErrorMessage);
        }
    }
}
