using Backend_Clothes_API.Models.DTOs;
using Backend_Clothes_API.Models.Responses;

namespace Backend_Clothes_API.Services
{
    public interface IProductService
    {
        Task<ApiResponse<IEnumerable<ProductDto>>> GetAllProductsAsync(string? searchTerm = null, Guid? categoryId = null, string? sortBy = null, int page = 1, int pageSize = 10);
        Task<ApiResponse<ProductDto>> GetProductByIdAsync(Guid id);
        Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto createProductDto);
        Task<ApiResponse<bool>> UpdateProductAsync(Guid id, UpdateProductDto updateProductDto);
        Task<ApiResponse<bool>> DeleteProductAsync(Guid id);
    }
}
