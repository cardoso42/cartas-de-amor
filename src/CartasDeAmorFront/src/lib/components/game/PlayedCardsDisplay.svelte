<script lang="ts">
  import type { CardType } from '$lib/types/game-types';
  
  export let playedCards: CardType[] = [];
  export let playerName: string = '';
  export let getCardName: (cardType: CardType) => string;

  // TODO: When playing a card against a protected target, the card is not shown in the played cards FIX IT

  const usualCardCount = 3; // Default number of cards to show normally
  const maxVisibleCards = 5; // Maximum number of cards to show on hover
  
  let isExpanded = false;
  let isHovering = false;
  
  // Show more cards on hover (but not all to avoid clutter)
  $: visibleCards = isExpanded 
    ? playedCards 
    : isHovering 
      ? playedCards.slice(-maxVisibleCards)
      : playedCards.slice(-usualCardCount);
  
  function toggleExpanded() {
    isExpanded = !isExpanded;
  }
  
  function handleMouseEnter() {
    if (!isExpanded) {
      isHovering = true;
    }
  }
  
  function handleMouseLeave() {
    isHovering = false;
  }
</script>

<div 
  class="played-cards-container"
  class:expanded={isExpanded}
  on:mouseenter={handleMouseEnter}
  on:mouseleave={handleMouseLeave}
  role="button"
  tabindex="0"
  on:click={toggleExpanded}
  on:keydown={(e) => e.key === 'Enter' && toggleExpanded()}
  title="Click to {isExpanded ? 'collapse' : 'expand'} all played cards for {playerName}"
>
  <!-- Card count indicator -->
  {#if playedCards.length > usualCardCount}
    <div class="card-count-badge">
      {playedCards.length}
    </div>
  {/if}
  
  <!-- Cards display -->
  <div class="played-cards" class:scrollable={isExpanded}>
    {#each visibleCards as playedCard, cardIndex}
      <div 
        class="card played-card face-up" 
        style="margin-left: {isExpanded ? 0 : cardIndex * 6}px;"
        title="{getCardName(playedCard)} (played {visibleCards.length - cardIndex} turn{visibleCards.length - cardIndex === 1 ? '' : 's'} ago)"
      >
        <div class="card-content">
          <div class="card-number">{playedCard}</div>
          <div class="card-name">{getCardName(playedCard)}</div>
        </div>
      </div>
    {/each}
  </div>
  
  <!-- Expansion indicator -->
  {#if playedCards.length > usualCardCount && !isExpanded}
    <div class="expansion-hint">
      <span class="expand-icon">⋯</span>
      <span class="expand-text">+{playedCards.length - (isHovering ? maxVisibleCards : usualCardCount)} more</span>
    </div>
  {/if}
  
  <!-- Collapse button when expanded -->
  {#if isExpanded}
    <div class="collapse-button">
      <span class="collapse-icon">⤴</span>
    </div>
  {/if}
</div>

<style>
  .played-cards-container {
    position: relative;
    cursor: pointer;
    transition: all 0.3s ease;
    background: rgba(0, 0, 0, 0.1);
    border-radius: 8px;
    padding: 4px;
    max-width: 120px;
  }
  
  .played-cards-container:hover {
    background: rgba(0, 0, 0, 0.2);
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
  }
  
  .played-cards-container.expanded {
    background: rgba(0, 0, 0, 0.85);
    backdrop-filter: blur(5px);
    border: 2px solid rgba(255, 215, 0, 0.6);
    max-width: 300px;
    max-height: 200px;
    z-index: 20;
    box-shadow: 0 8px 25px rgba(0, 0, 0, 0.7);
  }
  
  .played-cards-container:focus {
    outline: 2px solid #ffd700;
    outline-offset: 2px;
  }
  
  .card-count-badge {
    position: absolute;
    top: -8px;
    right: -8px;
    background: #ff4444;
    color: white;
    border-radius: 50%;
    width: 20px;
    height: 20px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 0.7rem;
    font-weight: bold;
    z-index: 10;
    border: 2px solid white;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
  }
  
  .played-cards {
    display: flex;
    gap: 2px;
    justify-content: center;
    align-items: center;
    transition: all 0.3s ease;
  }
  
  .played-cards.scrollable {
    flex-wrap: wrap;
    gap: 4px;
    max-height: 150px;
    overflow-y: auto;
    overflow-x: hidden;
    padding: 2px;
  }
  
  .played-cards.scrollable::-webkit-scrollbar {
    width: 4px;
  }
  
  .played-cards.scrollable::-webkit-scrollbar-track {
    background: rgba(255, 255, 255, 0.1);
    border-radius: 2px;
  }
  
  .played-cards.scrollable::-webkit-scrollbar-thumb {
    background: rgba(255, 215, 0, 0.6);
    border-radius: 2px;
  }
  
  .played-cards.scrollable::-webkit-scrollbar-thumb:hover {
    background: rgba(255, 215, 0, 0.8);
  }
  
  .played-card {
    width: 32px;
    height: 45px;
    background: linear-gradient(135deg, #fafafa 0%, #e0e0e0 100%);
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    padding: 2px;
    border-radius: 4px;
    border: 1px solid #333;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.3);
    transition: transform 0.2s ease;
    flex-shrink: 0;
  }
  
  .expanded .played-card {
    width: 40px;
    height: 56px;
    padding: 3px;
    margin: 1px;
  }
  
  .played-card:hover {
    transform: scale(1.1);
    z-index: 5;
    box-shadow: 0 3px 8px rgba(0, 0, 0, 0.4);
  }
  
  .expanded .played-card:hover {
    transform: scale(1.15);
  }
  
  .card-content {
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    height: 100%;
  }
  
  .card-number {
    font-size: 0.7rem;
    font-weight: bold;
    color: #9c27b0;
    text-align: center;
    line-height: 1;
  }
  
  .expanded .card-number {
    font-size: 0.8rem;
  }
  
  .card-name {
    font-size: 0.35rem;
    font-weight: bold;
    color: #333;
    text-align: center;
    line-height: 1;
  }
  
  .expanded .card-name {
    font-size: 0.4rem;
  }
  
  .expansion-hint {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 2px;
    margin-top: 2px;
    color: rgba(255, 255, 255, 0.8);
    font-size: 0.6rem;
    text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.8);
  }
  
  .expand-icon {
    font-size: 0.8rem;
    animation: pulse 2s ease-in-out infinite;
  }
  
  .expand-text {
    font-weight: bold;
  }
  
  @keyframes pulse {
    0%, 100% { opacity: 0.8; }
    50% { opacity: 1; }
  }
  
  .collapse-button {
    display: flex;
    align-items: center;
    justify-content: center;
    margin-top: 4px;
    color: #ffd700;
    font-size: 0.8rem;
    font-weight: bold;
  }
  
  .collapse-icon {
    animation: bounce 1.5s ease-in-out infinite;
  }
  
  @keyframes bounce {
    0%, 100% { transform: translateY(0); }
    50% { transform: translateY(-2px); }
  }
  
  /* Responsive adjustments */
  @media (max-width: 768px) {
    .played-cards-container.expanded {
      max-width: 250px;
      max-height: 150px;
    }
    
    .played-card {
      width: 28px;
      height: 39px;
    }
    
    .expanded .played-card {
      width: 35px;
      height: 49px;
    }
    
    .card-count-badge {
      width: 18px;
      height: 18px;
      font-size: 0.65rem;
    }
  }
</style>
