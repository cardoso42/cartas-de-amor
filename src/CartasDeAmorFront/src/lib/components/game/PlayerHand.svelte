<!-- PlayerHand.svelte -->
<script lang="ts">
  import { CardType } from '$lib/types/game-types';
  import { createEventDispatcher } from 'svelte';
  import { getCardName } from '$lib/utils/cardUtils';

  // Props
  export let cards: CardType[] = [];
  export let isLocalPlayer: boolean = false;
  export let cardsInHand: number = 1;
  export let isMyTurn: boolean = false;
  export let selectedCard: CardType | null = null;

  // Events
  const dispatch = createEventDispatcher<{
    cardClick: { cardType: CardType };
  }>();

  function handleCardClick(cardType: CardType) {
    dispatch('cardClick', { cardType });
  }
</script>

<div class="player-hand">
  {#if isLocalPlayer}
    <!-- Local player sees their cards face up -->
    {#each cards as card}
      <div 
        class="card player-card face-up" 
        class:clickable={isMyTurn}
        class:selected={selectedCard === card}
        on:click={() => handleCardClick(card)}
        on:keydown={(e) => e.key === 'Enter' && handleCardClick(card)}
        role="button"
        tabindex={isMyTurn ? 0 : undefined}
        title={isMyTurn ? `Click to play ${getCardName(card)}` : getCardName(card)}
      >
        <div class="card-content">
          <div class="card-number">{card}</div>
          <div class="card-name">{getCardName(card)}</div>
        </div>
      </div>
    {/each}
  {:else}
    <!-- Other players' cards are face down -->
    {#each Array(cardsInHand || 1) as _, i}
      <div class="card player-card face-down"></div>
    {/each}
  {/if}
</div>

<style>
  .player-hand {
    display: flex;
    gap: 4px;
  }
  
  .card {
    width: 50px;
    height: 70px;
    border-radius: 6px;
    border: 2px solid #333;
    box-shadow: 0 2px 6px rgba(0, 0, 0, 0.3);
    transition: transform 0.2s ease;
  }
  
  .card.clickable {
    cursor: pointer;
  }
  
  .card.clickable:hover {
    transform: scale(1.15);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.4);
  }
  
  .card.clickable:focus {
    outline: 2px solid #ffd700;
    outline-offset: 2px;
  }
  
  .card.selected {
    border-color: #ffd700;
    box-shadow: 0 0 10px rgba(255, 215, 0, 0.8);
  }
  
  .player-card {
    transform: scale(1.1);
  }
  
  .player-card:hover {
    transform: scale(1.2);
    z-index: 5;
  }
  
  .player-card.clickable:hover {
    transform: scale(1.25);
    z-index: 5;
  }
  
  .face-up {
    background: linear-gradient(135deg, #fafafa 0%, #e0e0e0 100%);
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    padding: 4px;
  }
  
  .face-down {
    background-image: url('/images/card-back.png');
    background-size: cover;
    background-position: center;
    background-repeat: no-repeat;
    position: relative;
  }
  
  /* Fallback styling when image fails to load */
  .face-down {
    background-color: #1565c0;
    background-image: url('/images/card-back.png'), linear-gradient(135deg, #1565c0 0%, #0d47a1 100%);
  }
  
  .card-content {
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    height: 100%;
  }
  
  .card-number {
    font-size: 1.1rem;
    font-weight: bold;
    color: #9c27b0;
    text-align: center;
  }
  
  .card-name {
    font-size: 0.6rem;
    font-weight: bold;
    color: #333;
    text-align: center;
    line-height: 1;
  }
</style>
