<script lang="ts">
  import { CardType, type InitialGameStatusDto, type CardRequirementsDto, CardActionRequirements } from '$lib/types/game-types';
  import { user } from '$lib/stores/userStore';
  import { signalR } from '$lib/services/signalRService';
  import { page } from '$app/stores';
  import { onMount } from 'svelte';
  import CardRequirementsModal from '../ui/CardRequirementsModal.svelte';
  import CardChoiceModal from '../ui/CardChoiceModal.svelte';
  import WoodenTable from '../../layout/WoodenTable.svelte';
  import CardDeck from '../cards/CardDeck.svelte';
  import PlayerArea from '../player/PlayerArea.svelte';
  import CardTypeSelector from '../ui/CardTypeSelector.svelte';
  import GameFlowInstructions from './GameFlowInstructions.svelte';
  import { getCardName, getRequirementName } from '$lib/utils/cardUtils';
  import { getPlayerPosition, getCurrentTurnPlayer, getPlayerDisplayName } from '$lib/utils/gameUtils';
  import { processGameData } from '$lib/utils/gameDataProcessor';
  
  // Props from parent component
  export let gameStatus: InitialGameStatusDto;
  export let currentUserEmail: string;
  export let currentTurnPlayerEmail: string = '';
  export let localPlayerPlayedCards: number[] = []; // Played cards for the local player
  // Animation support props
  export let hiddenCardType: CardType | null = null;
  export let animatingPlayerEmail: string = '';
  export let isAnimationPlaying: boolean = false;
  
  // Get room ID from URL params
  const roomId = $page.params.roomId;
  
  // Get local player data (the player at this client)
  let localPlayerName = '';
  user.subscribe(state => {
    localPlayerName = state.username || currentUserEmail.split('@')[0];
  });
  
  // Track card being played and its requirements
  let selectedCard: CardType | null = null;
  let cardRequirements: CardRequirementsDto | null = null;
  let showRequirementsModal = false;
  
  // Track requirement fulfillment state
  let selectedTargetEmail: string | null = null;
  let selectedCardType: CardType | null = null;
  
  // Card choice state
  let showCardChoiceModal = false;
  let cardsToChoose: CardType[] = [];
  
  // Game flow states
  let gameFlowState: 'idle' | 'showing_rules' | 'selecting_player' | 'selecting_card_type' = 'idle';
  let currentStep = 0;
  let totalSteps = 0;

  // Process the game data into display format (depends on currentTurnPlayer for turn indicator)
  $: players = gameStatus ? processGameData(gameStatus, currentUserEmail, currentTurnPlayer, localPlayerName, localPlayerPlayedCards) : [];
  $: totalPlayers = players.length;
  
  // Determine whose turn it is
  $: currentTurnPlayer = getCurrentTurnPlayer(gameStatus, currentTurnPlayerEmail);
  $: isMyTurn = currentTurnPlayer === currentUserEmail;
  
  // Store references to player areas for card position retrieval
  let playerAreaComponents: { [key: string]: PlayerArea } = {};
  
  // Export function to get card position for animations
  export function getPlayerCardPosition(playerEmail: string, cardType: CardType): { x: number; y: number; width: number; height: number } | null {
    const playerArea = playerAreaComponents[playerEmail];
    if (playerArea) {
      return playerArea.getCardPosition(cardType);
    }
    return null;
  }
  
  // Export function to get deck position for animations
  export function getDeckPosition(): { x: number; y: number; width: number; height: number } | null {
    const deckElement = document.querySelector('.deck-cards');
    if (deckElement) {
      const rect = deckElement.getBoundingClientRect();
      return {
        x: rect.left + rect.width / 2,
        y: rect.top + rect.height / 2,
        width: rect.width,
        height: rect.height
      };
    }
    return null;
  }
  
  // Export function to get player hand position for animations
  export function getPlayerHandPosition(playerEmail: string): { x: number; y: number; width: number; height: number } | null {
    const playerArea = playerAreaComponents[playerEmail];
    if (playerArea) {
      // For local player, try to get position of their hand area
      if (playerEmail === currentUserEmail) {
        const handElement = document.querySelector('.player-hand');
        if (handElement) {
          const rect = handElement.getBoundingClientRect();
          return {
            x: rect.left + rect.width / 2,
            y: rect.top + rect.height / 2,
            width: 65,
            height: 90
          };
        }
      } else {
        // For other players, use their general position and adjust for hand area
        const players = getProcessedPlayers();
        const player = players.find(p => p.email === playerEmail);
        if (player) {
          const totalPlayers = players.length;
          const playerPosition = player.position;
          
          // Calculate position using the same logic as PlayerArea
          const baseAngle = 180; // Start at bottom (180 degrees)
          const angleStep = 360 / totalPlayers;
          const angle = (baseAngle + (playerPosition * angleStep)) % 360;
          const distance = 320;
          
          // Get the center coordinates of the game table instead of window
          let centerX = window.innerWidth / 2; // fallback
          let centerY = window.innerHeight / 2; // fallback
          
          // Find the table element and get its center position
          const tableElement = document.querySelector('.table');
          if (tableElement) {
            const tableRect = tableElement.getBoundingClientRect();
            centerX = tableRect.left + tableRect.width / 2;
            centerY = tableRect.top + tableRect.height / 2;
          }
          
          const radians = (angle - 90) * Math.PI / 180;
          
          return {
            x: centerX + Math.cos(radians) * distance,
            y: centerY + Math.sin(radians) * distance,
            width: 65,
            height: 90
          };
        }
      }
    }
    return null;
  }
  
  // Export function to get processed players data for positioning calculations
  export function getProcessedPlayers() {
    return players;
  }
  
  // Set up SignalR handlers
  onMount(() => {
    signalR.registerHandlers({
      onCardRequirements: handleCardRequirements,
      onPlayCardError: handlePlayCardError,
      onChooseCard: handleChooseCard,
      onCardChoiceError: handleCardChoiceError,
    });
  });
  
  // Handle card requirements response from server
  function handleCardRequirements(requirements: CardRequirementsDto) {
    console.log('Received card requirements:', requirements);
    cardRequirements = requirements;
    
    // Reset requirement state
    selectedTargetEmail = null;
    selectedCardType = null;
    currentStep = 0;
    
    // Check if the card has requirements
    if (cardRequirements.requirements && cardRequirements.requirements.length > 0 && 
        cardRequirements.requirements[0] !== CardActionRequirements.None) {
      // Calculate steps needed
      const needsPlayer = cardRequirements.requirements.includes(CardActionRequirements.SelectPlayer);
      const needsCardType = cardRequirements.requirements.includes(CardActionRequirements.SelectCardType);
      
      totalSteps = (needsPlayer ? 1 : 0) + (needsCardType ? 1 : 0);
      
      // Show modal with rules first
      gameFlowState = 'showing_rules';
      showRequirementsModal = true;
    } else {
      // For cards with no requirements, we can play them directly
      playCard(cardRequirements.cardType, null, null);
    }
  }
  
  // Handle play card error from server
  function handlePlayCardError(error: string) {
    console.error('❌ Play card error from server:', error);
    
    // Show error to user (you might want to replace this with a proper notification system)
    alert(`Card play failed: ${error}`);
    
    // Reset the card playing state so user can try again
    resetCardPlayingState();
  }
  
  // Handle choose card event from server
  function handleChooseCard(data: { player: string }) {
    // Only show card choice modal if it's for the current player
    if (data.player !== currentUserEmail) {
      return;
    }
    
    // Get the current player's cards from the game status directly
    if (!gameStatus || !gameStatus.yourCards || gameStatus.yourCards.length === 0) {
      alert('Error: No cards available to choose from');
      return;
    }
    
    cardsToChoose = [...gameStatus.yourCards]; // Make a copy
    showCardChoiceModal = true;
  }
  
  // Handle card choice error from server
  function handleCardChoiceError(error: string) {
    alert(`Card choice failed: ${error}`);
  }
  
  // Handle modal events
  function handleModalClose() {
    showRequirementsModal = false;
    startNextStep();
  }
  
  // Handle card choice modal events
  function handleCardChoiceModalClose() {
    showCardChoiceModal = false;
    cardsToChoose = [];
  }
  
  async function handleCardChoiceSubmit(event: CustomEvent<{ keepCard: CardType; returnCards: CardType[] }>) {
    const { keepCard, returnCards } = event.detail;
    
    try {
      await signalR.submitCardChoice(roomId, keepCard, returnCards);
      
      // Close the modal
      showCardChoiceModal = false;
      cardsToChoose = [];
    } catch (error) {
      alert(`Failed to submit card choice: ${error}`);
    }
  }
  
  // Start the next step in the card playing process
  function startNextStep() {
    if (!cardRequirements) return;
    
    const needsPlayer = cardRequirements.requirements.includes(CardActionRequirements.SelectPlayer);
    const needsCardType = cardRequirements.requirements.includes(CardActionRequirements.SelectCardType);
    
    // Always do player selection first, then card type selection
    if (currentStep === 0 && needsPlayer) {
      gameFlowState = 'selecting_player';
    } else if ((currentStep === 0 && !needsPlayer && needsCardType) || 
               (currentStep === 1 && needsCardType)) {
      gameFlowState = 'selecting_card_type';
    } else {
      // All steps completed
      playCard(cardRequirements.cardType, selectedTargetEmail, selectedCardType);
      resetCardPlayingState();
    }
  }
  
  // Handle clicking on players for targeting
  function handlePlayerClick(playerEmail: string) {
    // Prevent interactions during animations
    if (isAnimationPlaying) {
      return;
    }
    
    if (gameFlowState !== 'selecting_player' || !cardRequirements) {
      return; // Not in player selection mode
    }
    
    // Check if this player is a valid target
    if (!cardRequirements.possibleTargets.includes(playerEmail)) {
      return;
    }
    
    selectedTargetEmail = playerEmail;
    currentStep++;
    
    // Move to next step
    startNextStep();
  }
  
  // Handle clicking on card types for selection
  function handleCardTypeClick(cardType: CardType) {
    // Prevent interactions during animations
    if (isAnimationPlaying) {
      return;
    }
    
    if (gameFlowState !== 'selecting_card_type' || !cardRequirements) {
      return; // Not in card type selection mode
    }
    
    // Check if this card type is valid
    if (!cardRequirements.possibleCardTypes.includes(cardType)) {
      return;
    }
    
    selectedCardType = cardType;
    currentStep++;
    
    // Move to next step
    startNextStep();
  }
  
  function resetCardPlayingState() {
    showRequirementsModal = false;
    selectedCard = null;
    cardRequirements = null;
    selectedTargetEmail = null;
    selectedCardType = null;
    gameFlowState = 'idle';
    currentStep = 0;
    totalSteps = 0;
  }
  
  // Placeholder function for actually playing a card
  async function playCard(cardType: CardType, targetPlayerEmail: string | null, targetCardType: CardType | null) {
    try {
      // Create the card play DTO
      const cardPlayDto = {
        cardType: cardType,
        targetPlayerEmail: targetPlayerEmail,
        targetCardType: targetCardType
      };
      
      // Send play card request to server
      await signalR.playCard(roomId, cardPlayDto);
      
      // Reset the card playing state after successful send
      // Note: The actual game state will be updated via SignalR events
      resetCardPlayingState();
    } catch (error) {
      console.error('❌ Error sending card play request:', error);
      // Let the PlayCardError handler deal with server-side errors
      // For client-side errors, we could show a different message
      alert(`Failed to send card play request: ${error}`);
      resetCardPlayingState();
    }  }

  // Handle clicking on a player's card
  async function handleCardClick(cardType: CardType) {
    // Prevent interactions during animations
    if (isAnimationPlaying) {
      return;
    }
    
    if (!isMyTurn) {
      return;
    }
    
    // Check if the local player actually has this card
    const localPlayer = players.find(p => p.isLocalPlayer);
    if (!localPlayer || !localPlayer.cards.includes(cardType)) {
      return;
    }
    
    selectedCard = cardType;
    
    try {
      // Get card requirements from server
      await signalR.getCardRequirements(roomId, cardType);
    } catch (error) {
      console.error('Error checking card requirements:', error);
    }
  }

  // Handle drawing a card by clicking on the deck
  async function handleDrawCard() {
    // Prevent interactions during animations
    if (isAnimationPlaying) {
      return;
    }
    
    if (!isMyTurn) {
      return; // Ignore clicks when it's not the player's turn
    }
    
    try {
      await signalR.drawCard(roomId);
    } catch (error) {
      console.error('Error drawing card:', error);
    }
  }
