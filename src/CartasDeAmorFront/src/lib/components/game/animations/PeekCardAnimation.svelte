<!-- PeekCardAnimation.svelte -->
<script lang="ts">
  import { onMount, createEventDispatcher } from 'svelte';

  export let invokerName: string = '';
  export let targetName: string = '';
  export let targetPosition: { x: number; y: number; width?: number; height?: number };
  export let invokerPosition: { x: number; y: number; width?: number; height?: number };
  export let isVisible: boolean = false;

  const dispatch = createEventDispatcher<{
    animationComplete: null;
  }>();

  let animationContainer: HTMLElement;
  let animationComplete = false;
  let hasStarted = false;

  // Animation phases
  let phase: 'extracting' | 'peeking' | 'returning' = 'extracting';
  
  onMount(() => {
    if (isVisible) {
      startAnimation();
    }
  });

  async function startAnimation() {
    if (hasStarted) return; // Prevent duplicate starts
    hasStarted = true;
    
    // Phase 1: Extract card from target's hand and move to invoker (0.6s)
    phase = 'extracting';
    await new Promise(resolve => setTimeout(resolve, 600));
    
    // Phase 2: Hold at invoker position - "peeking" (1s)
    phase = 'peeking';
    await new Promise(resolve => setTimeout(resolve, 1000));
    
    // Phase 3: Return card to target's hand (0.6s)
    phase = 'returning';
    await new Promise(resolve => setTimeout(resolve, 600));
    
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
    class="animation-overlay" 
    bind:this={animationContainer}
    style="
      --target-x: {targetPosition.x}px; 
      --target-y: {targetPosition.y}px; 
      --target-width: {targetPosition.width || 50}px; 
      --target-height: {targetPosition.height || 70}px;
      --invoker-x: {invokerPosition.x}px; 
      --invoker-y: {invokerPosition.y}px; 
      --invoker-width: {invokerPosition.width || 50}px; 
      --invoker-height: {invokerPosition.height || 70}px;
    "
  >
    <!-- Animated card -->
    <div 
      class="animated-card" 
      class:extracting={phase === 'extracting'}
      class:peeking={phase === 'peeking'}
      class:returning={phase === 'returning'}
    >
      <!-- Card back (since the card is face down when moving) -->
      <div class="card-face">
        <div class="card-back-pattern"></div>
      </div>
    </div>

    <!-- Action text overlay -->
    {#if phase === 'peeking'}
      <div class="action-text">
        <div class="peek-action">{invokerName} is peeking at {targetName}'s card</div>
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
    background: rgba(0, 0, 0, 0.2);
    backdrop-filter: blur(1px);
  }

  .animated-card {
    position: absolute;
    transform-style: preserve-3d;
    transition: none;
  }

  /* Extracting phase: card travels from target to invoker */
  .animated-card.extracting {
    left: var(--target-x);
    top: var(--target-y);
    width: var(--target-width);
    height: var(--target-height);
    transform: translate(-50%, -50%);
    animation: moveToInvoker 0.6s cubic-bezier(0.25, 0.46, 0.45, 0.94) forwards;
  }

  /* Peeking phase: card is held at invoker position */
  .animated-card.peeking {
    left: var(--invoker-x);
    top: var(--invoker-y);
    width: var(--invoker-width);
    height: var(--invoker-height);
    transform: translate(-50%, -50%) scale(1.1);
    animation: peekPulse 1s ease-in-out;
  }

  /* Returning phase: card travels back to target */
  .animated-card.returning {
    left: var(--invoker-x);
    top: var(--invoker-y);
    width: var(--invoker-width);
    height: var(--invoker-height);
    transform: translate(-50%, -50%);
    animation: returnToTarget 0.6s cubic-bezier(0.25, 0.46, 0.45, 0.94) forwards;
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
    top: 20%;
    left: 50%;
    transform: translateX(-50%);
    text-align: center;
    color: white;
    z-index: 1001;
    animation: textSlideIn 0.3s ease-out;
  }

  .peek-action {
    font-size: 1.2rem;
    font-weight: 600;
    text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.8);
    background: rgba(0, 0, 0, 0.6);
    padding: 0.5rem 1rem;
    border-radius: 8px;
    border: 1px solid rgba(255, 255, 255, 0.2);
  }

  /* Animation keyframes */
  @keyframes moveToInvoker {
    from {
      left: var(--target-x);
      top: var(--target-y);
      width: var(--target-width);
      height: var(--target-height);
      transform: translate(-50%, -50%) rotate(0deg);
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.6);
    }
    to {
      left: var(--invoker-x);
      top: var(--invoker-y);
      width: var(--invoker-width);
      height: var(--invoker-height);
      transform: translate(-50%, -50%) rotate(-3deg);
      box-shadow: 0 8px 20px rgba(0, 0, 0, 0.8);
    }
  }

  @keyframes peekPulse {
    0%, 100% {
      transform: translate(-50%, -50%) scale(1.1) rotate(-3deg);
      box-shadow: 0 8px 20px rgba(156, 39, 176, 0.6);
    }
    50% {
      transform: translate(-50%, -50%) scale(1.15) rotate(-1deg);
      box-shadow: 0 12px 30px rgba(156, 39, 176, 0.9);
    }
  }

  @keyframes returnToTarget {
    from {
      left: var(--invoker-x);
      top: var(--invoker-y);
      width: var(--invoker-width);
      height: var(--invoker-height);
      transform: translate(-50%, -50%) rotate(-3deg);
      box-shadow: 0 8px 20px rgba(0, 0, 0, 0.8);
    }
    to {
      left: var(--target-x);
      top: var(--target-y);
      width: var(--target-width);
      height: var(--target-height);
      transform: translate(-50%, -50%) rotate(0deg);
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.6);
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
