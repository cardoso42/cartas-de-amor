<script lang="ts">
  import { onMount } from 'svelte';
  import Navbar from '$lib/components/layout/Navbar.svelte';
  import auth from '$lib/stores/authStore';
  import { user } from '$lib/stores/userStore';
  import settings from '$lib/stores/settingsStore';
  import '$lib/styles/index.css';
  import { waitLocale } from '$lib/i18n'; // Initialize i18n

  // Initialize stores on app start
  onMount(() => {
    // Synchronize all stores from localStorage
    auth.synchronize();
    user.synchronize();
    settings.synchronize();
  });
</script>

{#await waitLocale()}
  <!-- Loading state while i18n is initializing -->
  <div class="loading-container">
    <div class="loading-spinner"></div>
  </div>
{:then}
  <div class="app">
    <Navbar />
    <main class="content">
      <slot />
    </main>
  </div>
{/await}

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

  .loading-container {
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 100vh;
    background-color: #f5f5f5;
  }

  .loading-spinner {
    width: 40px;
    height: 40px;
    border: 4px solid #e0e0e0;
    border-top: 4px solid #9c27b0;
    border-radius: 50%;
    animation: spin 1s linear infinite;
  }

  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }
</style>
