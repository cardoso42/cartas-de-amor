<script lang="ts">
  import { CardType, type InitialGameStatusDto, type CardRequirementsDto, CardActionRequirements } from '$lib/types/game-types';
  import { user } from '$lib/stores/userStore';
  import { signalR } from '$lib/services/signalRService';
  import { page } from '$app/stores';
  import { onMount } from 'svelte';
  import CardRequirementsModal from './CardRequirementsModal.svelte';
  
  // Props from parent component
  export let gameStatus: InitialGameStatusDto;
  export let currentUserEmail: string;
  export let currentTurnPlayerEmail: string = '';
  
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
  
  // Game flow states
  let gameFlowState: 'idle' | 'showing_rules' | 'selecting_player' | 'selecting_card_type' = 'idle';
  let currentStep = 0;
  let totalSteps = 0;
  
  // Process the game data into display format
  $: players = gameStatus ? processGameData(gameStatus, currentUserEmail) : [];
  $: totalPlayers = players.length;
  
  // Determine whose turn it is
  $: currentTurnPlayer = getCurrentTurnPlayer(gameStatus, currentTurnPlayerEmail);
  $: isMyTurn = currentTurnPlayer === currentUserEmail;
  
  // Set up SignalR handlers
  onMount(() => {
    signalR.registerHandlers({
      onCardRequirements: handleCardRequirements
    });
  });
  
  // Handle card requirements response from server
  function handleCardRequirements(requirements: any) {
    console.log('Received card requirements:', requirements);
    cardRequirements = requirements as CardRequirementsDto;
    
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
  
  function getRequirementName(requirement: CardActionRequirements): string {
    switch (requirement) {
      case CardActionRequirements.None: return 'None';
      case CardActionRequirements.SelectPlayer: return 'Select Player';
      case CardActionRequirements.SelectCardType: return 'Select Card Type';
      default: return 'Unknown';
    }
  }
  
  // Handle modal events
  function handleModalClose() {
    console.log('Modal closed - starting card playing flow');
    showRequirementsModal = false;
    startNextStep();
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
      console.log('Possible targets:', cardRequirements.possibleTargets.map(email => getPlayerDisplayName(email)));
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
    console.log('✅ Selected target player:', getPlayerDisplayName(playerEmail));
    
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
  function playCard(cardType: CardType, targetPlayerEmail: string | null, targetCardType: CardType | null) {
    console.log('🎮 Playing card:', {
      card: getCardName(cardType),
      target: targetPlayerEmail ? getPlayerDisplayName(targetPlayerEmail) : 'none',
      guessedCard: targetCardType ? getCardName(targetCardType) : 'none'
    });
    
    // TODO: Implement actual card playing logic here
    // This would typically involve calling a SignalR method like:
    // await signalR.playCard(roomId, cardType, targetPlayerEmail, targetCardType);
  }
  
  function getPlayerDisplayName(email: string): string {
    const player = players.find(p => p.email === email);
    return player?.name || email.split('@')[0];
  }
  
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

  function getCurrentTurnPlayer(status: InitialGameStatusDto | null, turnPlayerEmail: string): string {
    if (!status) return '';
    
    // If we have a turn player email from SignalR, use that
    if (turnPlayerEmail) return turnPlayerEmail;
    
    // Otherwise, use the FirstPlayerIndex from the game status
    if (status.allPlayersInOrder && status.firstPlayerIndex >= 0 && status.firstPlayerIndex < status.allPlayersInOrder.length) {
      return status.allPlayersInOrder[status.firstPlayerIndex];
    }
    
    return '';
  }
  
  function processGameData(status: InitialGameStatusDto, userEmail: string) {
    const processedPlayers = [];
    
    // Add local player at position 0 (bottom)
    processedPlayers.push({
      id: 0,
      name: localPlayerName, // Use the username from user store
      email: userEmail,
      isLocalPlayer: true,
      position: 0,
      tokens: status.score || 0,
      cards: status.yourCards || [],
      cardsInHand: (status.yourCards || []).length,
      isProtected: status.isProtected || false,
      isCurrentTurn: userEmail === currentTurnPlayer
    });
    
    // Add other players in order around the table
    (status.otherPlayersPublicData || []).forEach((player, index) => {
      processedPlayers.push({
        id: index + 1,
        name: player.username || player.userEmail.split('@')[0], // Use proper username or fallback to email username
        email: player.userEmail,
        isLocalPlayer: false,
        position: index + 1,
        tokens: player.score || 0,
        cards: [], // Other players' cards are hidden
        cardsInHand: player.cardsInHand || 1,
        isProtected: player.isProtected || false,
        isCurrentTurn: player.userEmail === currentTurnPlayer
      });
    });
    
    return processedPlayers;
  }
  
  function getPlayerPosition(playerIndex: number, totalPlayers: number) {
    if (totalPlayers === 1) {
      return { angle: 180, distance: 320 }; // Single player at bottom
    }
    
    const baseAngle = 180; // Start at bottom (180 degrees)
    const angleStep = 360 / totalPlayers;
    const angle = (baseAngle + (playerIndex * angleStep)) % 360;
    
    // Distance from center of table
    const distance = 320;
    
    return { angle, distance };
  }
  
  function getCardName(cardType: CardType): string {
    const cardNames = {
      [CardType.Spy]: 'Spy',
      [CardType.Guard]: 'Guard',
      [CardType.Priest]: 'Priest', 
      [CardType.Baron]: 'Baron',
      [CardType.Handmaid]: 'Handmaid',
      [CardType.Prince]: 'Prince',
      [CardType.Chanceller]: 'Chanceller',
      [CardType.King]: 'King',
      [CardType.Countess]: 'Countess',
      [CardType.Princess]: 'Princess'
    };
    return cardNames[cardType] || 'Unknown';
  }
</script>

<div class="game-container">
  <div class="table-container">
    <!-- The wooden circular table -->
    <div class="table">
      <!-- Card deck in the center -->
      <div class="deck-area">
        <div 
          class="card-deck" 
          class:clickable={isMyTurn}
          class:disabled={!isMyTurn}
          on:click={handleDrawCard}
          on:keydown={(e) => e.key === 'Enter' && handleDrawCard()}
          role="button"
          tabindex={isMyTurn ? 0 : -1}
          title={isMyTurn ? 'Click to draw a card' : 'Wait for your turn to draw a card'}
        >
          <div class="deck-cards">
            <!-- Deck cards stack -->
            <div class="deck-card"></div>
            <div class="deck-card"></div>
            <div class="deck-card"></div>
          </div>
          <div class="deck-label">
            {isMyTurn ? 'Draw Card' : 'Deck'}
          </div>
        </div>
      </div>
      
      <!-- Players positioned around the table -->
      {#each players as player, index (player.id)}
        {@const position = getPlayerPosition(index, totalPlayers)}
        {@const x = Math.cos((position.angle - 90) * Math.PI / 180) * position.distance}
        {@const y = Math.sin((position.angle - 90) * Math.PI / 180) * position.distance}
        {@const isValidTarget = gameFlowState === 'selecting_player' && cardRequirements && cardRequirements.possibleTargets.includes(player.email)}
        {@const isSelectedTarget = selectedTargetEmail === player.email}
        <div 
          class="player-area"
          class:local-player={player.isLocalPlayer}
          class:clickable-target={isValidTarget}
          class:invalid-target={gameFlowState === 'selecting_player' && cardRequirements && !cardRequirements.possibleTargets.includes(player.email)}
          class:selected-target={isSelectedTarget}
          style="
            left: 50%;
            top: 50%;
            transform: translate(calc(-50% + {x}px), calc(-50% + {y}px));
          "
          on:click={() => handlePlayerClick(player.email)}
          on:keydown={(e) => e.key === 'Enter' && handlePlayerClick(player.email)}
          role={isValidTarget ? 'button' : undefined}
          tabindex={isValidTarget ? 0 : -1}
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
          
          <!-- Player's hand (face down for others, face up for local player) -->
          <div class="player-hand">
            {#if player.isLocalPlayer}
              <!-- Local player sees their cards face up -->
              {#each player.cards as card}
                <div 
                  class="card player-card face-up" 
                  class:clickable={isMyTurn}
                  class:selected={selectedCard === card}
                  on:click={() => handleCardClick(card)}
                  on:keydown={(e) => e.key === 'Enter' && handleCardClick(card)}
                  role="button"
                  tabindex={isMyTurn ? 0 : -1}
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
              {#each Array(player.cardsInHand || 1) as _, i}
                <div class="card player-card face-down"></div>
              {/each}
            {/if}
          </div>
        </div>
      {/each}
      
      <!-- Card Type Selection Area (appears in center when needed) -->
      {#if gameFlowState === 'selecting_card_type' && cardRequirements}
        <div class="card-type-selection-area">
          <div class="card-type-title">
            <h3>Choose a card type:</h3>
            <p>Click on one of the cards below</p>
          </div>
          <div class="card-type-display">
            {#each cardRequirements.possibleCardTypes as cardType}
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
      {/if}
      
      <!-- Game Flow Instructions -->
      {#if gameFlowState !== 'idle'}
        <div class="flow-instructions">
          {#if gameFlowState === 'selecting_player'}
            <div class="instruction-bubble">
              <strong>Step {currentStep + 1} of {totalSteps}:</strong> Click on a player to target them
            </div>
          {:else if gameFlowState === 'selecting_card_type'}
            <div class="instruction-bubble">
              <strong>Step {currentStep + 1} of {totalSteps}:</strong> Click on a card type to select it
            </div>
          {/if}
        </div>
      {/if}
      
      <!-- Played cards positioned on the table surface in front of each player -->
      <!-- TODO: Add played cards when implementing card playing functionality -->
      <!-- {#each players as player, index (player.id)}
        {@const position = getPlayerPosition(index, totalPlayers)}
        {@const playedCardDistance = 150}
        {@const playedX = Math.cos((position.angle - 90) * Math.PI / 180) * playedCardDistance}
        {@const playedY = Math.sin((position.angle - 90) * Math.PI / 180) * playedCardDistance}
        <div 
          class="played-cards-area"
          style="
            left: 50%;
            top: 50%;
            transform: translate(calc(-50% + {playedX}px), calc(-50% + {playedY}px));
          "
        >
          <div class="card played-card face-up">
            <div class="card-content">
              <div class="card-number">2</div>
              <div class="card-name">Priest</div>
            </div>
          </div>
        </div>
      {/each} -->
    </div>
  </div>
</div>

<!-- Card Requirements Modal -->
<CardRequirementsModal
  bind:isOpen={showRequirementsModal}
  requirements={cardRequirements}
  players={players}
  currentUserEmail={currentUserEmail}
  on:close={handleModalClose}
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
  
  .table-container {
    position: relative;
    width: 600px;
    height: 600px;
  }
  
  .table {
    position: relative;
    width: 100%;
    height: 100%;
    background: radial-gradient(circle, #d4a574 10%, #8b4513 40%, #654321 70%, #4a2c17 100%);
    border-radius: 50%;
    box-shadow: 
      inset 0 0 0 8px #a0522d,
      inset 0 0 0 12px #8b4513,
      inset 0 0 40px rgba(101, 67, 33, 0.8),
      0 20px 40px rgba(0, 0, 0, 0.4),
      0 0 0 2px #5d4037;
    
    /* Enhanced wood grain effect */
    background-image: 
      repeating-linear-gradient(
        30deg,
        transparent,
        transparent 1px,
        rgba(139, 69, 19, 0.15) 1px,
        rgba(139, 69, 19, 0.15) 3px
      ),
      repeating-linear-gradient(
        120deg,
        transparent,
        transparent 2px,
        rgba(160, 82, 45, 0.1) 2px,
        rgba(160, 82, 45, 0.1) 4px
      );
  }
  
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
  
  .card-deck.clickable {
    cursor: pointer;
  }
  
  .card-deck.clickable:hover {
    transform: scale(1.05);
    box-shadow: 0 0 15px rgba(255, 215, 0, 0.8);
  }
  
  .card-deck.clickable:hover .deck-cards {
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.4);
  }
  
  .card-deck.disabled {
    cursor: not-allowed;
    opacity: 0.7;
  }
  
  .card-deck:focus {
    outline: 2px solid #ffd700;
    outline-offset: 2px;
  }
  
  .deck-cards {
    position: relative;
    width: 60px;
    height: 84px;
  }
  
  .deck-card {
    position: absolute;
    width: 60px;
    height: 84px;
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
  
  .played-cards {
    display: flex;
    gap: 2px;
    flex-wrap: wrap;
    justify-content: center;
    max-width: 100px;
  }
  
  .played-cards-area {
    position: absolute;
    display: flex;
    gap: 4px;
    flex-wrap: wrap;
    justify-content: center;
    z-index: 5;
  }
  
  .played-card {
    width: 40px;
    height: 56px;
    background: linear-gradient(135deg, #fafafa 0%, #e0e0e0 100%);
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    padding: 3px;
  }
  
  .played-card .card-number {
    font-size: 0.8rem;
  }
  
  .played-card .card-name {
    font-size: 0.4rem;
  }
  
  /* Card Type Selection Area */
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
  
  .card-type-card .card-number {
    font-size: 1.2rem;
    font-weight: bold;
    color: #9c27b0;
    text-align: center;
  }
  
  .card-type-card .card-name {
    font-size: 0.7rem;
    font-weight: bold;
    color: #333;
    text-align: center;
    line-height: 1;
  }
  
  /* Flow Instructions */
  .flow-instructions {
    position: absolute;
    top: 20px;
    left: 50%;
    transform: translateX(-50%);
    z-index: 20;
  }
  
  .instruction-bubble {
    background: rgba(0, 0, 0, 0.9);
    color: white;
    padding: 0.75rem 1.5rem;
    border-radius: 20px;
    border: 2px solid #ffd700;
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.5);
    text-align: center;
    font-size: 0.9rem;
    animation: instructionPulse 2s ease-in-out infinite;
  }
  
  @keyframes instructionPulse {
    0%, 100% { 
      box-shadow: 0 4px 15px rgba(0, 0, 0, 0.5);
    }
    50% { 
      box-shadow: 0 4px 20px rgba(255, 215, 0, 0.3);
    }
  }
  
  /* Responsive design for smaller screens */
  @media (max-width: 768px) {
    .table-container {
      width: 400px;
      height: 400px;
    }
    
    .token {
      font-size: 0.9rem;
    }
    
    .card {
      width: 40px;
      height: 56px;
    }
    
    .played-card {
      width: 32px;
      height: 45px;
    }
  }
</style>
