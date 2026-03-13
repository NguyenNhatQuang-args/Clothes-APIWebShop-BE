using Microsoft.EntityFrameworkCore;
using Backend_Clothes_API.Models.Entities;
using Backend_Clothes_API.Repositories.InterfaceRepo;
using Backend_Clothes_API.Data;

namespace Backend_Clothes_API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Email == emailOrUsername || u.UserName == emailOrUsername);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .ToListAsync();
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await SaveChangesAsync();
            return user;
        }

        public async Task<User> DeleteAsync(Guid id)
        {
            var user = await GetByIdAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await SaveChangesAsync();
            }
            return user ?? throw new KeyNotFoundException($"User with id {id} not found");
        }

        public async Task<bool> ExistsAsync(string email, string username)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email || u.UserName == username);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
