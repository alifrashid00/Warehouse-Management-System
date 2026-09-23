using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Api.Contracts;
using StockFlow.Application.Assistant;

namespace StockFlow.Api.Controllers;

[Authorize]
[Route("api/assistant")]
public class AssistantController : ApiControllerBase
{
    private readonly AssistantService _assistantService;

    public AssistantController(AssistantService assistantService)
    {
        _assistantService = assistantService;
    }

    [HttpPost]
    public async Task<IActionResult> Ask(AskRequest request, CancellationToken cancellationToken)
    {
        var result = await _assistantService.AskAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error!);
        }

        return Ok(ApiResponse<AskResponse>.Ok(result.Value));
    }
}
