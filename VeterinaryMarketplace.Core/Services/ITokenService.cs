using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeterinaryMarketplace.Core.DTOs.Auth;
using VeterinaryMarketplace.Core.Entities;

namespace VeterinaryMarketplace.Core.Services
{
    public interface ITokenService
    {
        Task<TokenResponseDto> CreateTokenAsync(AppUser user);
        Task<(TokenResponseDto? Token, string? ErrorMessage)> RefreshTokenAsync(string refreshToken);
    }
}