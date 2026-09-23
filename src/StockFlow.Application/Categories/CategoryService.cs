using StockFlow.Application.Categories.Dtos;
using StockFlow.Domain.Categories;

namespace StockFlow.Application.Categories;

public class CategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
        };

        var created = await _categoryRepository.AddAsync(category, cancellationToken);

        return new CategoryResponse(created.Id, created.Name, created.Description);
    }

    public async Task<List<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);

        return categories
            .Select(c => new CategoryResponse(c.Id, c.Name, c.Description))
            .ToList();
    }
}
