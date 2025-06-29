<!-- DrawCardAnimation.svelte -->
<script lang="ts">
  import { onMount, createEventDispatcher } from 'svelte';
  import { _ } from 'svelte-i18n';

  export let playerName: string = '';
  export let deckPosition: { x: number; y: number; width?: number; height?: number };
  export let playerPosition: { x: number; y: number; width?: number; height?: number };
  export let isVisible: boolean = false;

  const dispatch = createEventDispatcher<{
    animationComplete: null;
  }>();

  let animationContainer: HTMLElement;
  let animationComplete = false;
  let hasStarted = false;

  // Animation phases
  let phase: 'moving' | 'arriving' = 'moving';
  
  onMount(() => {
    if (isVisible) {
      startAnimation();
    }
  });

  async function startAnimation() {
    if (hasStarted) return; // Prevent duplicate starts
    hasStarted = true;
    
    // Phase 1: Move card from deck to player (0.5s)
    phase = 'moving';
    await new Promise(resolve => setTimeout(resolve, 500));
    
    // Phase 2: Card arrives and settles (0.2s)
    phase = 'arriving';
    await new Promise(resolve => setTimeout(resolve, 200));
    
    // Mark animation as complete and dispatch event
    if (!animationComplete) {
      animationComplete = true;
      dispatch('animationComplete');
    }
  }

  $: if (isVisible && !animationComplete && !hasStarted) {
    startAnimation();
  }
</script>

{#if isVisible && !animationComplete}
  <div class="animation-overlay" bind:this={animationContainer}>
    <!-- Animated card -->
    <div 
      class="animated-card" 
      class:moving={phase === 'moving'}
      class:arriving={phase === 'arriving'}
      style="
        --deck-x: {deckPosition.x}px; 
        --deck-y: {deckPosition.y}px; 
        --deck-width: {deckPosition.width || 70}px; 
        --deck-height: {deckPosition.height || 98}px;
        --player-x: {playerPosition.x}px; 
        --player-y: {playerPosition.y}px; 
        --player-width: {playerPosition.width || 65}px; 
        --player-height: {playerPosition.height || 90}px;
      "
    >
      <!-- Card back (since player doesn't know what card they drew yet) -->
      <div class="card-face">
        <div class="card-back-pattern"></div>
      </div>
    </div>

    <!-- Player name and action text -->
    {#if phase === 'arriving'}
      <div class="action-text">
        <div class="player-action">{$_('game.cardDrawn', { values: { playerName } })}</div>
      </div>
    {/if}
  </div>
{/if}

<style>
  .animation-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100vw;
    height: 100vh;
    pointer-events: none;
    z-index: 1000;
    background: rgba(0, 0, 0, 0.1);
    backdrop-filter: blur(1px);
  }

  .animated-card {
    position: absolute;
    transform-style: preserve-3d;
    transition: none;
  }

  /* Moving phase: card travels from deck to player */
  .animated-card.moving {
    left: var(--deck-x);
    top: var(--deck-y);
    width: var(--deck-width);
    height: var(--deck-height);
    transform: translate(-50%, -50%);
    animation: moveToPlayer 0.5s cubic-bezier(0.25, 0.46, 0.45, 0.94) forwards;
  }

  /* Arriving phase: card slightly grows and settles */
  .animated-card.arriving {
    left: var(--player-x);
    top: var(--player-y);
    width: var(--player-width);
    height: var(--player-height);
    transform: translate(-50%, -50%);
    animation: arriveAtPlayer 0.2s ease-out forwards;
  }

  .card-face {
    position: relative;
    width: 100%;
    height: 100%;
    background: linear-gradient(135deg, #1565c0 0%, #0d47a1 100%);
    border-radius: 12px;
    border: 3px solid #0d47a1;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.6);
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    overflow: hidden;
  }

  .card-back-pattern {
    width: 100%;
    height: 100%;
    background-image: 
      radial-gradient(circle at 25% 25%, rgba(255, 255, 255, 0.2) 20%, transparent 21%),
      radial-gradient(circle at 75% 75%, rgba(255, 255, 255, 0.2) 20%, transparent 21%);
    background-size: 30px 30px;
    border-radius: 9px;
  }

  .action-text {
    position: absolute;
    top: 25%;
    left: 50%;
    transform: translateX(-50%);
    text-align: center;
    color: white;
    z-index: 1001;
    animation: textSlideIn 0.3s ease-out;
  }

  .player-action {
    font-size: 1.2rem;
    font-weight: 600;
    text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.8);
    background: rgba(0, 0, 0, 0.6);
    padding: 0.5rem 1rem;
    border-radius: 8px;
    border: 1px solid rgba(255, 255, 255, 0.2);
  }

  /* Animation keyframes */
  @keyframes moveToPlayer {
    from {
      left: var(--deck-x);
      top: var(--deck-y);
      width: var(--deck-width);
      height: var(--deck-height);
      transform: translate(-50%, -50%) rotate(0deg);
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.6);
    }
    to {
      left: var(--player-x);
      top: var(--player-y);
      width: var(--player-width);
      height: var(--player-height);
      transform: translate(-50%, -50%) rotate(5deg);
      box-shadow: 0 8px 20px rgba(0, 0, 0, 0.8);
    }
  }

  @keyframes arriveAtPlayer {
    0% {
      transform: translate(-50%, -50%) rotate(5deg) scale(1);
    }
    50% {
      transform: translate(-50%, -50%) rotate(0deg) scale(1.1);
      box-shadow: 0 0 20px rgba(21, 101, 192, 0.8);
    }
    100% {
      transform: translate(-50%, -50%) rotate(0deg) scale(1);
      box-shadow: 0 4px 15px rgba(0, 0, 0, 0.6);
      opacity: 0;
    }
  }

  @keyframes textSlideIn {
    from {
      opacity: 0;
      transform: translateX(-50%) translateY(-20px);
    }
    to {
      opacity: 1;
      transform: translateX(-50%) translateY(0);
    }
  }
</style>
