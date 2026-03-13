using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_Clothes_API.Models.Entities
{
    [Table("Products")]
    public class Product
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        public int StockQuantity { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relationship
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; } = null!;
    }
}
