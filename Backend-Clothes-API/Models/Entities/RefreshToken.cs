using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Backend_Clothes_API.Models.Entities
{
    [Table("RefreshTokens")]
    public class RefreshToken
    {
        [Key]
        [Column("Id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("user_Id")]
        public Guid UserId { get; set; }

        [Required]
        [Column("Token")]
        public string Token { get; set; } = string.Empty;

        [Column("Expires_At")]
        public DateTime ExpiresAt { get; set; }

        [Column("Created_At")]
        public DateTime CreatedAt { get; set; }

        [Column("Revoked_At")]
        public DateTime? RevokedAt { get; set; }

        [Column("is_Revoked")]
        public bool IsRevoked { get; set; } = false;

        [MaxLength(50)]
        [Column("Created_By_Ip")]
        public string? CreatedByIp { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [NotMapped]
        public bool IsActive => !IsRevoked && DateTime.UtcNow <= ExpiresAt;
    }
}
