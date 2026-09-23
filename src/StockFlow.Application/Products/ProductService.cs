using StockFlow.Application.Common;
using StockFlow.Application.Products.Dtos;
using StockFlow.Domain.Products;

namespace StockFlow.Application.Products;

public class ProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductResponse>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        if (await _productRepository.SkuExistsAsync(request.Sku, cancellationToken))
        {
            var error = new Error("PRODUCT_SKU_DUPLICATE", $"A product with SKU '{request.Sku}' already exists.", ErrorType.Conflict);
            return Result<ProductResponse>.Failure(error);
        }

        var product = new Product
        {
            Sku = request.Sku,
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Unit = request.Unit,
            ReorderLevel = request.ReorderLevel,
            UnitCost = request.UnitCost,
        };

        var created = await _productRepository.AddAsync(product, cancellationToken);

        return Result<ProductResponse>.Success(ToResponse(created));
    }

    public async Task<List<ProductResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetAllAsync(cancellationToken);
        return products.Select(ToResponse).ToList();
    }

    private static ProductResponse ToResponse(Product product) => new(
        product.Id,
        product.Sku,
        product.Name,
        product.Description,
        product.CategoryId,
        product.Unit,
        product.ReorderLevel,
        product.UnitCost,
        product.IsActive);
}
