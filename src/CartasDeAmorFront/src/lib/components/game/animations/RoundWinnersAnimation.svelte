<!-- RoundWinnersAnimation.svelte -->
<script lang="ts">
  import { onMount, createEventDispatcher } from 'svelte';

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
    
    // Phase 1: Golden background appears with sparkles (0.3s)
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
    ? `🏆 ${winnerNames[0]} Wins! 🏆` 
    : `🏆 Round Winners: ${winnerNames.join(', ')} 🏆`;
</script>

{#if isVisible && !animationComplete}
  <div class="animation-overlay" bind:this={animationContainer}>
    <!-- Victory celebration effect -->
    <div 
      class="victory-celebration" 
      class:appearing={phase === 'appearing'}
      class:crown-phase={phase === 'crown'}
    >
      <!-- Golden background with sparkles -->
      <div class="golden-background">
        <!-- Sparkle effects -->
        {#each Array(20) as _, i}
          <div 
            class="sparkle" 
            style="
              left: {Math.random() * 100}%;
              top: {Math.random() * 100}%;
              animation-delay: {Math.random() * 2}s;
            "
          ></div>
        {/each}
      </div>

      <!-- Main content container -->
      <div class="victory-content">
        <!-- Crown icon -->
        <div class="crown-container">
          <div class="crown">👑</div>
          
          <!-- Confetti particles -->
          {#each Array(30) as _, i}
            <div 
              class="confetti" 
              style="
                left: {Math.random() * 100}%;
                animation-delay: {Math.random() * 3}s;
                --color: hsl({Math.random() * 360}, 70%, 60%);
              "
            ></div>
          {/each}
        </div>

        <!-- Winner announcement -->
        <div class="winner-announcement">
          <h2 class="victory-title">Round Complete!</h2>
          <div class="winner-names">{displayText}</div>
        </div>

        <!-- Victory rays -->
        <div class="victory-rays">
          {#each Array(8) as _, i}
            <div 
              class="ray" 
              style="transform: rotate({i * 45}deg)"
            ></div>
          {/each}
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
    display: flex;
    align-items: center;
    justify-content: center;
    pointer-events: none;
  }

  .victory-celebration {
    position: relative;
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    opacity: 0;
    transform: scale(0.8);
    transition: all 0.5s ease-out;
  }

  .victory-celebration.appearing {
    opacity: 1;
    transform: scale(1);
  }

  .victory-celebration.crown-phase {
    opacity: 1;
    transform: scale(1);
  }

  .golden-background {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: radial-gradient(
      circle at center,
      rgba(255, 215, 0, 0.3) 0%,
      rgba(255, 165, 0, 0.2) 30%,
      rgba(255, 215, 0, 0.1) 60%,
      transparent 100%
    );
    animation: golden-pulse 2s ease-in-out infinite alternate;
  }

  .sparkle {
    position: absolute;
    width: 4px;
    height: 4px;
    background: #ffd700;
    border-radius: 50%;
    animation: sparkle-twinkle 2s ease-in-out infinite;
    box-shadow: 0 0 6px #ffd700;
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
    font-size: 6rem;
    animation: crown-bounce 1.5s ease-in-out infinite;
    text-shadow: 0 0 20px rgba(255, 215, 0, 0.8);
    transform-origin: center bottom;
  }

  .crown-phase .crown {
    animation: crown-entrance 1.2s ease-out forwards, crown-bounce 1.5s ease-in-out infinite 1.2s;
  }

  .confetti {
    position: absolute;
    width: 8px;
    height: 8px;
    background: var(--color, #ffd700);
    top: -10px;
    opacity: 0;
  }

  .winner-announcement {
    background: rgba(255, 255, 255, 0.95);
    border: 3px solid #ffd700;
    border-radius: 20px;
    padding: 2rem 3rem;
    box-shadow: 
      0 10px 30px rgba(0, 0, 0, 0.3),
      inset 0 0 20px rgba(255, 215, 0, 0.2);
    transform: translateY(50px);
    opacity: 0;
  }

  .crown-phase .winner-announcement {
    animation: slide-up-fade-in 1s ease-out 0.5s forwards;
  }

  .victory-title {
    color: #9c27b0;
    font-size: 2.5rem;
    font-weight: bold;
    margin: 0 0 1rem 0;
    text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.1);
  }

  .winner-names {
    color: #333;
    font-size: 1.8rem;
    font-weight: 600;
    line-height: 1.3;
    margin: 0;
  }

  .victory-rays {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    width: 400px;
    height: 400px;
    opacity: 0;
  }

  .ray {
    position: absolute;
    top: 50%;
    left: 50%;
    width: 2px;
    height: 200px;
    background: linear-gradient(
      to bottom,
      transparent 0%,
      rgba(255, 215, 0, 0.6) 30%,
      rgba(255, 215, 0, 0.8) 50%,
      rgba(255, 215, 0, 0.6) 70%,
      transparent 100%
    );
    transform-origin: 1px 0;
    animation: ray-rotation 4s linear infinite;
  }

  /* Keyframe animations */
  @keyframes golden-pulse {
    0% { opacity: 0.6; transform: scale(1); }
    100% { opacity: 0.8; transform: scale(1.05); }
  }

  @keyframes sparkle-twinkle {
    0%, 100% { opacity: 0; transform: scale(0.5) rotate(0deg); }
    50% { opacity: 1; transform: scale(1.2) rotate(180deg); }
  }

  @keyframes crown-entrance {
    0% { 
      transform: translateY(-100px) scale(0.5) rotate(-10deg); 
      opacity: 0; 
    }
    60% { 
      transform: translateY(10px) scale(1.1) rotate(2deg); 
      opacity: 1; 
    }
    100% { 
      transform: translateY(0) scale(1) rotate(0deg); 
      opacity: 1; 
    }
  }

  @keyframes crown-bounce {
    0%, 100% { transform: translateY(0) scale(1); }
    50% { transform: translateY(-10px) scale(1.05); }
  }

  @keyframes confetti-fall {
    0% {
      opacity: 1;
      transform: translateY(-10px) rotate(0deg);
    }
    100% {
      opacity: 0;
      transform: translateY(400px) rotate(720deg);
    }
  }

  @keyframes slide-up-fade-in {
    0% {
      transform: translateY(50px);
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

  @keyframes rays-appear {
    0% { 
      opacity: 0; 
      transform: translate(-50%, -50%) scale(0.5); 
    }
    100% { 
      opacity: 0.7; 
      transform: translate(-50%, -50%) scale(1); 
    }
  }

  @keyframes ray-rotation {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }

  /* Responsive design */
  @media (max-width: 768px) {
    .crown {
      font-size: 4rem;
    }
    
    .victory-title {
      font-size: 2rem;
    }
    
    .winner-names {
      font-size: 1.4rem;
    }
    
    .winner-announcement {
      padding: 1.5rem 2rem;
      margin: 0 1rem;
    }
    
    .victory-rays {
      width: 300px;
      height: 300px;
    }
  }

  @media (max-width: 480px) {
    .crown {
      font-size: 3rem;
    }
    
    .victory-title {
      font-size: 1.6rem;
    }
    
    .winner-names {
      font-size: 1.2rem;
    }
    
    .winner-announcement {
      padding: 1rem 1.5rem;
    }
  }
</style>
