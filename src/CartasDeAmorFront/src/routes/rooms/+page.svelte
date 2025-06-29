<script lang="ts">
  import AuthGuard from '$lib/components/auth/AuthGuard.svelte';
  import { onMount, onDestroy } from 'svelte';
  import { getGameRooms, getUserActiveRooms, createGameRoom, type GameRoom } from '$lib/services/gameRoomService';
  import { signalR } from '$lib/services/signalRService';
  import { page } from '$app/stores';
  import { goto } from '$app/navigation';
  import { gameStore } from '$lib/stores/gameStore';
  import { _ } from 'svelte-i18n';
	import type { JoinRoomResultDto, GameStatusDto } from '$lib/types/game-types';
  
  // Import new components
  import ConnectionStatus from '$lib/components/rooms/ConnectionStatus.svelte';
  import CreateRoomForm from '$lib/components/rooms/CreateRoomForm.svelte';
  import ErrorDisplay from '$lib/components/rooms/ErrorDisplay.svelte';
  import PasswordModal from '$lib/components/rooms/PasswordModal.svelte';
  import RoomsList from '$lib/components/rooms/RoomsList.svelte';
	import { get } from 'svelte/store';
	import user from '$lib/stores/userStore';
  
  // Access the rooms from server-side load
  let gameRooms: GameRoom[] = $page.data.rooms || [];
  let error = $page.data.error || '';
  
  // Form data
  let roomPassword = '';
  
  // UI state
  let isLoading = false;
  let isCreatingRoom = false;
  let isJoiningRoom = false;
  let selectedRoomId: string | null = null;
  let passwordModalOpen = false;
  let signalRStatus: string = 'disconnected';
  let rejoiningRoomId: string | null = null; // Track room being rejoined
  let checkingActiveGame = true; // Track if we're checking for active games
  let hasActiveGame = false; // Track if user has an active game

  // Subscribe to SignalR status
  const unsubscribeSignalR = signalR.subscribe(state => {
    signalRStatus = state.status;
  });

  // Get the current user's email for ownership check
  let userEmail = '';
  const unsubscribeUser = user.subscribe(state => {
    userEmail = state.email || '';
  });

  // Function to refresh room list
  async function refreshRooms() {
    try {
      isLoading = true;
      gameRooms = await getGameRooms();
      error = '';
    } catch (err) {
      console.error('Error refreshing rooms:', err);
      error = 'Failed to load rooms. Please try again.';
    } finally {
      isLoading = false;
    }
  }

  // Function to check and auto-join active game
  async function checkAndJoinActiveGame() {
    try {
      checkingActiveGame = true;
      
      // Get user's active games
      const activeRooms = await getUserActiveRooms();
      
      if (activeRooms && activeRooms.length > 0) {
        // User has an active game, automatically join the first one
        hasActiveGame = true;
        const activeRoom = activeRooms[0];
                
        // Initialize SignalR connection if needed
        if (signalRStatus !== 'connected') {
          await signalR.initialize();
        }
        
        // Set up rejoining tracking
        rejoiningRoomId = activeRoom.id;
        
        // Join the room via SignalR (no password needed for rejoin)
        await signalR.joinRoom(activeRoom.id, null);
        
        return true; // Successfully found and joining active game
      } else {
        // No active games, user can see lobby
        hasActiveGame = false;
        return false;
      }
    } catch (err) {
      console.error('Error checking for active games:', err);
      hasActiveGame = false;
      return false;
    } finally {
      checkingActiveGame = false;
    }
  }

  // Create a new room
  async function handleCreateRoom(event: CustomEvent<{ roomName: string, roomPassword: string | null }>) {
    const { roomName, roomPassword } = event.detail;
    
    try {
      isCreatingRoom = true;
      
      // Create the room via API
      const roomId = await createGameRoom(roomName, roomPassword);
      joinRoomWithPassword(roomId, roomPassword);

    } catch (err) {
      console.error('Error creating room:', err);
      error = err instanceof Error ? err.message : 'Failed to create room. Please try again.';
    } finally {
      isCreatingRoom = false;
    }
  }

  // Prepare to join a room
  function handleJoinRoom(event: CustomEvent<{ roomId: string, requiresPassword: boolean }>) {
    const { roomId, requiresPassword } = event.detail;
    
    selectedRoomId = roomId;
    
    if (requiresPassword) {
      // Show password modal if room requires password
      passwordModalOpen = true;
      return;
    }
    
    joinRoomWithPassword(roomId, null);
  }
  
  // Join room with password
  async function joinRoomWithPassword(roomId: string, password: string | null) {
    try {
      isJoiningRoom = true;
      passwordModalOpen = false;
      
      // Initialize SignalR connection if needed
      if (signalRStatus !== 'connected') {
        await signalR.initialize();
      }
      
      // Join the room via SignalR
      await signalR.joinRoom(roomId, password);
      
    } catch (err) {
      console.error('Error joining room:', err);
      error = err instanceof Error ? err.message : $_('rooms.failedToJoinRoom');
    } finally {
      isJoiningRoom = false;
      roomPassword = '';
    }
  }
  
  // Handle password submit from modal
  function handlePasswordSubmit(event: CustomEvent<{password: string}>) {
    if (selectedRoomId) {
      joinRoomWithPassword(selectedRoomId, event.detail.password);
    }
  }
  
  // Cancel joining with password
  function handlePasswordCancel() {
    passwordModalOpen = false;
    selectedRoomId = null;
  }

  onMount(async () => {
    // Initial connection to SignalR
    try {
      await signalR.initialize();
    } catch (err) {
      console.error('Error connecting to game server:', err);
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
      onJoinedRoom: (joinRoomResult: JoinRoomResultDto) => {
        // Set game state in store before navigating
        gameStore.set({
          roomId: joinRoomResult.roomId,
          roomName: joinRoomResult.roomName,
          players: joinRoomResult.players,
          isRoomOwner: joinRoomResult.hostEmail === userEmail,
          hostEmail: joinRoomResult.hostEmail
        });

        goto(`/game/${joinRoomResult.roomId}`);
      },
      onCurrentGameStatus: (initialGameStatus: GameStatusDto | null) => {
        if (initialGameStatus && rejoiningRoomId) {
          // Game is in progress, set game state and navigate to game page
          gameStore.set({
            roomId: rejoiningRoomId,
            roomName: '',
            players: initialGameStatus.allPlayersInOrder || [],
            isRoomOwner: false,
            hostEmail: undefined
          });
        }
      },
    });
    
    // First check if user has any active games and auto-join if they do
    const hasActiveGame = await checkAndJoinActiveGame();
    
    // Only load the lobby interface if user doesn't have an active game
    if (!hasActiveGame) {
      // Refresh rooms immediately (in case server data is stale)
      await refreshRooms();
    }
  });
  
  onDestroy(() => {
    unsubscribeSignalR();
  });
