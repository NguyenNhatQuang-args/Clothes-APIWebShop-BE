# Frontend Integration Guide - Authentication

## Overview

Hệ thống authentication sử dụng JWT tokens được lưu trữ trong secure HttpOnly cookies. Frontend không cần quản lý tokens thủ công - browser sẽ tự động gửi cookies với mỗi request.

## Setup

### 1. Configure Fetch API

```javascript
// config/api.js
const API_BASE_URL = 'http://localhost:5000/api';

export const apiCall = async (endpoint, options = {}) => {
  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...options,
    credentials: 'include', // IMPORTANT: Auto-send cookies
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
  });

  if (!response.ok) {
    throw new Error(`API Error: ${response.statusText}`);
  }

  return response.json();
};
```

### 2. Configure Axios (Alternative)

```javascript
// config/axiosInstance.js
import axios from 'axios';

const instance = axios.create({
  baseURL: 'http://localhost:5000/api',
  withCredentials: true, // IMPORTANT: Auto-send cookies
  headers: {
    'Content-Type': 'application/json',
  },
});

export default instance;
```

## Authentication Flow

### Step 1: User Registration

```javascript
// pages/Register.jsx
import { useState } from 'react';
import { apiCall } from '../config/api';

export function RegisterPage() {
  const [formData, setFormData] = useState({
    username: '',
    email: '',
    password: '',
    firstName: '',
    lastName: '',
  });
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    try {
      const response = await apiCall('/auth/register', {
        method: 'POST',
        body: JSON.stringify(formData),
      });

      if (response.success) {
        // User registered and logged in automatically
        // Cookies are already set by backend
        window.location.href = '/dashboard';
      } else {
        setError(response.errors?.[0] || response.message);
      }
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <input
        type="text"
        placeholder="Username"
        value={formData.username}
        onChange={(e) => setFormData({...formData, username: e.target.value})}
        required
      />
      <input
        type="email"
        placeholder="Email"
        value={formData.email}
        onChange={(e) => setFormData({...formData, email: e.target.value})}
        required
      />
      <input
        type="password"
        placeholder="Password"
        value={formData.password}
        onChange={(e) => setFormData({...formData, password: e.target.value})}
        required
      />
      <input
        type="text"
        placeholder="First Name"
        value={formData.firstName}
        onChange={(e) => setFormData({...formData, firstName: e.target.value})}
      />
      <input
        type="text"
        placeholder="Last Name"
        value={formData.lastName}
        onChange={(e) => setFormData({...formData, lastName: e.target.value})}
      />
      {error && <div className="error">{error}</div>}
      <button type="submit">Register</button>
    </form>
  );
}
```

### Step 2: User Login

```javascript
// pages/Login.jsx
import { useState } from 'react';
import { apiCall } from '../config/api';

export function LoginPage() {
  const [credentials, setCredentials] = useState({
    emailOrUsername: '',
    password: '',
    rememberMe: false,
  });
  const [error, setError] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');

    try {
      const response = await apiCall('/auth/login', {
        method: 'POST',
        body: JSON.stringify(credentials),
      });

      if (response.success) {
        // Tokens automatically stored in secure cookies
        // Redirect to dashboard
        window.location.href = '/dashboard';
      } else {
        setError(response.message);
      }
    } catch (err) {
      setError(err.message);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <input
        type="text"
        placeholder="Email or Username"
        value={credentials.emailOrUsername}
        onChange={(e) => setCredentials({...credentials, emailOrUsername: e.target.value})}
        required
      />
      <input
        type="password"
        placeholder="Password"
        value={credentials.password}
        onChange={(e) => setCredentials({...credentials, password: e.target.value})}
        required
      />
      <label>
        <input
          type="checkbox"
          checked={credentials.rememberMe}
          onChange={(e) => setCredentials({...credentials, rememberMe: e.target.checked})}
        />
        Remember me
      </label>
      {error && <div className="error">{error}</div>}
      <button type="submit">Login</button>
    </form>
  );
}
```

