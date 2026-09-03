using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Application.DTOs.Authentication;
using ProductManagement.Application.Interfaces.Services;

namespace ProductManagement.API.Controllers;

using Asp.Versioning;

/// <summary>
/// Provides authentication and token management endpoints.
/// </summary>
[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(
        IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(
        RegisterDto registerDto)
    {
        var response =
            await _authService.RegisterAsync(registerDto);

        if (response == null)
        {
            return BadRequest(new
            {
                message =
                    "A user with this email already exists."
            });
        }

        return Ok(response);
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(
        LoginDto loginDto)
    {
        var response =
            await _authService.LoginAsync(loginDto);

        if (response == null)
        {
            return Unauthorized(new
            {
                message =
                    "Invalid email or password."
            });
        }

        return Ok(response);
    }

    // POST: api/auth/refresh-token
    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken(
        RefreshTokenRequestDto refreshTokenRequestDto)
    {
        var response =
            await _authService.RefreshTokenAsync(
                refreshTokenRequestDto);

        if (response == null)
        {
            return Unauthorized(new
            {
                message =
                    "Invalid or expired refresh token."
            });
        }

        return Ok(response);
    }
}