using Backend_Clothes_API.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend_Clothes_API.Services;

namespace Backend_Clothes_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? searchTerm,
            [FromQuery] Guid? categoryId,
            [FromQuery] string? sortBy,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _productService.GetAllProductsAsync(searchTerm, categoryId, sortBy, page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateProductDto createProductDto)
        {
            var result = await _productService.CreateProductAsync(createProductDto);
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, UpdateProductDto updateProductDto)
        {
            var result = await _productService.UpdateProductAsync(id, updateProductDto);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}
