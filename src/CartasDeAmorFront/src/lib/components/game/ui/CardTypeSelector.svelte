<!-- CardTypeSelector.svelte -->
<script lang="ts">
  import { CardType } from '$lib/types/game-types';
  import { createEventDispatcher } from 'svelte';
  import { getCardName } from '$lib/utils/cardUtils';

  // Props
  export let possibleCardTypes: CardType[] = [];
  export let selectedCardType: CardType | null = null;

  // Events
  const dispatch = createEventDispatcher<{
    cardTypeClick: { cardType: CardType };
  }>();

  function handleCardTypeClick(cardType: CardType) {
    dispatch('cardTypeClick', { cardType });
  }
</script>

<div class="card-type-selection-area">
  <div class="card-type-title">
    <h3>Choose a card type:</h3>
    <p>Click on one of the cards below</p>
  </div>
  <div class="card-type-display">
    {#each possibleCardTypes as cardType}
      {@const isSelected = selectedCardType === cardType}
      <div 
        class="card-type-card" 
        class:selected={isSelected}
        on:click={() => handleCardTypeClick(cardType)}
        on:keydown={(e) => e.key === 'Enter' && handleCardTypeClick(cardType)}
        role="button"
        tabindex="0"
        title="Click to select {getCardName(cardType)}"
      >
        <div class="card-content">
          <div class="card-number">{cardType}</div>
          <div class="card-name">{getCardName(cardType)}</div>
        </div>
      </div>
    {/each}
  </div>
</div>

<style>
  .card-type-selection-area {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    z-index: 15;
    background: rgba(0, 0, 0, 0.9);
    border: 3px solid #ffd700;
    border-radius: 16px;
    padding: 1.5rem;
    max-width: 400px;
    width: 90%;
    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.8);
  }
  
  .card-type-title {
    text-align: center;
    margin-bottom: 1rem;
    color: white;
  }
  
  .card-type-title h3 {
    margin: 0 0 0.5rem 0;
    color: #ffd700;
    font-size: 1.2rem;
  }
  
  .card-type-title p {
    margin: 0;
    color: #ccc;
    font-size: 0.9rem;
  }
  
  .card-type-display {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(80px, 1fr));
    gap: 0.75rem;
    justify-items: center;
  }
  
  .card-type-card {
    width: 70px;
    height: 98px;
    background: linear-gradient(135deg, #fafafa 0%, #e0e0e0 100%);
    border: 2px solid #333;
    border-radius: 8px;
    cursor: pointer;
    transition: all 0.2s ease;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    padding: 0.5rem;
  }
  
  .card-type-card:hover {
    transform: scale(1.1);
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.4);
    border-color: #ffd700;
  }
  
  .card-type-card.selected {
    border-color: #ffd700;
    box-shadow: 0 0 15px rgba(255, 215, 0, 0.8);
    background: linear-gradient(135deg, #fff8dc 0%, #f0e68c 100%);
  }
  
  .card-type-card:focus {
    outline: 2px solid #ffd700;
    outline-offset: 2px;
  }
  
  .card-content {
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    height: 100%;
  }
  
  .card-number {
    font-size: 1.2rem;
    font-weight: bold;
    color: #9c27b0;
    text-align: center;
  }
  
  .card-name {
    font-size: 0.7rem;
    font-weight: bold;
    color: #333;
    text-align: center;
    line-height: 1;
  }
</style>
