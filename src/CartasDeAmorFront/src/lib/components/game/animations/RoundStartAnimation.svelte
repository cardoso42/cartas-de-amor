<!-- RoundStartAnimation.svelte -->
<script lang="ts">
  import { onMount, createEventDispatcher } from 'svelte';
  import { _ } from 'svelte-i18n';

  export let players: Array<{
    name: string;
    position: { x: number; y: number; width?: number; height?: number };
    hasCards: boolean;
  }>;
  export let deckPosition: { x: number; y: number; width?: number; height?: number };
  export let tableCenter: { x: number; y: number };
  export let isVisible: boolean = false;

  const dispatch = createEventDispatcher<{
    animationComplete: null;
  }>();

  let animationContainer: HTMLElement;
  let animationComplete = false;
  let hasStarted = false;

  // Animation phases
  let phase: 'cardsReturn' | 'shuffling' | 'dealing' | 'complete' = 'cardsReturn';

  // Animated cards tracking
  let animatedCards: Array<{
    id: string;
    startX: number;
    startY: number;
    startWidth: number;
    startHeight: number;
    targetX: number;
    targetY: number;
    targetWidth: number;
    targetHeight: number;
    phase: 'returning' | 'dealing';
    delay: number;
  }> = [];

  onMount(() => {
    if (isVisible) {
      startAnimation();
    }
  });

  async function startAnimation() {
    if (hasStarted) return; // Prevent duplicate starts
    hasStarted = true;

    // Phase 1: Cards return to deck (1.5s)
    phase = 'cardsReturn';
    await new Promise(resolve => setTimeout(resolve, 1500));

    // Phase 2: Deck shuffling animation (1s)
    phase = 'shuffling';
    await new Promise(resolve => setTimeout(resolve, 1000));

    // Phase 3: Deal cards to players (2s)
    phase = 'dealing';
    await new Promise(resolve => setTimeout(resolve, 2000));

    // Phase 4: Brief pause (0.5s)
    phase = 'complete';
    await new Promise(resolve => setTimeout(resolve, 500));

    // Mark animation as complete and dispatch event
    if (!animationComplete) {
      animationComplete = true;
      dispatch('animationComplete');
    }
  }

  $: if (isVisible && !animationComplete && !hasStarted) {
    startAnimation();
  }

  // Create animated cards for players who had cards
  $: if (isVisible && phase === 'cardsReturn') {
    animatedCards = players
      .filter(player => player.hasCards)
      .map((player, index) => ({
        id: `return-${player.name}-${index}`,
        startX: player.position.x,
        startY: player.position.y,
        startWidth: player.position.width || 65,
        startHeight: player.position.height || 90,
        targetX: deckPosition.x,
        targetY: deckPosition.y,
        targetWidth: deckPosition.width || 70,
        targetHeight: deckPosition.height || 98,
        phase: 'returning',
        delay: index * 0.1
      }));
  }

  // Create animated cards for dealing to all players
  $: if (isVisible && phase === 'dealing') {
    animatedCards = players.map((player, index) => ({
      id: `deal-${player.name}-${index}`,
      startX: deckPosition.x,
      startY: deckPosition.y,
      startWidth: deckPosition.width || 70,
      startHeight: deckPosition.height || 98,
      targetX: player.position.x,
      targetY: player.position.y,
      targetWidth: player.position.width || 65,
      targetHeight: player.position.height || 90,
      phase: 'dealing',
      delay: index * 0.2
    }));
  }
</script>

