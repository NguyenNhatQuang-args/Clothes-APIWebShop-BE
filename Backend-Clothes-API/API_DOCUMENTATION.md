# API Endpoints Documentation

## Authentication Endpoints

### 1. Register User
**Endpoint:** `POST /api/auth/register`
**Authentication:** None
**Status Code:** 200 (Success), 400 (Bad Request)

**Request:**
```json
{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64_encoded_refresh_token",
    "expiresAt": "2024-01-20T15:30:00Z",
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "username": "john_doe",
      "email": "john@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "isActive": true,
      "isEmailVerified": false,
      "createdAt": "2024-01-19T15:30:00Z",
      "lastLogin": null
    }
  },
  "errors": null
}
```

**Response (Error):**
```json
{
  "success": false,
  "message": "Registration failed",
  "data": null,
  "errors": ["Email or username already exists"]
}
```

---

### 2. Login User
**Endpoint:** `POST /api/auth/login`
**Authentication:** None
**Status Code:** 200 (Success), 401 (Unauthorized), 400 (Bad Request)

**Request:**
```json
{
  "emailOrUsername": "john_doe",
  "password": "SecurePassword123!",
  "rememberMe": false
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64_encoded_refresh_token",
    "expiresAt": "2024-01-19T16:30:00Z",
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "username": "john_doe",
      "email": "john@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "isActive": true,
      "isEmailVerified": false,
      "createdAt": "2024-01-19T15:30:00Z",
      "lastLogin": "2024-01-19T16:00:00Z"
    }
  }
}
```

**Cookies Set:**
- `AccessToken` (HttpOnly, Secure, Expires: 60 minutes)
- `RefreshToken` (HttpOnly, Secure, Expires: 7 days)

---

### 3. Refresh Token
**Endpoint:** `POST /api/auth/refresh-token`
**Authentication:** None (Uses RefreshToken cookie)
**Status Code:** 200 (Success), 401 (Unauthorized)

**Request:**
```
Cookies: RefreshToken=base64_encoded_refresh_token
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Token refreshed successfully",
  "data": {
    "accessToken": "new_access_token...",
    "refreshToken": "new_refresh_token...",
    "expiresAt": "2024-01-19T17:30:00Z",
    "user": {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "username": "john_doe",
      "email": "john@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "isActive": true,
      "isEmailVerified": false,
      "createdAt": "2024-01-19T15:30:00Z",
      "lastLogin": "2024-01-19T16:00:00Z"
    }
  }
}
```

---

### 4. Get Current User (Protected)
**Endpoint:** `GET /api/auth/me`
**Authentication:** Bearer Token (JWT) or Cookie
**Status Code:** 200 (Success), 401 (Unauthorized)

**Request Headers:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response (Success):**
```json
{
  "success": true,
  "message": "User retrieved successfully",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "username": "john_doe",
    "email": "john@example.com"
  }
}
```

---

### 5. Change Password (Protected)
**Endpoint:** `POST /api/auth/change-password`
**Authentication:** Bearer Token (Required)
**Status Code:** 200 (Success), 400 (Bad Request), 401 (Unauthorized)

**Request:**
```json
{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewPassword456!",
  "confirmPassword": "NewPassword456!"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Password changed successfully",
  "data": true
}
```

---

### 6. Forgot Password
**Endpoint:** `POST /api/auth/forgot-password`
**Authentication:** None
**Status Code:** 200 (Always success for security)

**Request:**
```json
{
  "email": "john@example.com"
}
```

**Response:**
```json
{
  "success": true,
  "message": "If the email exists, a password reset link has been sent.",
  "data": true
}
```

---

### 7. Verify Reset Token
**Endpoint:** `POST /api/auth/verify-reset-token`
**Authentication:** None
**Status Code:** 200 (Success), 400 (Bad Request)

**Request:**
```json
{
  "email": "john@example.com",
  "token": "reset_token_from_email"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Reset token is valid",
  "data": true
}
```

---

### 8. Reset Password
**Endpoint:** `POST /api/auth/reset-password`
**Authentication:** None
**Status Code:** 200 (Success), 400 (Bad Request)

**Request:**
```json
{
  "email": "john@example.com",
  "resetToken": "reset_token_from_email",
  "newPassword": "NewPassword456!",
  "confirmPassword": "NewPassword456!"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Password reset successfully",
  "data": true
}
```

---

### 9. Update Profile (Protected)
**Endpoint:** `PUT /api/auth/profile`
**Authentication:** Bearer Token (Required)
**Status Code:** 200 (Success), 400 (Bad Request), 401 (Unauthorized)

**Request:**
```json
{
  "username": "john_doe_updated",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Profile updated successfully",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "username": "john_doe_updated",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "isActive": true,
    "isEmailVerified": false,
    "createdAt": "2024-01-19T15:30:00Z",
    "lastLogin": "2024-01-19T16:00:00Z"
  }
}
```

