using System.Security.Claims;
using Backend_Clothes_API.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend_Clothes_API.Services;

namespace Backend_Clothes_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var result = await _cartService.GetCartAsync(GetUserId());
            return Ok(result);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddToCart(AddToCartDto addToCartDto)
        {
            var result = await _cartService.AddToCartAsync(GetUserId(), addToCartDto);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut("items/{productId}")]
        public async Task<IActionResult> UpdateItem(Guid productId, UpdateCartItemDto updateDto)
        {
            var result = await _cartService.UpdateCartItemAsync(GetUserId(), productId, updateDto.Quantity);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("items/{productId}")]
        public async Task<IActionResult> RemoveItem(Guid productId)
        {
            var result = await _cartService.RemoveFromCartAsync(GetUserId(), productId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var result = await _cartService.ClearCartAsync(GetUserId());
            return Ok(result);
        }
    }
}
