<!-- RoundWinnersAnimation.svelte -->
<script lang="ts">
  import { onMount, createEventDispatcher } from 'svelte';
  import { _ } from 'svelte-i18n';

  export let center: { x: number; y: number };
  export let winnerNames: string[] = [];
  export let isVisible: boolean = false;

  const dispatch = createEventDispatcher<{
    animationComplete: null;
  }>();

  let animationContainer: HTMLElement;
  let animationComplete = false;
  let hasStarted = false;

  // Animation phases
  let phase: 'appearing' | 'crown' = 'appearing';
  
  onMount(() => {
    if (isVisible) {
      startAnimation();
    }
  });

  async function startAnimation() {
    if (hasStarted) {
      return; // Prevent duplicate starts
    }
    hasStarted = true;
    
    // Phase 1: Golden background appears with (0.3s)
    phase = 'appearing';
    
    await new Promise(resolve => setTimeout(resolve, 300));
    
    // Phase 2: Crown appears and player names slide in (2s)
    phase = 'crown';
    
    await new Promise(resolve => setTimeout(resolve, 3000));
    
    // Mark animation as complete and dispatch event
    if (!animationComplete) {
      animationComplete = true;
      dispatch('animationComplete');
    }
  }

  $: if (isVisible && !animationComplete && !hasStarted) {
    startAnimation();
  }

  // Format winner names for display
  $: displayText = winnerNames.length === 1 
    ? `🏆 ${winnerNames[0]} ${$_('game.wins')}! 🏆` 
    : `🏆 ${$_('game.roundWinners')}: ${winnerNames.join(', ')} 🏆`;
</script>

{#if isVisible && !animationComplete}
  <div 
    class="animation-overlay" 
    bind:this={animationContainer}
    style="
      --center-x: {center.x}px;
      --center-y: {center.y}px;
    "
  >
    <!-- Victory celebration effect -->
    <div 
      class="victory-celebration" 
      class:appearing={phase === 'appearing'}
      class:crown-phase={phase === 'crown'}
    >
      <!-- Main content container -->
      <div class="victory-content">
        <!-- Crown icon -->
        <div class="crown-container">
          <div class="crown">👑</div>
        </div>

        <!-- Winner announcement -->
        <div class="winner-announcement">
          <h2 class="victory-title">{$_('game.roundComplete')}!</h2>
          <div class="winner-names">{displayText}</div>
        </div>
      </div>
    </div>
  </div>
{/if}

<style>
  .animation-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100vw;
    height: 100vh;
    z-index: 3000;
    pointer-events: none;
  }

  .victory-celebration {
    position: absolute;
    top: var(--center-y);
    left: var(--center-x);
    transform: translate(-50%, -50%) scale(0.8);
    width: 400px;
    height: 300px;
    display: flex;
    align-items: center;
    justify-content: center;
    opacity: 0;
    transition: all 0.5s ease-out;
  }

  .victory-celebration.appearing {
    opacity: 1;
    transform: translate(-50%, -50%) scale(1);
  }

  .victory-celebration.crown-phase {
    opacity: 1;
    transform: translate(-50%, -50%) scale(1);
  }

  .victory-content {
    position: relative;
    text-align: center;
    z-index: 1;
  }

  .crown-container {
    position: relative;
    margin-bottom: 2rem;
  }

  .crown {
    font-size: clamp(2rem, 8vw, 4rem);
    animation: crown-bounce 1.5s ease-in-out infinite;
    text-shadow: 0 0 20px rgba(255, 215, 0, 0.8);
    transform-origin: center bottom;
  }

  .crown-phase .crown {
    animation: crown-entrance 1.2s ease-out forwards;
  }

  .winner-announcement {
    background: rgba(255, 255, 255, 0.95);
    border: 3px solid #ffd700;
    border-radius: 20px;
    padding: clamp(1rem, 4vw, 2rem) clamp(1.5rem, 6vw, 3rem);
    box-shadow: 
      0 10px 30px rgba(0, 0, 0, 0.3),
      inset 0 0 20px rgba(255, 215, 0, 0.2);
    transform: translateY(30px);
    opacity: 0;
    max-width: 350px;
  }

  .crown-phase .winner-announcement {
    animation: slide-up-fade-in 1s ease-out 0.5s forwards;
  }

  .victory-title {
    color: #9c27b0;
    font-size: clamp(1.2rem, 4vw, 1.8rem);
    font-weight: bold;
    margin: 0 0 1rem 0;
    text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.1);
  }

  .winner-names {
    color: #333;
    font-size: clamp(1rem, 3vw, 1.4rem);
    font-weight: 600;
    line-height: 1.3;
    margin: 0;
  }

  /* Keyframe animations */
  @keyframes crown-entrance {
    0% { 
      transform: translateY(-60px) scale(0.5) rotate(-10deg); 
      opacity: 0; 
    }
    60% { 
      transform: translateY(5px) scale(1.1) rotate(2deg); 
      opacity: 1; 
    }
    100% { 
      transform: translateY(0) scale(1) rotate(0deg); 
      opacity: 1; 
    }
  }

  @keyframes crown-bounce {
    0%, 100% { transform: translateY(0) scale(1); }
    50% { transform: translateY(-8px) scale(1.05); }
  }

  @keyframes slide-up-fade-in {
    0% {
      transform: translateY(30px);
      opacity: 0;
    }
    100% {
      transform: translateY(0);
      opacity: 1;
    }
  }

  @keyframes text-glow-pulse {
    0%, 100% { 
      text-shadow: 0 0 10px rgba(255, 215, 0, 0.5); 
    }
    50% { 
      text-shadow: 0 0 20px rgba(255, 215, 0, 0.8), 0 0 30px rgba(255, 215, 0, 0.6); 
    }
  }

  /* Responsive design */
  @media (max-width: 768px) {
    .crown {
      font-size: clamp(2rem, 8vw, 4rem);
    }
    
    .victory-title {
      font-size: clamp(1.2rem, 5vw, 2rem);
    }
    
    .winner-names {
      font-size: clamp(1rem, 4vw, 1.4rem);
    }
    
    .winner-announcement {
      padding: 1.5rem 2rem;
      margin: 0 1rem;
      max-width: 90%;
    }
  }

  @media (max-width: 480px) {
    .crown {
      font-size: clamp(1.5rem, 6vw, 3rem);
    }
    
    .victory-title {
      font-size: clamp(1rem, 4vw, 1.6rem);
    }
    
    .winner-names {
      font-size: clamp(0.8rem, 3vw, 1.2rem);
    }
    
    .winner-announcement {
      padding: 1rem 1.5rem;
      max-width: 95%;
    }
  }
</style>
