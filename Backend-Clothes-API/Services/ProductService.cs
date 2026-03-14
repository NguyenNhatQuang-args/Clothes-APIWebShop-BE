using Backend_Clothes_API.Models.DTOs;
using Backend_Clothes_API.Models.Entities;
using Backend_Clothes_API.Models.Responses;
using Backend_Clothes_API.Repositories.InterfaceRepo;

namespace Backend_Clothes_API.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository, ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        private static ProductDto MapToDto(Product p)
        {
            return new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Brand = p.Brand,
                Sizes = p.Sizes,
                Colors = p.Colors,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                ImageUrl = p.ImageUrl,
                AdditionalImages = p.Images.Select(img => new ProductImageDto
                {
                    Id = img.Id,
                    ImageUrl = img.ImageUrl
                }).ToList(),
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name ?? "N/A",
                CreatedAt = p.CreatedAt
            };
        }

        public async Task<ApiResponse<IEnumerable<ProductDto>>> GetAllProductsAsync(string? searchTerm = null, Guid? categoryId = null, string? sortBy = null, int page = 1, int pageSize = 10)
        {
            try
            {
                var products = await _productRepository.GetAllAsync(searchTerm, categoryId, sortBy, page, pageSize);
                var productDtos = products.Select(MapToDto);

                var totalCount = await _productRepository.GetTotalCountAsync(searchTerm, categoryId);
                var message = $"Products retrieved successfully. Total count: {totalCount}";

                return ApiResponse<IEnumerable<ProductDto>>.SuccessResponse(productDtos, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                return ApiResponse<IEnumerable<ProductDto>>.ErrorResponse("Failed to get products", ex.Message);
            }
        }

        public async Task<ApiResponse<ProductDto>> GetProductByIdAsync(Guid id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                {
                    return ApiResponse<ProductDto>.ErrorResponse("Product not found", "No product exists with the provided ID");
                }

                return ApiResponse<ProductDto>.SuccessResponse(MapToDto(product), "Product retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting product {id}");
                return ApiResponse<ProductDto>.ErrorResponse("Failed to get product", ex.Message);
            }
        }

        public async Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto createProductDto)
        {
            try
            {
                if (!await _categoryRepository.ExistsAsync(createProductDto.CategoryId))
                {
                    return ApiResponse<ProductDto>.ErrorResponse("Create failed", "Invalid Category ID");
                }

                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = createProductDto.Name,
                    Description = createProductDto.Description,
                    Brand = createProductDto.Brand,
                    Sizes = createProductDto.Sizes,
                    Colors = createProductDto.Colors,
                    Price = createProductDto.Price,
                    StockQuantity = createProductDto.StockQuantity,
                    ImageUrl = createProductDto.ImageUrl,
                    CategoryId = createProductDto.CategoryId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                if (createProductDto.AdditionalImages != null && createProductDto.AdditionalImages.Any())
                {
                    foreach (var imgUrl in createProductDto.AdditionalImages)
                    {
                        product.Images.Add(new ProductImage
                        {
                            Id = Guid.NewGuid(),
                            ProductId = product.Id,
                            ImageUrl = imgUrl,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                var createdProduct = await _productRepository.CreateAsync(product);
                
                // Fetch again to include category and images
                var result = await _productRepository.GetByIdAsync(createdProduct.Id);

                return ApiResponse<ProductDto>.SuccessResponse(MapToDto(result!), "Product created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return ApiResponse<ProductDto>.ErrorResponse("Failed to create product", ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> UpdateProductAsync(Guid id, UpdateProductDto updateProductDto)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Product not found", "No product exists with the provided ID");
                }

                if (updateProductDto.CategoryId.HasValue && updateProductDto.CategoryId.Value != product.CategoryId)
                {
                    if (!await _categoryRepository.ExistsAsync(updateProductDto.CategoryId.Value))
                    {
                        return ApiResponse<bool>.ErrorResponse("Update failed", "Invalid Category ID");
                    }
                    product.CategoryId = updateProductDto.CategoryId.Value;
                }

                if (!string.IsNullOrEmpty(updateProductDto.Name)) product.Name = updateProductDto.Name;
                if (updateProductDto.Description != null) product.Description = updateProductDto.Description;
                if (updateProductDto.Brand != null) product.Brand = updateProductDto.Brand;
                if (updateProductDto.Sizes != null) product.Sizes = updateProductDto.Sizes;
                if (updateProductDto.Colors != null) product.Colors = updateProductDto.Colors;
                if (updateProductDto.Price.HasValue) product.Price = updateProductDto.Price.Value;
                if (updateProductDto.StockQuantity.HasValue) product.StockQuantity = updateProductDto.StockQuantity.Value;
                if (updateProductDto.ImageUrl != null) product.ImageUrl = updateProductDto.ImageUrl;

                // Update Additional Images if provided
                if (updateProductDto.AdditionalImages != null)
                {
                    // Clear existing images and add new ones (Simplified logic)
                    product.Images.Clear();
                    foreach (var imgUrl in updateProductDto.AdditionalImages)
                    {
                        product.Images.Add(new ProductImage
                        {
                            Id = Guid.NewGuid(),
                            ProductId = product.Id,
                            ImageUrl = imgUrl,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
                
                product.UpdatedAt = DateTime.UtcNow;

                await _productRepository.UpdateAsync(product);

                return ApiResponse<bool>.SuccessResponse(true, "Product updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product {id}");
                return ApiResponse<bool>.ErrorResponse("Failed to update product", ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> DeleteProductAsync(Guid id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Product not found", "No product exists with the provided ID");
                }

                await _productRepository.DeleteAsync(product);

                return ApiResponse<bool>.SuccessResponse(true, "Product deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product {id}");
                return ApiResponse<bool>.ErrorResponse("Failed to delete product", ex.Message);
            }
        }
    }
}