{#if isVisible && !animationComplete}
  <div 
    class="animation-overlay" 
    bind:this={animationContainer}
  >
    <!-- Animated cards -->
    {#each animatedCards as card (card.id)}
      <div 
        class="animated-card {card.phase}"
        style="
          --start-x: {card.startX}px;
          --start-y: {card.startY}px;
          --start-width: {card.startWidth}px;
          --start-height: {card.startHeight}px;
          --target-x: {card.targetX}px;
          --target-y: {card.targetY}px;
          --target-width: {card.targetWidth}px;
          --target-height: {card.targetHeight}px;
          animation-delay: {card.delay}s;
        "
      >
        <!-- Card back (since cards are face down during this animation) -->
        <div class="card-face">
          <div class="card-back-pattern"></div>
        </div>
      </div>
    {/each}

    <!-- Deck shuffling effect -->
    {#if phase === 'shuffling'}
      <div 
        class="deck-shuffle-effect"
        style="
          left: {deckPosition.x}px;
          top: {deckPosition.y}px;
        "
      >
        <div class="shuffle-particles">
          {#each Array.from({ length: 12 }, (_, i) => i) as i}
            <div 
              class="particle" 
              style="
                --delay: {i * 0.1}s;
                --random-x: {Math.random()};
                --random-y: {Math.random()};
              "
            ></div>
          {/each}
        </div>
      </div>
    {/if}

    <!-- Action text overlay -->
    <div 
      class="action-text"
      style="
        left: {tableCenter.x}px;
        top: {tableCenter.y - 100}px;
      "
    >
      {#if phase === 'cardsReturn'}
        <div class="phase-text">{$_('game.cardsReturnToDeck')}</div>
      {:else if phase === 'shuffling'}
        <div class="phase-text shuffle-text">
          <span>🔀</span> {$_('game.shufflingDeck')}
        </div>
      {:else if phase === 'dealing'}
        <div class="phase-text">{$_('game.dealingNewCards')}</div>
      {/if}
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
    pointer-events: none;
    z-index: 1000;
    background: rgba(0, 0, 0, 0.4);
    backdrop-filter: blur(2px);
  }

  .animated-card {
    position: absolute;
    transform: translate(-50%, -50%);
    transform-style: preserve-3d;
    transition: none;
  }

  /* Returning phase: cards travel from players to deck */
  .animated-card.returning {
    left: var(--start-x);
    top: var(--start-y);
    width: var(--start-width);
    height: var(--start-height);
    animation: returnToDeck 1.5s cubic-bezier(0.25, 0.46, 0.45, 0.94) forwards;
  }

  /* Dealing phase: cards travel from deck to players */
  .animated-card.dealing {
    left: var(--start-x);
    top: var(--start-y);
    width: var(--start-width);
    height: var(--start-height);
    animation: dealToPlayer 2s cubic-bezier(0.25, 0.46, 0.45, 0.94) forwards;
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

  .deck-shuffle-effect {
    position: absolute;
    transform: translate(-50%, -50%);
    z-index: 1001;
  }

  .shuffle-particles {
    position: relative;
    width: 100%;
    height: 100%;
  }

  .particle {
    position: absolute;
    width: 8px;
    height: 8px;
    background: #ffd700;
    border-radius: 50%;
    opacity: 0;
    animation: shuffleParticle 1s ease-in-out infinite;
    animation-delay: var(--delay);
  }

  .action-text {
    position: absolute;
    transform: translateX(-50%);
    text-align: center;
    color: white;
    z-index: 1002;
    animation: textSlideIn 0.5s ease-out;
  }

  .phase-text {
    font-size: 1.4rem;
    font-weight: 600;
    text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.8);
    background: rgba(0, 0, 0, 0.6);
    padding: 0.8rem 1.5rem;
    border-radius: 12px;
    border: 1px solid rgba(255, 255, 255, 0.2);
  }

  .shuffle-text {
    background: linear-gradient(135deg, rgba(156, 39, 176, 0.8) 0%, rgba(123, 31, 162, 0.8) 100%);
    animation: shuffleGlow 1s ease-in-out infinite;
  }

  .shuffle-text span {
    font-size: 1.6rem;
    margin-right: 0.5rem;
    display: inline-block;
    animation: shuffleRotate 0.5s ease-in-out infinite;
  }

  /* Animation keyframes */
  @keyframes returnToDeck {
    from {
      left: var(--start-x);
      top: var(--start-y);
      width: var(--start-width);
      height: var(--start-height);
      transform: translate(-50%, -50%) rotate(0deg);
      opacity: 1;
    }
    to {
      left: var(--target-x);
      top: var(--target-y);
      width: var(--target-width);
      height: var(--target-height);
      transform: translate(-50%, -50%) rotate(5deg);
      opacity: 0.8;
    }
  }

  @keyframes dealToPlayer {
    0% {
      left: var(--start-x);
      top: var(--start-y);
      width: var(--start-width);
      height: var(--start-height);
      transform: translate(-50%, -50%) rotate(0deg);
      opacity: 1;
    }
    80% {
      opacity: 1;
    }
    100% {
      left: var(--target-x);
      top: var(--target-y);
      width: var(--target-width);
      height: var(--target-height);
      transform: translate(-50%, -50%) rotate(-2deg);
      opacity: 0.9;
    }
  }

  @keyframes shuffleParticle {
    0%, 100% {
      opacity: 0;
      transform: translate(0, 0) scale(0.5);
    }
    50% {
      opacity: 1;
      transform: translate(
        calc(var(--random-x, 0) * 40px - 20px), 
        calc(var(--random-y, 0) * 40px - 20px)
      ) scale(1);
    }
  }

  @keyframes shuffleGlow {
    0%, 100% {
      box-shadow: 0 0 20px rgba(156, 39, 176, 0.6);
    }
    50% {
      box-shadow: 0 0 30px rgba(156, 39, 176, 0.9);
    }
  }

  @keyframes shuffleRotate {
    0%, 100% {
      transform: rotate(0deg);
    }
    50% {
      transform: rotate(180deg);
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

  /* Create random positions for particles */
  .particle:nth-child(1) { --random-x: 0.2; --random-y: 0.8; }
  .particle:nth-child(2) { --random-x: 0.7; --random-y: 0.3; }
  .particle:nth-child(3) { --random-x: 0.1; --random-y: 0.5; }
  .particle:nth-child(4) { --random-x: 0.9; --random-y: 0.7; }
  .particle:nth-child(5) { --random-x: 0.4; --random-y: 0.1; }
  .particle:nth-child(6) { --random-x: 0.6; --random-y: 0.9; }
  .particle:nth-child(7) { --random-x: 0.3; --random-y: 0.4; }
  .particle:nth-child(8) { --random-x: 0.8; --random-y: 0.6; }
</style>
