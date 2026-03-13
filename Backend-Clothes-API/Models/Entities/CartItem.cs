using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_Clothes_API.Models.Entities
{
    [Table("CartItems")]
    public class CartItem
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid CartId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relationships
        [ForeignKey("CartId")]
        public virtual Cart Cart { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}
