<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  import { CardType } from '$lib/types/game-types';
  import { getCardName } from '$lib/utils/cardUtils';
  
  const dispatch = createEventDispatcher<{
    close: void;
    submit: { keepCard: CardType; returnCards: CardType[] };
  }>();
  
  // Props
  export let isOpen = false;
  export let cards: CardType[] = [];
  export let title = "Choose a Card to Keep";
  export let description = "Choose one card to keep. The remaining cards will be placed back on the deck in the order you arrange them.";
  
  // State
  let selectedKeepCard: CardType | null = null;
  let selectedKeepCardIndex: number | null = null;
  let sortableCards: CardType[] = [];
  let draggedCardIndex: number | null = null;
  
  // Reset state when modal opens with new cards
  $: if (isOpen && cards.length > 0) {
    selectedKeepCard = null;
    selectedKeepCardIndex = null;
    sortableCards = [];
  }
  
  // Update sortable cards when a card is selected to keep
  $: if (selectedKeepCard !== null && selectedKeepCardIndex !== null) {
    sortableCards = cards.filter((card, index) => index !== selectedKeepCardIndex);
  }
  
  function close() {
    dispatch('close');
  }
  
  function handleSubmit() {
    if (selectedKeepCard === null) {
      alert('Please select a card to keep');
      return;
    }
    
    if (sortableCards.length === 0 && cards.length > 1) {
      alert('Please arrange the remaining cards');
      return;
    }
    
    dispatch('submit', {
      keepCard: selectedKeepCard,
      returnCards: [...sortableCards] // Make a copy to avoid reference issues
    });
  }
  
  function selectCard(cardType: CardType, index: number) {
    selectedKeepCard = cardType;
    selectedKeepCardIndex = index;
  }
  
  function handleKeydown(event: KeyboardEvent) {
    if (event.key === 'Escape') {
      close();
    }
  }
  
  // Drag and drop handlers
  function handleDragStart(event: DragEvent, index: number) {
    draggedCardIndex = index;
    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = 'move';
      event.dataTransfer.setData('text/html', '');
    }
  }
  
  function handleDragEnd() {
    draggedCardIndex = null;
  }
  
  function handleDragOver(event: DragEvent) {
    event.preventDefault();
    if (event.dataTransfer) {
      event.dataTransfer.dropEffect = 'move';
    }
  }
  
  function handleDrop(event: DragEvent, dropIndex: number) {
    event.preventDefault();
    
    if (draggedCardIndex === null || draggedCardIndex === dropIndex) {
      return;
    }
    
    // Reorder the cards
    const newSortableCards = [...sortableCards];
    const draggedCard = newSortableCards[draggedCardIndex];
    
    // Remove the dragged card
    newSortableCards.splice(draggedCardIndex, 1);
    
    // Insert at new position
    const insertIndex = draggedCardIndex < dropIndex ? dropIndex - 1 : dropIndex;
    newSortableCards.splice(insertIndex, 0, draggedCard);
    
    sortableCards = newSortableCards;
    draggedCardIndex = null;
  }
  
  function moveCardUp(index: number) {
    if (index === 0) return;
    
    const newSortableCards = [...sortableCards];
    [newSortableCards[index - 1], newSortableCards[index]] = [newSortableCards[index], newSortableCards[index - 1]];
    sortableCards = newSortableCards;
  }
  
  function moveCardDown(index: number) {
    if (index === sortableCards.length - 1) return;
    
    const newSortableCards = [...sortableCards];
    [newSortableCards[index], newSortableCards[index + 1]] = [newSortableCards[index + 1], newSortableCards[index]];
    sortableCards = newSortableCards;
  }
</script>

<svelte:window on:keydown={handleKeydown} />

