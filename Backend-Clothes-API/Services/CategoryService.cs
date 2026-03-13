using Backend_Clothes_API.Models.DTOs;
using Backend_Clothes_API.Models.Entities;
using Backend_Clothes_API.Models.Responses;
using Backend_Clothes_API.Repositories.InterfaceRepo;

namespace Backend_Clothes_API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetAllAsync();
                var categoryDtos = categories.Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ProductCount = c.Products.Count
                });

                return ApiResponse<IEnumerable<CategoryDto>>.SuccessResponse(categoryDtos, "Categories retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                return ApiResponse<IEnumerable<CategoryDto>>.ErrorResponse("Failed to get categories", ex.Message);
            }
        }

        public async Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(Guid id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    return ApiResponse<CategoryDto>.ErrorResponse("Category not found", "No category exists with the provided ID");
                }

                var categoryDto = new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    ProductCount = category.Products.Count
                };

                return ApiResponse<CategoryDto>.SuccessResponse(categoryDto, "Category retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting category {id}");
                return ApiResponse<CategoryDto>.ErrorResponse("Failed to get category", ex.Message);
            }
        }

        public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            try
            {
                var category = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = createCategoryDto.Name,
                    Description = createCategoryDto.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdCategory = await _categoryRepository.CreateAsync(category);

                var categoryDto = new CategoryDto
                {
                    Id = createdCategory.Id,
                    Name = createdCategory.Name,
                    Description = createdCategory.Description,
                    ProductCount = 0
                };

                return ApiResponse<CategoryDto>.SuccessResponse(categoryDto, "Category created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return ApiResponse<CategoryDto>.ErrorResponse("Failed to create category", ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> UpdateCategoryAsync(Guid id, CreateCategoryDto updateCategoryDto)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Category not found", "No category exists with the provided ID");
                }

                category.Name = updateCategoryDto.Name;
                category.Description = updateCategoryDto.Description;
                category.UpdatedAt = DateTime.UtcNow;

                await _categoryRepository.UpdateAsync(category);

                return ApiResponse<bool>.SuccessResponse(true, "Category updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating category {id}");
                return ApiResponse<bool>.ErrorResponse("Failed to update category", ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> DeleteCategoryAsync(Guid id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Category not found", "No category exists with the provided ID");
                }

                if (await _categoryRepository.HasProductsAsync(id))
                {
                    return ApiResponse<bool>.ErrorResponse("Delete failed", "Cannot delete category because it contains products");
                }

                await _categoryRepository.DeleteAsync(category);

                return ApiResponse<bool>.SuccessResponse(true, "Category deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting category {id}");
                return ApiResponse<bool>.ErrorResponse("Failed to delete category", ex.Message);
            }
        }
    }
}
