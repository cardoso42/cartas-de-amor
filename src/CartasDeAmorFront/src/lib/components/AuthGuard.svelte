<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import { auth } from '$lib/stores/authStore';
  import { goto } from '$app/navigation';
  import { browser } from '$app/environment';
  import { page } from '$app/stores';
  import { REDIRECT_IF_AUTHENTICATED_ROUTES } from '$lib/config/routes-config';

  // Props
  export let requireAuth = true;
  export let redirectTo = requireAuth ? '/login' : '/dashboard';

  let isInitializing = true;
  let isAuthenticated: boolean;
  let unsubscribe: () => void;

  onMount(() => {
    if (browser) {
      // Synchronize with localStorage immediately
      const authState = auth.synchronize();
      
      // Now subscribe to future changes
      unsubscribe = auth.subscribe((state) => {
        isAuthenticated = state.isAuthenticated;
        
        if (!isInitializing) {
          checkAuth();
        }
      });
      
      // Initial check once mounted
      isInitializing = false;
      checkAuth();
    }
  });

  onDestroy(() => {
    if (unsubscribe) unsubscribe();
  });

  function checkAuth() {
    // Get current path
    const currentPath = $page.url.pathname;

    // If the current path is the same as the redirect path, do nothing
    if (redirectTo === currentPath) {
      return;
    }
    
    // Case 1: Page requires auth but user is not authenticated
    if (requireAuth && !isAuthenticated) {
      goto(redirectTo);
      return;
    }
    
    // Case 2: Only specific pages should redirect authenticated users
    // This allows authenticated users to still access most public pages
    if (REDIRECT_IF_AUTHENTICATED_ROUTES.includes(currentPath) && isAuthenticated) {
      goto(redirectTo);
      return;
    }
  }
</script>

<slot></slot>
