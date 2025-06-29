<!-- EliminationAnimation.svelte -->
<script lang="ts">
  import { onMount, createEventDispatcher } from 'svelte';
  import { _ } from 'svelte-i18n';

  export let playerName: string = '';
  export let center: { x: number; y: number };
  export let isVisible: boolean = false;

  const dispatch = createEventDispatcher<{
    animationComplete: null;
  }>();

  let animationContainer: HTMLElement;
  let animationComplete = false;
  let hasStarted = false;

  // Animation phases
  let phase: 'shake' | 'fade' = 'shake';
  
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
    
    // Phase 1: Shake the warning circle (1s)
    phase = 'shake';
    
    await new Promise(resolve => setTimeout(resolve, 1000));

    // Phase 4: Fade out (3s)
    phase = 'fade';
    
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
</script>

{#if isVisible && !animationComplete}
  <div 
    class="animation-container" 
    bind:this={animationContainer} 
    style="--center-x: {center.x}px; --center-y: {center.y}px;"
  >
    <!-- Player elimination effect -->
    <div 
      class="elimination-effect" 
      class:shaking={phase === 'shake'}
      class:fading={phase === 'fade'}
    >
      <!-- Red warning circle -->
      <div class="warning-circle">
        <div class="warning-icon">⚠</div>
      </div>
    </div>

    <!-- Elimination text -->
    {#if phase === 'fade'}
      <div class="elimination-text" class:fading={phase === 'fade'}>
        <div class="eliminated-label">{$_('game.eliminated')}!</div>
        <div class="player-name">{playerName}</div>
      </div>
    {/if}

  </div>
{/if}

<style>
  .animation-container {
    position: fixed;
    top: 0;
    left: 0;
    width: 100vw;
    height: 100vh;
    pointer-events: none;
    z-index: 1000;
  }

  .elimination-effect {
    position: absolute;
    left: var(--center-x);
    top: var(--center-y);
    width: 120px;
    height: 120px;
    transform: translate(-50%, -50%);
    z-index: 1001;
  }

  /* Shaking phase */
  .elimination-effect.shaking {
    animation: violentShake 1s ease-in-out;
  }

  /* Fading phase */
  .elimination-effect.fading {
    animation: fadeOut 3s ease-out forwards;
  }

  .warning-circle {
    position: absolute;
    width: 100%;
    height: 100%;
    background: radial-gradient(circle, #ff4444 0%, #cc0000 70%);
    border: 4px solid #990000;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 0 20px rgba(255, 68, 68, 0.8);
  }

  .warning-icon {
    font-size: 3rem;
    color: white;
    font-weight: bold;
    text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.8);
    animation: pulse 0.5s ease-in-out infinite alternate;
  }

  .elimination-text {
    position: absolute;
    top: calc(var(--center-y) * 0.6);
    left: var(--center-x);
    transform: translateX(-50%);
    text-align: center;
    color: white;
    z-index: 1002;
    animation: slideInFromTop 0.5s ease-out;
  }

  .elimination-text.fading {
    animation: fadeOut 3s ease-out forwards;
  }

  .eliminated-label {
    font-size: 3rem;
    font-weight: bold;
    color: #ff4444;
    text-shadow: 3px 3px 6px rgba(0, 0, 0, 0.8);
    margin-bottom: 0.5rem;
    animation: textPulse 1s ease-in-out infinite;
  }

  .player-name {
    font-size: 1.5rem;
    font-weight: 600;
    color: white;
    text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.8);
    background: rgba(0, 0, 0, 0.7);
    padding: 0.5rem 1rem;
    border-radius: 8px;
    border: 2px solid #ff4444;
  }

  /* Animation keyframes */
  @keyframes violentShake {
    0%, 100% { transform: translate(-50%, -50%) rotate(0deg); }
    10% { transform: translate(-48%, -52%) rotate(2deg); }
    20% { transform: translate(-52%, -48%) rotate(-2deg); }
    30% { transform: translate(-49%, -51%) rotate(1deg); }
    40% { transform: translate(-51%, -49%) rotate(-1deg); }
    50% { transform: translate(-50%, -50%) rotate(0deg); }
    60% { transform: translate(-52%, -52%) rotate(3deg); }
    70% { transform: translate(-48%, -48%) rotate(-3deg); }
    80% { transform: translate(-51%, -49%) rotate(2deg); }
    90% { transform: translate(-49%, -51%) rotate(-2deg); }
  }

  @keyframes pulseRed {
    0%, 100% { 
      box-shadow: 0 0 20px rgba(255, 68, 68, 0.8);
      transform: scale(1);
    }
    50% { 
      box-shadow: 0 0 40px rgba(255, 68, 68, 1);
      transform: scale(1.1);
    }
  }

  @keyframes explode {
    0% {
      transform: scale(1);
      opacity: 1;
    }
    50% {
      transform: scale(1.3);
      opacity: 0.8;
    }
    100% {
      transform: scale(2);
      opacity: 0;
    }
  }

  @keyframes crackAppear {
    from {
      opacity: 0;
      transform: translate(-50%, -50%) rotate(var(--rotation, 0deg)) scaleY(0);
    }
    to {
      opacity: 1;
      transform: translate(-50%, -50%) rotate(var(--rotation, 0deg)) scaleY(1);
    }
  }

  @keyframes shatterFly1 {
    0% { opacity: 1; transform: translate(0, 0) rotate(0deg); }
    100% { opacity: 0; transform: translate(-40px, -40px) rotate(180deg); }
  }

  @keyframes shatterFly2 {
    0% { opacity: 1; transform: translate(0, 0) rotate(0deg); }
    100% { opacity: 0; transform: translate(40px, -40px) rotate(-180deg); }
  }

  @keyframes shatterFly3 {
    0% { opacity: 1; transform: translate(0, 0) rotate(0deg); }
    100% { opacity: 0; transform: translate(-50px, 0px) rotate(90deg); }
  }

  @keyframes shatterFly4 {
    0% { opacity: 1; transform: translate(0, 0) rotate(0deg); }
    100% { opacity: 0; transform: translate(50px, 0px) rotate(-90deg); }
  }

  @keyframes shatterFly5 {
    0% { opacity: 1; transform: translate(0, 0) rotate(0deg); }
    100% { opacity: 0; transform: translate(-40px, 40px) rotate(270deg); }
  }

  @keyframes shatterFly6 {
    0% { opacity: 1; transform: translate(0, 0) rotate(0deg); }
    100% { opacity: 0; transform: translate(40px, 40px) rotate(-270deg); }
  }

  @keyframes shatterFly7 {
    0% { opacity: 1; transform: translate(0, 0) rotate(0deg); }
    100% { opacity: 0; transform: translate(0px, -60px) rotate(360deg); }
  }

  @keyframes shatterFly8 {
    0% { opacity: 1; transform: translate(0, 0) rotate(0deg); }
    100% { opacity: 0; transform: translate(0px, 60px) rotate(-360deg); }
  }

  @keyframes fadeOut {
    from {
      opacity: 1;
    }
    to {
      opacity: 0;
    }
  }

  @keyframes slideInFromTop {
    from {
      opacity: 0;
      transform: translateX(-50%) translateY(-30px);
    }
    to {
      opacity: 1;
      transform: translateX(-50%) translateY(0);
    }
  }

  @keyframes textPulse {
    0%, 100% { 
      text-shadow: 3px 3px 6px rgba(0, 0, 0, 0.8);
    }
    50% { 
      text-shadow: 3px 3px 6px rgba(0, 0, 0, 0.8), 0 0 20px rgba(255, 68, 68, 0.8);
    }
  }

  @keyframes pulse {
    0%, 100% { opacity: 1; }
    50% { opacity: 0.7; }
  }
</style>
