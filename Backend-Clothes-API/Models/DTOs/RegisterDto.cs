using System.ComponentModel.DataAnnotations;
namespace Backend_Clothes_API.Models.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(100, MinimumLength =3, ErrorMessage = ("Usernames must be between 3 and 100 characters long."))]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength =6, ErrorMessage = ("Passwords must be between 6 and 100 characters long."))]
        public string Password { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = ("First name cannot exceed 50 characters."))]
        public string? FirstName { get; set; }

        [StringLength(50, ErrorMessage = ("Last name cannot exceed 50 characters."))]
        public string? LastName { get; set; }
    }
}
