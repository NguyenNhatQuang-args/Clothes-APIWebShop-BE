using Backend_Clothes_API.Models.DTOs;
using Backend_Clothes_API.Models.Responses;

namespace Backend_Clothes_API.Services
{
    public interface ICategoryService
    {
        Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllCategoriesAsync();
        Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(Guid id);
        Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryDto createCategoryDto);
        Task<ApiResponse<bool>> UpdateCategoryAsync(Guid id, CreateCategoryDto updateCategoryDto);
        Task<ApiResponse<bool>> DeleteCategoryAsync(Guid id);
    }
}