### Step 3: Protected Routes

```javascript
// components/ProtectedRoute.jsx
import { useEffect, useState } from 'react';
import { apiCall } from '../config/api';

export function ProtectedRoute({ children }) {
  const [isAuthenticated, setIsAuthenticated] = useState(null);
  const [user, setUser] = useState(null);

  useEffect(() => {
    const checkAuth = async () => {
      try {
        const response = await apiCall('/auth/me');
        if (response.success) {
          setUser(response.data);
          setIsAuthenticated(true);
        } else {
          setIsAuthenticated(false);
        }
      } catch (err) {
        setIsAuthenticated(false);
      }
    };

    checkAuth();
  }, []);

  if (isAuthenticated === null) return <div>Loading...</div>;
  if (!isAuthenticated) return <div>Redirecting to login...</div>;

  return children;
}

// Usage
function App() {
  return (
    <ProtectedRoute>
      <Dashboard />
    </ProtectedRoute>
  );
}
```

### Step 4: Get Current User

```javascript
// hooks/useAuth.js
import { useEffect, useState } from 'react';
import { apiCall } from '../config/api';

export function useAuth() {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchUser = async () => {
      try {
        const response = await apiCall('/auth/me');
        if (response.success) {
          setUser(response.data);
        }
      } catch (err) {
        console.error('Failed to fetch user:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchUser();
  }, []);

  return { user, loading };
}

// Usage
function Profile() {
  const { user, loading } = useAuth();

  if (loading) return <div>Loading...</div>;
  if (!user) return <div>Not authenticated</div>;

  return <div>Welcome, {user.username}!</div>;
}
```

### Step 5: Logout

```javascript
// components/LogoutButton.jsx
import { apiCall } from '../config/api';

export function LogoutButton() {
  const handleLogout = async () => {
    try {
      await apiCall('/auth/logout', {
        method: 'POST',
      });
      // Cookies automatically cleared
      window.location.href = '/login';
    } catch (err) {
      console.error('Logout failed:', err);
    }
  };

  return <button onClick={handleLogout}>Logout</button>;
}
```

## Advanced Features

### Change Password

```javascript
// pages/ChangePassword.jsx
async function handleChangePassword(currentPassword, newPassword) {
  const response = await apiCall('/auth/change-password', {
    method: 'POST',
    body: JSON.stringify({
      currentPassword,
      newPassword,
      confirmPassword: newPassword,
    }),
  });

  if (response.success) {
    alert('Password changed successfully');
  } else {
    alert(response.message);
  }
}
```

### Forgot Password

```javascript
// pages/ForgotPassword.jsx
async function handleForgotPassword(email) {
  const response = await apiCall('/auth/forgot-password', {
    method: 'POST',
    body: JSON.stringify({ email }),
  });

  alert(response.message);
  // Check email for reset link
}
```

### Reset Password

```javascript
// pages/ResetPassword.jsx
async function handleResetPassword(email, token, newPassword) {
  const response = await apiCall('/auth/reset-password', {
    method: 'POST',
    body: JSON.stringify({
      email,
      resetToken: token,
      newPassword,
      confirmPassword: newPassword,
    }),
  });

  if (response.success) {
    window.location.href = '/login';
  } else {
    alert(response.message);
  }
}
```

### Update Profile

```javascript
// pages/EditProfile.jsx
async function handleUpdateProfile(username, firstName, lastName) {
  const response = await apiCall('/auth/profile', {
    method: 'PUT',
    body: JSON.stringify({ username, firstName, lastName }),
  });

  if (response.success) {
    setUser(response.data);
    alert('Profile updated successfully');
  } else {
    alert(response.message);
  }
}
```

### Token Refresh (Automatic)

