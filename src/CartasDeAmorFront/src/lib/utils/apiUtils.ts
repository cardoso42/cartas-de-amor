// apiUtils.ts - Utility functions for API calls
import auth from '$lib/stores/authStore';
import { get } from 'svelte/store';

// Define types more explicitly
interface RequestOptions extends RequestInit {
  headers?: Record<string, string>;
}

/**
 * Makes an authenticated API request
 * @param url API endpoint URL
 * @param options Request options
 * @returns Response data
 */
export async function apiRequest<T>(
  url: string, 
  options: RequestOptions = {}
): Promise<T> {
  // Get token from auth store
  const token = get(auth).token;
  
  // Set up default headers
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers || {})
  };
  
  // Add auth token if available
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }
  
  try {
    const response = await fetch(url, {
      ...options,
      headers,
    });
    
    const data = await response.json();
    
    if (!response.ok) {
      throw new Error(data.message || `API request failed with status ${response.status}`);
    }
    
    return data as T;
  } catch (error) {
    console.error('API request error:', error);
    throw error;
  }
}

/**
 * Makes a GET request to the API
 */
export function apiGet<T>(url: string, options: RequestOptions = {}): Promise<T> {
  return apiRequest<T>(url, { ...options, method: 'GET' });
}

/**
 * Makes a POST request to the API
 */
export function apiPost<T>(url: string, data: unknown, options: RequestOptions = {}): Promise<T> {
  return apiRequest<T>(url, {
    ...options,
    method: 'POST',
    body: JSON.stringify(data),
  });
}

/**
 * Makes a PUT request to the API
 */
export function apiPut<T>(url: string, data: unknown, options: RequestOptions = {}): Promise<T> {
  return apiRequest<T>(url, {
    ...options,
    method: 'PUT',
    body: JSON.stringify(data),
  });
}

/**
 * Makes a DELETE request to the API
 */
export function apiDelete<T>(url: string, options: RequestOptions = {}): Promise<T> {
  return apiRequest<T>(url, { ...options, method: 'DELETE' });
}
