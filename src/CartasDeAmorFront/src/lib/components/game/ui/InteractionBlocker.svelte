<!-- InteractionBlocker.svelte -->
<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  
  // Props
  export let isBlocking = false;
  export let showOverlay = true;
  export let overlayOpacity = 0.3;
  export let blockPointerEvents = true;
  export let preventScrolling = false;
  export let zIndex = 1500; // Between modals (1000) and animations (2000)
  
  const dispatch = createEventDispatcher<{
    blocked: { event: string };
  }>();
  
  // Handle blocked interactions
  function handleBlockedClick(event: MouseEvent) {
    if (isBlocking) {
      event.preventDefault();
      event.stopPropagation();
      dispatch('blocked', { event: 'click' });
    }
  }
  
  function handleBlockedKeydown(event: KeyboardEvent) {
    if (isBlocking) {
      // Allow essential keys like Escape for accessibility
      if (!['Escape', 'Tab'].includes(event.key)) {
        event.preventDefault();
        event.stopPropagation();
        dispatch('blocked', { event: 'keydown' });
      }
    }
  }
  
  // Prevent scrolling when blocking
  $: if (preventScrolling && isBlocking) {
    document.body.style.overflow = 'hidden';
  } else if (preventScrolling) {
    document.body.style.overflow = '';
  }
</script>

<svelte:window 
  on:keydown={handleBlockedKeydown}
/>

{#if isBlocking}
  <div 
    class="interaction-blocker"
    class:show-overlay={showOverlay}
    class:block-pointer-events={blockPointerEvents}
    style="
      z-index: {zIndex};
      background-color: rgba(0, 0, 0, {showOverlay ? overlayOpacity : 0});
    "
    on:click={handleBlockedClick}
    on:keydown={handleBlockedKeydown}
    role="presentation"
    aria-hidden="true"
  >
    <!-- Optional slot for showing content during blocking -->
    <slot />
  </div>
{/if}

<style>
  .interaction-blocker {
    position: fixed;
    top: 0;
    left: 0;
    width: 100vw;
    height: 100vh;
    transition: opacity 0.3s ease;
  }
  
  .interaction-blocker.block-pointer-events {
    pointer-events: all;
    cursor: not-allowed;
  }
  
  .interaction-blocker:not(.block-pointer-events) {
    pointer-events: none;
  }
  
  /* When showing overlay */
  .interaction-blocker.show-overlay {
    backdrop-filter: blur(1px);
  }
</style>
