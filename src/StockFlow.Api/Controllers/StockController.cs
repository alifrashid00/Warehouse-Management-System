using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Api.Contracts;
using StockFlow.Application.Inventory;
using StockFlow.Application.Inventory.Dtos;

namespace StockFlow.Api.Controllers;

[Authorize]
[Route("api/stock")]
public class StockController : ApiControllerBase
{
    private readonly StockService _stockService;

    public StockController(StockService stockService)
    {
        _stockService = stockService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStockLevels([FromQuery] string? search, [FromQuery] bool lowOnly, CancellationToken cancellationToken)
    {
        var levels = await _stockService.GetStockLevelsAsync(search, lowOnly, cancellationToken);
        return Ok(ApiResponse<List<StockLevelResponse>>.Ok(levels));
    }
}
