<!-- AnimationManager.svelte -->
<script lang="ts" context="module">
  import type { CardType } from '$lib/types/game-types';

  // Animation types
  export interface AnimationRequest {
    id: string;
    type: 'elimination' | 'showCard' | 'guessCard' | 'cardPlay' | 'drawCard' | 'peekCard' | 'roundWinners' | 'gameOver' | 'custom';
    data: any;
    callback?: () => void;
  }

  export interface EliminationAnimationData {
    playerName: string;
    center:  { x: number; y: number };
  }

  export interface ShowCardAnimationData {
    targetPlayerName: string;
    cardType: CardType;
    sourcePosition: { x: number; y: number; width?: number; height?: number };
    tableCenterPosition: { x: number; y: number };
  }

  export interface GuessCardAnimationData {
    invokerName: string;
    targetName: string;
    guessedCardType: CardType;
  }

  export interface CardPlayAnimationData {
    playerName: string;
    cardType: CardType;
    sourcePosition: { x: number; y: number; width?: number; height?: number };
    playedCardsPosition: { x: number; y: number; width?: number; height?: number };
    tableCenterPosition: { x: number; y: number };
  }

  export interface DrawCardAnimationData {
    playerName: string;
    deckPosition: { x: number; y: number; width?: number; height?: number };
    playerPosition: { x: number; y: number; width?: number; height?: number };
  }

  export interface PeekCardAnimationData {
    invokerName: string;
    targetName: string;
    targetPosition: { x: number; y: number; width?: number; height?: number };
    invokerPosition: { x: number; y: number; width?: number; height?: number };
  }

  export interface RoundWinnersAnimationData {
    animationCenter: { x: number; y: number };
    winnerNames: string[];
  }

  export interface GameOverAnimationData {
    winnerNames: string[];
    winnerEmails: string[];
    currentUserEmail: string;
  }
</script>