</script>

<div class="game-container">
  <WoodenTable>
    <CardDeck 
      {isMyTurn} 
      cardsRemainingInDeck={gameStatus?.cardsRemainingInDeck || 0}
      {isAnimationPlaying}
      on:drawCard={handleDrawCard}
    />
    
    <!-- Players positioned around the table -->
    {#each players as player, index (player.id)}
      {@const position = getPlayerPosition(index, totalPlayers)}
      {@const isValidTarget = gameFlowState === 'selecting_player' && cardRequirements && cardRequirements.possibleTargets.includes(player.email)}
      {@const isSelectedTarget = selectedTargetEmail === player.email}
      <PlayerArea 
        bind:this={playerAreaComponents[player.email]}
        {player}
        {position}
        isValidTarget={!!isValidTarget}
        isSelectedTarget={!!isSelectedTarget}
        {isMyTurn}
        {selectedCard}
        {gameFlowState}
        {hiddenCardType}
        {animatingPlayerEmail}
        {isAnimationPlaying}
        on:playerClick={(e) => handlePlayerClick(e.detail.playerEmail)}
        on:cardClick={(e) => handleCardClick(e.detail.cardType)}
      />
    {/each}    
  </WoodenTable>
</div>

<!-- Card Type Selection Area (appears in center when needed) - Hidden during animations -->
{#if gameFlowState === 'selecting_card_type' && cardRequirements && !isAnimationPlaying}
  <CardTypeSelector 
    possibleCardTypes={cardRequirements.possibleCardTypes}
    {selectedCardType}
    on:cardTypeClick={(e) => handleCardTypeClick(e.detail.cardType)}
  />
{/if}

<!-- Game Flow Instructions -->
<GameFlowInstructions 
  {gameFlowState}
  {currentStep}
  {totalSteps}
/>

<!-- Card Requirements Modal - Hidden during animations -->
{#if !isAnimationPlaying}
  <CardRequirementsModal
    bind:isOpen={showRequirementsModal}
    requirements={cardRequirements}
    players={players}
    on:close={handleModalClose}
  />
{/if}

<!-- Card Choice Modal - Hidden during animations -->
{#if !isAnimationPlaying}
  <CardChoiceModal
    bind:isOpen={showCardChoiceModal}
    cards={cardsToChoose}
    on:close={handleCardChoiceModalClose}
    on:submit={handleCardChoiceSubmit}
  />
{/if}

<style>
  .game-container {
    flex: 1;
    min-width: 600px;
    height: var(--game-table-height, 80vh);
    min-height: 600px;
    display: flex;
    justify-content: center;
    align-items: center;
    background: linear-gradient(135deg, #3e2723 0%, #5d4037 50%, #3e2723 100%);
    border-radius: 12px;
    padding: 2rem;
    position: relative;
  }

  @media (max-width: 1024px) {
    .game-container {
      min-width: 500px;
      padding: 1.5rem;
    }
  }

  @media (max-width: 768px) {
    .game-container {
      min-width: unset;
      width: 100%;
      height: var(--game-table-height, 70vh);
      min-height: 500px;
      padding: 1rem;
    }
  }
</style>
