using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VeterinaryMarketplace.Core.DTOs.Auth;
using VeterinaryMarketplace.Core.Entities;
using VeterinaryMarketplace.Core.Services;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;

namespace VeterinaryMarketplace.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IValidator<RegisterDto> _registerValidator;
        private readonly IValidator<LoginDto> _loginValidator;
        private readonly IValidator<RefreshTokenRequestDto> _refreshTokenValidator;
        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenService tokenService,
            IMapper mapper,
            IValidator<RegisterDto> registerValidator,
            IValidator<LoginDto> loginValidator,
            IValidator<RefreshTokenRequestDto> refreshTokenValidator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _refreshTokenValidator = refreshTokenValidator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var validationResult = await _registerValidator.ValidateAsync(registerDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var user = _mapper.Map<AppUser>(registerDto);
            user.RegisteredAt = DateTime.Now;

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Errors = errors });
            }

            await _userManager.AddToRoleAsync(user, "User");

            return Ok(new { Message = "Kullanıcı başarıyla oluşturuldu ve User rolü atandı" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var validationResult = await _loginValidator.ValidateAsync(loginDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return Unauthorized(new { Message = "E-posta veya şifre hatalı" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized(new { Message = "E-posta veya şifre hatalı" });
            }

            var tokenResponse = await _tokenService.CreateTokenAsync(user);

            return Ok(tokenResponse);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            var validationResult = await _refreshTokenValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var (token, errorMessage) = await _tokenService.RefreshTokenAsync(request.RefreshToken);
            if (token == null)
            {
                return BadRequest(new { Message = errorMessage });
            }
            return Ok(token);
        }

        [Authorize]
        [HttpGet("secret-room")]
        public IActionResult GetSecretRoom()
        {
            return Ok(new { Message = "Güvenlik duvarı çalışıyor" });
        }

        [Authorize(Roles = "User")]
        [HttpGet("user-only-room")]
        public IActionResult GetUserRoom()
        {
            return Ok(new { Message = "Bir 'User' (Müşteri) olarak buraya girmeye yetkin var. Evcil hayvanlarını buradan yönetebilirsin." });
        }

        [Authorize(Roles = "Veterinarian")]
        [HttpGet("veterinarian-only-room")]
        public IActionResult GetVeterinarianRoom()
        {
            return Ok(new { Message = "Hoş geldin Doktor. Sadece veterinerlerin görebildiği klinik paneline eriştin." });
        }
    }
}