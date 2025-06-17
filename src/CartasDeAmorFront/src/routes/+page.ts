import { redirect } from '@sveltejs/kit';

// We only need to handle server-side redirect for the root path
// Client-side redirects will be handled by the page component

export const load = (() => {
  // For root path, redirect to the welcome page
  // This helps SSR to have a default route
  throw redirect(302, '/welcome');
});
