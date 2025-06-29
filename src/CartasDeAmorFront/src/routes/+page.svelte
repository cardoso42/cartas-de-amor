<script lang="ts">
  import { auth } from '$lib/stores/authStore';
  import { goto } from '$app/navigation';
  import { onMount } from 'svelte';
  import { browser } from '$app/environment';
  import { _ } from 'svelte-i18n';

  let isLoading = true;

  onMount(() => {
    // Only execute in browser
    if (!browser) return;
    
    // Synchronize auth state with localStorage and redirect
    const isAuthenticated = auth.synchronize();
    
    if (isAuthenticated) {
      goto('/dashboard');
    } else {
      goto('/welcome');
    }
    isLoading = false;
  });
</script>

<svelte:head>
  <title>{$_('app.name')} | {$_('common.loading')}...</title>
</svelte:head>

{#if isLoading}
  <div class="loading">
    <p>{$_('common.loading')}...</p>
  </div>
{:else}
  <div class="loading">
    <p>{$_('common.loading')}...</p>
  </div>
{/if}

<style>
  .loading {
    display: flex;
    justify-content: center;
    align-items: center;
    height: 100vh;
    font-size: 1.2rem;
    color: #666;
  }
  
  /* Add a simple loading animation */
  .loading p {
    animation: pulse 1.5s infinite;
  }
  
  @keyframes pulse {
    0% { opacity: 0.5; }
    50% { opacity: 1; }
    100% { opacity: 0.5; }
  }
</style>
