using System.Security.Cryptography;
using System.Text;
using Backend_Clothes_API.Helpers;
using Backend_Clothes_API.Models.DTOs;
using Backend_Clothes_API.Models.Entities;
using Backend_Clothes_API.Models.Responses;
using Backend_Clothes_API.Repositories.InterfaceRepo;
using Backend_Clothes_API.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend_Clothes_API.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterDto registerDto);
        Task<ApiResponse<AuthResponse>> LoginAsync(LoginDto loginDto, string? ipAddress = null);
        Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string refreshToken, string? ipAddress = null);
        Task<ApiResponse<bool>> RevokeTokenAsync(string refreshToken);
        Task<ApiResponse<bool>> LogoutAsync(Guid userId);
        Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto);
        Task<ApiResponse<bool>> ForgotPasswordAsync(string email);
        Task<ApiResponse<bool>> VerifyResetTokenAsync(string email, string token);
        Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<ApiResponse<UserDto>> UpdateProfileAsync(Guid userId, UpdateProfileDto updateProfileDto);
        Task<ApiResponse<bool>> DeactivateAccountAsync(Guid userId);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ApplicationDbContext _context;
        private readonly JwtHelper _jwtHelper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            ApplicationDbContext context,
            JwtHelper jwtHelper,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _context = context;
            _jwtHelper = jwtHelper;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userRepository.GetByEmailOrUsernameAsync(registerDto.Email);
                if (existingUser != null)
                {
                    return ApiResponse<AuthResponse>.ErrorResponse(
                        "Registration failed",
                        "Email or username already exists");
                }

                // Hash password
                var passwordHash = HashPassword(registerDto.Password);

                // Create new user
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    UserName = registerDto.Username,
                    Email = registerDto.Email,
                    PasswordHash = passwordHash,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    Role = "User", // Default role
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdUser = await _userRepository.CreateAsync(user);

                // Generate tokens
                var accessToken = _jwtHelper.GenerateAccessToken(createdUser);
                var refreshToken = _jwtHelper.GenerateRefreshToken();

                // Save refresh token
                var tokenEntity = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = createdUser.Id,
                    Token = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7")),
                    CreatedAt = DateTime.UtcNow
                };

                _context.RefreshTokens.Add(tokenEntity);
                await _context.SaveChangesAsync();

                var userDto = new UserDto
                {
                    Id = createdUser.Id,
                    Username = createdUser.UserName,
                    Email = createdUser.Email,
                    Role = createdUser.Role,
                    FirstName = createdUser.FirstName,
                    LastName = createdUser.LastName,
                    IsActive = createdUser.IsActive,
                    IsEmailVerified = createdUser.IsEmailVerified,
                    CreatedAt = createdUser.CreatedAt,
                    LastLogin = createdUser.LastLogin
                };

                var authResponse = new AuthResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60")),
                    User = userDto
                };

                return ApiResponse<AuthResponse>.SuccessResponse(authResponse, "User registered successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return ApiResponse<AuthResponse>.ErrorResponse("Registration failed", ex.Message);
            }
        }

        public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginDto loginDto, string? ipAddress = null)
        {
            try
            {
                // Find user by email or username
                var user = await _userRepository.GetByEmailOrUsernameAsync(loginDto.EmailOrUsername);
                if (user == null)
                {
                    return ApiResponse<AuthResponse>.ErrorResponse(
                        "Login failed",
                        "Invalid email/username or password");
                }

                // Verify password
                if (!VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    return ApiResponse<AuthResponse>.ErrorResponse(
                        "Login failed",
                        "Invalid email/username or password");
                }

                if (!user.IsActive)
                {
                    return ApiResponse<AuthResponse>.ErrorResponse(
                        "Login failed",
                        "User account is inactive");
                }

                // Update last login
                user.LastLogin = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                // Generate tokens
                var accessToken = _jwtHelper.GenerateAccessToken(user);
                var refreshToken = _jwtHelper.GenerateRefreshToken();

                // Create and save refresh token entity
                var tokenEntity = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7")),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByIp = ipAddress
                };

                _context.RefreshTokens.Add(tokenEntity);
                await _context.SaveChangesAsync();

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Role = user.Role,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    IsEmailVerified = user.IsEmailVerified,
                    CreatedAt = user.CreatedAt,
                    LastLogin = user.LastLogin
                };

                var authResponse = new AuthResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60")),
                    User = userDto
                };

                return ApiResponse<AuthResponse>.SuccessResponse(authResponse, "Login successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return ApiResponse<AuthResponse>.ErrorResponse("Login failed", ex.Message);
            }
        }

        public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string refreshToken, string? ipAddress = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    return ApiResponse<AuthResponse>.ErrorResponse(
                        "Token refresh failed",
                        "Refresh token is required");
                }

                // Find the refresh token in database
                var storedToken = await _context.RefreshTokens
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Token == refreshToken);

                if (storedToken == null)
                {
                    return ApiResponse<AuthResponse>.ErrorResponse(
                        "Token refresh failed",
                        "Invalid refresh token");
                }

                if (storedToken.IsRevoked)
                {
                    return ApiResponse<AuthResponse>.ErrorResponse(
                        "Token refresh failed",
                        "Refresh token has been revoked");
                }

                if (DateTime.UtcNow > storedToken.ExpiresAt)
                {
                    return ApiResponse<AuthResponse>.ErrorResponse(
                        "Token refresh failed",
                        "Refresh token has expired");
                }

                var user = storedToken.User;
                if (user == null || !user.IsActive)
                {
                    return ApiResponse<AuthResponse>.ErrorResponse(
                        "Token refresh failed",
                        "User is inactive or not found");
                }

                // Generate new tokens
                var newAccessToken = _jwtHelper.GenerateAccessToken(user);
                var newRefreshToken = _jwtHelper.GenerateRefreshToken();

                // Revoke old token and create new one
                storedToken.IsRevoked = true;
                storedToken.RevokedAt = DateTime.UtcNow;

                var newTokenEntity = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Token = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7")),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByIp = ipAddress
                };

                _context.RefreshTokens.Update(storedToken);
                _context.RefreshTokens.Add(newTokenEntity);
                await _context.SaveChangesAsync();

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Role = user.Role,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    IsEmailVerified = user.IsEmailVerified,
                    CreatedAt = user.CreatedAt,
                    LastLogin = user.LastLogin
                };

                var authResponse = new AuthResponse
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60")),
                    User = userDto
                };

                return ApiResponse<AuthResponse>.SuccessResponse(authResponse, "Token refreshed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return ApiResponse<AuthResponse>.ErrorResponse("Token refresh failed", ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> RevokeTokenAsync(string refreshToken)
        {
            try
            {
                var token = await _context.RefreshTokens
                    .FirstOrDefaultAsync(t => t.Token == refreshToken);

                if (token == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Token revocation failed", "Token not found");
                }

                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;

                _context.RefreshTokens.Update(token);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Token revoked successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token revocation");
                return ApiResponse<bool>.ErrorResponse("Token revocation failed", ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> LogoutAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Logout failed", "User not found");
                }

                // Revoke all active refresh tokens for this user
                var activeTokens = await _context.RefreshTokens
                    .Where(t => t.UserId == userId && !t.IsRevoked)
                    .ToListAsync();

                foreach (var token in activeTokens)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                }

                if (activeTokens.Any())
                {
                    _context.RefreshTokens.UpdateRange(activeTokens);
                    await _context.SaveChangesAsync();
                }

                return ApiResponse<bool>.SuccessResponse(true, "Logout successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return ApiResponse<bool>.ErrorResponse("Logout failed", ex.Message);
            }
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Operation failed", "User not found");
                }

                // Verify current password
                if (!VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
                {
                    return ApiResponse<bool>.ErrorResponse("Password change failed", "Current password is incorrect");
                }

                // Hash new password
                var newPasswordHash = HashPassword(changePasswordDto.NewPassword);

                // Update password
                user.PasswordHash = newPasswordHash;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                _logger.LogInformation($"User {userId} changed password successfully");
                return ApiResponse<bool>.SuccessResponse(true, "Password changed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return ApiResponse<bool>.ErrorResponse("Password change failed", ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                {
                    // Return success for security reasons
                    return ApiResponse<bool>.SuccessResponse(true, 
                        "If the email exists, a password reset link has been sent");
                }

                // In a real scenario, you would generate a reset token and send an email
                // For now, this is a placeholder
                var resetToken = GenerateResetToken();

                // You would store this token in database and send email
                // Example: var resetTokenEntity = new PasswordResetToken { ... };

                _logger.LogInformation($"Password reset requested for user {user.Id}");
                return ApiResponse<bool>.SuccessResponse(true, 
                    "If the email exists, a password reset link has been sent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in forgot password");
                return ApiResponse<bool>.ErrorResponse("Operation failed", ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> VerifyResetTokenAsync(string email, string token)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Verification failed", "User not found");
                }

                // In a real scenario, you would verify the reset token from database
                // For now, this is a placeholder
                return ApiResponse<bool>.SuccessResponse(true, "Reset token is valid");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying reset token");
                return ApiResponse<bool>.ErrorResponse("Verification failed", ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(resetPasswordDto.Email);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Reset failed", "User not found");
                }

                // In a real scenario, you would verify the reset token first
                // For now, assuming token is valid

                // Hash new password
                var newPasswordHash = HashPassword(resetPasswordDto.NewPassword);

                // Update password
                user.PasswordHash = newPasswordHash;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                _logger.LogInformation($"Password reset for user {user.Id}");
                return ApiResponse<bool>.SuccessResponse(true, "Password reset successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return ApiResponse<bool>.ErrorResponse("Reset failed", ex.Message);
            }
        }

        public async Task<ApiResponse<UserDto>> UpdateProfileAsync(Guid userId, UpdateProfileDto updateProfileDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<UserDto>.ErrorResponse("Update failed", "User not found");
                }

                // Check if new username already exists (if provided)
                if (!string.IsNullOrEmpty(updateProfileDto.Username) && updateProfileDto.Username != user.UserName)
                {
                    var existingUser = await _userRepository.GetByUsernameAsync(updateProfileDto.Username);
                    if (existingUser != null)
                    {
                        return ApiResponse<UserDto>.ErrorResponse("Update failed", "Username already taken");
                    }
                    user.UserName = updateProfileDto.Username;
                }

                // Update optional fields
                if (!string.IsNullOrEmpty(updateProfileDto.FirstName))
                {
                    user.FirstName = updateProfileDto.FirstName;
                }

                if (!string.IsNullOrEmpty(updateProfileDto.LastName))
                {
                    user.LastName = updateProfileDto.LastName;
                }

                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Role = user.Role,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    IsEmailVerified = user.IsEmailVerified,
                    CreatedAt = user.CreatedAt,
                    LastLogin = user.LastLogin
                };

                _logger.LogInformation($"Profile updated for user {userId}");
                return ApiResponse<UserDto>.SuccessResponse(userDto, "Profile updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return ApiResponse<UserDto>.ErrorResponse("Update failed", ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> DeactivateAccountAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Deactivation failed", "User not found");
                }

                // Deactivate account
                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                // Revoke all refresh tokens
                var activeTokens = await _context.RefreshTokens
                    .Where(t => t.UserId == userId && !t.IsRevoked)
                    .ToListAsync();

                foreach (var token in activeTokens)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                }

                if (activeTokens.Any())
                {
                    _context.RefreshTokens.UpdateRange(activeTokens);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"Account deactivated for user {userId}");
                return ApiResponse<bool>.SuccessResponse(true, "Account deactivated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating account");
                return ApiResponse<bool>.ErrorResponse("Deactivation failed", ex.Message);
            }
        }

        private string GenerateResetToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
