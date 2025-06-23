<script lang="ts">
  import AuthGuard from '$lib/components/auth/AuthGuard.svelte';
  import { onMount, onDestroy } from 'svelte';
  import { getGameRooms, createGameRoom, type GameRoom } from '$lib/services/gameRoomService';
  import { signalR } from '$lib/services/signalRService';
  import { page } from '$app/stores';
  import { goto } from '$app/navigation';
  import { gameStore } from '$lib/stores/gameStore';
	import type { JoinRoomResultDto } from '$lib/types/game-types';
	import auth from '$lib/stores/authStore';
  
  // Import reusable UI components
  import { Card, Button, LoadingSpinner } from '$lib/components/ui';
  
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
      error = err instanceof Error ? err.message : 'Failed to join room. Please try again.';
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
    });
    
    // Refresh rooms immediately (in case server data is stale)
    await refreshRooms();
  });
  
  onDestroy(() => {
    unsubscribeSignalR();
  });
</script>

<svelte:head>
  <title>Game Lobby | Love Letter</title>
</svelte:head>

<AuthGuard requireAuth={true} redirectTo="/login">
  <div class="lobby-container">
    <h1>Game Lobby</h1>
    
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
</style>
