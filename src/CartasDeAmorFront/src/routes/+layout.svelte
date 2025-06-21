<script lang="ts">
  import { onMount } from 'svelte';
  import Navbar from '$lib/components/layout/Navbar.svelte';
  import auth from '$lib/stores/authStore';
  import { user } from '$lib/stores/userStore';
  import '$lib/styles/index.css';

  // Initialize stores on app start
  onMount(() => {
    // Synchronize both stores from localStorage
    auth.synchronize();
    user.synchronize();
  });
</script>

<div class="app">
  <Navbar />
  <main class="content">
    <slot />
  </main>
</div>

<style>
  :global(body) {
    margin: 0;
    padding: 0;
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen,
      Ubuntu, Cantarell, 'Open Sans', 'Helvetica Neue', sans-serif;
    line-height: 1.6;
    color: #333;
    background-color: #f5f5f5;
  }

  :global(*, *::before, *::after) {
    box-sizing: border-box;
  }
  
  .app {
    min-height: 100vh;
    display: flex;
    flex-direction: column;
  }
  
  .content {
    flex: 1;
    padding: 1rem;
    max-width: 1200px;
    width: 100%;
    margin: 0 auto;
  }
</style>
