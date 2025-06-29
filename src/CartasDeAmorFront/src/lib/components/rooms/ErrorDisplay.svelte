<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  import { _ } from 'svelte-i18n';
  
  export let message: string = '';
  
  const dispatch = createEventDispatcher<{
    retry: void
  }>();
</script>

{#if message}
  <div class="error-message">
    <p>{message}</p>
    <button class="danger small" on:click={() => dispatch('retry')}>{$_('common.retry')}</button>
  </div>
{/if}

<style>
  .error-message {
    background-color: #ffebee;
    color: #c62828;
    padding: 1rem;
    border-radius: 4px;
    margin-bottom: 1rem;
    display: flex;
    justify-content: space-between;
    align-items: center;
  }
  
  .error-message p {
    margin: 0;
  }
  
  /* Button styles now come from global buttons.css */
</style>
