import { browser } from '$app/environment';

export const load = (() => {
  // For server-side rendering (SSR) we need to check if we're in a browser
  if (!browser) {
    return {}; // Return empty object for SSR
  }
  
  return {};
});

