<!-- ShowCardAnimation.svelte -->
<script lang="ts">
  import { onMount, createEventDispatcher } from 'svelte';
  import { getCardName } from '$lib/utils/cardUtils';
  import type { CardType } from '$lib/types/game-types';

  export let targetPlayerName: string = '';
  export let cardType: CardType;
  export let sourcePosition: { x: number; y: number; width?: number; height?: number };
  export let tableCenterPosition: { x: number; y: number };
  export let isVisible: boolean = false;

  const dispatch = createEventDispatcher<{
    animationComplete: null;
  }>();

  let animationContainer: HTMLElement;
  let animationComplete = false;
  let hasStarted = false;

  // Animation phases
  let phase: 'moving' | 'flipping' | 'revealed' | 'returning' = 'moving';
  
  onMount(() => {
    if (isVisible) {
      startAnimation();
    }
  });

  async function startAnimation() {
    if (hasStarted) return; // Prevent duplicate starts
    hasStarted = true;
    
    // Phase 1: Move to center (0.5s)
    phase = 'moving';
    
    await new Promise(resolve => setTimeout(resolve, 500));
    
    // Phase 2: Flip animation (0.5s)
    phase = 'flipping';
    
    await new Promise(resolve => setTimeout(resolve, 500));
    
    // Phase 3: Show revealed card (2s)
    phase = 'revealed';
    
    await new Promise(resolve => setTimeout(resolve, 2000));
    
    // Phase 4: Return to original position (0.8s)
    phase = 'returning';
    
    await new Promise(resolve => setTimeout(resolve, 800));
    
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
      class:flipping={phase === 'flipping'}
      class:revealed={phase === 'revealed'}
      class:returning={phase === 'returning'}
      style="
        --source-x: {sourcePosition.x}px; 
        --source-y: {sourcePosition.y}px; 
        --source-width: {sourcePosition.width || 50}px; 
        --source-height: {sourcePosition.height || 70}px;
        --center-x: {tableCenterPosition.x}px;
        --center-y: {tableCenterPosition.y}px;
"
    >
      <!-- Card back (visible during moving and first half of flipping) -->
      <div class="card-face card-back" class:hidden={phase === 'revealed' || phase === 'returning'}>
        <div class="card-back-pattern"></div>
      </div>
      
      <!-- Card front (visible during second half of flipping and revealed) -->
      <div class="card-face card-front" class:visible={phase === 'revealed' || phase === 'returning'}>
        <div class="card-content">
          <div class="card-number">{getCardName(cardType)}</div>
          <div class="card-name">Card {cardType}</div>
        </div>
      </div>
    </div>

    <!-- Information text -->
    {#if phase === 'revealed'}
      <div class="info-text">
        You saw {targetPlayerName}'s card!
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
    background: rgba(0, 0, 0, 0.3);
    backdrop-filter: blur(2px);
  }

  .animated-card {
    position: absolute;
    width: 120px;
    height: 170px;
    transform-style: preserve-3d;
    transition: none;
  }

  /* Moving phase: card travels from source to center */
  .animated-card.moving {
    left: var(--source-x);
    top: var(--source-y);
    width: var(--source-width);
    height: var(--source-height);
    transform: translate(-50%, -50%);
    animation: moveToCenter 1s ease-out forwards;
  }

  /* Flipping phase: card rotates to reveal front */
  .animated-card.flipping {
    left: var(--center-x);
    top: var(--center-y);
    transform: translate(-50%, -50%);
    animation: flipCard 0.5s ease-in-out forwards;
  }

  /* Revealed phase: card is stable in center */
  .animated-card.revealed {
    left: var(--center-x);
    top: var(--center-y);
    transform: translate(-50%, -50%) scale(1.2);
    animation: pulseGlow 2s ease-in-out infinite;
  }

  /* Returning phase: card moves back to original position */
  .animated-card.returning {
    left: var(--center-x);
    top: var(--center-y);
    transform: translate(-50%, -50%) scale(1.2);
    animation: returnToSource 0.8s ease-in-out forwards;
  }

  .card-face {
    position: absolute;
    width: 100%;
    height: 100%;
    border-radius: 12px;
    border: 3px solid #2c1810;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.6);
    backface-visibility: hidden;
  }

  .card-back {
    background: linear-gradient(135deg, #1565c0 0%, #0d47a1 100%);
    transform: rotateY(0deg);
  }

  .card-back.hidden {
    opacity: 0;
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

  .card-front {
    background: linear-gradient(135deg, #fafafa 0%, #e0e0e0 100%);
    transform: rotateY(180deg);
    opacity: 0;
  }

  .card-front.visible {
    opacity: 1;
    transform: rotateY(0deg);
  }

  .card-content {
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    align-items: center;
    height: 100%;
    padding: 12px 8px;
  }

  .card-number {
    font-size: 2rem;
    font-weight: bold;
    color: #9c27b0;
    text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.3);
  }

  .card-name {
    font-size: 0.9rem;
    font-weight: bold;
    color: #333;
    text-align: center;
    line-height: 1.2;
    text-shadow: 1px 1px 1px rgba(255, 255, 255, 0.8);
  }

  .info-text {
    position: absolute;
    top: 60%;
    left: 50%;
    transform: translateX(-50%);
    background: rgba(0, 0, 0, 0.8);
    color: white;
    padding: 12px 24px;
    border-radius: 8px;
    font-size: 1.1rem;
    font-weight: 600;
    text-align: center;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.4);
    animation: slideInFromBottom 0.3s ease-out;
  }

  /* Animation keyframes */
  @keyframes moveToCenter {
    from {
      left: var(--source-x);
      top: var(--source-y);
      width: var(--source-width);
      height: var(--source-height);
      transform: translate(-50%, -50%);
    }
    to {
      left: var(--center-x);
      top: var(--center-y);
      width: 120px;
      height: 170px;
      transform: translate(-50%, -50%);
    }
  }

  @keyframes flipCard {
    0% {
      transform: translate(-50%, -50%) rotateY(0deg);
    }
    50% {
      transform: translate(-50%, -50%) rotateY(90deg) scale(1.1);
    }
    100% {
      transform: translate(-50%, -50%) rotateY(0deg) scale(1);
    }
  }

  @keyframes pulseGlow {
    0%, 100% {
      box-shadow: 0 0 20px rgba(156, 39, 176, 0.6);
    }
    50% {
      box-shadow: 0 0 40px rgba(156, 39, 176, 0.9);
    }
  }

  @keyframes returnToSource {
    from {
      left: var(--center-x);
      top: var(--center-y);
      width: 120px;
      height: 170px;
      transform: translate(-50%, -50%) scale(1.2);
    }
    to {
      left: var(--source-x);
      top: var(--source-y);
      width: var(--source-width);
      height: var(--source-height);
      transform: translate(-50%, -50%) scale(1);
    }
  }

  @keyframes slideInFromBottom {
    from {
      opacity: 0;
      transform: translateX(-50%) translateY(20px);
    }
    to {
      opacity: 1;
      transform: translateX(-50%) translateY(0);
    }
  }
</style>
