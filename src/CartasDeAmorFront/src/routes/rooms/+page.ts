import auth from '$lib/stores/authStore';
import { redirect } from '@sveltejs/kit';
import type { PageLoad } from './$types';

export const load = (() => {
  // Check authentication status
  let isAuthenticated = false;

  // Subscribe to the auth store to get the current authentication state
  const unsubscribe = auth.subscribe(state => {
    isAuthenticated = state.isAuthenticated;
  });
  unsubscribe();

  // If not authenticated, redirect to login
  if (!isAuthenticated) {
    throw redirect(302, '/login');
  }

  return {};
}) satisfies PageLoad;
