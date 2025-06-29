<!-- LoadingSpinner.svelte -->
<script lang="ts">
  import { _ } from 'svelte-i18n';
  
  export let size: 'small' | 'medium' | 'large' = 'medium';
  export let color: string = 'var(--primary-color)';
  export let text: string = $_('common.loading');
</script>

<div class="loading-spinner-container" class:has-text={!!text}>
  <div 
    class="loading-spinner" 
    class:small={size === 'small'}
    class:medium={size === 'medium'}
    class:large={size === 'large'}
    style="border-color: {color} transparent transparent transparent;"
  ></div>
  {#if text}
    <span class="loading-text">{text}</span>
  {/if}
</div>

<style>
  .loading-spinner-container {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.5rem;
  }

  .loading-spinner {
    border: 3px solid transparent;
    border-radius: 50%;
    animation: spin 1s linear infinite;
  }

  .loading-spinner.small {
    width: 16px;
    height: 16px;
    border-width: 2px;
  }

  .loading-spinner.medium {
    width: 24px;
    height: 24px;
    border-width: 3px;
  }

  .loading-spinner.large {
    width: 32px;
    height: 32px;
    border-width: 4px;
  }

  .loading-text {
    font-size: 0.9rem;
    color: var(--text-muted);
  }

  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }
</style>
