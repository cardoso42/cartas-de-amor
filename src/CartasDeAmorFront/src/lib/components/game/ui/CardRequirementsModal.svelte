<script lang="ts">
  import { CardType, CardActionRequirements, type CardRequirementsDto } from '$lib/types/game-types';
  import { createEventDispatcher } from 'svelte';
  import { getCardName } from '$lib/utils/cardUtils';
  import { _ } from 'svelte-i18n';
  
  // Simple interface for player display context
  interface PlayerDisplayInfo {
    email: string;
    name?: string;
  }
  
  export let isOpen = false;
  export let requirements: CardRequirementsDto | null = null;
  export let players: PlayerDisplayInfo[] = []; // For context only
  
  const dispatch = createEventDispatcher();
  
  // Determine what actions are needed
  $: needsPlayerSelection = requirements?.requirements.includes(CardActionRequirements.SelectPlayer) || false;
  $: needsCardTypeSelection = requirements?.requirements.includes(CardActionRequirements.SelectCardType) || false;
  
  function close() {
    dispatch('close');
  }
  
  function handleKeydown(event: KeyboardEvent) {
    if (event.key === 'Escape') {
      close();
    }
  }
</script>

<svelte:window on:keydown={handleKeydown} />

{#if isOpen && requirements}
  <div class="modal-backdrop" on:click={close} on:keydown={handleKeydown} role="presentation">
    <div class="modal-content" on:click|stopPropagation on:keydown|stopPropagation role="dialog" aria-modal="true" aria-labelledby="modal-title" tabindex="-1">
      <!-- Modal Header -->
      <div class="modal-header">
        <h2 id="modal-title">{$_('game.playingCard', { values: { cardName: getCardName(requirements.cardType) } })}</h2>
        <button class="close-button" on:click={close} aria-label={$_('game.closeModal')}>×</button>
      </div>
      
      <!-- Card Effect Description -->
      <div class="card-description">
        {#if requirements.message}
          <p class="card-rule">{requirements.message}</p>
        {:else}
          <p class="card-rule">{$_('game.cardRequiresChoices')}</p>
        {/if}
      </div>
      
      <!-- Next Steps Information -->
      <div class="next-steps">
        <h3>{$_('game.whatHappensNext')}</h3>
        <div class="steps-list">
          {#if needsPlayerSelection && needsCardTypeSelection}
            <div class="step">
              <span class="step-number">1</span>
              <span class="step-text">{$_('game.selectTargetPlayer')}</span>
            </div>
            <div class="step">
              <span class="step-number">2</span>
              <span class="step-text">{$_('game.chooseCardType')}</span>
            </div>
          {:else if needsPlayerSelection}
            <div class="step">
              <span class="step-number">1</span>
              <span class="step-text">{$_('game.selectTargetPlayer')}</span>
            </div>
          {:else if needsCardTypeSelection}
            <div class="step">
              <span class="step-number">1</span>
              <span class="step-text">{$_('game.chooseCardTypeOnly')}</span>
            </div>
          {:else}
            <div class="step">
              <span class="step-number">✓</span>
              <span class="step-text">{$_('game.noAdditionalRequirements')}</span>
            </div>
          {/if}
        </div>
      </div>
      
      <!-- Valid Targets Preview (if applicable) -->
      {#if needsPlayerSelection && requirements.possibleTargets.length > 0}
        <div class="targets-preview">
          <h4>{$_('game.validTargets')}</h4>
          <div class="target-list">
            {#each requirements.possibleTargets as targetEmail}
              {@const player = players.find(p => p.email === targetEmail)}
              <span class="target-name">{player?.name || targetEmail.split('@')[0]}</span>
            {/each}
          </div>
        </div>
      {/if}
      
      <!-- Modal Actions -->
      <div class="modal-actions">
        <button class="primary-button" on:click={close}>
          {$_('game.gotItLetsPlay')}
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
    backdrop-filter: blur(2px);
  }
  
  .modal-content {
    background: linear-gradient(135deg, #2c1810 0%, #3e2723 100%);
    border: 3px solid #8b4513;
    border-radius: 12px;
    padding: 2rem;
    max-width: 500px;
    width: 90%;
    max-height: 80vh;
    overflow-y: auto;
    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.5);
    color: white;
  }
  
  .modal-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1rem;
    border-bottom: 2px solid #8b4513;
    padding-bottom: 1rem;
  }
  
  .modal-header h2 {
    margin: 0;
    color: #ffd700;
    font-size: 1.5rem;
  }
  
  .close-button {
    background: none;
    border: none;
    color: white;
    font-size: 1.5rem;
    cursor: pointer;
    padding: 0.5rem;
    border-radius: 4px;
    transition: background-color 0.2s;
  }
  
  .close-button:hover {
    background: rgba(255, 255, 255, 0.1);
  }
  
  .card-description {
    margin-bottom: 1.5rem;
    padding: 1rem;
    background: rgba(0, 0, 0, 0.3);
    border-radius: 8px;
    border-left: 4px solid #ffd700;
  }
  
  .card-rule {
    margin: 0;
    font-size: 1rem;
    line-height: 1.4;
  }
  
  .next-steps {
    margin-bottom: 1.5rem;
  }
  
  .next-steps h3 {
    margin: 0 0 1rem 0;
    color: #ffd700;
    font-size: 1.1rem;
  }
  
  .steps-list {
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
  }
  
  .step {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    padding: 0.5rem;
    background: rgba(0, 100, 0, 0.1);
    border-radius: 8px;
    border-left: 3px solid #00ff00;
  }
  
  .step-number {
    background: #00ff00;
    color: #000;
    width: 24px;
    height: 24px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: bold;
    font-size: 0.9rem;
    flex-shrink: 0;
  }
  
  .step-text {
    flex: 1;
    font-size: 0.95rem;
  }
  
  .targets-preview {
    margin-bottom: 1.5rem;
  }
  
  .targets-preview h4 {
    margin: 0 0 0.75rem 0;
    color: #ffd700;
    font-size: 1rem;
  }
  
  .target-list {
    display: flex;
    flex-wrap: wrap;
    gap: 0.5rem;
  }
  
  .target-name {
    background: rgba(0, 255, 0, 0.2);
    color: #00ff00;
    padding: 0.25rem 0.5rem;
    border-radius: 12px;
    border: 1px solid #00ff00;
    font-size: 0.9rem;
  }
    
  .step-number {
    width: 32px;
    height: 32px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    background: rgba(255, 255, 255, 0.1);
    color: rgba(255, 255, 255, 0.5);
    font-weight: bold;
    transition: all 0.3s ease;
  }
  
  .modal-actions {
    display: flex;
    gap: 1rem;
    justify-content: flex-end;
    border-top: 2px solid #8b4513;
    padding-top: 1rem;
  }
  
  .primary-button {
    padding: 0.75rem 1.5rem;
    border: none;
    border-radius: 6px;
    font-weight: bold;
    cursor: pointer;
    transition: all 0.2s ease;
  }
  
  .primary-button {
    background: #ffd700;
    color: #2c1810;
  }
  
  .primary-button:hover:not(:disabled) {
    background: #ffed4e;
    transform: translateY(-1px);
  }
  
  .primary-button:disabled {
    background: rgba(255, 215, 0, 0.3);
    color: rgba(44, 24, 16, 0.5);
    cursor: not-allowed;
  }
  
  /* Mobile responsiveness */
  @media (max-width: 768px) {
    .modal-content {
      margin: 1rem;
      padding: 1.5rem;
    }
  }
</style>
