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
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

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
        private readonly IVeterinarianDetailService _vetDetailService;
        private readonly IAppointmentService _appointmentService;
        private readonly IService<Pet> _petService;
        private readonly IService<Treatment> _treatmentService;
        private readonly IService<WorkingHour> _workingHourService;

        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenService tokenService,
            IMapper mapper,
            IValidator<RegisterDto> registerValidator,
            IValidator<LoginDto> loginValidator,
            IValidator<RefreshTokenRequestDto> refreshTokenValidator,
            IVeterinarianDetailService vetDetailService,
            IAppointmentService appointmentService,
            IService<Pet> petService,
            IService<Treatment> treatmentService,
            IService<WorkingHour> workingHourService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _refreshTokenValidator = refreshTokenValidator;
            _vetDetailService = vetDetailService;
            _appointmentService = appointmentService;
            _petService = petService;
            _treatmentService = treatmentService;
            _workingHourService = workingHourService;
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

        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = _userManager.Users.ToList();
            var userList = new System.Collections.Generic.List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? "User"
                });
            }

            return Ok(userList);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("assign-vet-role/{userId}")]
        public async Task<IActionResult> AssignVetRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { Message = "Kullanıcı bulunamadı" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Veterinarian"))
            {
                return BadRequest(new { Message = "Kullanıcı zaten veteriner rolüne sahip" });
            }

            // Önceki rolleri temizleyip sadece Veterinarian yapabiliriz veya ekleyebiliriz.
            // Genelde e-ticaret vs değilse eklemek yeterlidir. Mevcut role ekleyelim.
            await _userManager.RemoveFromRolesAsync(user, roles);
            var result = await _userManager.AddToRoleAsync(user, "Veterinarian");

            if (!result.Succeeded)
            {
                return BadRequest(new { Message = "Rol ataması başarısız" });
            }

            return Ok(new { Message = "Kullanıcı başarıyla veteriner yapıldı" });
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("remove-vet-role/{userId}")]
        public async Task<IActionResult> RemoveVetRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { Message = "Kullanıcı bulunamadı" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Veterinarian"))
            {
                return BadRequest(new { Message = "Kullanıcı zaten veteriner rolüne sahip değil" });
            }

            // Veteriner profilini de sil (eğer varsa)
            var vetProfile = await _vetDetailService.GetByUserIdAsync(userId);
            if (vetProfile != null)
            {
                var vetAppointments = await _appointmentService.Where(a => a.VeterinarianDetailId == vetProfile.Id).ToListAsync();
                if (vetAppointments.Any())
                {
                    await _appointmentService.RemoveRangeAsync(vetAppointments);
                }
                await _vetDetailService.RemoveAsync(vetProfile);
            }

            var userTreatments = await _treatmentService.Where(t => t.UserID == userId).ToListAsync();
            if (userTreatments.Any())
            {
                await _treatmentService.RemoveRangeAsync(userTreatments);
            }

            var userWorkingHours = await _workingHourService.Where(w => w.UserId == userId).ToListAsync();
            if (userWorkingHours.Any())
            {
                await _workingHourService.RemoveRangeAsync(userWorkingHours);
            }

            await _userManager.RemoveFromRolesAsync(user, roles);
            var result = await _userManager.AddToRoleAsync(user, "User");

            if (!result.Succeeded)
            {
                return BadRequest(new { Message = "Rol alma başarısız" });
            }

            return Ok(new { Message = "Kullanıcının veteriner yetkisi alındı ve tekrar 'User' yapıldı" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-user/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { Message = "Kullanıcı bulunamadı" });
            }
            
            var vetProfile = await _vetDetailService.GetByUserIdAsync(userId);
            if (vetProfile != null)
            {
                var vetAppointments = await _appointmentService.Where(a => a.VeterinarianDetailId == vetProfile.Id).ToListAsync();
                if (vetAppointments.Any())
                {
                    await _appointmentService.RemoveRangeAsync(vetAppointments);
                }
                await _vetDetailService.RemoveAsync(vetProfile);
            }

            var userPets = await _petService.Where(p => p.OwnerId == userId).ToListAsync();
            foreach (var pet in userPets)
            {
                var petAppointments = await _appointmentService.Where(a => a.PetId == pet.Id).ToListAsync();
                if (petAppointments.Any())
                {
                    await _appointmentService.RemoveRangeAsync(petAppointments);
                }
            }
            if (userPets.Any())
            {
                await _petService.RemoveRangeAsync(userPets);
            }

            var userTreatments = await _treatmentService.Where(t => t.UserID == userId).ToListAsync();
            if (userTreatments.Any())
            {
                await _treatmentService.RemoveRangeAsync(userTreatments);
            }

            var userWorkingHours = await _workingHourService.Where(w => w.UserId == userId).ToListAsync();
            if (userWorkingHours.Any())
            {
                await _workingHourService.RemoveRangeAsync(userWorkingHours);
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { Message = "Kullanıcı silinemedi." });
            }

            return Ok(new { Message = "Kullanıcı başarıyla silindi." });
        }

        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(new { Message = "Kullanıcı bulunamadı." });

            // Sadece User veya Admin ise güncelleme izni ver (Veterinerler kısıtlanmış)
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Veterinarian"))
            {
                return BadRequest(new { Message = "Veterinerler kişisel bilgilerini bu alandan güncelleyemez." });
            }

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.UserName = dto.UserName;
            user.Email = dto.Email;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { Message = "Profil güncellenemedi." });
            }

            return Ok(new { Message = "Profil başarıyla güncellendi." });
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(new { Message = "Kullanıcı bulunamadı." });

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new { Message = "Mevcut şifre hatalı veya yeni şifre kurallara uymuyor." });
            }

            return Ok(new { Message = "Şifre başarıyla güncellendi." });
        }
    }
}