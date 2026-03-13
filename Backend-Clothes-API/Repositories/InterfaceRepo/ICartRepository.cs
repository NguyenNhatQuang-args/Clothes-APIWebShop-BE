using Backend_Clothes_API.Models.Entities;

namespace Backend_Clothes_API.Repositories.InterfaceRepo
{
    public interface ICartRepository
    {
        Task<Cart?> GetByUserIdAsync(Guid userId);
        Task<Cart> CreateAsync(Cart cart);
        Task UpdateAsync(Cart cart);
        Task DeleteItemAsync(CartItem item);
        Task ClearCartAsync(Guid cartId);
    }
}