<script lang="ts">
  import { createEventDispatcher, onDestroy, tick } from 'svelte';
  import EliminationAnimation from './EliminationAnimation.svelte';
  import ShowCardAnimation from './ShowCardAnimation.svelte';
  import GuessCardAnimation from './GuessCardAnimation.svelte';
  import CardPlayAnimation from './CardPlayAnimation.svelte';
  import DrawCardAnimation from './DrawCardAnimation.svelte';
  import PeekCardAnimation from './PeekCardAnimation.svelte';
  import RoundWinnersAnimation from './RoundWinnersAnimation.svelte';
  import GameOverAnimation from './GameOverAnimation.svelte';
  import settings from '$lib/stores/settingsStore';

  const dispatch = createEventDispatcher<{
    animationComplete: { id: string; type: string };
    animationStateChange: { isAnimating: boolean; currentAnimation: string | null };
  }>();

  // Animation queue and state
  let animationQueue: AnimationRequest[] = [];
  let currentAnimation: AnimationRequest | null = null;
  let isAnimating = false;
  let animationTimeout: number | null = null;
  
  // Export animation state for reactive access
  export { isAnimating };
  export let currentAnimationType: string | null = null;
  
  // Subscribe to settings store for animations enabled state
  $: animationsEnabled = $settings.animationsEnabled;
  
  // Update current animation type reactively
  $: currentAnimationType = currentAnimation?.type || null;
  
  // Dispatch state changes
  $: {
    dispatch('animationStateChange', {
      isAnimating,
      currentAnimation: currentAnimationType
    });
  }

  // Maximum animation duration as fallback (in ms)
  const MAX_ANIMATION_DURATION = 10000;

  /**
   * Add an animation to the queue
   */
  export function queueAnimation(animationRequest: Omit<AnimationRequest, 'id'>) {
    const id = generateAnimationId();
    
    const animation: AnimationRequest = {
      id,
      ...animationRequest
    };

    // If animations are disabled, immediately complete the animation without showing it
    if (!animationsEnabled) {
      // Execute callback if provided
      if (animation.callback) {
        try {
          animation.callback();
        } catch (error) {
          console.error('Error executing animation callback (animations disabled):', error);
        }
      }
      
      // Dispatch completion event immediately
      setTimeout(() => {
        dispatch('animationComplete', {
          id: animation.id,
          type: animation.type
        });
      }, 0);
      return id;
    }

    // Add to end of queue (FIFO - first in, first out)
    animationQueue.push(animation);

    // Start animation if nothing is currently playing
    if (!isAnimating) {
      processQueue();
    }

    return id;
  }

  /**
   * Cancel a specific animation
   */
  export function cancelAnimation(id: string): boolean {
    // Check if it's the current animation
    if (currentAnimation?.id === id) {
      stopCurrentAnimation();
      processQueue();
      return true;
    }

    // Check if it's in the queue
    const queueIndex = animationQueue.findIndex(a => a.id === id);
    if (queueIndex !== -1) {
      animationQueue.splice(queueIndex, 1);
      return true;
    }

    return false;
  }

  /**
   * Clear all animations
   */
  export function clearAllAnimations() {
    stopCurrentAnimation();
    animationQueue = [];
  }

  /**
   * Get current animation status
   */
  export function getStatus() {
    return {
      isAnimating,
      currentAnimation: currentAnimation?.id || null,
      queueLength: animationQueue.length,
      queue: animationQueue.map(a => ({ id: a.id, type: a.type }))
    };
  }

  // Internal functions
  function generateAnimationId(): string {
    return `anim_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  async function processQueue() {
    if (isAnimating || animationQueue.length === 0) {
      return;
    }

    // Set isAnimating IMMEDIATELY to prevent race conditions
    isAnimating = true;
    
    currentAnimation = animationQueue.shift()!;
    
    // Force a tick to ensure Svelte has updated the DOM
    await tick();
    
    startAnimation(currentAnimation);
  }

  function startAnimation(animation: AnimationRequest) {
    // Set a fallback timeout to prevent stuck animations
    animationTimeout = window.setTimeout(() => {
      console.warn(`⏰ Animation timed out: ${animation.id}`);
      handleAnimationComplete(animation.id);
    }, MAX_ANIMATION_DURATION);
  }

  function stopCurrentAnimation() {
    if (animationTimeout) {
      clearTimeout(animationTimeout);
      animationTimeout = null;
    }
    
    if (currentAnimation) {
      isAnimating = false;
      currentAnimation = null;
    }
  }

  async function handleAnimationComplete(animationId: string) {
    // Guard against empty or invalid animation IDs
    if (!animationId) {
      return;
    }
    
    if (currentAnimation?.id !== animationId) {
      return;
    }

    const completedAnimation = currentAnimation;
    stopCurrentAnimation();
    
    // Execute callback if provided
    if (completedAnimation.callback) {
      try {
        completedAnimation.callback();
      } catch (error) {
        console.error('Error executing animation callback:', error);
      }
    }
    
    // Dispatch completion event
    dispatch('animationComplete', {
      id: completedAnimation.id,
      type: completedAnimation.type
    });

    // Process next animation in queue
    await tick();
    processQueue();
  }

  // Cleanup on component destroy
  onDestroy(() => {
    if (animationTimeout) {
      clearTimeout(animationTimeout);
    }
  });

  // Helper functions for specific animation types
  export function queueEliminationAnimation(data: EliminationAnimationData, callback?: () => void): string {
    return queueAnimation({
      type: 'elimination',
      data,
      callback
    });
  }

  export function queueShowCardAnimation(data: ShowCardAnimationData, callback?: () => void): string {
    return queueAnimation({
      type: 'showCard',
      data,
      callback
    });
  }

  export function queueGuessCardAnimation(data: GuessCardAnimationData, callback?: () => void): string {
    return queueAnimation({
      type: 'guessCard',
      data,
      callback
    });
  }

  export function queueCardPlayAnimation(data: CardPlayAnimationData, callback?: () => void): string {
    return queueAnimation({
      type: 'cardPlay',
      data,
      callback
    });
  }

  export function queueDrawCardAnimation(data: DrawCardAnimationData, callback?: () => void): string {
    return queueAnimation({
      type: 'drawCard',
      data,
      callback
    });
  }

  export function queuePeekCardAnimation(data: PeekCardAnimationData, callback?: () => void): string {
    return queueAnimation({
      type: 'peekCard',
      data,
      callback
    });
  }

  export function queueRoundWinnersAnimation(data: RoundWinnersAnimationData, callback?: () => void): string {
    return queueAnimation({
      type: 'roundWinners',
      data,
      callback
    });
  }

  export function queueGameOverAnimation(data: GameOverAnimationData, callback?: () => void): string {
    return queueAnimation({
      type: 'gameOver',
      data,
      callback
    });
  }
</script>

<!-- Render current animation based on type -->
{#if currentAnimation && isAnimating && animationsEnabled}
  <div class="animation-manager-overlay">
    {#if currentAnimation.type === 'elimination'}
      <EliminationAnimation
        playerName={currentAnimation.data.playerName}
        center={currentAnimation.data.center}
        isVisible={true}
        on:animationComplete={() => handleAnimationComplete(currentAnimation?.id || '')}
      />
    {:else if currentAnimation.type === 'showCard'}
      <ShowCardAnimation
        targetPlayerName={currentAnimation.data.targetPlayerName}
        cardType={currentAnimation.data.cardType}
        sourcePosition={currentAnimation.data.sourcePosition}
        tableCenterPosition={currentAnimation.data.tableCenterPosition}
        isVisible={true}
        on:animationComplete={() => handleAnimationComplete(currentAnimation?.id || '')}
      />
    {:else if currentAnimation.type === 'guessCard'}
      <GuessCardAnimation
        invokerName={currentAnimation.data.invokerName}
        targetName={currentAnimation.data.targetName}
        guessedCardType={currentAnimation.data.guessedCardType}
        isVisible={true}
        on:animationComplete={() => handleAnimationComplete(currentAnimation?.id || '')}
      />
    {:else if currentAnimation.type === 'cardPlay'}
      <CardPlayAnimation
        playerName={currentAnimation.data.playerName}
        cardType={currentAnimation.data.cardType}
        sourcePosition={currentAnimation.data.sourcePosition}
        playedCardsPosition={currentAnimation.data.playedCardsPosition}
        tableCenterPosition={currentAnimation.data.tableCenterPosition}
        isVisible={true}
        on:animationComplete={() => handleAnimationComplete(currentAnimation?.id || '')}
      />
    {:else if currentAnimation.type === 'drawCard'}
      <DrawCardAnimation
        playerName={currentAnimation.data.playerName}
        deckPosition={currentAnimation.data.deckPosition}
        playerPosition={currentAnimation.data.playerPosition}
        isVisible={true}
        on:animationComplete={() => handleAnimationComplete(currentAnimation?.id || '')}
      />
    {:else if currentAnimation.type === 'peekCard'}
      <PeekCardAnimation
        invokerName={currentAnimation.data.invokerName}
        targetName={currentAnimation.data.targetName}
        targetPosition={currentAnimation.data.targetPosition}
        invokerPosition={currentAnimation.data.invokerPosition}
        isVisible={true}
        on:animationComplete={() => handleAnimationComplete(currentAnimation?.id || '')}
      />
    {:else if currentAnimation.type === 'roundWinners'}
      <RoundWinnersAnimation
        center={currentAnimation.data.animationCenter}
        winnerNames={currentAnimation.data.winnerNames}
        isVisible={true}
        on:animationComplete={() => handleAnimationComplete(currentAnimation?.id || '')}
      />
    {:else if currentAnimation.type === 'gameOver'}
      <GameOverAnimation
        winnerNames={currentAnimation.data.winnerNames}
        winnerEmails={currentAnimation.data.winnerEmails}
        currentUserEmail={currentAnimation.data.currentUserEmail}
        isVisible={true}
        on:animationComplete={() => handleAnimationComplete(currentAnimation?.id || '')}
      />
    {/if}
  </div>
{/if}

<!-- Debug information (only in development) -->
{#if import.meta.env.DEV}
  <div class="animation-debug" class:visible={isAnimating || animationQueue.length > 0}>
    <div class="debug-content">
      <div class="debug-status">
        🎬 Animations: {animationsEnabled ? (isAnimating ? '▶️' : '⏸️') : '🚫'} | Queue: {animationQueue.length}
      </div>
      
      {#if currentAnimation}
        <div class="debug-current">
          Current: {currentAnimation.type} (#{currentAnimation.id.slice(-4)})
        </div>
      {/if}
      
      {#if animationQueue.length > 0}
        <div class="debug-queue">
          Queue: {animationQueue.map(a => a.type).join(', ')}
        </div>
      {/if}
    </div>
  </div>
{/if}

<style>
  .animation-manager-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100vw;
    height: 100vh;
    pointer-events: none;
    z-index: 2000;
  }

  /* Debug styles (only in development) */
  .animation-debug {
    position: fixed;
    top: 10px;
    right: 10px;
    background: rgba(0, 0, 0, 0.8);
    color: white;
    padding: 8px 12px;
    border-radius: 6px;
    font-family: 'Courier New', monospace;
    font-size: 0.7rem;
    z-index: 9999;
    opacity: 0;
    transition: opacity 0.3s ease;
    pointer-events: none;
    max-width: 250px;
  }

  .animation-debug.visible {
    opacity: 1;
  }

  .debug-content {
    display: flex;
    flex-direction: column;
    gap: 2px;
  }

  .debug-status {
    color: #4CAF50;
    font-weight: bold;
  }

  .debug-current {
    color: #FF9800;
  }

  .debug-queue {
    color: #2196F3;
    font-size: 0.6rem;
    word-break: break-all;
  }

  /* Production - hide debug */
  @media (prefers-reduced-motion: reduce) {
    .animation-debug {
      display: none;
    }
  }
</style>
