<!-- PlayerArea.svelte -->
<script lang="ts">
  import { CardType } from '$lib/types/game-types';
  import { createEventDispatcher } from 'svelte';
  import PlayerHand from './PlayerHand.svelte';
  import PlayedCardsDisplay from './PlayedCardsDisplay.svelte';

  // Props
  export let player: {
    id: number;
    name: string;
    email: string;
    isLocalPlayer: boolean;
    position: number;
    tokens: number;
    cards: CardType[];
    cardsInHand: number;
    playedCards: number[];
    isProtected: boolean;
    isCurrentTurn: boolean;
  };
  export let position: { angle: number; distance: number };
  export let isValidTarget: boolean = false;
  export let isSelectedTarget: boolean = false;
  export let isMyTurn: boolean = false;
  export let selectedCard: CardType | null = null;
  export let gameFlowState: 'idle' | 'showing_rules' | 'selecting_player' | 'selecting_card_type' = 'idle';
  // Animation support props
  export let hiddenCardType: CardType | null = null;
  export let animatingPlayerEmail: string = '';

  // Calculate positions for player area and played cards
  $: playerX = Math.cos((position.angle - 90) * Math.PI / 180) * position.distance;
  $: playerY = Math.sin((position.angle - 90) * Math.PI / 180) * position.distance;
  $: playedCardDistance = 150;
  $: playedX = Math.cos((position.angle - 90) * Math.PI / 180) * playedCardDistance;
  $: playedY = Math.sin((position.angle - 90) * Math.PI / 180) * playedCardDistance;

  // Events
  const dispatch = createEventDispatcher<{
    playerClick: { playerEmail: string };
    cardClick: { cardType: CardType };
    cardPosition: { playerEmail: string; cardType: CardType; position: { x: number; y: number; width: number; height: number } };
  }>();

  let playerHandComponent: PlayerHand;

  function handlePlayerClick() {
    dispatch('playerClick', { playerEmail: player.email });
  }

  function handleCardClick(event: CustomEvent<{ cardType: CardType }>) {
    dispatch('cardClick', { cardType: event.detail.cardType });
  }

  // Export function to get card position for animations
  export function getCardPosition(cardType: CardType): { x: number; y: number; width: number; height: number } | null {
    if (playerHandComponent) {
      return playerHandComponent.getCardPosition(cardType);
    }
    return null;
  }
</script>

<div 
  class="player-area"
  class:local-player={player.isLocalPlayer}
  class:clickable-target={isValidTarget}
  class:invalid-target={gameFlowState === 'selecting_player' && !isValidTarget}
  class:selected-target={isSelectedTarget}
  style="
    left: 50%;
    top: 50%;
    transform: translate(calc(-50% + {playerX}px), calc(-50% + {playerY}px));
  "
  on:click={handlePlayerClick}
  on:keydown={(e) => e.key === 'Enter' && handlePlayerClick()}
  role={isValidTarget ? 'button' : undefined}
  title={gameFlowState === 'selecting_player' 
    ? (isValidTarget ? `Click to target ${player.name}` : `Cannot target ${player.name}`)
    : undefined}
