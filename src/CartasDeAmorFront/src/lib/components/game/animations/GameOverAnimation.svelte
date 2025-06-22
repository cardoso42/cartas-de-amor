<!-- GameOverAnimation.svelte -->
<script lang="ts">
  import { onMount, createEventDispatcher } from 'svelte';

  export let winnerNames: string[] = [];
  export let winnerEmails: string[] = [];
  export let currentUserEmail: string = '';
  export let isVisible: boolean = false;

  const dispatch = createEventDispatcher<{
    animationComplete: null;
  }>();

  let animationComplete = false;
  let hasStarted = false;
  
  onMount(() => {
    if (isVisible && !hasStarted) {
      startAnimation();
    }
  });

  async function startAnimation() {
    if (hasStarted) return;
    hasStarted = true;
        
    // Wait for the full animation duration (12 seconds)
    await new Promise(resolve => setTimeout(resolve, 12000));
    
    if (!animationComplete) {
      animationComplete = true;
      dispatch('animationComplete');
    }
  }

  $: if (isVisible && !hasStarted && !animationComplete) {
    startAnimation();
  }

  $: displayText = winnerNames.length === 0 
    ? 'Unknown Champion' 
    : winnerNames.length === 1 
      ? winnerNames[0] 
      : winnerNames.join(' & ');

  // Check if current user is among the winners
  $: isCurrentUserWinner = winnerEmails.includes(currentUserEmail);
  
  // Personalized congratulations message
  $: congratulationsText = isCurrentUserWinner 
    ? 'Congratulations on your victory!' 
    : `Congratulations to ${winnerNames.length > 1 ? 'the winners' : 'the winner'}!`;
</script>

