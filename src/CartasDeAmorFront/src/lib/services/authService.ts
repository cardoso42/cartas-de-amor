import auth from '$lib/stores/authStore';
import API_CONFIG from '$lib/config/api-config';
import { apiPost } from '$lib/utils/apiUtils';

interface LoginResponse {
  token: string;
  success: boolean;
  message?: string;
}

interface RegisterResponse {
  accessToken?: string;
  success: boolean;
  message?: string;
}

interface ApiLoginResponse {
  username: string;
  email: string;
  token: string;
  expiration: string;
  message: string;
}

interface ApiRegisterResponse {
  accessToken: string;
  message: string;
}

export async function login(email: string, password: string): Promise<LoginResponse> {
  try {
    // Using the centralized API configuration and utility function
    const data = await apiPost<ApiLoginResponse>(API_CONFIG.auth.login, { email, password });

    if (data.token) {
      // Store the token in the auth store
      auth.login(data.token);
      return { 
        token: data.token, 
        success: true 
      };
    } else {
      return {
        token: '',
        success: false,
        message: data.message || 'Login failed. Please check your credentials.'
      };
    }
  } catch (error) {
    console.error('Login error:', error);
    return { 
      token: '', 
      success: false,
      message: 'An error occurred during login. Please try again.' 
    };
  }
}

export async function register(username: string, email: string, password: string): Promise<RegisterResponse> {
  try {
    const data = await apiPost<ApiRegisterResponse>(API_CONFIG.auth.register, {
      username,
      email,
      password
    });

    if (data.accessToken) {
      // Store the access token in auth store
      auth.login(data.accessToken);
    }
    
    return {
      accessToken: data.accessToken,
      success: true,
      message: data.message || 'Account created successfully.'
    };
  } catch (error) {
    console.error('Registration error:', error);
    const errorMessage = error instanceof Error ? error.message : 'An error occurred during registration. Please try again.';
    return {
      success: false,
      message: errorMessage
    };
  }
}

export function logout() {
  auth.logout();
}
