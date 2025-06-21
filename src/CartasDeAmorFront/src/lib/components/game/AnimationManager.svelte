<!-- AnimationManager.svelte -->
<script lang="ts" context="module">
  import type { CardType } from '$lib/types/game-types';

  // Animation types
  export interface AnimationRequest {
    id: string;
    type: 'elimination' | 'showCard' | 'cardPlay' | 'custom';
    priority: number; // Higher priority animations interrupt lower priority ones
    data: any;
  }

  export interface EliminationAnimationData {
    playerName: string;
  }

  export interface ShowCardAnimationData {
    targetPlayerName: string;
    cardType: CardType;
    sourcePosition: { x: number; y: number; width?: number; height?: number };
  }
</script>

<script lang="ts">
  import { createEventDispatcher, onDestroy } from 'svelte';
  import EliminationAnimation from './EliminationAnimation.svelte';
  import ShowCardAnimation from './ShowCardAnimation.svelte';

  const dispatch = createEventDispatcher<{
    animationComplete: { id: string; type: string };
  }>();

  // Animation queue and state
  let animationQueue: AnimationRequest[] = [];
  let currentAnimation: AnimationRequest | null = null;
  let isAnimating = false;
  let animationTimeout: number | null = null;

  // Animation priorities (higher = more important)
  const ANIMATION_PRIORITIES = {
    elimination: 100,
    showCard: 80,
    cardPlay: 60,
    custom: 40
  } as const;

  // Maximum animation duration as fallback (in ms)
  const MAX_ANIMATION_DURATION = 10000;

  /**
   * Add an animation to the queue
   */
  export function queueAnimation(animationRequest: Omit<AnimationRequest, 'id' | 'priority'>) {
    const id = generateAnimationId();
    const priority = ANIMATION_PRIORITIES[animationRequest.type] || ANIMATION_PRIORITIES.custom;
    
    const animation: AnimationRequest = {
      id,
      priority,
      ...animationRequest
    };

    console.log(`🎬 Queuing animation:`, animation);

    // Check if we should interrupt current animation
    if (currentAnimation && animation.priority > currentAnimation.priority) {
      console.log(`⚡ Interrupting lower priority animation (${currentAnimation.priority} -> ${animation.priority})`);
      stopCurrentAnimation();
      
      // Add current animation back to queue if it wasn't completed
      if (currentAnimation) {
        animationQueue.unshift(currentAnimation);
      }
      
      // Start the high priority animation immediately
      currentAnimation = animation;
      startAnimation(animation);
    } else {
      // Add to queue based on priority
      const insertIndex = animationQueue.findIndex(a => a.priority < animation.priority);
      if (insertIndex === -1) {
        animationQueue.push(animation);
      } else {
        animationQueue.splice(insertIndex, 0, animation);
      }

      // Start animation if nothing is currently playing
      if (!isAnimating) {
        processQueue();
      }
    }

    return id;
  }

  /**
   * Cancel a specific animation
   */
  export function cancelAnimation(id: string): boolean {
    // Check if it's the current animation
    if (currentAnimation?.id === id) {
      console.log(`🛑 Cancelling current animation: ${id}`);
      stopCurrentAnimation();
      processQueue();
      return true;
    }

    // Check if it's in the queue
    const queueIndex = animationQueue.findIndex(a => a.id === id);
    if (queueIndex !== -1) {
      console.log(`🗑️ Removing animation from queue: ${id}`);
      animationQueue.splice(queueIndex, 1);
      return true;
    }

    return false;
  }

  /**
   * Clear all animations
   */
  export function clearAllAnimations() {
    console.log(`🧹 Clearing all animations`);
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
      queue: animationQueue.map(a => ({ id: a.id, type: a.type, priority: a.priority }))
    };
  }

  // Internal functions
  function generateAnimationId(): string {
    return `anim_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  function processQueue() {
    if (isAnimating || animationQueue.length === 0) {
      return;
    }

    currentAnimation = animationQueue.shift()!;
    startAnimation(currentAnimation);
  }

  function startAnimation(animation: AnimationRequest) {
    console.log(`▶️ Starting animation:`, animation);
    isAnimating = true;

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
      console.log(`⏹️ Stopping animation: ${currentAnimation.id}`);
      isAnimating = false;
      currentAnimation = null;
    }
  }

  function handleAnimationComplete(animationId: string) {
    if (currentAnimation?.id !== animationId) {
      console.warn(`⚠️ Received completion for non-current animation: ${animationId}`);
      return;
    }

    console.log(`✅ Animation completed: ${animationId}`);
    
    const completedAnimation = currentAnimation;
    stopCurrentAnimation();
    
    // Dispatch completion event
    dispatch('animationComplete', {
      id: completedAnimation.id,
      type: completedAnimation.type
    });

    // Process next animation in queue
    processQueue();
  }

  // Cleanup on component destroy
  onDestroy(() => {
    if (animationTimeout) {
      clearTimeout(animationTimeout);
    }
  });

  // Helper functions for specific animation types
  export function queueEliminationAnimation(data: EliminationAnimationData): string {
    return queueAnimation({
      type: 'elimination',
      data
    });
  }

  export function queueShowCardAnimation(data: ShowCardAnimationData): string {
    return queueAnimation({
      type: 'showCard',
      data
    });
  }
</script>

<!-- Render current animation based on type -->
{#if currentAnimation && isAnimating}
  <div class="animation-manager-overlay">
    {#if currentAnimation.type === 'elimination'}
      <EliminationAnimation
        playerName={currentAnimation.data.playerName}
        isVisible={true}
        on:animationComplete={() => handleAnimationComplete(currentAnimation?.id || '')}
      />
    {:else if currentAnimation.type === 'showCard'}
      <ShowCardAnimation
        targetPlayerName={currentAnimation.data.targetPlayerName}
        cardType={currentAnimation.data.cardType}
        sourcePosition={currentAnimation.data.sourcePosition}
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
        🎬 Animations: {isAnimating ? '▶️' : '⏸️'} | Queue: {animationQueue.length}
      </div>
      
      {#if currentAnimation}
        <div class="debug-current">
          Current: {currentAnimation.type} (#{currentAnimation.id.slice(-4)})
        </div>
      {/if}
      
      {#if animationQueue.length > 0}
        <div class="debug-queue">
          Queue: {animationQueue.map(a => `${a.type}(${a.priority})`).join(', ')}
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
