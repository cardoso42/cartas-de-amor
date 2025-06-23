<!-- GuessCardAnimation.svelte -->
<script lang="ts">
  import { onMount, createEventDispatcher } from 'svelte';
  import { getCardName } from '$lib/utils/cardUtils';
  import type { CardType } from '$lib/types/game-types';

  export let invokerName: string = '';
  export let targetName: string = '';
  export let guessedCardType: CardType;
  export let isVisible: boolean = false;

  const dispatch = createEventDispatcher<{
    animationComplete: null;
  }>();

  let animationContainer: HTMLElement;
  let animationComplete = false;
  let hasStarted = false;

  // Animation phases
  let phase: 'appearing' | 'visible' | 'fading' = 'appearing';
  
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
    
    // Phase 1: Parchment appears (0.8s)
    phase = 'appearing';
    await new Promise(resolve => setTimeout(resolve, 800));
    
    // Phase 2: Show message (4s)
    phase = 'visible';
    await new Promise(resolve => setTimeout(resolve, 4000));
    
    // Phase 3: Fade out (0.8s)
    phase = 'fading';    
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
    <div class="parchment-container" class:appearing={phase === 'appearing'} class:visible={phase === 'visible'} class:fading={phase === 'fading'}>
      
      <!-- Medieval Parchment -->
      <div class="parchment-scroll">
        <div class="parchment-paper">
          
          <!-- Elegant header with decorative elements -->
          <div class="parchment-header">
            <div class="decorative-line"></div>
            <h2 class="announcement-title">Royal Decree</h2>
            <div class="decorative-line"></div>
          </div>
          
          <div class="parchment-content">
            <!-- Main announcement text -->
            <div class="announcement-text">
              <div class="player-declaration">
                <span class="player-name invoker">{invokerName}</span>
              </div>
              
              <p class="decree-line">
                acused
              </p>
              
              <div class="player-declaration">
                <span class="player-name target">{targetName}</span>
              </div>
              
              <p class="decree-line">
                of corrupting
              </p>
              
              <!-- Elegant card reveal -->
              <div class="card-revelation">
                <div class="card-frame">
                  <div class="card-number">{guessedCardType}</div>
                  <div class="card-title">{getCardName(guessedCardType)}</div>
                </div>
              </div>
            </div>
          </div>
          
          <!-- Royal seal - positioned more naturally -->
          <div class="royal-seal">
            <div class="seal-background"></div>
            <div class="seal-text">⚜</div>
          </div>
        </div>
      </div>
      
    </div>
  </div>
{/if}

