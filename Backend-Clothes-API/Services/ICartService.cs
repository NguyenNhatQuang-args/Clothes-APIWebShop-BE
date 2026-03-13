using Backend_Clothes_API.Models.DTOs;
using Backend_Clothes_API.Models.Responses;

namespace Backend_Clothes_API.Services
{
    public interface ICartService
    {
        Task<ApiResponse<CartDto>> GetCartAsync(Guid userId);
        Task<ApiResponse<CartDto>> AddToCartAsync(Guid userId, AddToCartDto addToCartDto);
        Task<ApiResponse<CartDto>> UpdateCartItemAsync(Guid userId, Guid productId, int quantity);
        Task<ApiResponse<CartDto>> RemoveFromCartAsync(Guid userId, Guid productId);
        Task<ApiResponse<bool>> ClearCartAsync(Guid userId);
    }
}
