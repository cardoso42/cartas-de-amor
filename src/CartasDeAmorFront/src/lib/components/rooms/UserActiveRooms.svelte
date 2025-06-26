<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  import type { GameRoom } from '$lib/services/gameRoomService';
  
  export let rooms: GameRoom[] = [];
  export let isLoading = false;
  export let isJoining = false;
  export let selectedRoomId: string | null = null;
  
  const dispatch = createEventDispatcher<{
    refresh: void,
    rejoin: { roomId: string },
    leave: { roomId: string }
  }>();
  
  function handleRejoin(roomId: string) {
    dispatch('rejoin', { roomId });
  }
  
  function handleLeave(roomId: string) {
    dispatch('leave', { roomId });
  }
</script>

{#if rooms.length > 0 || isLoading}
  <div class="user-active-rooms">
    <div class="rooms-header">
      <h2>Your Unfinished Games</h2>
      <button
        class="small" 
        on:click={() => dispatch('refresh')} 
        disabled={isLoading}
        title="Refresh your games"
      >
        {isLoading ? 'Refreshing...' : '↻ Refresh'}
      </button>
    </div>
    
    {#if isLoading && rooms.length === 0}
      <p class="loading">Loading your games...</p>
    {:else if rooms.length === 0}
      <p class="no-rooms">You don't have any unfinished games.</p>
    {:else}
      <div class="rooms-list">
        {#each rooms as room (room.id)}
          <div class="room-card">
            <div class="room-info">
              <h3>
                {room.roomName}
                {#if room.hasPassword}
                  <span class="lock-icon" title="Password protected">🔒</span>
                {/if}
              </h3>
              <div class="room-details">
                <span class="players">{room.currentPlayers} Players</span>
                <span class="status">In Progress</span>
              </div>
            </div>
            <div class="room-actions">
              <button 
                class="small rejoin-btn"
                on:click={() => handleRejoin(room.id)}
              >
                {#if isJoining && selectedRoomId === room.id}
                  Rejoining...
                {:else}
                  Rejoin Game
                {/if}
              </button>
              <button 
                class="small leave-btn"
                on:click={() => handleLeave(room.id)}
                title="Leave this game"
              >
                Leave
              </button>
            </div>
          </div>
        {/each}
      </div>
    {/if}
  </div>
{/if}

<style>
  .user-active-rooms {
    background-color: #e8f5e8;
    border-radius: 8px;
    padding: 1.5rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    margin-bottom: 2rem;
  }
  
  .rooms-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1rem;
  }
  
  h2 {
    color: #2e7d32;
    margin-bottom: 0;
  }
  
  .no-rooms, .loading {
    color: #666;
    font-style: italic;
    text-align: center;
    padding: 2rem 0;
  }
  
  .rooms-list {
    display: flex;
    flex-direction: column;
    gap: 1rem;
  }
  
  .room-card {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1rem;
    border: 1px solid #c8e6c9;
    border-radius: 4px;
    background-color: #f1f8e9;
  }
  
  .room-info {
    flex: 1;
    margin-right: 1rem;
  }
  
  .room-actions {
    display: flex;
    gap: 0.5rem;
    align-items: center;
  }
  
  .room-info h3 {
    margin: 0 0 0.5rem 0;
    color: #333;
    display: flex;
    align-items: center;
    gap: 0.5rem;
  }
  
  .lock-icon {
    font-size: 0.9em;
    opacity: 0.7;
  }
  
  .room-details {
    display: flex;
    gap: 1rem;
    color: #666;
    font-size: 0.9rem;
    flex-wrap: wrap;
  }
  
  .status {
    background-color: #4caf50;
    color: white;
    padding: 0.2rem 0.5rem;
    border-radius: 12px;
    font-size: 0.8rem;
    font-weight: 500;
  }
  
  .rejoin-btn {
    background-color: #4caf50;
    color: white;
  }
  
  .rejoin-btn:hover {
    background-color: #45a049;
  }
  
  .leave-btn {
    background-color: #f44336;
    color: white;
  }
  
  .leave-btn:hover {
    background-color: #da190b;
  }
</style>
