import auth from '$lib/stores/authStore';
import { redirect } from '@sveltejs/kit';
import type { PageLoad } from './$types';
import { getGameRooms } from '$lib/services/gameRoomService';

export const load = (async () => {
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

  // Fetch initial list of game rooms (will be refreshed client-side)
  // This helps with SEO and initial loading state
  try {
    const rooms = await getGameRooms();
    return { rooms };
  } catch (error) {
    console.error('Failed to load game rooms:', error);
    return { 
      rooms: [],
      error: 'Failed to load game rooms. Please try again.'
    };
  }
}) satisfies PageLoad;
