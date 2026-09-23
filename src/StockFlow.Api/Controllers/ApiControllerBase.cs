using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Api.Contracts;
using StockFlow.Application.Common;

namespace StockFlow.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected bool TryGetCurrentUserId(out int userId)
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
    }

    protected IActionResult HandleFailure(Error error)
    {
        var response = ApiResponse<object>.Fail(error.Code, error.Message);

        return error.Type switch
        {
            ErrorType.Validation => BadRequest(response),
            ErrorType.NotFound => NotFound(response),
            ErrorType.Conflict => Conflict(response),
            ErrorType.Unauthorized => Unauthorized(response),
            ErrorType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, response),
            _ => StatusCode(StatusCodes.Status500InternalServerError, response),
        };
    }
}
