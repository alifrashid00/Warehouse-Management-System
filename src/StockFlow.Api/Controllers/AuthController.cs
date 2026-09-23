using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Api.Contracts;
using StockFlow.Application.Users;
using StockFlow.Application.Users.Dtos;

namespace StockFlow.Api.Controllers;

[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error!);
        }

        return Ok(ApiResponse<UserResponse>.Ok(result.Value));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error!);
        }

        return Ok(ApiResponse<LoginResponse>.Ok(result.Value));
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _authService.GetByIdAsync(userId, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error!);
        }

        return Ok(ApiResponse<UserResponse>.Ok(result.Value));
    }
}
