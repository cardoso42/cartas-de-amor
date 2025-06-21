<!-- CardDeck.svelte -->
<script lang="ts">
  import { createEventDispatcher } from 'svelte';

  // Props
  export let isMyTurn: boolean = false;
  export let cardsRemainingInDeck: number = 0;

  // Events
  const dispatch = createEventDispatcher<{
    drawCard: void;
  }>();

  function handleDrawCard() {
    if (!isMyTurn) {
      return; // Ignore clicks when it's not the player's turn
    }
    dispatch('drawCard');
  }
</script>

<div class="deck-area">
  <div 
    class="card-deck" 
    class:disabled={!isMyTurn}
    on:click={handleDrawCard}
    on:keydown={(e) => e.key === 'Enter' && handleDrawCard()}
    role="button"
    tabindex={isMyTurn ? 0 : undefined}
    title={isMyTurn ? 'Click to draw a card' : 'Wait for your turn to draw a card'}
  >
    <div 
      class="deck-cards"
      class:clickable={isMyTurn}
    >
      <!-- Deck cards stack -->
      <div class="deck-card"></div>
      <div class="deck-card"></div>
      <div class="deck-card"></div>
    </div>
    <div class="deck-label">
      {isMyTurn ? 'Draw Card' : 'Deck'}
    </div>
    <div class="deck-counter">
      {cardsRemainingInDeck} cards left
    </div>
  </div>
</div>

<style>
  .deck-area {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    z-index: 10;
  }
  
  .card-deck {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.5rem;
    transition: transform 0.2s ease, box-shadow 0.2s ease;
  }
  
  .card-deck.disabled {
    cursor: not-allowed;
    opacity: 0.7;
  }
  
  .card-deck:focus {
    outline: 2px solid #ffd700;
    outline-offset: 2px;
  }

  .deck-cards.clickable {
    cursor: pointer;
  }
  
  .deck-cards.clickable:hover {
    transform: scale(1.05);
    box-shadow: 0 0 15px rgba(255, 215, 0, 0.8);
  }
  
  .deck-cards {
    position: relative;
    width: 70px;
    height: 98px;
  }
  
  .deck-card {
    position: absolute;
    width: 70px;
    height: 98px;
    background-image: url('/images/card-back.png'), linear-gradient(135deg, #1565c0 0%, #0d47a1 100%);
    background-size: cover;
    background-position: center;
    background-repeat: no-repeat;
    border-radius: 8px;
    border: 2px solid #0d47a1;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
  }
  
  .deck-card:nth-child(1) {
    top: 0;
    left: 0;
  }
  
  .deck-card:nth-child(2) {
    top: -2px;
    left: -1px;
  }
  
  .deck-card:nth-child(3) {
    top: -4px;
    left: -2px;
  }
  
  .deck-label {
    color: white;
    font-size: 0.8rem;
    font-weight: bold;
    text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.8);
  }
  
  .deck-counter {
    color: #ffd700;
    font-size: 0.7rem;
    font-weight: bold;
    text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.8);
    background: rgba(0, 0, 0, 0.3);
    padding: 0.2rem 0.5rem;
    border-radius: 8px;
    border: 1px solid rgba(255, 215, 0, 0.3);
  }
</style>
