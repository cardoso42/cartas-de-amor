/**
 * Interface for JWT payload with common fields
 */
export interface JwtPayload {
  // Standard JWT claims
  iss?: string;        // Issuer
  sub?: string;        // Subject (usually user ID)
  aud?: string | string[];  // Audience
  exp?: number;        // Expiration time (in seconds since Unix epoch)
  nbf?: number;        // Not valid before
  iat?: number;        // Issued at (in seconds since Unix epoch)
  jti?: string;        // JWT ID
  
  // Custom claims commonly used in auth tokens
  name?: string;       // User's full name
  email?: string;      // User's email
  roles?: string[];    // User's roles
  permissions?: string[]; // User's permissions
  
  // Additional claims can be accessed via indexing
  [key: string]: unknown;
}

/**
 * Parse a JWT token and return the payload
 * @param token JWT token
 * @returns Decoded token payload or null if invalid
 */
export function parseJwt(token: string): JwtPayload | null {
  try {
    // Check if token is valid
    if (!token || typeof token !== 'string' || token.split('.').length !== 3) {
      return null;
    }
    
    // Split the token and get the payload (second part)
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );

    return JSON.parse(jsonPayload);
  } catch (error) {
    console.error('Error parsing JWT token:', error);
    return null;
  }
}

/**
 * Check if a token is expired
 * @param token JWT token
 * @returns true if expired, false otherwise
 */
export function isTokenExpired(token: string): boolean {
  try {
    const decodedToken = parseJwt(token);
    
    if (!decodedToken || !decodedToken.exp) {
      return true;
    }
    
    // exp is in seconds, Date.now() is in milliseconds
    const currentTime = Date.now() / 1000;
    return decodedToken.exp < currentTime;
  } catch (error) {
    console.error('Error checking token expiration:', error);
    return true;
  }
}
