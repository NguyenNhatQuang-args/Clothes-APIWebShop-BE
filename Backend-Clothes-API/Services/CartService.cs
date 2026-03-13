using Backend_Clothes_API.Models.DTOs;
using Backend_Clothes_API.Models.Entities;
using Backend_Clothes_API.Models.Responses;
using Backend_Clothes_API.Repositories.InterfaceRepo;

namespace Backend_Clothes_API.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly ILogger<CartService> _logger;

        public CartService(ICartRepository cartRepository, IProductRepository productRepository, ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        private CartDto MapToDto(Cart cart)
        {
            var itemDtos = cart.Items.Select(i => new CartItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                ProductImageUrl = i.Product.ImageUrl,
                UnitPrice = i.Product.Price,
                Quantity = i.Quantity,
                SubTotal = i.Product.Price * i.Quantity
            }).ToList();

            return new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = itemDtos,
                TotalAmount = itemDtos.Sum(i => i.SubTotal)
            };
        }

        private async Task<Cart> GetOrCreateCartAsync(Guid userId)
        {
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            if (cart == null)
            {
                cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _cartRepository.CreateAsync(cart);
                // Fetch again to include properties if needed
                cart = await _cartRepository.GetByUserIdAsync(userId);
            }
            return cart!;
        }

        public async Task<ApiResponse<CartDto>> GetCartAsync(Guid userId)
        {
            try
            {
                var cart = await GetOrCreateCartAsync(userId);
                return ApiResponse<CartDto>.SuccessResponse(MapToDto(cart), "Cart retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting cart for user {userId}");
                return ApiResponse<CartDto>.ErrorResponse("Failed to get cart", ex.Message);
            }
        }

        public async Task<ApiResponse<CartDto>> AddToCartAsync(Guid userId, AddToCartDto addToCartDto)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(addToCartDto.ProductId);
                if (product == null)
                {
                    return ApiResponse<CartDto>.ErrorResponse("Add to cart failed", "Product not found");
                }

                if (product.StockQuantity < addToCartDto.Quantity)
                {
                    return ApiResponse<CartDto>.ErrorResponse("Add to cart failed", "Not enough stock available");
                }

                var cart = await GetOrCreateCartAsync(userId);
                var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == addToCartDto.ProductId);

                if (existingItem != null)
                {
                    existingItem.Quantity += addToCartDto.Quantity;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    cart.Items.Add(new CartItem
                    {
                        Id = Guid.NewGuid(),
                        CartId = cart.Id,
                        ProductId = addToCartDto.ProductId,
                        Quantity = addToCartDto.Quantity,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                await _cartRepository.UpdateAsync(cart);
                
                // Refresh to get full product info for mapping
                var updatedCart = await _cartRepository.GetByUserIdAsync(userId);
                return ApiResponse<CartDto>.SuccessResponse(MapToDto(updatedCart!), "Product added to cart successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding to cart for user {userId}");
                return ApiResponse<CartDto>.ErrorResponse("Failed to add to cart", ex.Message);
            }
        }

        public async Task<ApiResponse<CartDto>> UpdateCartItemAsync(Guid userId, Guid productId, int quantity)
        {
            try
            {
                if (quantity <= 0)
                {
                    return await RemoveFromCartAsync(userId, productId);
                }

                var cart = await _cartRepository.GetByUserIdAsync(userId);
                if (cart == null)
                {
                    return ApiResponse<CartDto>.ErrorResponse("Update failed", "Cart not found");
                }

                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (item == null)
                {
                    return ApiResponse<CartDto>.ErrorResponse("Update failed", "Product not in cart");
                }

                // Check stock
                var product = await _productRepository.GetByIdAsync(productId);
                if (product != null && product.StockQuantity < quantity)
                {
                    return ApiResponse<CartDto>.ErrorResponse("Update failed", "Not enough stock available");
                }

                item.Quantity = quantity;
                item.UpdatedAt = DateTime.UtcNow;

                await _cartRepository.UpdateAsync(cart);

                var updatedCart = await _cartRepository.GetByUserIdAsync(userId);
                return ApiResponse<CartDto>.SuccessResponse(MapToDto(updatedCart!), "Cart updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating cart item for user {userId}");
                return ApiResponse<CartDto>.ErrorResponse("Failed to update cart", ex.Message);
            }
        }

        public async Task<ApiResponse<CartDto>> RemoveFromCartAsync(Guid userId, Guid productId)
        {
            try
            {
                var cart = await _cartRepository.GetByUserIdAsync(userId);
                if (cart == null)
                {
                    return ApiResponse<CartDto>.ErrorResponse("Remove failed", "Cart not found");
                }

                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    await _cartRepository.DeleteItemAsync(item);
                }

                var updatedCart = await _cartRepository.GetByUserIdAsync(userId);
                return ApiResponse<CartDto>.SuccessResponse(MapToDto(updatedCart!), "Item removed from cart successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing cart item for user {userId}");
                return ApiResponse<CartDto>.ErrorResponse("Failed to remove item", ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> ClearCartAsync(Guid userId)
        {
            try
            {
                var cart = await _cartRepository.GetByUserIdAsync(userId);
                if (cart != null)
                {
                    await _cartRepository.ClearCartAsync(cart.Id);
                }
                return ApiResponse<bool>.SuccessResponse(true, "Cart cleared successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error clearing cart for user {userId}");
                return ApiResponse<bool>.ErrorResponse("Failed to clear cart", ex.Message);
            }
        }
    }
}
