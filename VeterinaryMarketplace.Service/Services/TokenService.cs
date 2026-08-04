using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VeterinaryMarketplace.Core.DTOs.Auth;
using VeterinaryMarketplace.Core.Entities;
using VeterinaryMarketplace.Core.Services;
using VeterinaryMarketplace.Core.Repositories;

namespace VeterinaryMarketplace.Service.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;
        private readonly IGenericRepository<RefreshToken> _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;

        public TokenService(IConfiguration configuration, UserManager<AppUser> userManager, IGenericRepository<RefreshToken> refreshTokenRepository, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _userManager = userManager;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<TokenResponseDto> CreateTokenAsync(AppUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.GivenName, user.FirstName ?? ""),
                new Claim(ClaimTypes.Surname, user.LastName ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var userRoles = await _userManager.GetRolesAsync(user);

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AppSettings:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["AppSettings:ValidIssuer"],
                audience: _configuration["AppSettings:ValidAudience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = GenerateRefreshTokenString();

            RefreshToken token1 = new RefreshToken();
            token1.UserId = user.Id;
            token1.ExpiresTime = DateTime.UtcNow.AddDays(7);
            token1.IsRevoked = false;
            token1.Token = refreshToken;
            token1.Id = Guid.NewGuid();

            await _refreshTokenRepository.AddAsync(token1);
            await _unitOfWork.CommitAsync();

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                AccessTokenExpiration = DateTime.UtcNow.AddMinutes(15),
                RefreshToken = refreshToken,
                RefreshTokenExpiration = token1.ExpiresTime
            };
        }
        public async Task<(TokenResponseDto? Token, string? ErrorMessage)> RefreshTokenAsync(string refreshToken)
        {
            var token2 = await _refreshTokenRepository.Where(x => x.Token == refreshToken).FirstOrDefaultAsync();

            if (token2 == null || token2.IsRevoked == true || token2.ExpiresTime <= DateTime.UtcNow)
            {
                return (null, "Token geçersiz, iptal edilmiş veya geçerlilik süresi dolmuş.");
            }

            _refreshTokenRepository.Update(token2);
            await _unitOfWork.CommitAsync();

            var user = await _userManager.FindByIdAsync(token2.UserId);
            var newToken = await CreateTokenAsync(user);
            return (newToken, null);
        }

        private string GenerateRefreshTokenString()
        {
            byte[] dizi = new byte[32];
            RandomNumberGenerator.Fill(dizi);
            return Convert.ToBase64String(dizi);
        }
    }
}