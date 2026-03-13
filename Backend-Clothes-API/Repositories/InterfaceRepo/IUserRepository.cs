using Backend_Clothes_API.Models.Entities;

namespace Backend_Clothes_API.Repositories.InterfaceRepo
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailOrUsernameAsync (string emailOrUsername);
        Task<IEnumerable<User>> GetAllAsync();
        Task <User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task <User> DeleteAsync(Guid id);
        Task <bool> ExistsAsync (string email, string username);
        Task SaveChangesAsync();
    }
}
