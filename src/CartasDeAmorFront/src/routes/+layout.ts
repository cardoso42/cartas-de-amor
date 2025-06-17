import auth from '$lib/stores/authStore';
import { redirect } from '@sveltejs/kit';
import type { LayoutLoad } from './$types';
import { PUBLIC_ROUTES, REDIRECT_IF_AUTHENTICATED_ROUTES } from '$lib/config/routes-config';

export const load = (({ url }) => {
  let isAuthenticated = false;

  // Subscribe to the auth store to get the current authentication state
  const unsubscribe = auth.subscribe(state => {
    isAuthenticated = state.isAuthenticated;
  });
  unsubscribe();

  // If not authenticated and trying to access a protected route, redirect to login
  if (!isAuthenticated && !PUBLIC_ROUTES.includes(url.pathname)) {
    throw redirect(302, '/login');
  }

  // If authenticated and on a page that should redirect authenticated users, redirect to dashboard
  // Only specific pages (like login) should redirect authenticated users
  if (isAuthenticated && REDIRECT_IF_AUTHENTICATED_ROUTES.includes(url.pathname)) {
    throw redirect(302, '/dashboard');
  }

  return {};
}) satisfies LayoutLoad;
