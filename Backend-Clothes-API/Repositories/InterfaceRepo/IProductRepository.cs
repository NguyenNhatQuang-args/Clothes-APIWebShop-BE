using Backend_Clothes_API.Models.Entities;

namespace Backend_Clothes_API.Repositories.InterfaceRepo
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync(string? searchTerm = null, Guid? categoryId = null, string? sortBy = null, int page = 1, int pageSize = 10);
        Task<int> GetTotalCountAsync(string? searchTerm = null, Guid? categoryId = null);
        Task<Product?> GetByIdAsync(Guid id);
        Task<Product> CreateAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(Product product);
        Task<bool> ExistsAsync(Guid id);
    }
}
