using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Api.Contracts;
using StockFlow.Application.Categories;
using StockFlow.Application.Categories.Dtos;

namespace StockFlow.Api.Controllers;

[Authorize]
[Route("api/categories")]
public class CategoriesController : ApiControllerBase
{
    private readonly CategoryService _categoryService;

    public CategoriesController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [Authorize(Roles = "Admin,WarehouseManager")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await _categoryService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponse<CategoryResponse>.Ok(category));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<List<CategoryResponse>>.Ok(categories));
    }
}
