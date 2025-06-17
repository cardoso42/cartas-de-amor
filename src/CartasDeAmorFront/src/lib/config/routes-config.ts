// Define routes configuration for the application

// Routes that do not require authentication
export const PUBLIC_ROUTES = [
  '/login',
  '/',
  '/welcome'
];

// Routes that should redirect to dashboard if already logged in
// Only login should redirect authenticated users, allowing them to still visit public pages
export const REDIRECT_IF_AUTHENTICATED_ROUTES = [
  '/login',
];