</script>

<svelte:head>
  <title>{$_('navigation.rooms')} | {$_('app.name')}</title>
</svelte:head>

<AuthGuard requireAuth={true} redirectTo="/login">
  <div class="lobby-container">
    {#if checkingActiveGame}
      <!-- Show loading screen while checking for active games -->
      <div class="checking-games">
        <h1>{$_('app.name')}</h1>
        <div class="loading-content">
          <div class="spinner"></div>
          <p>{$_('rooms.checkingForActiveGames')}</p>
        </div>
      </div>
    {:else if hasActiveGame}
      <!-- Show joining active game screen -->
      <div class="joining-game">
        <h1>{$_('app.name')}</h1>
        <div class="joining-content">
          <div class="spinner"></div>
          <p>{$_('rooms.rejoiningActiveGame')}</p>
        </div>
      </div>
    {:else}
      <!-- Show normal lobby interface -->
      <h1>{$_('rooms.gameLobby')}</h1>
      
      <ErrorDisplay message={error} on:retry={refreshRooms} />
      
      <CreateRoomForm 
        isCreating={isCreatingRoom} 
        on:create={handleCreateRoom} 
      />
      
      <RoomsList 
        rooms={gameRooms} 
        isLoading={isLoading}
        isJoining={isJoiningRoom}
        selectedRoomId={selectedRoomId}
        on:refresh={refreshRooms}
        on:join={handleJoinRoom}
      />
      
      <PasswordModal 
        open={passwordModalOpen}
        on:join={handlePasswordSubmit}
        on:cancel={handlePasswordCancel}
      />
      
      <ConnectionStatus status={signalRStatus} />
    {/if}
  </div>
</AuthGuard>

<style>
  .lobby-container {
    max-width: 800px;
    margin: 0 auto;
    position: relative;
    padding-bottom: 40px; /* Space for connection status */
  }
  
  h1 {
    color: #9c27b0;
    margin-bottom: 2rem;
  }

  .checking-games, .joining-game {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-height: 60vh;
    text-align: center;
  }

  .loading-content, .joining-content {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 1rem;
  }

  .spinner {
    width: 40px;
    height: 40px;
    border: 4px solid #f3f3f3;
    border-top: 4px solid #9c27b0;
    border-radius: 50%;
    animation: spin 1s linear infinite;
  }

  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }

  .checking-games p, .joining-game p {
    color: #666;
    font-size: 1.1rem;
    margin: 0;
  }
</style>
