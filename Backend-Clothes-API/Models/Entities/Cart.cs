using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_Clothes_API.Models.Entities
{
    [Table("Carts")]
    public class Cart
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relationship
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
