using Backend_Clothes_API.Data;
using Backend_Clothes_API.Models.Entities;
using Backend_Clothes_API.Repositories.InterfaceRepo;
using Microsoft.EntityFrameworkCore;

namespace Backend_Clothes_API.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;

        public CartRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Cart?> GetByUserIdAsync(Guid userId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Cart> CreateAsync(Cart cart)
        {
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task UpdateAsync(Cart cart)
        {
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteItemAsync(CartItem item)
        {
            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
        }

        public async Task ClearCartAsync(Guid cartId)
        {
            var items = await _context.CartItems.Where(i => i.CartId == cartId).ToListAsync();
            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
    }
}
