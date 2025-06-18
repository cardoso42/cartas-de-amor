<script lang="ts">
  import AuthGuard from '$lib/components/AuthGuard.svelte';
  import { onMount, onDestroy } from 'svelte';
  import { page } from '$app/stores';
  import { signalR } from '$lib/services/signalRService';
  import { goto } from '$app/navigation';
  import type { InitialGameStatusDto } from '$lib/types/game-types';

  // Get room ID from URL params
  const roomId = $page.data.roomId;
  
  // Game state
  let gameStatus: InitialGameStatusDto | null = null;
  let isConnected = false;
  let connectionError = '';
  
  // Handle connection events
  const unsubscribeSignalR = signalR.subscribe(state => {
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

  // Initialize SignalR and join room on mount
  onMount(async () => {
    try {
      // Initialize SignalR if not already connected
      if (!isConnected) {
        await signalR.initialize();
      }
      
      // Check if we are already in this room
      // If not, this will throw an error and we'll redirect back to rooms
      // This happens if user directly navigates to /game/[roomId]
      const state = await new Promise<any>((resolve) => {
        const unsub = signalR.subscribe(s => {
          resolve(s);
          unsub();
        });
      });
      
      if (state.connection) {
        // Set up event handlers for game events
        state.connection.on('RoundStarted', (initialGameStatus: InitialGameStatusDto) => {
          console.log('Game started:', initialGameStatus);
          gameStatus = initialGameStatus;
        });
        
        state.connection.on('NextTurn', (playerEmail: string) => {
          console.log('Next turn:', playerEmail);
        });
        
        state.connection.on('PlayerDrewCard', (playerEmail: string) => {
          console.log('Player drew card:', playerEmail);
        });
      }
    } catch (err) {
      console.error('Error connecting to game room:', err);
      // If we can't connect, go back to rooms
      goto('/rooms');
    }
  });
  
  onDestroy(() => {
    // Clean up subscriptions
    unsubscribeSignalR();
  });
</script>

<AuthGuard requireAuth={true} redirectTo="/login">
  <div class="game-container">
    <header class="game-header">
      <h1>Game Room: {roomId}</h1>
      <div class="game-actions">
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

    <div class="game-content">
      <p>
        This is a placeholder for the game interface. When a game is started, 
        the interface will be populated with game elements.
      </p>
      
      <div class="info-box">
        <h3>How to play:</h3>
        <ol>
          <li>Wait for all players to join the room</li>
          <li>The room creator can start the game when ready</li>
          <li>Follow the prompts and take your turn when it's your move</li>
        </ol>
      </div>
      
      <div class="waiting-message">
        Waiting for game to start...
      </div>
    </div>
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
  
  .game-content {
    background-color: white;
    border-radius: 8px;
    padding: 2rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  }
  
  .info-box {
    background-color: #f3e5f5;
    border-radius: 4px;
    padding: 1rem;
    margin: 2rem 0;
  }
  
  .info-box h3 {
    color: #7b1fa2;
    margin-top: 0;
  }
  
  .waiting-message {
    text-align: center;
    font-size: 1.2rem;
    color: #7b1fa2;
    padding: 2rem;
    border: 2px dashed #e1bee7;
    border-radius: 8px;
    margin-top: 2rem;
  }
</style>
