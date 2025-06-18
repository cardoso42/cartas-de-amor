<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  import type { GameRoom } from '$lib/services/gameRoomService';
  
  export let rooms: GameRoom[] = [];
  export let isLoading = false;
  export let isJoining = false;
  export let selectedRoomId: string | null = null;
  
  const dispatch = createEventDispatcher<{
    refresh: void,
    join: { roomId: string, requiresPassword: boolean }
  }>();
  
  function handleJoin(roomId: string, requiresPassword: boolean) {
    dispatch('join', { roomId, requiresPassword });
  }
</script>

<div class="available-rooms">
  <div class="rooms-header">
    <h2>Available Rooms</h2>
    <button
      class="small" 
      on:click={() => dispatch('refresh')} 
      disabled={isLoading}
      title="Refresh room list"
    >
      {isLoading ? 'Refreshing...' : '↻ Refresh'}
    </button>
  </div>
  
  {#if isLoading && rooms.length === 0}
    <p class="loading">Loading available rooms...</p>
  {:else if rooms.length === 0}
    <p class="no-rooms">No game rooms available. Create one to get started!</p>
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
            </div>
          </div>
          <button 
            class="small"
            on:click={() => handleJoin(room.id, room.hasPassword)}
          >
            {#if isJoining && selectedRoomId === room.id}
              Joining...
            {:else}
              Join
            {/if}
          </button>
        </div>
      {/each}
    </div>
  {/if}
</div>

<style>
  .available-rooms {
    background-color: white;
    border-radius: 8px;
    padding: 1.5rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  }
  
  .rooms-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1rem;
  }
  
  h2 {
    color: #7b1fa2;
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
    border: 1px solid #eee;
    border-radius: 4px;
    background-color: #f9f9f9;
  }
  
  .room-info {
    flex: 1;
    margin-right: 1rem;
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
  
</style>
