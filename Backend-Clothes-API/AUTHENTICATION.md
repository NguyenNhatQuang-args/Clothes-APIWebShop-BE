# JWT Authentication Implementation - Backend Clothes API

## Overview
This backend implements a secure JWT-based authentication system with cookie storage, providing user registration, login, token refresh, and logout functionality.

## Features

### 🔐 Security Features
- **JWT Access Tokens**: Short-lived tokens for API request authentication
- **Refresh Tokens**: Long-lived tokens for obtaining new access tokens
- **HttpOnly Cookies**: Tokens stored in secure, httpOnly cookies (protected from XSS attacks)
- **Password Hashing**: SHA256 password hashing with salt
- **Token Revocation**: Support for revoking refresh tokens on logout
- **CORS Protection**: SameSite=Strict cookies for CSRF protection

### 📋 API Endpoints

#### Registration
```
POST /api/auth/register
Content-Type: application/json

{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe"
}

Response:
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "accessToken": "eyJhbGc...",
    "refreshToken": "...",
    "expiresAt": "2024-01-XX...",
    "user": {
      "id": "...",
      "username": "john_doe",
      "email": "john@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "isActive": true,
      "isEmailVerified": false,
      "createdAt": "2024-01-XX..."
    }
  }
}
```

#### Login
```
POST /api/auth/login
Content-Type: application/json

{
  "emailOrUsername": "john_doe",
  "password": "SecurePassword123!",
  "rememberMe": false
}

Response: Same as registration
```

#### Refresh Token
```
POST /api/auth/refresh-token
Cookies: RefreshToken=...

Response:
{
  "success": true,
  "message": "Token refreshed successfully",
  "data": {
    "accessToken": "new_token...",
    "refreshToken": "new_refresh_token...",
    "expiresAt": "2024-01-XX...",
    "user": {...}
  }
}
```

#### Get Current User (Protected)
```
GET /api/auth/me
Authorization: Bearer <access_token>

Response:
{
  "success": true,
  "message": "User retrieved successfully",
  "data": {
    "id": "...",
    "username": "john_doe",
    "email": "john@example.com"
  }
}
```

#### Logout (Protected)
```
POST /api/auth/logout
Authorization: Bearer <access_token>

Response:
{
  "success": true,
  "message": "Logout successful",
  "data": true
}
```

## Configuration

