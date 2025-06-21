<script lang="ts">
  import { CardType, type InitialGameStatusDto, type CardRequirementsDto, CardActionRequirements } from '$lib/types/game-types';
  import { user } from '$lib/stores/userStore';
  import { signalR } from '$lib/services/signalRService';
  import { page } from '$app/stores';
  import { onMount } from 'svelte';
  import CardRequirementsModal from './CardRequirementsModal.svelte';
  import CardChoiceModal from './CardChoiceModal.svelte';
  import WoodenTable from './WoodenTable.svelte';
  import CardDeck from './CardDeck.svelte';
  import PlayerArea from './PlayerArea.svelte';
  import CardTypeSelector from './CardTypeSelector.svelte';
  import GameFlowInstructions from './GameFlowInstructions.svelte';
  import { getCardName, getRequirementName } from '$lib/utils/cardUtils';
  import { getPlayerPosition, getCurrentTurnPlayer, getPlayerDisplayName } from '$lib/utils/gameUtils';
  import { processGameData } from '$lib/utils/gameDataProcessor';
  
  // Props from parent component
  export let gameStatus: InitialGameStatusDto;
  export let currentUserEmail: string;
  export let currentTurnPlayerEmail: string = '';
  export let localPlayerPlayedCards: number[] = []; // Played cards for the local player
  
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
      
      console.log(`Card ${getCardName(cardRequirements.cardType)} has requirements:`, 
        cardRequirements.requirements.map(req => getRequirementName(req)).join(', '));
      
      // Calculate steps needed
      const needsPlayer = cardRequirements.requirements.includes(CardActionRequirements.SelectPlayer);
      const needsCardType = cardRequirements.requirements.includes(CardActionRequirements.SelectCardType);
      
      totalSteps = (needsPlayer ? 1 : 0) + (needsCardType ? 1 : 0);
      
      // Show modal with rules first
      gameFlowState = 'showing_rules';
      showRequirementsModal = true;
      
      console.log(`Starting ${totalSteps}-step card playing process`);
    } else {
      console.log(`Card ${getCardName(cardRequirements.cardType)} has no special requirements - can be played immediately`);
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
    console.log('Modal closed - starting card playing flow');
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
      console.log('Step 1: Select a target player');
      console.log('Possible targets:', cardRequirements.possibleTargets.map(email => getPlayerDisplayName(email, players)));
    } else if ((currentStep === 0 && !needsPlayer && needsCardType) || 
               (currentStep === 1 && needsCardType)) {
      gameFlowState = 'selecting_card_type';
      console.log('Step 2: Select a card type');
      console.log('Possible card types:', cardRequirements.possibleCardTypes.map(ct => getCardName(ct)));
    } else {
      // All steps completed
      console.log('All requirements fulfilled, playing card');
      playCard(cardRequirements.cardType, selectedTargetEmail, selectedCardType);
      resetCardPlayingState();
    }
  }
  
  // Handle clicking on players for targeting
  function handlePlayerClick(playerEmail: string) {
    if (gameFlowState !== 'selecting_player' || !cardRequirements) {
      return; // Not in player selection mode
    }
    
    // Check if this player is a valid target
    if (!cardRequirements.possibleTargets.includes(playerEmail)) {
      console.log('Invalid target player selected');
      return;
    }
    
    selectedTargetEmail = playerEmail;
    currentStep++;
    console.log('✅ Selected target player:', getPlayerDisplayName(playerEmail, players));
    
    // Move to next step
    startNextStep();
  }
  
  // Handle clicking on card types for selection
  function handleCardTypeClick(cardType: CardType) {
    if (gameFlowState !== 'selecting_card_type' || !cardRequirements) {
      return; // Not in card type selection mode
    }
    
    // Check if this card type is valid
    if (!cardRequirements.possibleCardTypes.includes(cardType)) {
      console.log('Invalid card type selected');
      return;
    }
    
    selectedCardType = cardType;
    currentStep++;
    console.log('✅ Selected card type:', getCardName(cardType));
    
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
    console.log('🎮 Playing card:', {
      card: getCardName(cardType),
      target: targetPlayerEmail ? getPlayerDisplayName(targetPlayerEmail, players) : 'none',
      guessedCard: targetCardType ? getCardName(targetCardType) : 'none'
    });
    
    try {
      // Create the card play DTO
      const cardPlayDto = {
        cardType: cardType,
        targetPlayerEmail: targetPlayerEmail,
        targetCardType: targetCardType
      };
      
      // Send play card request to server
      await signalR.playCard(roomId, cardPlayDto);
      console.log('✅ Card play request sent successfully');
      
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
    if (!isMyTurn) {
      console.log('Not your turn - cannot play cards');
      return;
    }
    
    // Check if the local player actually has this card
    const localPlayer = players.find(p => p.isLocalPlayer);
    if (!localPlayer || !localPlayer.cards.includes(cardType)) {
      console.log('You do not have this card');
      return;
    }
    
    console.log(`Attempting to play card: ${getCardName(cardType)}`);
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
    if (!isMyTurn) {
      return; // Ignore clicks when it's not the player's turn
    }
    
    try {
      await signalR.drawCard(roomId);
      console.log('Draw card request sent');
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
      on:drawCard={handleDrawCard}
    />
    
    <!-- Players positioned around the table -->
    {#each players as player, index (player.id)}
      {@const position = getPlayerPosition(index, totalPlayers)}
      {@const isValidTarget = gameFlowState === 'selecting_player' && cardRequirements && cardRequirements.possibleTargets.includes(player.email)}
      {@const isSelectedTarget = selectedTargetEmail === player.email}
      <PlayerArea 
        {player}
        {position}
        isValidTarget={!!isValidTarget}
        isSelectedTarget={!!isSelectedTarget}
        {isMyTurn}
        {selectedCard}
        {gameFlowState}
        on:playerClick={(e) => handlePlayerClick(e.detail.playerEmail)}
        on:cardClick={(e) => handleCardClick(e.detail.cardType)}
      />
    {/each}    
  </WoodenTable>
</div>

<!-- Card Type Selection Area (appears in center when needed) -->
{#if gameFlowState === 'selecting_card_type' && cardRequirements}
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

<!-- Card Requirements Modal -->
<CardRequirementsModal
  bind:isOpen={showRequirementsModal}
  requirements={cardRequirements}
  players={players}
  on:close={handleModalClose}
/>

<!-- Card Choice Modal -->
<CardChoiceModal
  bind:isOpen={showCardChoiceModal}
  cards={cardsToChoose}
  on:close={handleCardChoiceModalClose}
  on:submit={handleCardChoiceSubmit}
/>

<style>
  .game-container {
    width: 100%;
    height: 80vh;
    display: flex;
    justify-content: center;
    align-items: center;
    background: linear-gradient(135deg, #3e2723 0%, #5d4037 50%, #3e2723 100%);
    border-radius: 12px;
    padding: 2rem;
  }
</style>
