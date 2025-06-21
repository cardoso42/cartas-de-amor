<!-- EliminationAnimation.svelte -->
<script lang="ts">
  import { onMount, createEventDispatcher } from 'svelte';

  export let playerName: string = '';
  export let isVisible: boolean = false;

  const dispatch = createEventDispatcher<{
    animationComplete: null;
  }>();

  let animationContainer: HTMLElement;
  let animationComplete = false;

  // Animation phases
  let phase: 'shake' | 'crack' | 'shatter' | 'fade' = 'shake';
  
  onMount(() => {
    if (isVisible) {
      startAnimation();
    }
  });

  async function startAnimation() {
    // Phase 1: Shake the warning circle (0.8s)
    phase = 'shake';
    
    await new Promise(resolve => setTimeout(resolve, 800));
    
    // Phase 2: Show crack effect (0.6s)
    phase = 'crack';
    
    await new Promise(resolve => setTimeout(resolve, 600));
    
    // Phase 3: Shatter effect (0.8s) - player name starts showing
    phase = 'shatter';
    
    await new Promise(resolve => setTimeout(resolve, 800));
    
    // Phase 4: Fade out (2.8s) - extended time to show player name
    phase = 'fade';
    
    await new Promise(resolve => setTimeout(resolve, 2800));
    
    // Mark animation as complete and dispatch event
    animationComplete = true;
    dispatch('animationComplete');
  }

  $: if (isVisible && !animationComplete) {
    startAnimation();
  }
</script>

{#if isVisible && !animationComplete}
  <div class="animation-overlay" bind:this={animationContainer}>
    <!-- Player elimination effect -->
    <div 
      class="elimination-effect" 
      class:shaking={phase === 'shake'}
      class:cracking={phase === 'crack'}
      class:shattering={phase === 'shatter'}
      class:fading={phase === 'fade'}
    >
      <!-- Red warning circle -->
      <div class="warning-circle">
        <div class="warning-icon">⚠</div>
      </div>
      
      <!-- Crack lines -->
      {#if phase === 'crack' || phase === 'shatter' || phase === 'fade'}
        <div class="crack-lines">
          <div class="crack crack-1"></div>
          <div class="crack crack-2"></div>
          <div class="crack crack-3"></div>
          <div class="crack crack-4"></div>
        </div>
      {/if}
      
      <!-- Shatter fragments -->
      {#if phase === 'shatter' || phase === 'fade'}
        <div class="shatter-fragments">
          {#each Array(8) as _, i}
            <div class="fragment fragment-{i + 1}"></div>
          {/each}
        </div>
      {/if}
    </div>

    <!-- Elimination text -->
    {#if phase === 'shatter' || phase === 'fade'}
      <div class="elimination-text" class:fading={phase === 'fade'}>
        <div class="eliminated-label">ELIMINATED!</div>
        <div class="player-name">{playerName}</div>
      </div>
    {/if}

    <!-- Dark overlay for dramatic effect -->
    <div class="dark-overlay" class:visible={phase === 'crack' || phase === 'shatter'} class:fading={phase === 'fade'}></div>
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
  }

  .elimination-effect {
    position: absolute;
    left: 50%;
    top: 50%;
    width: 120px;
    height: 120px;
    transform: translate(-50%, -50%);
    z-index: 1001;
  }

  /* Shaking phase */
  .elimination-effect.shaking {
    animation: violentShake 0.8s ease-in-out;
  }

  /* Cracking phase */
  .elimination-effect.cracking .warning-circle {
    animation: pulseRed 0.6s ease-in-out;
  }

  /* Shattering phase */
  .elimination-effect.shattering .warning-circle {
    animation: explode 0.8s ease-out forwards;
  }

  /* Fading phase */
  .elimination-effect.fading {
    animation: fadeOut 2.8s ease-out forwards;
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

  .crack-lines {
    position: absolute;
    width: 100%;
    height: 100%;
    top: 0;
    left: 0;
  }

  .crack {
    position: absolute;
    background: #333;
    border-radius: 1px;
    opacity: 0;
    animation: crackAppear 0.6s ease-out forwards;
  }

  .crack-1 {
    width: 2px;
    height: 60px;
    top: 50%;
    left: 30%;
    transform: translate(-50%, -50%) rotate(45deg);
    animation-delay: 0.05s;
  }

  .crack-2 {
    width: 2px;
    height: 50px;
    top: 30%;
    left: 70%;
    transform: translate(-50%, -50%) rotate(-30deg);
    animation-delay: 0.1s;
  }

  .crack-3 {
    width: 2px;
    height: 45px;
    top: 70%;
    left: 20%;
    transform: translate(-50%, -50%) rotate(60deg);
    animation-delay: 0.15s;
  }

  .crack-4 {
    width: 2px;
    height: 55px;
    top: 60%;
    left: 80%;
    transform: translate(-50%, -50%) rotate(-45deg);
    animation-delay: 0.2s;
  }

  .shatter-fragments {
    position: absolute;
    width: 100%;
    height: 100%;
    top: 0;
    left: 0;
  }

  .fragment {
    position: absolute;
    width: 20px;
    height: 20px;
    background: radial-gradient(circle, #ff6666 0%, #cc0000 100%);
    border: 1px solid #990000;
    opacity: 0;
  }

  .fragment-1 {
    top: 20%;
    left: 20%;
    animation: shatterFly1 0.8s ease-out forwards;
  }

  .fragment-2 {
    top: 20%;
    right: 20%;
    animation: shatterFly2 0.8s ease-out forwards;
  }

  .fragment-3 {
    top: 50%;
    left: 10%;
    animation: shatterFly3 0.8s ease-out forwards;
  }

  .fragment-4 {
    top: 50%;
    right: 10%;
    animation: shatterFly4 0.8s ease-out forwards;
  }

  .fragment-5 {
    bottom: 20%;
    left: 20%;
    animation: shatterFly5 0.8s ease-out forwards;
  }

  .fragment-6 {
    bottom: 20%;
    right: 20%;
    animation: shatterFly6 0.8s ease-out forwards;
  }

  .fragment-7 {
    top: 40%;
    left: 50%;
    animation: shatterFly7 0.8s ease-out forwards;
  }

  .fragment-8 {
    bottom: 40%;
    left: 50%;
    animation: shatterFly8 0.8s ease-out forwards;
  }

  .elimination-text {
    position: absolute;
    top: 30%;
    left: 50%;
    transform: translateX(-50%);
    text-align: center;
    color: white;
    z-index: 1002;
    animation: slideInFromTop 0.5s ease-out;
  }

  .elimination-text.fading {
    animation: fadeOut 2.8s ease-out forwards;
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

  .dark-overlay {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.7);
    opacity: 0;
    z-index: 999;
  }

  .dark-overlay.visible {
    opacity: 1;
    transition: opacity 0.3s ease-in;
  }

  .dark-overlay.fading {
    opacity: 0;
    transition: opacity 2.8s ease-out;
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