```javascript
// hooks/useTokenRefresh.js
import { useEffect } from 'react';
import { apiCall } from '../config/api';

export function useTokenRefresh() {
  useEffect(() => {
    // Refresh token every 55 minutes (before 60 min expiry)
    const interval = setInterval(async () => {
      try {
        await apiCall('/auth/refresh-token', {
          method: 'POST',
        });
        console.log('Token refreshed');
      } catch (err) {
        console.error('Token refresh failed:', err);
      }
    }, 55 * 60 * 1000);

    return () => clearInterval(interval);
  }, []);
}

// Use in App component
function App() {
  useTokenRefresh(); // Auto-refresh every 55 minutes

  return <Routes>...</Routes>;
}
```

## Error Handling

```javascript
// utils/handleApiError.js
export function handleApiError(error, response) {
  if (response.status === 401) {
    // Unauthorized - redirect to login
    window.location.href = '/login';
    return 'Your session has expired. Please login again.';
  }

  if (response.status === 400) {
    // Validation error
    return response.data.errors?.[0] || response.data.message;
  }

  if (response.status === 403) {
    // Forbidden
    return 'You do not have permission to perform this action.';
  }

  if (response.status === 404) {
    // Not found
    return 'The requested resource was not found.';
  }

  if (response.status === 500) {
    // Server error
    return 'An error occurred. Please try again later.';
  }

  return error.message || 'An unexpected error occurred.';
}
```

## State Management (Zustand Example)

```javascript
// store/authStore.js
import { create } from 'zustand';
import { apiCall } from '../config/api';

export const useAuthStore = create((set) => ({
  user: null,
  isAuthenticated: false,
  loading: true,

  // Fetch current user
  fetchUser: async () => {
    try {
      const response = await apiCall('/auth/me');
      if (response.success) {
        set({ user: response.data, isAuthenticated: true, loading: false });
      } else {
        set({ isAuthenticated: false, loading: false });
      }
    } catch (err) {
      set({ isAuthenticated: false, loading: false });
    }
  },

  // Login
  login: async (emailOrUsername, password) => {
    const response = await apiCall('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ emailOrUsername, password }),
    });

    if (response.success) {
      set({ user: response.data.user, isAuthenticated: true });
    }
    return response;
  },

  // Register
  register: async (formData) => {
    const response = await apiCall('/auth/register', {
      method: 'POST',
      body: JSON.stringify(formData),
    });

    if (response.success) {
      set({ user: response.data.user, isAuthenticated: true });
    }
    return response;
  },

  // Logout
  logout: async () => {
    await apiCall('/auth/logout', { method: 'POST' });
    set({ user: null, isAuthenticated: false });
  },
}));

// Usage in components
function Dashboard() {
  const { user, isAuthenticated } = useAuthStore();

  if (!isAuthenticated) return <Redirect to="/login" />;

  return <div>Welcome, {user.username}!</div>;
}
```

## Important Notes

1. **Do NOT manually manage tokens** - Let the browser handle cookies automatically
2. **Always use `credentials: 'include'`** with fetch or `withCredentials: true` with axios
3. **HttpOnly cookies** cannot be accessed by JavaScript - this is for security
4. **The server automatically validates** tokens on protected endpoints
5. **Logout clears cookies** - No manual token cleanup needed
6. **Token refresh** is automatic via the refresh-token endpoint
7. **HTTPS required** in production - Cookies marked with Secure flag

## Troubleshooting

### Cookies not being sent
- Make sure `credentials: 'include'` or `withCredentials: true` is set
- Check CORS configuration allows credentials
- Verify browser cookie settings

### 401 Unauthorized errors
- Session may have expired - User should login again
- Token may have been revoked
- Browser cookies may have been cleared

### CORS errors
- Ensure frontend origin is in `Cors:AllowedOrigins` in appsettings.json
- Development mode allows `AllowLocalhost` policy

### Token expired
- The frontend should automatically handle refresh
- Or user should logout and login again