>
  <!-- Player name -->
  <div class="player-name" class:protected={player.isProtected} class:current-turn={player.isCurrentTurn}>
    {player.name}
    {#if player.isProtected}
      <span class="protection-icon">🛡️</span>
    {/if}
    {#if gameFlowState === 'selecting_player'}
      {#if isValidTarget}
        <span class="target-icon">🎯</span>
      {:else}
        <span class="no-target-icon">❌</span>
      {/if}
    {/if}
  </div>
  
  <!-- Player tokens display (hearts underneath name) -->
  <div class="player-tokens">
    {#each Array(player.tokens) as _, i}
      <div class="token">♥</div>
    {/each}
  </div>
  
  <!-- Player's hand -->
  <PlayerHand 
    bind:this={playerHandComponent}
    cards={player.cards}
    isLocalPlayer={player.isLocalPlayer}
    cardsInHand={player.cardsInHand}
    {isMyTurn}
    {selectedCard}
    {hiddenCardType}
    {animatingPlayerEmail}
    on:cardClick={handleCardClick}
  />
</div>

<!-- Played cards positioned on the table surface in front of the player -->
{#if player.playedCards && player.playedCards.length > 0}
  <div 
    class="played-cards-area"
    style="
      left: 50%;
      top: 50%;
      transform: translate(calc(-50% + {playedX}px), calc(-50% + {playedY}px));
    "
  >
    <PlayedCardsDisplay 
      playedCards={player.playedCards} 
      playerName={player.name}
    />
  </div>
{/if}

<style>
  /* Player positioning around the circular table */
  .player-area {
    position: absolute;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.5rem;
    pointer-events: auto;
    transition: transform 0.2s ease, box-shadow 0.2s ease;
  }
  
  /* Player targeting states */
  .player-area.clickable-target {
    cursor: pointer;
  }
  
  .player-area.clickable-target:hover {
    transform: scale(1.05);
  }
    
  .player-area.clickable-target:hover :global(.player-hand .card) {
    box-shadow: 0 0 15px rgba(0, 255, 0, 0.6);
  }
  
  .player-area.selected-target {
    transform: scale(1.1);
    box-shadow: 0 0 20px rgba(255, 215, 0, 0.8);
  }
  
  .player-area.invalid-target {
    opacity: 0.5;
    cursor: not-allowed;
  }
  
  .player-area.clickable-target:focus {
    outline: 2px solid #00ff00;
    outline-offset: 4px;
  }
  
  .player-name {
    color: white;
    font-weight: bold;
    font-size: 0.9rem;
    text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.8);
    background: rgba(0, 0, 0, 0.3);
    padding: 0.25rem 0.75rem;
    border-radius: 12px;
    min-width: 80px;
    text-align: center;
  }
  
  .local-player .player-name {
    background: rgba(156, 39, 176, 0.7);
    color: white;
  }
  
  .player-name.protected {
    background: rgba(255, 193, 7, 0.8);
    color: #333;
    border: 2px solid #ffc107;
  }
  
  .local-player .player-name.protected {
    background: rgba(255, 193, 7, 0.9);
    color: #333;
    border: 2px solid #ffc107;
  }
  
  .protection-icon {
    margin-left: 0.25rem;
    font-size: 0.8rem;
  }
  
  .target-icon {
    margin-left: 0.25rem;
    font-size: 0.8rem;
    animation: targetPulse 1.5s ease-in-out infinite;
  }
  
  .no-target-icon {
    margin-left: 0.25rem;
    font-size: 0.8rem;
    opacity: 0.7;
  }
  
  @keyframes targetPulse {
    0%, 100% { opacity: 1; }
    50% { opacity: 0.5; }
  }
  
  .player-name.current-turn {
    border: 2px solid #ffd700;
    box-shadow: 0 0 8px rgba(255, 215, 0, 0.6);
    background: rgba(0, 0, 0, 0.4);
  }
  
  .local-player .player-name.current-turn {
    border: 2px solid #ffd700;
    box-shadow: 0 0 8px rgba(255, 215, 0, 0.6);
    background: rgba(156, 39, 176, 0.8);
    animation: turnPulse 2s ease-in-out infinite;
  }
  
  @keyframes turnPulse {
    0%, 100% {
      box-shadow: 0 0 8px rgba(255, 215, 0, 0.6);
    }
    50% {
      box-shadow: 0 0 15px rgba(255, 215, 0, 0.8);
    }
  }
  
  .player-tokens {
    display: flex;
    gap: 2px;
    justify-content: center;
    align-items: center;
    flex-wrap: wrap;
    max-width: 80px;
  }
  
  .token {
    color: #ff6b9d;
    font-size: 1rem;
    text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.8);
    animation: pulse 2s infinite;
  }
  
  @keyframes pulse {
    0%, 100% { opacity: 1; }
    50% { opacity: 0.8; }
  }
  
  .played-cards-area {
    position: absolute;
    display: flex;
    gap: 4px;
    flex-wrap: wrap;
    justify-content: center;
    z-index: 5;
  }
</style>
