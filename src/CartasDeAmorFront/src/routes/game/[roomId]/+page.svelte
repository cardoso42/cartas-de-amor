<script lang="ts">
  import AuthGuard from '$lib/components/auth/AuthGuard.svelte';
  import GameLobby from '$lib/components/game/core/GameLobby.svelte';
  import { onMount, onDestroy } from 'svelte';
  import { page } from '$app/stores';
  import { signalR } from '$lib/services/signalRService';
  import { goto } from '$app/navigation';
  import { get } from 'svelte/store';
  import { user } from '$lib/stores/userStore';
  import { gameStore } from '$lib/stores/gameStore';
  import { getCardName } from '$lib/utils/cardUtils';
  import { get as getStore } from 'svelte/store';
  import { getPlayerPlayedCardsPosition, getPlayerScreenPosition } from '$lib/utils/gameUtils';
  import { isPlayerProtected } from '$lib/utils/gameDataProcessor';
  import GameTable from '$lib/components/game/core/GameTable.svelte';
  import AnimationManager from '$lib/components/game/animations/AnimationManager.svelte';
  import { InteractionBlocker, GameLog } from '$lib/components/game/ui';
  import type { LogEntry } from '$lib/components/game/ui/GameLog.svelte';
  import type { 
    InitialGameStatusDto, 
    PrivatePlayerUpdateDto, 
    PublicPlayerUpdateDto,
    CardType
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

  // Animation control
  let hiddenCardType: CardType | null = null;
  let animatingPlayerEmail: string = '';
  
  // Animation blocking state
  let isAnimationPlaying = false;
  let currentAnimationType: string | null = null;
  
  // Reference to GameTable for getting card positions
  let gameTableComponent: any;
  
  // Animation manager reference
  let animationManager: AnimationManager;

  // Game log state
  let logEntries: LogEntry[] = [];
  let isLogCollapsed: boolean = false;

  // Get initial game data from gameStore
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
  
  function showNotification(message: string, type: 'info' | 'success' | 'warning' | 'error' = 'info') {
    const logEntry: LogEntry = {
      id: `${Date.now()}-${Math.random()}`,
      timestamp: new Date(),
      message,
      type
    };
    
    // Add to log entries
    logEntries = [logEntry, ...logEntries].slice(0, 100); // Keep last 100 entries
    
    // Still log to console for debugging
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
  
  // Handle log events
  function handleLogToggle(event: CustomEvent<{ collapsed: boolean }>) {
    isLogCollapsed = event.detail.collapsed;
  }
  
  function handleLogClear() {
    logEntries = [];
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
    const playerEmail = playerPublicData.userEmail;
    
    // Update player's data
    if (playerEmail !== userEmail) {
      // Update other players' public data
      const otherPlayers = gameStatus.otherPlayersPublicData || [];
      const playerToUpdate = otherPlayers.find(p => p.userEmail === playerEmail);
      
      if (playerToUpdate) {
        // Update player's status, which includes protection status
        playerToUpdate.status = playerPublicData.status;
        playerToUpdate.score = playerPublicData.score;
        playerToUpdate.cardsInHand = playerPublicData.holdingCardsCount;
        playerToUpdate.playedCards = playerPublicData.playedCards;
        
        // Since PlayerStatusDto has both status and isProtected, we need to derive isProtected from status
        playerToUpdate.isProtected = isPlayerProtected(playerPublicData.status);
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
          const playerName = getPlayerDisplayName(playerEmail);
          showNotification(`Player joined: ${playerName}`, 'info');
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
          
          // Add round started notification to log
          showNotification('New round started!', 'success');
          const firstPlayerName = getPlayerDisplayName(currentTurnPlayerEmail);
          showNotification(`${firstPlayerName} goes first`, 'info');
        },
        onNextTurn: (playerEmail: string) => {
          console.log('Next turn:', playerEmail);
          currentTurnPlayerEmail = playerEmail;
          const playerName = getPlayerDisplayName(playerEmail);
          showNotification(`${playerName}'s turn`, 'info');
        },
        onPlayerDrewCard: (playerEmail: string) => {
          console.log('Player drew card:', playerEmail);
          
          const playerName = getPlayerDisplayName(playerEmail);
          
          // Get deck position and player position for animation
          let deckPosition = { x: window.innerWidth / 2, y: window.innerHeight / 2, width: 70, height: 98 };
          let playerPosition = { x: window.innerWidth / 2, y: window.innerHeight / 2, width: 65, height: 90 };
          
          if (gameTableComponent) {
            // Get deck position
            const deckPos = gameTableComponent.getDeckPosition();
            if (deckPos) {
              deckPosition = deckPos;
            }
            
            // Get player hand position
            const playerPos = gameTableComponent.getPlayerHandPosition(playerEmail);
            if (playerPos) {
              playerPosition = playerPos;
            }
          }
          
          // Queue draw card animation using animation manager
          animationManager.queueDrawCardAnimation({
            playerName,
            deckPosition,
            playerPosition
          });
          
          // Update the visual representation to show the player now has an additional card (after animation starts)
          setTimeout(() => {
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
          }, 500); // Start updating after animation begins
          
          // Show notification after the animation completes
          setTimeout(() => {
            showNotification(`${playerName} drew a card`, 'info');
          }, 800); // Total animation time is about 0.7s
        },
        onPrivatePlayerUpdate: (playerUpdate: PrivatePlayerUpdateDto) => {
          console.log('Private player update:', playerUpdate);
          // Update the game status with the player's new cards and status
          if (gameStatus) {
            if (playerUpdate.holdingCards) {
              gameStatus.yourCards = playerUpdate.holdingCards;
            }
            
            // Update protection status derived from player status
            gameStatus.isProtected = isPlayerProtected(playerUpdate.status);
            
            // Update score if provided
            if (playerUpdate.score !== undefined) {
              gameStatus.score = playerUpdate.score;
            }
            
            gameStatus = { ...gameStatus }; // Trigger reactivity
          }
        },
        onDrawCardError: (error: string) => {
          console.error('Draw card error:', error);
          showError(`Failed to draw card: ${error}`);
          showNotification(`Draw card failed: ${error}`, 'error');
        },
        onGameStartError: (error: string) => {
          console.error('Game start error:', error);
          isGameStarting = false;
          showError(`Failed to start game: ${error}`);
          showNotification(`Game start failed: ${error}`, 'error');
        },
        // New MessageFactory events replace old CardResult events
        onPlayCard: (data: { player: string; cardType: number }) => {
          console.log('PlayCard event:', data);
          
          const playerEmail = data.player;
          const playedCard = data.cardType as CardType;
          const playerName = getPlayerDisplayName(playerEmail);
          
          // Get card source position from player's hand
          let sourcePosition = { x: window.innerWidth / 2, y: window.innerHeight / 2, width: 50, height: 70 };
          
          if (gameTableComponent) {
            const cardPosition = gameTableComponent.getPlayerCardPosition(playerEmail, playedCard);
            if (cardPosition) {
              sourcePosition = cardPosition;
            } else {
              // Fallback to general player position
              const players = gameTableComponent.getProcessedPlayers();
              const playerPos = getPlayerScreenPosition(playerEmail, players);
              sourcePosition = { ...playerPos, width: 65, height: 90 };
            }
          } else {
            // Fallback to center when no game table component
            sourcePosition = { x: window.innerWidth / 2, y: window.innerHeight / 2, width: 65, height: 90 };
          }
          
          // Get played cards destination position
          const players = gameTableComponent?.getProcessedPlayers() || [];
          const playedCardsPosition = getPlayerPlayedCardsPosition(playerEmail, players);
          
          // Queue card play animation using animation manager
          animationManager.queueCardPlayAnimation({
            playerName,
            cardType: playedCard,
            sourcePosition,
            playedCardsPosition
          });
          
          // Handle the card play event logic after a delay to let animation start
          setTimeout(() => {
            handleCardPlayed(data);
          }, 200);
          
          // Show notification after the animation completes
          setTimeout(() => {
            showNotification(`${playerName} played ${getCardName(playedCard)}`, 'info');
          }, 3500); // Total animation time is about 3.8s, show notification a bit before end
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
          
          // Get display names for the animation
          const invokerName = getPlayerDisplayName(data.invoker);
          const targetName = getPlayerDisplayName(data.target);
          const guessedCardType = data.cardType as CardType;
          
          // Queue guess card animation using animation manager
          animationManager.queueGuessCardAnimation({
            invokerName,
            targetName,
            guessedCardType
          });
          
          // Show notification after a short delay to let animation play
          setTimeout(() => {
            showNotification(`${invokerName} guessed ${targetName} has ${getCardName(data.cardType)}`, 'info');
          }, 2000);
        },
        onPeekCard: (data: { invoker: string; target: string }) => {
          console.log('PeekCard event:', data);
          const invokerName = getPlayerDisplayName(data.invoker);
          const targetName = getPlayerDisplayName(data.target);
          showNotification(`${invokerName} looked at ${targetName}'s card`, 'info');
        },
        onShowCard: (data: { invoker: string; target: string, cardType: number }) => {
          console.log('ShowCard event:', data);
          
          // Only show animation if this is relevant to the current user
          // (i.e., the current user is the one who gets to see the card)
          if (data.invoker === userEmail) {
            const targetPlayerName = getPlayerDisplayName(data.target);
            
            // Get specific card position from GameTable
            let sourcePosition = { x: window.innerWidth / 2, y: window.innerHeight / 2 };
            
            if (gameTableComponent) {
              const cardPosition = gameTableComponent.getPlayerCardPosition(data.target, data.cardType);
              if (cardPosition) {
                sourcePosition = cardPosition;
              } else {
                // Fallback to general player position
                const players = gameTableComponent.getProcessedPlayers();
                sourcePosition = getPlayerScreenPosition(data.target, players);
              }
            } else {
              // Fallback to center when no game table component
              sourcePosition = { x: window.innerWidth / 2, y: window.innerHeight / 2 };
            }
            
            // Hide the card in the target player's hand
            hiddenCardType = data.cardType as CardType;
            animatingPlayerEmail = data.target;
            
            // Queue show card animation using animation manager
            animationManager.queueShowCardAnimation({
              targetPlayerName: targetPlayerName,
              cardType: data.cardType as CardType,
              sourcePosition
            });
          } else {
            // For other players, just show the regular notification
            const invokerName = getPlayerDisplayName(data.invoker);
            const targetName = getPlayerDisplayName(data.target);
            showNotification(`${invokerName} looked at ${targetName}'s card`, 'info');
          }
        },
        onCompareCards: (data: { invoker: string; target: string }) => {
          console.log('CompareCards event:', data);
          const invokerName = getPlayerDisplayName(data.invoker);
          const targetName = getPlayerDisplayName(data.target);
          showNotification(`${invokerName} compared cards with ${targetName}`, 'info');
        },
        onComparisonTie: (data: { invoker: string; target: string }) => {
          console.log('ComparisonTie event:', data);
          const invokerName = getPlayerDisplayName(data.invoker);
          const targetName = getPlayerDisplayName(data.target);
          showNotification(`${invokerName} and ${targetName} tied in comparison`, 'info');
        },
        onDiscardCard: (data: { target: string; cardType: number }) => {
          console.log('DiscardCard event:', data);
          const targetName = getPlayerDisplayName(data.target);
          const cardName = getCardName(data.cardType);
          showNotification(`${targetName} discarded ${cardName}`, 'info');
          
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
          
          const playerEmail = data.player;
          const playerName = getPlayerDisplayName(playerEmail);
          
          // Get deck position and player position for animation
          let deckPosition = { x: window.innerWidth / 2, y: window.innerHeight / 2, width: 70, height: 98 };
          let playerPosition = { x: window.innerWidth / 2, y: window.innerHeight / 2, width: 65, height: 90 };
          
          if (gameTableComponent) {
            // Get deck position
            const deckPos = gameTableComponent.getDeckPosition();
            if (deckPos) {
              deckPosition = deckPos;
            }
            
            // Get player hand position
            const playerPos = gameTableComponent.getPlayerHandPosition(playerEmail);
            if (playerPos) {
              playerPosition = playerPos;
            }
          }
          
          // Queue draw card animation using animation manager
          animationManager.queueDrawCardAnimation({
            playerName,
            deckPosition,
            playerPosition
          });
          
          // Update the visual representation to show the player now has an additional card (after animation starts)
          setTimeout(() => {
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
          }, 500); // Start updating after animation begins
          
          // Show notification after the animation completes
          setTimeout(() => {
            showNotification(`${playerName} drew a card`, 'info');
          }, 800); // Total animation time is about 0.7s
        },
        onCardReturnedToDeck: (data: { player: string; cardCount: number }) => {
          console.log('CardReturnedToDeck event:', data);
          const playerName = getPlayerDisplayName(data.player);
          showNotification(`${playerName} returned ${data.cardCount} card(s) to the deck`, 'info');
          // Increase deck count when cards are returned to deck
          if (gameStatus) {
            gameStatus.cardsRemainingInDeck = (gameStatus.cardsRemainingInDeck || 0) + data.cardCount;
          }
        },
        onPlayerEliminated: (data: { player: string }) => {
          console.log('PlayerEliminated event:', data);
          
          // Get player display name for animation
          const eliminatedPlayerName = getPlayerDisplayName(data.player);
          
          // Queue elimination animation using animation manager
          animationManager.queueEliminationAnimation({
            playerName: eliminatedPlayerName
          });
          
          // Show notification after a short delay to let animation play
          setTimeout(() => {
            showNotification(`${eliminatedPlayerName} was eliminated!`, 'warning');
          }, 500);
        },
        onSwitchCards: (data: { invoker: string; target: string }) => {
          console.log('SwitchCards event:', data);
          const invokerName = getPlayerDisplayName(data.invoker);
          const targetName = getPlayerDisplayName(data.target);
          showNotification(`${invokerName} switched cards with ${targetName}`, 'info');
        },
        onPlayerProtected: (data: { player: string }) => {
          console.log('PlayerProtected event:', data);
          const playerName = getPlayerDisplayName(data.player);
          showNotification(`${playerName} is now protected for 1 turn`, 'info');
        },
        onChooseCard: (data: { player: string }) => {
          console.log('ChooseCard event:', data);
          // This indicates the player needs to choose cards (like Chancellor effect)
          const playerName = getPlayerDisplayName(data.player);
          if (data.player === userEmail) {
            showNotification('You need to choose which cards to keep', 'info');
          } else {
            showNotification(`${playerName} needs to choose cards`, 'info');
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
          showNotification(`Card choice failed: ${error}`, 'error');
        },
        onMandatoryCardPlay: (message: string, requiredCardType: number) => {
          console.log('Mandatory card play:', message, requiredCardType);
          showError(`You must play the ${getCardName(requiredCardType)} card! ${message}`);
          showNotification(`You must play ${getCardName(requiredCardType)}! ${message}`, 'warning');
        },
        onRoundWinners: (winners: string[]) => {
          console.log('Round winners:', winners);
          const winnerNames = winners.map(email => getPlayerDisplayName(email));
          
          // Queue round winners animation using animation manager
          animationManager.queueRoundWinnersAnimation({
            winnerNames
          });
          
          // Show notification after animation completes
          setTimeout(() => {
            const winnerNamesText = winnerNames.join(', ');
            showNotification(`Round won by: ${winnerNamesText}`, 'success');
          }, 3300); // Total animation time is 3.3s (0.3s + 2s + 1s)
        },
        onBonusPoints: (players: string[]) => {
          console.log('Bonus points awarded to:', players);
          const playerNames = players.map(email => getPlayerDisplayName(email)).join(', ');
          showNotification(`Bonus points awarded to: ${playerNames}`, 'success');
        },
        onGameOver: (winners: string[]) => {
          console.log('Game over, winners:', winners);
          const winnerNames = winners.map(email => getPlayerDisplayName(email));
          
          // Queue game over animation using animation manager
          animationManager.queueGameOverAnimation({
            winnerNames,
            winnerEmails: winners,
            currentUserEmail: userEmail
          });
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
      <div class="game-layout">
        <GameTable 
          bind:this={gameTableComponent}
          gameStatus={gameStatus} 
          currentUserEmail={userEmail} 
          currentTurnPlayerEmail={currentTurnPlayerEmail} 
          localPlayerPlayedCards={localPlayerPlayedCards}
          {hiddenCardType}
          {animatingPlayerEmail}
          {isAnimationPlaying}
        />
        <GameLog 
          {logEntries}
          isCollapsed={isLogCollapsed}
          on:toggle={handleLogToggle}
          on:clear={handleLogClear}
        />
      </div>
    {/if}

    <!-- Animation Manager - handles all animations centrally -->
    <AnimationManager 
      bind:this={animationManager}
      on:animationComplete={(event) => {
        if (event.detail.type === 'showCard') {
          hiddenCardType = null;
          animatingPlayerEmail = '';
        } else if (event.detail.type === 'gameOver') {
          goto('/rooms');
        }
        // Card play animations don't need special cleanup
      }}
      on:animationStateChange={(event) => {
        isAnimationPlaying = event.detail.isAnimating;
        currentAnimationType = event.detail.currentAnimation;
        console.log(`🎬 Animation state changed: ${isAnimationPlaying ? 'PLAYING' : 'STOPPED'} (${currentAnimationType || 'none'})`);
      }}
    />
    
    <!-- Interaction Blocker - prevents user interactions during animations -->
    <InteractionBlocker 
      isBlocking={isAnimationPlaying}
      showOverlay={true}
      overlayOpacity={0.2}
      blockPointerEvents={true}
      preventScrolling={false}
      zIndex={1500}
      on:blocked={(event) => {
        console.log(`🚫 Interaction blocked during animation: ${event.detail.event} (current: ${currentAnimationType})`);
      }}
    />
  </div>
</AuthGuard>

<style>
  .game-container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 1rem;
    position: relative;
  }
  
  .game-layout {
    display: flex;
    gap: 1.5rem;
    justify-content: center;
    align-items: stretch;
    min-height: 80vh;
    max-height: 80vh;
  }
  
  @media (max-width: 1024px) {
    .game-layout {
      gap: 1rem;
    }
  }
  
  @media (max-width: 768px) {
    .game-layout {
      flex-direction: column;
      gap: 1rem;
      max-height: none;
    }
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