### appsettings.json
```json
{
  "Jwt": {
    "SecretKey": "your_secret_key_here_min_32_chars",
    "Issuer": "RoxiosAPI",
    "Audience": "RoxiosClient",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Environment Variables
- `DB_CONNECTION_STRING`: PostgreSQL database connection string

## Architecture

### Project Structure
```
Backend-Clothes-API/
├── Controllers/
│   └── AuthController.cs           # Authentication endpoints
├── Services/
│   └── AuthService.cs              # Business logic for auth
├── Repositories/
│   ├── IUserRepository.cs           # User repository interface
│   └── UserRepository.cs            # User repository implementation
├── Models/
│   ├── DTOs/
│   │   ├── RegisterDto.cs
│   │   ├── LoginDto.cs
│   │   └── UserDto.cs
│   ├── Entities/
│   │   ├── User.cs
│   │   └── RefreshToken.cs
│   └── Responses/
│       ├── AuthResponse.cs
│       └── ApiResponse.cs
├── Helpers/
│   └── JwtHelper.cs                # JWT token generation and validation
├── Data/
│   └── ApplicationDbContext.cs      # Entity Framework context
└── Program.cs                       # Service configuration
```

### Key Classes

#### User Entity
- Id (Guid): Primary key
- UserName (string): Unique username
- Email (string): Unique email address
- PasswordHash (string): Hashed password
- FirstName, LastName (string?): Optional user info
- IsActive (bool): Account status
- IsEmailVerified (bool): Email verification status
- CreatedAt, UpdatedAt, LastLogin (DateTime): Timestamps
- RefreshTokens (ICollection): Related refresh tokens

#### RefreshToken Entity
- Id (Guid): Primary key
- UserId (Guid): Foreign key to User
- Token (string): The refresh token value
- ExpiresAt (DateTime): Token expiration time
- CreatedAt (DateTime): Creation timestamp
- RevokedAt (DateTime?): Revocation timestamp if revoked
- IsRevoked (bool): Revocation status
- CreatedByIp (string?): IP address of token creator
- IsActive (bool): Computed property (not revoked and not expired)

#### JwtHelper
Handles JWT token operations:
- `GenerateAccessToken(User)`: Creates a short-lived JWT
- `GenerateRefreshToken()`: Creates a random refresh token
- `ValidateToken(string)`: Validates and extracts claims
- `GetUserIdFromToken(string)`: Extracts user ID from token

#### AuthService
Business logic layer:
- `RegisterAsync()`: User registration with validation
- `LoginAsync()`: User authentication
- `RefreshTokenAsync()`: Token refresh with validation
- `RevokeTokenAsync()`: Token revocation
- `LogoutAsync()`: Complete logout with token revocation

#### AuthController
HTTP endpoints for authentication operations

### Authentication Flow

#### Registration Flow
1. User submits registration data
2. Validate input and check for existing user
3. Hash password with SHA256
4. Create new User entity
5. Generate JWT access token
6. Generate refresh token and store in database
7. Set secure HttpOnly cookies
8. Return tokens and user info

#### Login Flow
1. User submits email/username and password
2. Find user by email or username
3. Verify password hash
4. Check if user is active
5. Update last login timestamp
6. Generate new tokens
7. Store refresh token in database
8. Set secure HttpOnly cookies
9. Return tokens and user info

#### Token Refresh Flow
1. Read refresh token from secure cookie
2. Validate token exists in database
3. Check if token is revoked or expired
4. Check if associated user is active
5. Revoke old token
6. Generate new access and refresh tokens
7. Store new refresh token
8. Update secure cookies
9. Return new tokens

#### Protected Endpoint Access
1. Client sends request with JWT in Authorization header or cookie
2. JWT middleware validates token signature and claims
3. Extracts claims and populates User principal
4. Controller action can access authenticated user info
5. If token is invalid or expired, return 401 Unauthorized

## Security Considerations

### Best Practices Implemented
✅ **HttpOnly Cookies**: Tokens stored in HttpOnly cookies prevent XSS attacks
✅ **Secure Flag**: Cookies marked as Secure to only transmit over HTTPS
✅ **SameSite=Strict**: Prevents CSRF attacks
✅ **Password Hashing**: SHA256 hashing for password storage
✅ **Token Validation**: Comprehensive JWT validation
✅ **Token Revocation**: Tokens can be revoked for logout
✅ **Expiration Times**: Separate short-lived access and longer-lived refresh tokens
✅ **IP Tracking**: Optional IP address tracking for audit trail

### Additional Security Recommendations
- Use HTTPS in production (enforced via Secure flag)
- Implement rate limiting on auth endpoints
- Add email verification for registration
- Implement account lockout after failed login attempts
- Add 2FA/MFA for sensitive operations
- Use a proper secret key management system (not hardcoded)
- Implement token blacklisting for more robust revocation
- Add audit logging for security events

## Database Schema

### Users Table
```sql
CREATE TABLE "Users" (
    "Id" uuid PRIMARY KEY,
    "Username" varchar(100) UNIQUE NOT NULL,
    "Email" varchar(255) UNIQUE NOT NULL,
    "Password_Hash" varchar(max) NOT NULL,
    "FirstName" varchar(50),
    "LastName" varchar(50),
    "is_Active" boolean DEFAULT true,
    "is_email_verified" boolean DEFAULT false,
    "Created_At" timestamp DEFAULT CURRENT_TIMESTAMP,
    "Updated_At" timestamp DEFAULT CURRENT_TIMESTAMP,
    "Last_Login_At" timestamp
);
```

### RefreshTokens Table
```sql
CREATE TABLE "RefreshTokens" (
    "Id" uuid PRIMARY KEY,
    "user_Id" uuid NOT NULL,
    "Token" varchar(max) UNIQUE NOT NULL,
    "Expires_At" timestamp NOT NULL,
    "Created_At" timestamp DEFAULT CURRENT_TIMESTAMP,
    "Revoked_At" timestamp,
    "is_Revoked" boolean DEFAULT false,
    "Created_By_Ip" varchar(50),
    FOREIGN KEY ("user_Id") REFERENCES "Users"("Id") ON DELETE CASCADE
);
```

## Testing

### Register New User
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "Password123!",
    "firstName": "Test",
    "lastName": "User"
  }'
```

### Login
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "emailOrUsername": "testuser",
    "password": "Password123!"
  }'
```

### Access Protected Endpoint
```bash
curl -X GET http://localhost:5000/api/auth/me \
  -H "Authorization: Bearer <access_token>"
```

### Logout
```bash
curl -X POST http://localhost:5000/api/auth/logout \
  -H "Authorization: Bearer <access_token>"
```

## Troubleshooting

### Issue: "Invalid token" when accessing protected endpoints
- Verify the access token is being sent correctly
- Check token hasn't expired (default: 60 minutes)
- Verify JWT_SECRET_KEY matches between token generation and validation

### Issue: "User already exists"
- Username or email already registered
- Check database for existing user
- Use different email/username for registration

### Issue: Cookies not persisting
- Ensure HTTPS is used in production (Secure flag)
- Check browser cookie settings
- Verify SameSite settings match your frontend domain

### Issue: Token refresh returns 401
- Refresh token may have expired (default: 7 days)
- User account may be inactive
- Refresh token may have been revoked
- Token may not exist in database

## Future Enhancements

1. **Email Verification**: Send verification email on registration
2. **Two-Factor Authentication**: Add TOTP or SMS-based 2FA
3. **OAuth Integration**: Support OAuth providers (Google, GitHub, etc.)
4. **Token Blacklisting**: Implement token blacklist for better revocation
5. **Rate Limiting**: Add rate limiting on auth endpoints
6. **Account Lockout**: Implement account lockout after failed attempts
7. **Password Reset**: Add forgot password functionality
8. **Audit Logging**: Comprehensive security event logging
9. **Role-Based Access Control**: Add roles and permissions system
10. **Session Management**: Add session tracking and device management

## References

- [JWT Best Practices](https://tools.ietf.org/html/rfc8949)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
