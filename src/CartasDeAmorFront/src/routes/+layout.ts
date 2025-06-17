import auth from '$lib/stores/authStore';
import { redirect } from '@sveltejs/kit';
import type { LayoutLoad } from './$types';

export const load = (({ url }) => {
  let isAuthenticated = false;

  // Subscribe to the auth store to get the current authentication state
  const unsubscribe = auth.subscribe(state => {
    isAuthenticated = state.isAuthenticated;
  });
  unsubscribe();

  // If not authenticated and not on the login page or home page, redirect to login
  if (!isAuthenticated && url.pathname !== '/login' && url.pathname !== '/') {
    throw redirect(302, '/login');
  }

  // If authenticated and on the login page, redirect to home
  if (isAuthenticated && url.pathname === '/login') {
    throw redirect(302, '/');
  }

  return {};
}) satisfies LayoutLoad;
