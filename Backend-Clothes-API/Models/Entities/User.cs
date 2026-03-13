using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace Backend_Clothes_API.Models.Entities
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("Id")]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("Username")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("Password_Hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(50)]
        [Column("FirstName")]
        public string? FirstName { get; set; }

        [MaxLength(50)]
        [Column("LastName")]
        public string? LastName { get; set; }

        [Column("is_Active")]
        public bool IsActive { get; set; } = true;

        [Column("is_email_verified")]
        public bool IsEmailVerified { get; set; } = false;

        [Required]
        [MaxLength(20)]
        [Column("Role")]
        public string Role { get; set; } = "User";

        [Column("Created_At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("Updated_At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("Last_Login_At")]
        public DateTime? LastLogin { get; set; }

        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    }
}
