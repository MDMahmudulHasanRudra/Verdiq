using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Verdiq.API.Models;
using Verdiq.Application.DTOs.Auth;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Interfaces;
using Verdiq.Infrastructure.Data;

namespace Verdiq.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;
    private readonly IJwtService _jwtService;
    private readonly AppDbContext _context;

    public AuthController(IAuthService authService,
        IJwtService jwtService, AppDbContext context)
    {
        _authService = authService;
        _jwtService = jwtService;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        var (success, message, user, accessToken, refreshToken) =
            await _authService.RegisterAsync(dto.FullName, dto.Email, dto.Password, dto.Phone, dto.Role, dto.ChamberId);

        if (!success)
            return BadRequest(new AuthResponseDto { Success = false, Message = message });

        return Ok(new AuthResponseDto
        {
            Success = true,
            Message = message,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = user != null ? MapUserInfo(user) : null
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
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
            User = user != null ? MapUserInfo(user) : null
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

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<AuthResponseDto>> GetCurrentUser()
    {
        var userId = GetUserId();
        var user = await _context.Users.Include(u => u.Chamber).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return NotFound(new AuthResponseDto { Success = false, Message = "User not found" });

        return Ok(new AuthResponseDto
        {
            Success = true,
            Message = "User retrieved",
            User = MapUserInfo(user)
        });
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<ActionResult<AuthResponseDto>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = GetUserId();

        var (success, message, user) =
            await _authService.UpdateProfileAsync(userId, dto.FullName, dto.Phone, dto.BarCouncilId);

        if (!success)
            return BadRequest(new AuthResponseDto { Success = false, Message = message });

        return Ok(new AuthResponseDto
        {
            Success = true,
            Message = message,
            User = user != null ? MapUserInfo(user) : null
        });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<AuthResponseDto>> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = GetUserId();

        var (success, message) =
            await _authService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);

        if (!success)
            return BadRequest(new AuthResponseDto { Success = false, Message = message });

        return Ok(new AuthResponseDto { Success = true, Message = message });
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

    private UserInfoDto MapUserInfo(User user)
    {
        var chamberName = user.Chamber?.Name
            ?? _context.Chambers.Where(c => c.Id == user.ChamberId).Select(c => c.Name).FirstOrDefault()
            ?? "";

        return new UserInfoDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role.ToString(),
            AvatarUrl = user.AvatarUrl,
            BarCouncilId = user.BarCouncilId,
            ChamberId = user.ChamberId,
            ChamberName = chamberName
        };
    }
}
