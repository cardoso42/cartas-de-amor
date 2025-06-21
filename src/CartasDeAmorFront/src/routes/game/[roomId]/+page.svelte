<script lang="ts">
  import AuthGuard from '$lib/components/AuthGuard.svelte';
  import GameLobby from '$lib/components/game/GameLobby.svelte';
  import { onMount, onDestroy } from 'svelte';
  import { page } from '$app/stores';
  import { signalR } from '$lib/services/signalRService';
  import { goto } from '$app/navigation';
  import { get } from 'svelte/store';
  import { user } from '$lib/stores/userStore';
  import { gameStore } from '$lib/stores/gameStore';
  import type { 
    InitialGameStatusDto, 
    PrivatePlayerUpdateDto, 
    CardActionResultDto,
    PublicPlayerUpdateDto 
  } from '$lib/types/game-types';

  // Get room ID from URL params
  const roomId = $page.data.roomId;
  
  // Get the current user's email for ownership check
  let userEmail = '';
  const unsubscribeUser = user.subscribe(state => {
    userEmail = state.email || '';
    console.log('User email from userStore:', userEmail);
  });
  
  // Game state
  let gameStatus: InitialGameStatusDto | null = null;
  let localPlayerPlayedCards: number[] = []; // Track local player's played cards
  let isConnected = false;
  let connectionError = '';
  let roomName = '';
  let isRoomOwner = false;
  let players: string[] = [];
  let isGameStarting = false;
  let currentTurnPlayerEmail = '';
  let errorMessage = '';
  let errorTimeout: ReturnType<typeof setTimeout> | undefined;
  
  // Track recent draw events to prevent duplicate processing
  let recentDrawEvents = new Set<string>();

  // Get initial game data from gameStore
  import { get as getStore } from 'svelte/store';
	import GameTable from '$lib/components/game/GameTable.svelte';
  const initialGameData = getStore(gameStore);
  if (initialGameData && initialGameData.roomId === roomId) {
    roomName = initialGameData.roomName || '';
    players = initialGameData.players || [];
    isRoomOwner = initialGameData.isRoomOwner;
  }
  
  // Handle connection events
  let unsubscribeSignalR = () => {};
  unsubscribeSignalR = signalR.subscribe(state => {
    isConnected = state.status === 'connected';
    if (state.error) {
      connectionError = state.error;
    }
  });
  
  async function leaveRoom() {
    try {
      await signalR.leaveRoom(roomId);
      goto('/rooms');
    } catch (err) {
      console.error('Error leaving room:', err);
      // If error, go back to rooms anyway
      goto('/rooms');
    }
  }
  
  // Function to show error messages with auto-dismiss
  function showError(message: string, duration = 5000) {
    if (errorTimeout) {
      clearTimeout(errorTimeout);
    }
    errorMessage = message;
    errorTimeout = setTimeout(() => {
      errorMessage = '';
    }, duration);
  }
  
  // Function to dismiss error manually
  function dismissError() {
    if (errorTimeout) {
      clearTimeout(errorTimeout);
    }
    errorMessage = '';
  }
  
  function showNotification(message: string, type: 'info' | 'success' | 'warning' = 'info') {
    // For now, just log to console. Could be enhanced with a proper notification system
    console.log(`[${type.toUpperCase()}] ${message}`);
  }
  
  function getPlayerDisplayName(email: string): string {
    // Handle undefined or null email
    if (!email) {
      console.warn('getPlayerDisplayName called with undefined/null email');
      return 'Unknown Player';
    }
    
    // If it's the current user, return "You"
    if (email === userEmail) {
      return 'You';
    }
    
    // Try to find the player in the game status to get their username
    if (gameStatus?.otherPlayersPublicData) {
      const player = gameStatus.otherPlayersPublicData.find(p => p.userEmail === email);
      if (player?.username) {
        return player.username;
      }
    }
    
    // Fall back to the part before @ in the email
    return email.split('@')[0];
  }
  
  function getCardName(cardType: number): string {
    // Handle undefined or null cardType
    if (cardType === undefined || cardType === null) {
      console.warn('getCardName called with undefined/null cardType');
      return 'Unknown Card';
    }
    
    const cardNames = {
      0: 'Spy',
      1: 'Guard',
      2: 'Priest',
      3: 'Baron',
      4: 'Servant',
      5: 'Prince',
      6: 'Chancellor',
      7: 'King',
      8: 'Countess',
      9: 'Princess'
    };
    return cardNames[cardType as keyof typeof cardNames] || `Card ${cardType}`;
  }
  
  // Handle card played event - triggered when a card is initially played
  function handleCardPlayed(data: { player: string; cardType: number }) {
    if (!gameStatus) {
      return;
    }
    
    const playerEmail = data.player;
    const playedCard = data.cardType;
    
    console.log(`Handling card play: ${getPlayerDisplayName(playerEmail)} played ${getCardName(playedCard)}`);
    
    // If the player is the current user, update their cards in hand and played cards
    if (playerEmail === userEmail && gameStatus.yourCards) {    
      // Add to local player's played cards
      localPlayerPlayedCards = [...localPlayerPlayedCards, playedCard];
    } else {
      // Update other players' card count and played cards
      const otherPlayers = gameStatus.otherPlayersPublicData || [];
      const playerToUpdate = otherPlayers.find(p => p.userEmail === playerEmail);
      
      if (playerToUpdate) {
        // Reduce their cards in hand count
        playerToUpdate.cardsInHand = Math.max(0, (playerToUpdate.cardsInHand || 1) - 1);
        
        // Add the played card to their played cards list
        if (!playerToUpdate.playedCards) {
          playerToUpdate.playedCards = [];
        }
        playerToUpdate.playedCards.push(playedCard);
        
        console.log(`Updated ${getPlayerDisplayName(playerEmail)} - cards in hand: ${playerToUpdate.cardsInHand}, played card: ${getCardName(playedCard)}`);
      }
    }
    
    // Trigger reactivity by creating a new gameStatus object
    gameStatus = { ...gameStatus };
  }

  // Update player data from public player updates (without card type info)
  function updatePlayerDataFromPublicUpdate(playerPublicData: PublicPlayerUpdateDto) {
    if (!gameStatus || !playerPublicData) {
      return;
    }
    
    console.log('Updating player data from public update:', playerPublicData);
    
    // Handle any additional player data updates that don't involve card playing
    // For example, status changes, protection status, etc.
    const invokerEmail = playerPublicData.userEmail;
    
    // Update invoker's data
    if (invokerEmail === userEmail) {
      // Update current player's status if needed
      // (This might include protection status, elimination status, etc.)
    } else {
      // Update other players' public data
      const otherPlayers = gameStatus.otherPlayersPublicData || [];
      const playerToUpdate = otherPlayers.find(p => p.userEmail === invokerEmail);
      
      if (playerToUpdate) {
        // Update any public player data from the CardActionResultDto
        // This could include status changes, card counts, etc.
        console.log(`Updated public data for ${getPlayerDisplayName(invokerEmail)}`);
      }
    }
    
    // Trigger reactivity by creating a new gameStatus object
    gameStatus = { ...gameStatus };
  }
  
  async function startGame() {
    if (!isRoomOwner) {
      return; // Only room owner can start the game
    }
    
    try {
      isGameStarting = true;
      await signalR.startGame(roomId);
      // No need to do anything here, the RoundStarted event will update the UI
    } catch (err) {
      console.error('Error starting game:', err);
      isGameStarting = false;
    }
  }

  // Initialize SignalR and join room on mount
  onMount(async () => {
    try {
      // Initialize SignalR if not already connected
      if (!isConnected) {
        await signalR.initialize();
      }

      // Get the current state synchronously
      const state = get(signalR);

      // Now check if we are already in this room (async logic after handlers)
      await new Promise<any>((resolve) => {
        let unsub = () => {};
        unsub = signalR.subscribe(s => {
          resolve(s);
          unsub();
        });
      });

      signalR.registerHandlers({
        onUserJoined: (playerEmail: string) => {
          console.log('Player joined:', playerEmail);
          if (!players.includes(playerEmail)) {
            players = [...players, playerEmail];
          }
        },
        onRoundStarted: (initialGameStatus: InitialGameStatusDto) => {
          console.log('Round started:', initialGameStatus);
          gameStatus = initialGameStatus;
          localPlayerPlayedCards = []; // Reset played cards for new round
          isGameStarting = false;
          
          // Set initial turn player from game status
          const status = initialGameStatus;
          if (status.allPlayersInOrder && status.firstPlayerIndex >= 0 && status.firstPlayerIndex < status.allPlayersInOrder.length) {
            currentTurnPlayerEmail = status.allPlayersInOrder[status.firstPlayerIndex];
            console.log('Initial turn player:', currentTurnPlayerEmail);
          }
        },
        onNextTurn: (playerEmail: string) => {
          console.log('Next turn:', playerEmail);
          currentTurnPlayerEmail = playerEmail;
        },
        onPlayerDrewCard: (playerEmail: string) => {
          console.log('Player drew card:', playerEmail);
                    
          // Update the visual representation to show the player now has an additional card
          if (gameStatus && playerEmail !== userEmail) {
            // Find the player in otherPlayersPublicData and increment their cardsInHand
            const otherPlayers = gameStatus.otherPlayersPublicData || [];
            const playerToUpdate = otherPlayers.find(p => p.userEmail === playerEmail);
            
            if (playerToUpdate) {
              playerToUpdate.cardsInHand = (playerToUpdate.cardsInHand || 1) + 1;
              // Trigger reactivity by creating a new gameStatus object
              gameStatus = { ...gameStatus, otherPlayersPublicData: [...otherPlayers] };
              console.log(`Updated ${playerEmail} cards in hand to ${playerToUpdate.cardsInHand}`);
            }
          }

          // Decrease the deck count when any player draws a card
          if (gameStatus && gameStatus.cardsRemainingInDeck > 0) {
            gameStatus.cardsRemainingInDeck -= 1;
          }
        },
        onPrivatePlayerUpdate: (playerUpdate: PrivatePlayerUpdateDto) => {
          console.log('Private player update:', playerUpdate);
          // Update the game status with the player's new cards
          if (gameStatus && playerUpdate.holdingCards) {
            gameStatus.yourCards = playerUpdate.holdingCards;
            gameStatus = { ...gameStatus }; // Trigger reactivity
          }
        },
        onDrawCardError: (error: string) => {
          console.error('Draw card error:', error);
          showError(`Failed to draw card: ${error}`);
        },
        onGameStartError: (error: string) => {
          console.error('Game start error:', error);
          isGameStarting = false;
          showError(`Failed to start game: ${error}`);
        },
        // New MessageFactory events replace old CardResult events
        onPlayCard: (data: { player: string; cardType: number }) => {
          console.log('PlayCard event:', data);
          // Handle the card play event with the card type information
          handleCardPlayed(data);
          // Also show notification
          showNotification(`${getPlayerDisplayName(data.player)} played ${getCardName(data.cardType)}`, 'info');
        },
        onGuessCard: (data: { invoker: string; cardType: number; target: string }) => {
          console.log('GuessCard event received:', data);
          console.log('Invoker:', data.invoker, 'Target:', data.target, 'CardType:', data.cardType);
          
          // Validate data before using it
          if (!data.invoker || !data.target || data.cardType === undefined) {
            console.error('GuessCard event received with invalid data:', data);
            showNotification('Someone made a guess', 'info');
            return;
          }
          
          showNotification(`${getPlayerDisplayName(data.invoker)} guessed ${getPlayerDisplayName(data.target)} has ${getCardName(data.cardType)}`, 'info');
        },
        onPeekCard: (data: { invoker: string; target: string }) => {
          console.log('PeekCard event:', data);
          showNotification(`${getPlayerDisplayName(data.invoker)} looked at ${getPlayerDisplayName(data.target)}'s card`, 'info');
        },
        onShowCard: (data: { invoker: string; target: string, card: number }) => {
          console.log('ShowCard event:', data);
          showNotification(`${getPlayerDisplayName(data.invoker)} looked at ${getPlayerDisplayName(data.target)}'s card: ${data.card}`, 'info');
        },
        onCompareCards: (data: { invoker: string; target: string }) => {
          console.log('CompareCards event:', data);
          showNotification(`${getPlayerDisplayName(data.invoker)} compared cards with ${getPlayerDisplayName(data.target)}`, 'info');
        },
        onComparisonTie: (data: { invoker: string; target: string }) => {
          console.log('ComparisonTie event:', data);
          showNotification(`${getPlayerDisplayName(data.invoker)} and ${getPlayerDisplayName(data.target)} tied in comparison`, 'info');
        },
        onDiscardCard: (data: { target: string; cardType: number }) => {
          console.log('DiscardCard event:', data);
          showNotification(`${getPlayerDisplayName(data.target)} discarded ${getCardName(data.cardType)}`, 'info');
          
          // Add the discarded card to the player's played cards display
          if (gameStatus) {
            const playerEmail = data.target;
            const discardedCard = data.cardType;
            
            if (playerEmail === userEmail) {
              // Add to local player's played cards
              localPlayerPlayedCards = [...localPlayerPlayedCards, discardedCard];
            } else {
              // Update other players' played cards
              const otherPlayers = gameStatus.otherPlayersPublicData || [];
              const playerToUpdate = otherPlayers.find(p => p.userEmail === playerEmail);
              
              if (playerToUpdate) {
                // Add the discarded card to their played cards list
                if (!playerToUpdate.playedCards) {
                  playerToUpdate.playedCards = [];
                }
                playerToUpdate.playedCards.push(discardedCard);
                
                console.log(`Added discarded card to ${getPlayerDisplayName(playerEmail)}: ${getCardName(discardedCard)}`);
              }
            }
            
            // Trigger reactivity by creating a new gameStatus object
            gameStatus = { ...gameStatus };
          }
        },
        onDrawCard: (data: { player: string }) => {
          console.log('DrawCard event:', data);
          showNotification(`${getPlayerDisplayName(data.player)} drew a card`, 'info');
          // Decrease deck count when a card is drawn
          if (gameStatus) {
            gameStatus.cardsRemainingInDeck = Math.max(0, (gameStatus.cardsRemainingInDeck || 0) - 1);
          }
        },
        onCardReturnedToDeck: (data: { player: string; cardCount: number }) => {
          console.log('CardReturnedToDeck event:', data);
          showNotification(`${getPlayerDisplayName(data.player)} returned ${data.cardCount} card(s) to the deck`, 'info');
          // Increase deck count when cards are returned to deck
          if (gameStatus) {
            gameStatus.cardsRemainingInDeck = (gameStatus.cardsRemainingInDeck || 0) + data.cardCount;
          }
        },
        onPlayerEliminated: (data: { player: string }) => {
          console.log('PlayerEliminated event:', data);
          showNotification(`${getPlayerDisplayName(data.player)} was eliminated!`, 'warning');
        },
        onSwitchCards: (data: { invoker: string; target: string }) => {
          console.log('SwitchCards event:', data);
          showNotification(`${getPlayerDisplayName(data.invoker)} switched cards with ${getPlayerDisplayName(data.target)}`, 'info');
        },
        onPlayerProtected: (data: { player: string }) => {
          console.log('PlayerProtected event:', data);
          showNotification(`${getPlayerDisplayName(data.player)} is now protected for 1 turn`, 'info');
        },
        onChooseCard: (data: { player: string }) => {
          console.log('ChooseCard event:', data);
          // This indicates the player needs to choose cards (like Chancellor effect)
          if (data.player === userEmail) {
            showNotification('You need to choose which cards to keep', 'info');
          } else {
            showNotification(`${getPlayerDisplayName(data.player)} needs to choose cards`, 'info');
          }
        },
        onPublicPlayerUpdate: (data: PublicPlayerUpdateDto) => {
          console.log('PublicPlayerUpdate event:', data);
          // Update the UI based on public player information (without card type)
          updatePlayerDataFromPublicUpdate(data);
        },
        // Other game events
        onCardChoiceSubmitted: (playerUpdate: PublicPlayerUpdateDto) => {
          console.log('Card choice submitted:', playerUpdate);
          showNotification(`${getPlayerDisplayName(playerUpdate.userEmail)} submitted their card choice`, 'info');
        },
        onCardChoiceError: (error: string) => {
          console.error('Card choice error:', error);
          showError(`Failed to submit card choice: ${error}`);
        },
        onMandatoryCardPlay: (message: string, requiredCardType: number) => {
          console.log('Mandatory card play:', message, requiredCardType);
          showError(`You must play the ${getCardName(requiredCardType)} card! ${message}`);
        },
        onRoundWinners: (winners: string[]) => {
          console.log('Round winners:', winners);
          const winnerNames = winners.map(email => getPlayerDisplayName(email)).join(', ');
          showNotification(`Round won by: ${winnerNames}`, 'success');
        },
        onBonusPoints: (players: string[]) => {
          console.log('Bonus points awarded to:', players);
          const playerNames = players.map(email => getPlayerDisplayName(email)).join(', ');
          showNotification(`Bonus points awarded to: ${playerNames}`, 'success');
        },
        onGameOver: (winners: string[]) => {
          console.log('Game over, winners:', winners);
          const winnerNames = winners.map(email => getPlayerDisplayName(email)).join(', ');
          showNotification(`🎉 Game Over! Winners: ${winnerNames}`, 'success');
        }
      });
    } catch (err) {
      console.error('Error connecting to game room:', err);
      // If we can't connect, go back to rooms
      goto('/rooms');
    }
  });
  
  onDestroy(() => {
    // Clean up subscriptions and timeouts
    unsubscribeSignalR();
    unsubscribeUser();
    if (errorTimeout) {
      clearTimeout(errorTimeout);
    }
  });