{#if isOpen}
  <div class="modal-backdrop" on:click={close} on:keydown={handleKeydown} role="presentation">
    <div class="modal-content" on:click|stopPropagation on:keydown|stopPropagation role="dialog" aria-modal="true" aria-labelledby="modal-title" tabindex="-1">
      <!-- Modal Header -->
      <div class="modal-header">
        <h2 id="modal-title">{title}</h2>
        <button class="close-button" on:click={close} aria-label="Close modal">×</button>
      </div>
      
      <!-- Modal Description -->
      <div class="modal-description">
        <p>{description}</p>
      </div>
      
      <!-- Instructions -->
      <div class="instructions">
        <h3>Instructions:</h3>
        <ol>
          <li>Select the card you want to keep by clicking on it</li>
          <li>Arrange the remaining cards in the order they should go back on the deck (top to bottom)</li>
          <li>Use drag & drop or the arrow buttons to reorder cards</li>
        </ol>
      </div>
      
      <!-- Step 1: Select card to keep -->
      <div class="selection-step">
        <h3>Step 1: Select card to keep</h3>
        <div class="card-options">
          {#each cards as cardType, index (`card-${index}-${cardType}`)}
            {@const isSelected = selectedKeepCardIndex === index}
            <div 
              class="card-option" 
              class:selected={isSelected}
              on:click={() => selectCard(cardType, index)}
              on:keydown={(e) => e.key === 'Enter' && selectCard(cardType, index)}
              role="button"
              tabindex="0"
              title="Click to keep {getCardName(cardType)}"
            >
              <div class="card-number">{cardType}</div>
              <div class="card-name">{getCardName(cardType)}</div>
              {#if isSelected}
                <div class="selected-indicator">✓</div>
              {/if}
            </div>
          {/each}
        </div>
      </div>
      
      <!-- Step 2: Arrange remaining cards (only show if a card is selected) -->
      {#if selectedKeepCard !== null && sortableCards.length > 0}
        <div class="selection-step">
          <h3>Step 2: Arrange remaining cards</h3>
          <p class="step-description">Order these cards from top to bottom as they should be placed back on the deck:</p>
          
          <div class="sortable-cards">
            {#each sortableCards as cardType, index (`sortable-${index}-${cardType}`)}
              <div 
                class="sortable-card"
                draggable="true"
                on:dragstart={(e) => handleDragStart(e, index)}
                on:dragend={handleDragEnd}
                on:dragover={handleDragOver}
                on:drop={(e) => handleDrop(e, index)}
                role="listitem"
              >
                <div class="card-content">
                  <div class="card-number">{cardType}</div>
                  <div class="card-name">{getCardName(cardType)}</div>
                </div>
                
                <div class="card-controls">
                  <button 
                    class="move-button" 
                    on:click={() => moveCardUp(index)}
                    disabled={index === 0}
                    aria-label="Move {getCardName(cardType)} up"
                    title="Move up"
                  >↑</button>
                  <button 
                    class="move-button" 
                    on:click={() => moveCardDown(index)}
                    disabled={index === sortableCards.length - 1}
                    aria-label="Move {getCardName(cardType)} down"
                    title="Move down"
                  >↓</button>
                </div>
                
                <div class="position-indicator">{index + 1}</div>
              </div>
            {/each}
          </div>
        </div>
      {/if}
      
      <!-- Modal Actions -->
      <div class="modal-actions">
        <button class="cancel-button" on:click={close}>Cancel</button>
        <button 
          class="submit-button" 
          on:click={handleSubmit}
          disabled={selectedKeepCard === null}
        >
          Submit Choice
        </button>
      </div>
    </div>
  </div>
{/if}

<style>
  .modal-backdrop {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.7);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 1000;
    padding: 1rem;
  }
  
  .modal-content {
    background: white;
    border-radius: 12px;
    max-width: 600px;
    width: 100%;
    max-height: 90vh;
    overflow-y: auto;
    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
    position: relative;
  }
  
  .modal-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1.5rem 1.5rem 0 1.5rem;
    border-bottom: 1px solid #e0e0e0;
    margin-bottom: 1rem;
  }
  
  .modal-header h2 {
    margin: 0;
    color: #333;
    font-size: 1.5rem;
  }
  
  .close-button {
    background: none;
    border: none;
    font-size: 2rem;
    color: #666;
    cursor: pointer;
    padding: 0;
    width: 2rem;
    height: 2rem;
    display: flex;
    justify-content: center;
    align-items: center;
    border-radius: 50%;
    transition: background-color 0.2s ease;
  }
  
  .close-button:hover {
    background-color: #f0f0f0;
  }
  
  .close-button:focus {
    outline: 2px solid #4CAF50;
    outline-offset: 2px;
  }
  
  .modal-description {
    padding: 0 1.5rem;
    margin-bottom: 1rem;
  }
  
  .modal-description p {
    margin: 0;
    color: #666;
    line-height: 1.5;
  }
  
  .instructions {
    padding: 0 1.5rem;
    margin-bottom: 1.5rem;
    background: #f8f9ff;
    border-radius: 8px;
    padding: 1rem 1.5rem;
    margin: 0 1.5rem 1.5rem 1.5rem;
  }
  
  .instructions h3 {
    margin: 0 0 0.5rem 0;
    color: #333;
    font-size: 1rem;
  }
  
  .instructions ol {
    margin: 0;
    padding-left: 1.5rem;
    color: #666;
  }
  
  .instructions li {
    margin-bottom: 0.25rem;
  }
  
  .selection-step {
    padding: 0 1.5rem;
    margin-bottom: 1.5rem;
  }
  
  .selection-step h3 {
    margin: 0 0 1rem 0;
    color: #333;
    font-size: 1.2rem;
  }
  
  .step-description {
    margin: 0 0 1rem 0;
    color: #666;
    font-size: 0.9rem;
  }
  
  .card-options {
    display: flex;
    gap: 1rem;
    flex-wrap: wrap;
    justify-content: center;
  }
  
  .card-option {
    position: relative;
    width: 100px;
    height: 140px;
    background: linear-gradient(135deg, #fafafa 0%, #e0e0e0 100%);
    border: 2px solid #ccc;
    border-radius: 8px;
    cursor: pointer;
    transition: all 0.2s ease;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    text-align: center;
    padding: 0.5rem;
  }
  
  .card-option:hover {
    transform: scale(1.05);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
  }
  
  .card-option.selected {
    border-color: #4CAF50;
    background: linear-gradient(135deg, #e8f5e8 0%, #c8e6c9 100%);
    box-shadow: 0 0 15px rgba(76, 175, 80, 0.4);
  }
  
  .card-option:focus {
    outline: 2px solid #4CAF50;
    outline-offset: 2px;
  }
  
  .card-number {
    font-size: 1.5rem;
    font-weight: bold;
    color: #9c27b0;
    margin-bottom: 0.5rem;
  }
  
  .card-name {
    font-size: 0.8rem;
    font-weight: bold;
    color: #333;
    line-height: 1.2;
  }
  
  .selected-indicator {
    position: absolute;
    top: 0.25rem;
    right: 0.25rem;
    background: #4CAF50;
    color: white;
    border-radius: 50%;
    width: 1.5rem;
    height: 1.5rem;
    display: flex;
    justify-content: center;
    align-items: center;
    font-size: 0.8rem;
    font-weight: bold;
  }
  
  .sortable-cards {
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
    min-height: 100px;
    padding: 1rem;
    border: 2px dashed #ccc;
    border-radius: 8px;
    background: #fafafa;
  }
  
  .sortable-card {
    display: flex;
    align-items: center;
    gap: 1rem;
    background: white;
    border: 1px solid #ddd;
    border-radius: 8px;
    padding: 0.75rem;
    cursor: grab;
    transition: all 0.2s ease;
    position: relative;
  }
  
  .sortable-card:hover {
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    border-color: #4CAF50;
  }
  
  .sortable-card:active {
    cursor: grabbing;
  }
  
  .card-content {
    display: flex;
    align-items: center;
    gap: 1rem;
    flex: 1;
  }
  
  .sortable-card .card-number {
    font-size: 1.2rem;
    margin: 0;
    min-width: 2rem;
  }
  
  .sortable-card .card-name {
    font-size: 1rem;
    margin: 0;
  }
  
  .card-controls {
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
  }
  
  .move-button {
    background: #f0f0f0;
    border: 1px solid #ccc;
    border-radius: 4px;
    width: 2rem;
    height: 1.5rem;
    cursor: pointer;
    font-size: 0.8rem;
    display: flex;
    justify-content: center;
    align-items: center;
    transition: background-color 0.2s ease;
  }
  
  .move-button:hover:not(:disabled) {
    background: #e0e0e0;
  }
  
  .move-button:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
  
  .position-indicator {
    position: absolute;
    top: 0.25rem;
    left: 0.25rem;
    background: #2196F3;
    color: white;
    border-radius: 50%;
    width: 1.5rem;
    height: 1.5rem;
    display: flex;
    justify-content: center;
    align-items: center;
    font-size: 0.7rem;
    font-weight: bold;
  }
  
  .modal-actions {
    display: flex;
    gap: 1rem;
    justify-content: flex-end;
    padding: 1.5rem;
    border-top: 1px solid #e0e0e0;
    margin-top: 1rem;
  }
  
  .cancel-button,
  .submit-button {
    padding: 0.75rem 1.5rem;
    border-radius: 6px;
    font-size: 1rem;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s ease;
    border: none;
  }
  
  .cancel-button {
    background: #f5f5f5;
    color: #666;
  }
  
  .cancel-button:hover {
    background: #e0e0e0;
  }
  
  .submit-button {
    background: #4CAF50;
    color: white;
  }
  
  .submit-button:hover:not(:disabled) {
    background: #45a049;
  }
  
  .submit-button:disabled {
    background: #ccc;
    cursor: not-allowed;
  }
  
  .cancel-button:focus,
  .submit-button:focus {
    outline: 2px solid #4CAF50;
    outline-offset: 2px;
  }
  
  /* Responsive design */
  @media (max-width: 768px) {
    .modal-content {
      margin: 0.5rem;
      max-height: 95vh;
    }
    
    .card-options {
      gap: 0.5rem;
    }
    
    .card-option {
      width: 80px;
      height: 112px;
    }
    
    .card-number {
      font-size: 1.2rem;
    }
    
    .card-name {
      font-size: 0.7rem;
    }
    
    .modal-actions {
      flex-direction: column;
    }
    
    .sortable-card {
      padding: 0.5rem;
    }
    
    .card-content {
      gap: 0.5rem;
    }
  }
</style>