---

### 10. Deactivate Account (Protected)
**Endpoint:** `POST /api/auth/deactivate-account`
**Authentication:** Bearer Token (Required)
**Status Code:** 200 (Success), 401 (Unauthorized)

**Request:**
```
No body required
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Account deactivated successfully",
  "data": true
}
```

---

### 11. Logout (Protected)
**Endpoint:** `POST /api/auth/logout`
**Authentication:** Bearer Token (Required)
**Status Code:** 200 (Success), 401 (Unauthorized)

**Request:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Logout successful",
  "data": true
}
```

**Cookies Cleared:**
- `AccessToken` - Deleted
- `RefreshToken` - Deleted

---

## Error Responses

### Validation Error (400)
```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    "Username is required.",
    "Email is required.",
    "Password must be between 6 and 100 characters."
  ]
}
```

### Unauthorized (401)
```json
{
  "success": false,
  "message": "Unauthorized",
  "data": null,
  "errors": ["Missing or invalid authorization token"]
}
```

### Not Found (404)
```json
{
  "success": false,
  "message": "Not Found",
  "data": null,
  "errors": ["User not found"]
}
```

### Server Error (500)
```json
{
  "success": false,
  "message": "Internal Server Error",
  "data": null,
  "errors": ["An unexpected error occurred. Please try again later."]
}
```

---

## Authentication Methods

### 1. JWT Token (Bearer)
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 2. HttpOnly Cookie (Automatic)
Tokens are automatically sent in secure HttpOnly cookies by the browser.

---

## Rate Limiting

- No rate limiting currently implemented
- Recommended: Implement rate limiting on auth endpoints
  - Max 5 failed login attempts per IP per hour
  - Max 10 registration attempts per IP per day

---

## CORS Configuration

**Allowed Origins (Development):**
- `http://localhost:3000`
- `http://localhost:5173`
- Any origin (`*`)

**Allowed Methods:**
- GET, POST, PUT, DELETE, OPTIONS

**Allowed Headers:**
- Content-Type
- Authorization

**Credentials:** 
- Included for cookie-based auth

---

## Token Claims

### Access Token (JWT)
```json
{
  "sub": "user_id",
  "name": "username",
  "email": "user@example.com",
  "jti": "unique_jwt_id",
  "iat": 1234567890,
  "iss": "RoxiosAPI",
  "aud": "RoxiosClient",
  "exp": 1234571490
}
```

### Refresh Token
- Random Base64 encoded 32-byte value
- Stored in database with expiration
- Can be revoked

---

## Usage Examples

### cURL

#### Register
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "john_doe",
    "email": "john@example.com",
    "password": "SecurePassword123!",
    "firstName": "John",
    "lastName": "Doe"
  }'
```

#### Login
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -c cookies.txt \
  -d '{
    "emailOrUsername": "john_doe",
    "password": "SecurePassword123!"
  }'
```

#### Access Protected Endpoint
```bash
curl -X GET http://localhost:5000/api/auth/me \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -b cookies.txt
```

#### Logout
```bash
curl -X POST http://localhost:5000/api/auth/logout \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -b cookies.txt
```

### JavaScript (Fetch API)

#### Register
```javascript
const response = await fetch('http://localhost:5000/api/auth/register', {
  method: 'POST',
  credentials: 'include',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    username: 'john_doe',
    email: 'john@example.com',
    password: 'SecurePassword123!',
    firstName: 'John',
    lastName: 'Doe'
  })
});
const data = await response.json();
```

#### Login
```javascript
const response = await fetch('http://localhost:5000/api/auth/login', {
  method: 'POST',
  credentials: 'include', // Important for cookies
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    emailOrUsername: 'john_doe',
    password: 'SecurePassword123!'
  })
});
const data = await response.json();
// Cookies are automatically stored by browser
```

#### Access Protected Endpoint
```javascript
const response = await fetch('http://localhost:5000/api/auth/me', {
  method: 'GET',
  credentials: 'include', // Cookies sent automatically
  headers: {
    'Authorization': `Bearer ${accessToken}`
  }
});
const user = await response.json();
```

#### Logout
```javascript
const response = await fetch('http://localhost:5000/api/auth/logout', {
  method: 'POST',
  credentials: 'include',
  headers: {
    'Authorization': `Bearer ${accessToken}`
  }
});
const result = await response.json();
// Cookies are automatically deleted by server
```

---

## Implementation Notes

1. **Cookies are automatically set/sent** by the browser - no manual token management needed
2. **HttpOnly flag** prevents JavaScript access - protects against XSS
3. **Secure flag** ensures HTTPS usage in production
4. **SameSite=Strict** provides CSRF protection
5. **Token refresh** automatically updates both access and refresh tokens
6. **Deactivate account** revokes all tokens for security