</script>

<AuthGuard requireAuth={true} redirectTo="/login">
  <div class="game-container">
    <header class="game-header">
      <h1>Game Room: {roomName}</h1>
      <div class="game-actions">
        {#if isRoomOwner && !gameStatus}
          <button on:click={startGame} disabled={isGameStarting || players.length < 2 || !isConnected} class="primary">
            {#if isGameStarting}Starting...{:else}Start Game{/if}
          </button>
        {/if}
        <button on:click={leaveRoom} class="danger small">Leave Room</button>
      </div>
    </header>

    <div class="connection-status" class:connected={isConnected}>
      {#if isConnected}
        <span class="status-indicator"></span> Connected to game server
      {:else}
        <span class="status-indicator"></span> 
        {#if connectionError}
          Connection error: {connectionError}
        {:else}
          Connecting to game server...
        {/if}
      {/if}
    </div>

    <!-- Error message display -->
    {#if errorMessage}
      <div class="error-message">
        <span class="error-text">{errorMessage}</span>
        <button class="error-dismiss" on:click={dismissError} title="Dismiss error">×</button>
      </div>
    {/if}

    {#if !gameStatus}
      <GameLobby {players} {userEmail} />
    {:else}
      <GameTable gameStatus={gameStatus} currentUserEmail={userEmail} currentTurnPlayerEmail={currentTurnPlayerEmail} localPlayerPlayedCards={localPlayerPlayedCards} />
    {/if}
  </div>
</AuthGuard>

<style>
  .game-container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 1rem;
  }
  
  .game-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 2rem;
    padding-bottom: 1rem;
    border-bottom: 1px solid #eee;
  }
  
  h1 {
    color: #9c27b0;
    margin: 0;
  }
  
  .game-actions {
    display: flex;
    gap: 1rem;
  }
  
  .connection-status {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem 1rem;
    background-color: #ffebee;
    color: #c62828;
    border-radius: 4px;
    font-size: 0.9rem;
    margin-bottom: 1rem;
  }
  
  .connection-status.connected {
    background-color: #e8f5e9;
    color: #2e7d32;
  }
  
  .status-indicator {
    width: 10px;
    height: 10px;
    border-radius: 50%;
    background-color: #c62828;
  }
  
  .connection-status.connected .status-indicator {
    background-color: #2e7d32;
  }
  
  .error-message {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0.75rem 1rem;
    background-color: #ffebee;
    color: #c62828;
    border: 1px solid #ffcdd2;
    border-radius: 4px;
    margin-bottom: 1rem;
    animation: slideIn 0.3s ease-out;
  }
  
  .error-text {
    flex: 1;
    font-weight: 500;
  }
  
  .error-dismiss {
    background: none;
    border: none;
    color: #c62828;
    font-size: 1.2rem;
    font-weight: bold;
    cursor: pointer;
    margin-left: 1rem;
    padding: 0;
    width: 20px;
    height: 20px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 50%;
    transition: background-color 0.2s ease;
  }
  
  .error-dismiss:hover {
    background-color: rgba(198, 40, 40, 0.1);
  }
  
  @keyframes slideIn {
    from {
      opacity: 0;
      transform: translateY(-10px);
    }
    to {
      opacity: 1;
      transform: translateY(0);
    }
  }
  
  button.primary {
    background-color: #9c27b0;
    color: white;
    border: none;
    padding: 0.5rem 1rem;
    border-radius: 4px;
    font-weight: bold;
    cursor: pointer;
  }
  
  button.primary:hover {
    background-color: #7b1fa2;
  }
  
  button.primary:disabled {
    background-color: #e1bee7;
    cursor: not-allowed;
  }
</style>
