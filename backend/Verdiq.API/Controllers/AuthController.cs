using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Auth;
using Verdiq.Application.Validators;
using Verdiq.Domain.Interfaces;
using Verdiq.Infrastructure.Data;
using Verdiq.Infrastructure.Services;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IJwtService _jwtService;
    private readonly AppDbContext _context;

    public AuthController(IAuthService authService, ITwoFactorService twoFactorService,
        IJwtService jwtService, AppDbContext context)
    {
        _authService = authService;
        _twoFactorService = twoFactorService;
        _jwtService = jwtService;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        var (isValid, error) = AuthValidators.ValidateRegister(dto);
        if (!isValid)
            return BadRequest(new AuthResponseDto { Success = false, Message = error });

        var (success, message, user, accessToken, refreshToken) =
            await _authService.RegisterAsync(dto.FullName, dto.Email, dto.Password, dto.Phone, dto.Role);

        if (!success)
            return BadRequest(new AuthResponseDto { Success = false, Message = message });

        return Ok(new AuthResponseDto
        {
            Success = true,
            Message = message,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = user != null ? new UserInfoDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role.ToString(),
                AvatarUrl = user.AvatarUrl,
                BarCouncilId = user.BarCouncilId,
                IsActive = user.IsActive,
                TwoFactorEnabled = user.TwoFactorEnabled
            } : null
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var (isValid, error) = AuthValidators.ValidateLogin(dto);
        if (!isValid)
            return BadRequest(new AuthResponseDto { Success = false, Message = error });

        var (success, message, user, accessToken, refreshToken) =
            await _authService.LoginAsync(dto.Email, dto.Password);

        if (!success)
            return Unauthorized(new AuthResponseDto { Success = false, Message = message });

        return Ok(new AuthResponseDto
        {
            Success = true,
            Message = message,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TempToken = accessToken == null ? refreshToken : null,
            User = user != null ? new UserInfoDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role.ToString(),
                AvatarUrl = user.AvatarUrl,
                BarCouncilId = user.BarCouncilId,
                IsActive = user.IsActive,
                TwoFactorEnabled = user.TwoFactorEnabled
            } : null
        });
    }

    [HttpPost("2fa/verify")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> VerifyTwoFactor([FromBody] TwoFactorVerifyDto dto, [FromQuery] string tempToken)
    {
        var userId = _jwtService.ValidateToken(tempToken);
        if (userId == null)
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Invalid or expired verification token"));

        var valid = await _twoFactorService.VerifyAsync(userId.Value, dto.Code);
        if (!valid)
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Invalid 2FA code"));

        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null)
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail("User not found"));

        var (accessToken, refreshToken) = _jwtService.GenerateTokens(user);
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        user.LoginAttempts = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto
        {
            Success = true,
            Message = "2FA verified",
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new UserInfoDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                TwoFactorEnabled = true
            }
        }));
    }

    [HttpPost("2fa/setup")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<TwoFactorSetupResponse>>> SetupTwoFactor()
    {
        var userId = GetUserId();
        var setup = await _twoFactorService.SetupAsync(userId);
        return Ok(ApiResponse<TwoFactorSetupResponse>.Ok(setup));
    }

    [HttpPost("2fa/disable")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> DisableTwoFactor()
    {
        var userId = GetUserId();
        await _twoFactorService.DisableAsync(userId);
        return Ok(ApiResponse<object>.Ok(null!, "2FA disabled"));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] TokenRefreshDto dto)
    {
        var (success, message, accessToken, refreshToken) =
            await _authService.RefreshTokenAsync(dto.AccessToken, dto.RefreshToken);

        if (!success)
            return Unauthorized(new AuthResponseDto { Success = false, Message = message });

        return Ok(new AuthResponseDto
        {
            Success = true,
            Message = message,
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
            return Unauthorized();

        await _authService.LogoutAsync(Guid.Parse(userIdClaim));
        return Ok(new { success = true, message = "Logged out successfully" });
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return claim != null ? Guid.Parse(claim) : Guid.Empty;
    }
}
