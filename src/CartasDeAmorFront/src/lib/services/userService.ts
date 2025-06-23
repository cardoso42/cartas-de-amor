import API_CONFIG from '$lib/config/api-config';
import { apiDelete } from '$lib/utils/apiUtils';
import { parseJwt, type JwtPayload } from '$lib/utils/jwtUtils';

export interface UserData {
  username: string;
  email: string;
  joinedDate: string;
}

/**
 * Get the current user's profile from the JWT token
 * Since there's no specific endpoint for getting user profile in the API,
 * we'll extract basic information from the token and return it
 */
export function getCurrentUserProfile(): UserData | null {
  try {
    // Check local storage for the token
    const token = localStorage.getItem('authToken');
    
    if (!token) {
      return null;
    }
    
    // Decode the JWT token
    const decodedToken: JwtPayload | null = parseJwt(token);
    
    if (!decodedToken) {
      return null;
    }
    
    // Extract user data from the decoded token
    // The actual properties will depend on what's included in your JWT
    return {
      username: decodedToken.username || 'Unknown',
      email: decodedToken.userEmail  || decodedToken.sub || 'Unknown',
      joinedDate: decodedToken.iat ? new Date(decodedToken.iat * 1000).toISOString() : new Date().toISOString(),
      // These stats would typically come from a separate API call
    };
  } catch (error) {
    console.error('Error getting current user profile:', error);
    return null;
  }
}

// TODO: Implement proper API endpoint for changing profile data

/**
 * Delete the user's account
 */
export async function deleteAccount(email: string): Promise<{ success: boolean; message: string }> {
  try {
    // Use the delete account endpoint from API config
    await apiDelete(API_CONFIG.auth.deleteAccount(email));
    return {
      success: true,
      message: 'Account deleted successfully'
    };
  } catch (error) {
    console.error('Error deleting account:', error);
    return {
      success: false,
      message: error instanceof Error ? error.message : 'An error occurred deleting your account'
    };
  }
}
