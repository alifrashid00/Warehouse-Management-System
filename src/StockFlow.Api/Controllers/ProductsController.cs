using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Api.Contracts;
using StockFlow.Application.Products;
using StockFlow.Application.Products.Dtos;

namespace StockFlow.Api.Controllers;

[Authorize]
[Route("api/products")]
public class ProductsController : ApiControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    [Authorize(Roles = "Admin,WarehouseManager")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await _productService.CreateAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return HandleFailure(result.Error!);
        }

        return Ok(ApiResponse<ProductResponse>.Ok(result.Value));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var products = await _productService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<List<ProductResponse>>.Ok(products));
    }
}
