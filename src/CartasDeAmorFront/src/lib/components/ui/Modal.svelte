<!-- Modal.svelte - Reusable modal component -->
<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  import { fly, fade } from 'svelte/transition';
  import { _ } from 'svelte-i18n';
  
  // Props
  export let isOpen = false;
  export let title = '';
  export let size: 'sm' | 'md' | 'lg' | 'xl' = 'md';
  export let closable = true;
  export let backdrop = true; // Allow closing by clicking backdrop
  export let theme: 'default' | 'game' = 'default';
  
  const dispatch = createEventDispatcher<{
    close: void;
    open: void;
  }>();
  
  function close() {
    if (closable) {
      isOpen = false;
      dispatch('close');
    }
  }
  
  function handleBackdropClick(event: MouseEvent) {
    if (backdrop && event.target === event.currentTarget) {
      close();
    }
  }
  
  function handleKeydown(event: KeyboardEvent) {
    if (event.key === 'Escape' && closable) {
      close();
    }
  }
  
  $: if (isOpen) {
    dispatch('open');
  }
  
  $: modalClasses = [
    'modal-content',
    `modal-${size}`,
    theme === 'game' && 'game-theme'
  ].filter(Boolean).join(' ');
</script>

<svelte:window on:keydown={handleKeydown} />

{#if isOpen}
  <div 
    class="modal-backdrop" 
    on:click={handleBackdropClick}
    on:keydown={handleKeydown}
    role="presentation"
    transition:fade={{ duration: 200 }}
  >
    <div 
      class={modalClasses}
      on:click|stopPropagation
      on:keydown|stopPropagation
      role="dialog"
      aria-modal="true"
      aria-labelledby={title ? 'modal-title' : undefined}
      tabindex="-1"
      transition:fly={{ y: -50, duration: 300 }}
    >
      {#if title || closable}
        <div class="modal-header">
          {#if title}
            <h2 id="modal-title" class="modal-title">{title}</h2>
          {/if}
          {#if closable}
            <button 
              class="close-button" 
              on:click={close}
              aria-label={$_('game.closeModal')}
              type="button"
            >
              ×
            </button>
          {/if}
        </div>
      {/if}
      
      <div class="modal-body">
        <slot />
      </div>
      
      <slot name="footer">
        <!-- Footer slot for actions -->
      </slot>
    </div>
  </div>
{/if}

<style>
  .modal-backdrop {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.7);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: var(--z-index-modal);
    backdrop-filter: blur(2px);
    padding: 1rem;
  }
  
  .modal-content {
    background: white;
    border-radius: var(--border-radius-lg);
    width: 100%;
    max-height: 90vh;
    overflow-y: auto;
    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
    position: relative;
  }
  
  /* Modal sizes */
  .modal-sm {
    max-width: 400px;
  }
  
  .modal-md {
    max-width: 600px;
  }
  
  .modal-lg {
    max-width: 800px;
  }
  
  .modal-xl {
    max-width: 1200px;
  }
  
  .modal-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1.5rem;
    border-bottom: 1px solid #e0e0e0;
  }
  
  .modal-title {
    margin: 0;
    color: var(--text-dark);
    font-size: 1.5rem;
    font-weight: 600;
  }
  
  .close-button {
    background: none;
    border: none;
    font-size: 1.5rem;
    color: #666;
    cursor: pointer;
    padding: 0.5rem;
    width: 2rem;
    height: 2rem;
    display: flex;
    justify-content: center;
    align-items: center;
    border-radius: 50%;
    transition: background-color var(--transition-speed) ease;
  }
  
  .close-button:hover {
    background-color: #f0f0f0;
  }
  
  .close-button:focus {
    outline: 2px solid var(--primary-color);
    outline-offset: 2px;
  }
  
  .modal-body {
    padding: 1.5rem;
  }
  
  /* Game theme */
  .modal-content.game-theme {
    background: linear-gradient(135deg, #2c1810 0%, #3e2723 100%);
    border: 3px solid #8b4513;
    color: white;
  }
  
  .modal-content.game-theme .modal-header {
    border-bottom-color: #8b4513;
  }
  
  .modal-content.game-theme .modal-title {
    color: #ffd700;
  }
  
  .modal-content.game-theme .close-button {
    color: white;
  }
  
  .modal-content.game-theme .close-button:hover {
    background: rgba(255, 255, 255, 0.1);
  }
  
  /* Mobile responsiveness */
  @media (max-width: 768px) {
    .modal-backdrop {
      padding: 0.5rem;
    }
    
    .modal-content {
      margin: 0;
      width: 100%;
      max-height: 95vh;
    }
    
    .modal-header {
      padding: 1rem;
    }
    
    .modal-body {
      padding: 1rem;
    }
    
    .modal-title {
      font-size: 1.25rem;
    }
  }
</style>
