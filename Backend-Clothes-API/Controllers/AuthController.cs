using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Backend_Clothes_API.Models.DTOs;
using Backend_Clothes_API.Models.Responses;
using Backend_Clothes_API.Services;

namespace Backend_Clothes_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private const string AccessTokenCookieName = "AccessToken";
        private const string RefreshTokenCookieName = "RefreshToken";

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage)).ToList();
                return BadRequest(ApiResponse<AuthResponse>.ErrorResponse("Validation failed", errors));
            }

            var result = await _authService.RegisterAsync(registerDto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            // Set authentication cookies
            SetAuthenticationCookies(result.Data!);

            return Ok(result);
        }

        /// <summary>
        /// Login user
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage)).ToList();
                return BadRequest(ApiResponse<AuthResponse>.ErrorResponse("Validation failed", errors));
            }

            var ipAddress = GetClientIpAddress();
            var result = await _authService.LoginAsync(loginDto, ipAddress);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            // Set authentication cookies
            SetAuthenticationCookies(result.Data!);

            return Ok(result);
        }

        /// <summary>
        /// Refresh access token using refresh token from cookies
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken()
        {
            var refreshToken = Request.Cookies[RefreshTokenCookieName];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(ApiResponse<AuthResponse>.ErrorResponse(
                    "Token refresh failed",
                    "Refresh token not found in cookies"));
            }

            var ipAddress = GetClientIpAddress();
            var result = await _authService.RefreshTokenAsync(refreshToken, ipAddress);

            if (!result.Success)
            {
                return Unauthorized(result);
            }

            // Update authentication cookies
            SetAuthenticationCookies(result.Data!);

            return Ok(result);
        }

        /// <summary>
        /// Logout user and revoke tokens
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<bool>>> Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ApiResponse<bool>.ErrorResponse("Logout failed", "Invalid user token"));
            }

            // Call logout service
            var result = await _authService.LogoutAsync(userId);

            // Clear authentication cookies
            ClearAuthenticationCookies();

            // Sign out from cookie authentication
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Ok(result);
        }

        /// <summary>
        /// Get current authenticated user information
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        public ActionResult<ApiResponse<UserDto>> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse<UserDto>.ErrorResponse("Authentication failed", "Invalid token"));
            }

            var userDto = new UserDto
            {
                Id = Guid.Parse(userId),
                Username = username ?? string.Empty,
                Email = email ?? string.Empty
            };

            return Ok(ApiResponse<UserDto>.SuccessResponse(userDto, "User retrieved successfully"));
        }

        /// <summary>
        /// Change password for authenticated user
        /// </summary>
        [Authorize]
        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage)).ToList();
                return BadRequest(ApiResponse<bool>.ErrorResponse("Validation failed", errors));
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ApiResponse<bool>.ErrorResponse("Operation failed", "Invalid user token"));
            }

            var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Request password reset email
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse<bool>>> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage)).ToList();
                return BadRequest(ApiResponse<bool>.ErrorResponse("Validation failed", errors));
            }

            var result = await _authService.ForgotPasswordAsync(forgotPasswordDto.Email);

            // Always return success for security reasons (don't reveal if email exists)
            return Ok(ApiResponse<bool>.SuccessResponse(true, 
                "If the email exists, a password reset link has been sent."));
        }

        /// <summary>
        /// Verify reset token validity
        /// </summary>
        [HttpPost("verify-reset-token")]
        public async Task<ActionResult<ApiResponse<bool>>> VerifyResetToken([FromBody] VerifyEmailDto verifyDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage)).ToList();
                return BadRequest(ApiResponse<bool>.ErrorResponse("Validation failed", errors));
            }

            var result = await _authService.VerifyResetTokenAsync(verifyDto.Email, verifyDto.Token);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Reset password using reset token
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse<bool>>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage)).ToList();
                return BadRequest(ApiResponse<bool>.ErrorResponse("Validation failed", errors));
            }

            var result = await _authService.ResetPasswordAsync(resetPasswordDto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        [Authorize]
        [HttpPut("profile")]
        public async Task<ActionResult<ApiResponse<UserDto>>> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(b => b.ErrorMessage)).ToList();
                return BadRequest(ApiResponse<UserDto>.ErrorResponse("Validation failed", errors));
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ApiResponse<UserDto>.ErrorResponse("Operation failed", "Invalid user token"));
            }

            var result = await _authService.UpdateProfileAsync(userId, updateProfileDto);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Deactivate user account
        /// </summary>
        [Authorize]
        [HttpPost("deactivate-account")]
        public async Task<ActionResult<ApiResponse<bool>>> DeactivateAccount()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ApiResponse<bool>.ErrorResponse("Operation failed", "Invalid user token"));
            }

            var result = await _authService.DeactivateAccountAsync(userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            // Clear cookies on successful deactivation
            ClearAuthenticationCookies();

            return Ok(result);
        }

        #region Private Methods

        private void SetAuthenticationCookies(AuthResponse authResponse)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = HttpContext.Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = authResponse.ExpiresAt
            };

            Response.Cookies.Append(AccessTokenCookieName, authResponse.AccessToken, cookieOptions);

            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = HttpContext.Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            Response.Cookies.Append(RefreshTokenCookieName, authResponse.RefreshToken, refreshCookieOptions);
        }

        private void ClearAuthenticationCookies()
        {
            Response.Cookies.Delete(AccessTokenCookieName);
            Response.Cookies.Delete(RefreshTokenCookieName);
        }

        private string? GetClientIpAddress()
        {
            if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                return forwardedFor.ToString().Split(',').FirstOrDefault()?.Trim();
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        #endregion
    }
}
