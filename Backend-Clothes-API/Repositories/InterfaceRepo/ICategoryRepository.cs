using Backend_Clothes_API.Models.Entities;

namespace Backend_Clothes_API.Repositories.InterfaceRepo
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(Guid id);
        Task<Category> CreateAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(Category category);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> HasProductsAsync(Guid id);
    }
}
