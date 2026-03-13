# Quick Start Guide - JWT Authentication

## What Was Implemented

### ✅ Complete Authentication System
1. **UserRepository** - Full CRUD operations for users
2. **AuthService** - Business logic for registration, login, token refresh, logout
3. **AuthController** - REST API endpoints for authentication
4. **JWT Helper** - Token generation and validation
5. **Cookie-Based Storage** - Secure HttpOnly cookies for tokens
6. **Database Schema** - User and RefreshToken entities

## How It Works

### 1. User Registration
- User submits: username, email, password, firstName, lastName
- System hashes password and creates user
- Returns JWT access token + refresh token in secure cookies

### 2. User Login
- User submits: email/username + password
- System verifies credentials
- Returns JWT access token + refresh token in secure cookies
- Updates last login timestamp

### 3. Token Management
- **Access Token**: Short-lived (60 min), used for API requests
- **Refresh Token**: Long-lived (7 days), stored in database, used to get new access token
- Both stored in secure, HttpOnly cookies

### 4. Protected Endpoints
- Endpoints marked with `[Authorize]` require valid JWT
- JWT extracted from Authorization header or cookies
- User claims available via `User` principal

### 5. Logout
- Revokes all refresh tokens for user
- Clears authentication cookies
- User must login again to get new tokens

## API Usage Examples

### Register
```json
POST /api/auth/register
{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

### Login
```json
POST /api/auth/login
{
  "emailOrUsername": "john_doe",
  "password": "SecurePass123!"
}
```

### Access Protected Resource
```
GET /api/auth/me
Authorization: Bearer <access_token>
```

### Refresh Token
```
POST /api/auth/refresh-token
(Refresh token automatically sent in cookie)
```

### Logout
```
POST /api/auth/logout
Authorization: Bearer <access_token>
```

## Security Features

✅ **HttpOnly Cookies** - Cannot be accessed by JavaScript (prevents XSS)
✅ **Secure Flag** - Only transmitted over HTTPS
✅ **SameSite=Strict** - Prevents CSRF attacks
✅ **Password Hashing** - SHA256 hashed passwords
✅ **Token Validation** - Signature, expiration, and claim validation
✅ **Token Revocation** - Refresh tokens tracked in database
✅ **User Status Checks** - Verifies user is active before auth

## Configuration

Edit `appsettings.json` to change:
- JWT secret key (minimum 32 characters)
- Token expiration times
- Issuer/Audience
- Database connection

## Files Created/Modified

### New Files
- `Services/AuthService.cs` - Authentication business logic
- `Controllers/AuthController.cs` - Authentication endpoints
- `AUTHENTICATION.md` - Complete documentation

### Modified Files
- `Repositories/UserRepository.cs` - Implemented all methods
- `Models/Entities/RefreshToken.cs` - Fixed ID type to Guid
- `Models/Responses/AuthResponse.cs` - Removed duplicate UserDto
- `Program.cs` - Added authentication configuration
- `appsettings.json` - Fixed Jwt section names

## Next Steps

1. **Database Migration**
   - Run migrations to create Users and RefreshTokens tables

2. **Update Frontend**
   - Login calls POST /api/auth/login
   - Store tokens in cookies (sent automatically by browser)
   - Get current user from GET /api/auth/me
   - Logout calls POST /api/auth/logout

3. **Protect Other Endpoints**
   - Add `[Authorize]` attribute to controllers
   - Access authenticated user via `User` property

4. **Additional Security**
   - Enable HTTPS in production
   - Use strong JWT secret key (not development key)
   - Implement rate limiting on auth endpoints
   - Add email verification
   - Add 2FA for enhanced security

## Testing Checklist

- [ ] Register a new user
- [ ] Login with correct credentials
- [ ] Login fails with wrong credentials
- [ ] Can access protected endpoint when logged in
- [ ] Cannot access protected endpoint when logged out
- [ ] Token refresh works with refresh token
- [ ] Logout revokes tokens
- [ ] Cookies are HttpOnly and Secure

## Common Issues & Solutions

### "Invalid token" error
- Token might be expired (default 60 minutes)
- Token signature doesn't match (check secret key)
- Token was revoked on logout

### CORS errors
- Check frontend origin is in CORS_ALLOWED_ORIGINS
- Credentials must be included in fetch requests: `credentials: 'include'`

### "User already exists"
- Email or username already registered
- Try different email/username

### Cookies not persisting
- HTTPS required for Secure flag in production
- Browser might have cookie restrictions
- Check SameSite settings
