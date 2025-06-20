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
  
  // Update player data when a card is played based on CardResult events
  function updatePlayerDataFromCardResult(cardResult: CardActionResultDto) {
    if (!gameStatus || !cardResult?.invoker) {
      return;
    }
    
    const invokerEmail = cardResult.invoker.userEmail;
    const playedCard = cardResult.cardType;
    
    console.log(`Updating UI for card play: ${getPlayerDisplayName(invokerEmail)} played ${getCardName(playedCard)}`);
    
    // If the invoker is the current user, update their cards in hand and played cards
    if (invokerEmail === userEmail && gameStatus.yourCards) {
      // Remove the played card from the local player's hand
      const cardIndex = gameStatus.yourCards.indexOf(playedCard);
      if (cardIndex > -1) {
        gameStatus.yourCards.splice(cardIndex, 1);
        console.log(`Removed ${getCardName(playedCard)} from your hand`);
      }
      
      // Add to local player's played cards
      localPlayerPlayedCards = [...localPlayerPlayedCards, playedCard];
    } else {
      // Update other players' card count and played cards
      const otherPlayers = gameStatus.otherPlayersPublicData || [];
      const playerToUpdate = otherPlayers.find(p => p.userEmail === invokerEmail);
      
      if (playerToUpdate) {
        // Reduce their cards in hand count
        playerToUpdate.cardsInHand = Math.max(0, (playerToUpdate.cardsInHand || 1) - 1);
        
        // Add the played card to their played cards list
        if (!playerToUpdate.playedCards) {
          playerToUpdate.playedCards = [];
        }
        playerToUpdate.playedCards.push(playedCard);
        
        console.log(`Updated ${getPlayerDisplayName(invokerEmail)} - cards in hand: ${playerToUpdate.cardsInHand}, played card: ${getCardName(playedCard)}`);
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
        // Card result events
        onCardResultNone: (cardResult: CardActionResultDto) => {
          console.log('Card had no effect:', cardResult);
          updatePlayerDataFromCardResult(cardResult);
          if (cardResult.invoker) {
            showNotification(`${getPlayerDisplayName(cardResult.invoker.userEmail)} played ${getCardName(cardResult.cardType)} - no effect`, 'info');
          }
        },
        onCardResultShowCard: (cardResult: CardActionResultDto) => {
          console.log('Card result - show card:', cardResult);
          updatePlayerDataFromCardResult(cardResult);
          if (cardResult.invoker && cardResult.target) {
            showNotification(`${getPlayerDisplayName(cardResult.invoker.userEmail)} looked at ${getPlayerDisplayName(cardResult.target.userEmail)}'s card`, 'info');
          }
        },
        onCardResultPlayerEliminated: (cardResult: CardActionResultDto) => {
          console.log('Card result - player eliminated:', cardResult);
          updatePlayerDataFromCardResult(cardResult);
          if (cardResult.invoker && cardResult.target) {
            if (cardResult.target.status === 2) {
              showNotification(`${getPlayerDisplayName(cardResult.invoker.userEmail)} eliminated ${getPlayerDisplayName(cardResult.target.userEmail)}!`, 'warning');
            } else {
              showNotification(`${getPlayerDisplayName(cardResult.target.userEmail)} was eliminated!`, 'warning');
            }
          }
        },
        onCardResultSwitchCards: (cardResult: CardActionResultDto) => {
          console.log('Card result - switch cards:', cardResult);
          updatePlayerDataFromCardResult(cardResult);
          if (cardResult.invoker && cardResult.target) {
            showNotification(`${getPlayerDisplayName(cardResult.invoker.userEmail)} switched cards with ${getPlayerDisplayName(cardResult.target.userEmail)}`, 'info');
          }
        },
        onCardResultDiscardAndDrawCard: (cardResult: CardActionResultDto) => {
          console.log('Card result - discard and draw:', cardResult);
          updatePlayerDataFromCardResult(cardResult);
          if (cardResult.invoker) {
            showNotification(`${getPlayerDisplayName(cardResult.invoker.userEmail)} discarded and drew a new card`, 'info');
          }
        },
        onCardResultProtectionGranted: (cardResult: CardActionResultDto) => {
          console.log('Card result - protection granted:', cardResult);
          updatePlayerDataFromCardResult(cardResult);
          if (cardResult.invoker) {
            showNotification(`${getPlayerDisplayName(cardResult.invoker.userEmail)} is now protected for 1 turn`, 'info');
          }
        },
        onCardResultChooseCard: (cardResult: CardActionResultDto) => {
          console.log('Card result - choose card:', cardResult);
          updatePlayerDataFromCardResult(cardResult);
          if (cardResult.invoker) {
            showNotification(`${getPlayerDisplayName(cardResult.invoker.userEmail)} needs to choose a card`, 'info');
          }
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
