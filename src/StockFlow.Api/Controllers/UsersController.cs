using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Api.Contracts;
using StockFlow.Application.Users;
using StockFlow.Application.Users.Dtos;

namespace StockFlow.Api.Controllers;

[Authorize(Roles = "Admin")]
[Route("api/users")]
public class UsersController : ApiControllerBase
{
    private readonly AuthService _authService;

    public UsersController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> SetStatus(int id, SetUserStatusRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var actorId))
        {
            return Unauthorized();
        }

        var result = await _authService.SetStatusAsync(actorId, id, request, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error!);
        }

        return Ok(ApiResponse<UserResponse>.Ok(result.Value));
    }
}