{#if isVisible && !animationComplete}
  <div class="game-over-overlay">
    <!-- Background with golden glow -->
    <div class="background-glow"></div>
    
    <!-- Confetti particles -->
    {#each Array(50) as _, i}
      <div 
        class="confetti"
        style="
          left: {Math.random() * 100}%;
          animation-delay: {Math.random() * 3}s;
          --color: hsl({Math.random() * 360}, 70%, 60%);
          --rotation: {Math.random() * 360}deg;
        "
      ></div>
    {/each}
    
    <!-- Main celebration content -->
    <div class="celebration-content">
      <!-- Crown and trophy -->
      <div class="celebration-icons">
        <div class="crown">👑</div>
        <div class="trophy">🏆</div>
      </div>
      
      <!-- Main announcement -->
      <div class="announcement-card">
        <h1 class="game-over-title">🎉 GAME OVER! 🎉</h1>
        <div class="winner-section">
          <div class="winner-label">
            {winnerNames.length > 1 ? 'Champions' : 'Champion'}:
          </div>
          <div class="winner-names">{displayText}</div>
        </div>
        <div class="congratulations" class:user-won={isCurrentUserWinner}>{congratulationsText}</div>
      </div>
      
      <!-- Sparkle effects -->
      {#each Array(20) as _, i}
        <div 
          class="sparkle"
          style="
            left: {Math.random() * 100}%;
            top: {Math.random() * 100}%;
            animation-delay: {Math.random() * 4}s;
          "
        ></div>
      {/each}
    </div>
  </div>
{/if}

<style>
  .game-over-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100vw;
    height: 100vh;
    z-index: 3000;
    pointer-events: none;
    display: flex;
    align-items: center;
    justify-content: center;
    animation: overlayAppear 1s ease-out forwards;
  }

  .background-glow {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: radial-gradient(
      circle at center,
      rgba(255, 215, 0, 0.3) 0%,
      rgba(255, 193, 7, 0.2) 40%,
      rgba(156, 39, 176, 0.1) 80%,
      transparent 100%
    );
    animation: backgroundPulse 4s ease-in-out infinite;
  }

  .confetti {
    position: absolute;
    width: 8px;
    height: 8px;
    background: var(--color);
    animation: confettiFall 6s linear infinite;
    transform: rotate(var(--rotation));
  }

  .celebration-content {
    position: relative;
    text-align: center;
    animation: contentSlideUp 1.5s ease-out 0.5s forwards;
    opacity: 0;
    transform: translateY(50px);
  }

  .celebration-icons {
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 2rem;
    margin-bottom: 2rem;
    animation: iconsZoom 2s ease-out 1s forwards;
    opacity: 0;
    transform: scale(0);
  }

  .crown {
    font-size: 4rem;
    text-shadow: 0 0 20px rgba(255, 215, 0, 0.8);
    animation: bounce 2s ease-in-out infinite 2s;
  }

  .trophy {
    font-size: 3.5rem;
    text-shadow: 0 0 15px rgba(255, 193, 7, 0.6);
    animation: float 3s ease-in-out infinite 2.5s;
  }

  .announcement-card {
    background: rgba(255, 255, 255, 0.95);
    border: 3px solid #ffd700;
    border-radius: 20px;
    padding: 3rem 4rem;
    box-shadow: 
      0 20px 50px rgba(0, 0, 0, 0.3),
      inset 0 0 40px rgba(255, 215, 0, 0.2);
    backdrop-filter: blur(10px);
    max-width: 600px;
    margin: 0 auto;
  }

  .game-over-title {
    color: #9c27b0;
    font-size: 3rem;
    font-weight: bold;
    margin: 0 0 2rem 0;
    text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.2);
    animation: titleGlow 3s ease-in-out infinite 2s;
  }

  .winner-section {
    margin: 2rem 0;
  }

  .winner-label {
    color: #d4af37;
    font-size: 1.3rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 2px;
    margin-bottom: 1rem;
  }

  .winner-names {
    color: #333;
    font-size: 2.2rem;
    font-weight: bold;
    line-height: 1.3;
    text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.1);
    margin-bottom: 1.5rem;
  }

  .congratulations {
    color: #666;
    font-size: 1.1rem;
    font-style: italic;
    opacity: 0;
    animation: fadeInText 1.5s ease-out 4s forwards;
  }

  .congratulations.user-won {
    color: #d4af37;
    font-size: 1.3rem;
    font-weight: 600;
    text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.2);
    animation: fadeInTextGlow 1.5s ease-out 4s forwards;
  }

  .sparkle {
    position: absolute;
    width: 4px;
    height: 4px;
    background: #ffd700;
    border-radius: 50%;
    animation: sparkleShine 3s ease-in-out infinite;
    box-shadow: 0 0 6px #ffd700;
  }

  /* Keyframe animations */
  @keyframes overlayAppear {
    from {
      opacity: 0;
    }
    to {
      opacity: 1;
    }
  }

  @keyframes backgroundPulse {
    0%, 100% {
      opacity: 1;
    }
    50% {
      opacity: 0.7;
    }
  }

  @keyframes confettiFall {
    0% {
      transform: translateY(-100vh) rotate(var(--rotation));
      opacity: 1;
    }
    100% {
      transform: translateY(100vh) rotate(calc(var(--rotation) + 360deg));
      opacity: 0;
    }
  }

  @keyframes contentSlideUp {
    to {
      opacity: 1;
      transform: translateY(0);
    }
  }

  @keyframes iconsZoom {
    to {
      opacity: 1;
      transform: scale(1);
    }
  }

  @keyframes bounce {
    0%, 100% {
      transform: translateY(0);
    }
    50% {
      transform: translateY(-10px);
    }
  }

  @keyframes float {
    0%, 100% {
      transform: translateY(0) rotate(0deg);
    }
    50% {
      transform: translateY(-8px) rotate(5deg);
    }
  }

  @keyframes titleGlow {
    0%, 100% {
      text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.2);
    }
    50% {
      text-shadow: 
        2px 2px 4px rgba(0, 0, 0, 0.2),
        0 0 20px rgba(156, 39, 176, 0.4),
        0 0 30px rgba(255, 215, 0, 0.3);
    }
  }

  @keyframes fadeInText {
    to {
      opacity: 1;
    }
  }

  @keyframes fadeInTextGlow {
    to {
      opacity: 1;
      text-shadow: 
        1px 1px 2px rgba(0, 0, 0, 0.2),
        0 0 10px rgba(212, 175, 55, 0.5),
        0 0 20px rgba(212, 175, 55, 0.3);
    }
  }

  @keyframes sparkleShine {
    0%, 100% {
      opacity: 0;
      transform: scale(0);
    }
    50% {
      opacity: 1;
      transform: scale(1);
    }
  }
</style>