<style>
  @import url('https://fonts.googleapis.com/css2?family=Cinzel:wght@400;600;700&family=Cinzel+Decorative:wght@700&family=Cormorant+Garamond:ital,wght@0,400;0,600;1,400&display=swap');

  .animation-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100vw;
    height: 100vh;
    pointer-events: none;
    z-index: 1000;
    background: rgba(0, 0, 0, 0.7);
    backdrop-filter: blur(3px);
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .parchment-container {
    opacity: 0;
    transform: scale(0.8);
  }

  .parchment-container.appearing {
    animation: parchmentAppear 0.8s ease-out forwards;
  }

  .parchment-container.visible {
    opacity: 1;
    transform: scale(1);
  }

  .parchment-container.fading {
    animation: parchmentFade 0.8s ease-out forwards;
  }

  .parchment-scroll {
    position: relative;
    max-width: 600px;
    width: 92%;
  }

  .parchment-paper {
    background: 
      linear-gradient(145deg, #faf6eb 0%, #f0e8d0 50%, #e8dcc0 100%);
    border: 1px solid #d4c5a0;
    border-radius: 8px;
    box-shadow: 
      0 20px 60px rgba(0, 0, 0, 0.4),
      0 4px 15px rgba(0, 0, 0, 0.2),
      inset 0 1px 0 rgba(255, 255, 255, 0.3);
    padding: 3.5rem 3rem 2.5rem;
    position: relative;
    min-height: 400px;
    overflow: hidden;
  }

  /* Subtle parchment texture */
  .parchment-paper::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: 
      radial-gradient(circle at 25% 25%, rgba(139, 69, 19, 0.03) 1px, transparent 2px),
      radial-gradient(circle at 75% 75%, rgba(160, 82, 45, 0.03) 1px, transparent 2px),
      linear-gradient(90deg, transparent 0%, rgba(139, 69, 19, 0.01) 50%, transparent 100%);
    background-size: 60px 60px, 80px 80px, 100% 2px;
    opacity: 0.6;
    pointer-events: none;
  }

  /* Elegant header section */
  .parchment-header {
    margin-bottom: 2.5rem;
    text-align: center;
  }

  .decorative-line {
    height: 1px;
    background: linear-gradient(90deg, transparent 0%, #a67c52 20%, #8b6914 50%, #a67c52 80%, transparent 100%);
    margin: 1rem auto;
    width: 200px;
  }

  .announcement-title {
    font-family: 'Cinzel Decorative', 'Times New Roman', serif;
    font-size: 2rem;
    font-weight: 700;
    color: #2c1810;
    margin: 0;
    letter-spacing: 3px;
    text-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
    text-transform: uppercase;
  }

  /* Content section */
  .parchment-content {
    position: relative;
    z-index: 1;
  }

  .announcement-text {
    text-align: center;
    line-height: 1.8;
  }

  .decree-line {
    font-family: 'Cormorant Garamond', 'Times New Roman', serif;
    font-size: 1.8rem;
    color: #3a2820;
    margin: 1.5rem 0;
    font-style: italic;
    font-weight: 600;
    line-height: 1.4;
  }

  .player-declaration {
    margin: 2rem 0;
  }

  .player-name {
    display: inline-block;
    font-family: 'Cinzel', 'Times New Roman', serif;
    font-size: 2.2rem;
    font-weight: 700;
    padding: 0.8rem 2rem;
    border-radius: 6px;
    position: relative;
    text-shadow: 0 3px 6px rgba(0, 0, 0, 0.2);
    transition: all 0.3s ease;
    letter-spacing: 1.5px;
  }

  .player-name.invoker {
    background: linear-gradient(135deg, #fff8f0 0%, #ffeedd 100%);
    color: #8b2635;
    border: 1px solid #d4af37;
    box-shadow: 0 2px 8px rgba(139, 38, 53, 0.15);
  }

  .player-name.target {
    background: linear-gradient(135deg, #f0f8ff 0%, #e6f3ff 100%);
    color: #1e3a8a;
    border: 1px solid #6b7280;
    box-shadow: 0 2px 8px rgba(30, 58, 138, 0.15);
  }

  /* Elegant card display */
  .card-revelation {
    margin: 2.5rem 0;
    display: flex;
    justify-content: center;
  }

  .card-frame {
    background: linear-gradient(135deg, #1a0d2e 0%, #2d1b3d 50%, #0f051a 100%);
    border: 3px solid #d4af37;
    border-radius: 12px;
    padding: 2rem 2.5rem;
    text-align: center;
    position: relative;
    box-shadow: 
      0 12px 35px rgba(0, 0, 0, 0.5),
      0 0 0 1px rgba(212, 175, 55, 0.4),
      inset 0 2px 4px rgba(255, 255, 255, 0.15);
    min-width: 220px;
  }

  .card-frame::before {
    content: '';
    position: absolute;
    top: -2px;
    left: -2px;
    right: -2px;
    bottom: -2px;
    background: linear-gradient(45deg, #d4af37, #ffd700, #d4af37);
    border-radius: 12px;
    z-index: -1;
    opacity: 0.9;
  }

  .card-number {
    font-family: 'Cinzel', serif;
    font-size: 4rem;
    font-weight: 700;
    color: #ffd700;
    margin-bottom: 0.8rem;
    text-shadow: 4px 4px 8px rgba(0, 0, 0, 0.8);
    line-height: 1;
  }

  .card-title {
    font-family: 'Cormorant Garamond', serif;
    font-size: 1.3rem;
    font-weight: 600;
    color: #f8f9fa;
    text-transform: uppercase;
    letter-spacing: 2.5px;
    opacity: 0.95;
  }

  /* Elegant royal seal */
  .royal-seal {
    position: absolute;
    top: 2rem;
    right: 2rem;
    width: 50px;
    height: 50px;
    display: flex;
    align-items: center;
    justify-content: center;
    animation: sealBounce 0.8s ease-out 0.6s both;
  }

  .seal-background {
    position: absolute;
    width: 100%;
    height: 100%;
    background: radial-gradient(circle, #8b2635 0%, #5d1a23 70%);
    border-radius: 50%;
    box-shadow: 
      0 4px 12px rgba(139, 38, 53, 0.4),
      inset 0 1px 2px rgba(255, 255, 255, 0.2);
  }

  .seal-text {
    font-size: 1.5rem;
    color: #ffd700;
    z-index: 1;
    text-shadow: 0 1px 2px rgba(0, 0, 0, 0.5);
  }
  
  /* Animation Keyframes */
  @keyframes parchmentAppear {
    from {
      opacity: 0;
      transform: scale(0.8) rotateY(-15deg);
    }
    to {
      opacity: 1;
      transform: scale(1) rotateY(0deg);
    }
  }

  @keyframes parchmentFade {
    from {
      opacity: 1;
      transform: scale(1);
    }
    to {
      opacity: 0;
      transform: scale(0.95);
    }
  }

  @keyframes sealBounce {
    0% {
      opacity: 0;
      transform: scale(0.3) rotate(-45deg);
    }
    50% {
      opacity: 1;
      transform: scale(1.2) rotate(0deg);
    }
    70% {
      transform: scale(0.9) rotate(5deg);
    }
    100% {
      opacity: 1;
      transform: scale(1) rotate(0deg);
    }
  }

  /* Responsive Design */
  @media (max-width: 768px) {
    .parchment-paper {
      padding: 2.5rem 2rem 2rem;
      min-height: 350px;
    }

    .announcement-title {
      font-size: 1.7rem;
      letter-spacing: 2px;
    }

    .decorative-line {
      width: 150px;
    }

    .player-name {
      font-size: 1.8rem;
      padding: 0.7rem 1.5rem;
      letter-spacing: 1px;
    }

    .decree-line {
      font-size: 1.5rem;
      margin: 1.2rem 0;
    }

    .card-number {
      font-size: 3.5rem;
    }

    .card-title {
      font-size: 1.1rem;
      letter-spacing: 2px;
    }

    .card-frame {
      padding: 1.8rem 2rem;
      min-width: 200px;
    }

    .royal-seal {
      width: 40px;
      height: 40px;
      top: 1.5rem;
      right: 1.5rem;
    }

    .seal-text {
      font-size: 1.2rem;
    }
  }

  @media (max-width: 480px) {
    .parchment-paper {
      padding: 2rem 1.5rem 1.5rem;
      min-height: 320px;
    }

    .announcement-title {
      font-size: 1.5rem;
      letter-spacing: 1.5px;
    }

    .decorative-line {
      width: 120px;
      margin: 0.8rem auto;
    }

    .player-name {
      font-size: 1.5rem;
      padding: 0.6rem 1.2rem;
      letter-spacing: 0.5px;
    }

    .decree-line {
      font-size: 1.3rem;
      margin: 1rem 0;
    }

    .card-number {
      font-size: 3rem;
    }

    .card-title {
      font-size: 1rem;
      letter-spacing: 1.5px;
    }

    .card-frame {
      padding: 1.5rem 1.8rem;
      min-width: 180px;
    }

    .royal-seal {
      width: 35px;
      height: 35px;
      top: 1.2rem;
      right: 1.2rem;
    }

    .seal-text {
      font-size: 1rem;
    }
  }
</style>
