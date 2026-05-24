using Microsoft.AspNetCore.Mvc;
using Verdiq.Application.DTOs.Auth;
using Verdiq.Application.Validators;
using Verdiq.Domain.Interfaces;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
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
                IsActive = user.IsActive
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
            User = user != null ? new UserInfoDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role.ToString(),
                AvatarUrl = user.AvatarUrl,
                BarCouncilId = user.BarCouncilId,
                IsActive = user.IsActive
            } : null
        });
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
}
