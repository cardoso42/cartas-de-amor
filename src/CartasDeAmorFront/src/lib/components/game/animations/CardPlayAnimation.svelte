<!-- CardPlayAnimation.svelte -->
<script lang="ts">
  import { onMount, createEventDispatcher } from 'svelte';
  import { getCardName } from '$lib/utils/cardUtils';
  import type { CardType } from '$lib/types/game-types';

  export let playerName: string = '';
  export let cardType: CardType;
  export let sourcePosition: { x: number; y: number; width?: number; height?: number };
  export let playedCardsPosition: { x: number; y: number; width?: number; height?: number };
  export let isVisible: boolean = false;

  const dispatch = createEventDispatcher<{
    animationComplete: null;
  }>();

  let animationContainer: HTMLElement;
  let animationComplete = false;
  let hasStarted = false;

  // Animation phases
  let phase: 'extracting' | 'growing' | 'displaying' | 'moving' | 'fading' = 'extracting';
  
  onMount(() => {
    if (isVisible) {
      startAnimation();
    }
  });

  async function startAnimation() {
    if (hasStarted) return; // Prevent duplicate starts
    hasStarted = true;
    
    // Phase 1: Extract card from hand and move to center (0.8s)
    phase = 'extracting';
    await new Promise(resolve => setTimeout(resolve, 800));
    
    // Phase 2: Grow the card larger in center (0.4s)
    phase = 'growing';
    await new Promise(resolve => setTimeout(resolve, 400));
    
    // Phase 3: Display card prominently (1.5s)
    phase = 'displaying';
    await new Promise(resolve => setTimeout(resolve, 1500));
    
    // Phase 4: Move to played cards area (0.8s)
    phase = 'moving';
    await new Promise(resolve => setTimeout(resolve, 800));
    
    // Phase 5: Fade out as it settles into played cards (0.3s)
    phase = 'fading';
    await new Promise(resolve => setTimeout(resolve, 300));
    
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
      class:extracting={phase === 'extracting'}
      class:growing={phase === 'growing'}
      class:displaying={phase === 'displaying'}
      class:moving={phase === 'moving'}
      class:fading={phase === 'fading'}
      style="
        --source-x: {sourcePosition.x}px; 
        --source-y: {sourcePosition.y}px; 
        --source-width: {sourcePosition.width || 50}px; 
        --source-height: {sourcePosition.height || 70}px;
        --played-x: {playedCardsPosition.x}px; 
        --played-y: {playedCardsPosition.y}px; 
        --played-width: {playedCardsPosition.width || 32}px; 
        --played-height: {playedCardsPosition.height || 45}px;
      "
    >
      <!-- Card face -->
      <div class="card-face">
        <div class="card-content">
          <div class="card-number">{cardType}</div>
          <div class="card-name">{getCardName(cardType)}</div>
        </div>
        
        <!-- Glowing effect during display phase -->
        {#if phase === 'displaying'}
          <div class="card-glow"></div>
        {/if}
      </div>
    </div>

    <!-- Player name and action text -->
    {#if phase === 'displaying'}
      <div class="action-text">
        <div class="player-action">{playerName} played</div>
        <div class="card-title-display">{getCardName(cardType)}</div>
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
    transform-style: preserve-3d;
    transition: none;
  }

  /* Extracting phase: card travels from source position to center */
  .animated-card.extracting {
    left: var(--source-x);
    top: var(--source-y);
    width: var(--source-width);
    height: var(--source-height);
    transform: translate(-50%, -50%);
    animation: extractToCenter 0.8s ease-out forwards;
  }

  /* Growing phase: card grows larger in center */
  .animated-card.growing {
    left: 50%;
    top: 50%;
    width: 120px;
    height: 170px;
    transform: translate(-50%, -50%) scale(1);
    animation: growCard 0.4s ease-out forwards;
  }

  /* Displaying phase: card is prominently displayed */
  .animated-card.displaying {
    left: 50%;
    top: 50%;
    width: 160px;
    height: 224px;
    transform: translate(-50%, -50%) scale(1);
    animation: cardPulse 1.5s ease-in-out;
  }

  /* Moving phase: card moves to played cards area */
  .animated-card.moving {
    left: 50%;
    top: 50%;
    width: 160px;
    height: 224px;
    transform: translate(-50%, -50%);
    animation: moveToPlayedCards 0.8s ease-in-out forwards;
  }

  /* Fading phase: card fades out as it settles */
  .animated-card.fading {
    left: var(--played-x);
    top: var(--played-y);
    width: var(--played-width);
    height: var(--played-height);
    transform: translate(-50%, -50%);
    animation: fadeOut 0.3s ease-out forwards;
  }

  .card-face {
    position: relative;
    width: 100%;
    height: 100%;
    background: linear-gradient(135deg, #fafafa 0%, #e0e0e0 100%);
    border-radius: 12px;
    border: 3px solid #2c1810;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.6);
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    overflow: hidden;
  }

  .card-content {
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    height: 100%;
    padding: 12px 8px;
    z-index: 2;
  }

  .card-number {
    font-size: 2.5rem;
    font-weight: bold;
    color: #9c27b0;
    text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.3);
    margin-bottom: 0.5rem;
  }

  .card-name {
    font-size: 1rem;
    font-weight: bold;
    color: #333;
    text-align: center;
    line-height: 1.2;
    text-shadow: 1px 1px 1px rgba(255, 255, 255, 0.8);
  }

  .card-glow {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: radial-gradient(circle at center, rgba(156, 39, 176, 0.3) 0%, transparent 70%);
    border-radius: 12px;
    animation: glowPulse 1.5s ease-in-out infinite;
  }

  .action-text {
    position: absolute;
    top: 70%;
    left: 50%;
    transform: translateX(-50%);
    text-align: center;
    color: white;
    z-index: 1001;
    animation: textSlideIn 0.5s ease-out;
  }

  .player-action {
    font-size: 1.2rem;
    font-weight: 600;
    margin-bottom: 0.5rem;
    text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.8);
  }

  .card-title-display {
    font-size: 1.8rem;
    font-weight: bold;
    color: #ffd700;
    text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.8);
    text-transform: uppercase;
    letter-spacing: 1px;
  }

  /* Animation keyframes */
  @keyframes extractToCenter {
    from {
      left: var(--source-x);
      top: var(--source-y);
      width: var(--source-width);
      height: var(--source-height);
      transform: translate(-50%, -50%) scale(1);
    }
    to {
      left: 50%;
      top: 50%;
      width: 120px;
      height: 170px;
      transform: translate(-50%, -50%) scale(1);
    }
  }

  @keyframes growCard {
    from {
      transform: translate(-50%, -50%) scale(1);
    }
    to {
      transform: translate(-50%, -50%) scale(1.33); /* 160/120 = 1.33 */
    }
  }

  @keyframes cardPulse {
    0%, 100% {
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.6);
      transform: translate(-50%, -50%) scale(1);
    }
    50% {
      box-shadow: 0 8px 25px rgba(156, 39, 176, 0.8);
      transform: translate(-50%, -50%) scale(1.05);
    }
  }

  @keyframes moveToPlayedCards {
    from {
      left: 50%;
      top: 50%;
      width: 160px;
      height: 224px;
      transform: translate(-50%, -50%) scale(1);
    }
    to {
      left: var(--played-x);
      top: var(--played-y);
      width: var(--played-width);
      height: var(--played-height);
      transform: translate(-50%, -50%) scale(1);
    }
  }

  @keyframes fadeOut {
    from {
      opacity: 1;
    }
    to {
      opacity: 0;
    }
  }

  @keyframes glowPulse {
    0%, 100% {
      opacity: 0.3;
    }
    50% {
      opacity: 0.6;
    }
  }

  @keyframes textSlideIn {
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
