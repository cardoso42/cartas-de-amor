<script lang="ts">
  import AuthGuard from '$lib/components/AuthGuard.svelte';
  import { onMount } from 'svelte';

  // This would be populated from an API call in a real implementation
  let gameRooms = [
    { id: '1', name: 'Royal Court', players: 2, maxPlayers: 4, status: 'waiting' },
    { id: '2', name: 'Princess Palace', players: 3, maxPlayers: 4, status: 'in-progress' },
    { id: '3', name: 'Castle Gardens', players: 1, maxPlayers: 4, status: 'waiting' },
  ];

  let newRoomName = '';

  function createRoom() {
    if (!newRoomName.trim()) return;
    
    // In a real implementation, this would call an API to create a room
    const newRoom = {
      id: Math.random().toString(36).substring(2, 9),
      name: newRoomName,
      players: 1,
      maxPlayers: 4,
      status: 'waiting'
    };
    
    gameRooms = [...gameRooms, newRoom];
    newRoomName = '';
  }

  function joinRoom(roomId: string) {
    // In a real implementation, this would call an API to join the room
    alert(`Joining room ${roomId}`);
  }

  onMount(() => {
    // In a real implementation, this would fetch available rooms from the server
    console.log('Fetching game rooms...');
  });
</script>

<svelte:head>
  <title>Game Lobby | Love Letter</title>
</svelte:head>

<AuthGuard requireAuth={true} redirectTo="/login">
  <div class="lobby-container">
    <h1>Game Lobby</h1>
    
    <div class="create-room">
      <h2>Create New Room</h2>
      <div class="form-group">
        <input 
          type="text" 
          placeholder="Enter room name" 
          bind:value={newRoomName}
        />
        <button on:click={createRoom}>Create Room</button>
      </div>
    </div>
    
    <div class="available-rooms">
      <h2>Available Rooms</h2>
      
      {#if gameRooms.length === 0}
        <p class="no-rooms">No game rooms available. Create one to get started!</p>
      {:else}
        <div class="rooms-list">
          {#each gameRooms as room}
            <div class="room-card">
              <div class="room-info">
                <h3>{room.name}</h3>
                <div class="room-details">
                  <span class="players">{room.players}/{room.maxPlayers} Players</span>
                  <span class="status" class:in-progress={room.status === 'in-progress'}>{room.status}</span>
                </div>
              </div>
              <button 
                class="join-btn"
                disabled={room.status === 'in-progress' || room.players >= room.maxPlayers}
                on:click={() => joinRoom(room.id)}
              >
                {room.status === 'in-progress' ? 'In Progress' : 'Join'}
              </button>
            </div>
          {/each}
        </div>
      {/if}
    </div>
  </div>
</AuthGuard>

<style>
  .lobby-container {
    max-width: 800px;
    margin: 0 auto;
  }
  
  h1 {
    color: #9c27b0;
    margin-bottom: 2rem;
  }
  
  h2 {
    color: #7b1fa2;
    margin-bottom: 1rem;
  }
  
  .create-room {
    background-color: white;
    border-radius: 8px;
    padding: 1.5rem;
    margin-bottom: 2rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  }
  
  .form-group {
    display: flex;
    gap: 1rem;
  }
  
  input {
    flex: 1;
    padding: 0.75rem;
    border: 1px solid #ddd;
    border-radius: 4px;
    font-size: 1rem;
  }
  
  button {
    background-color: #9c27b0;
    color: white;
    border: none;
    border-radius: 4px;
    padding: 0.75rem 1.5rem;
    cursor: pointer;
    font-weight: 500;
  }
  
  button:hover {
    background-color: #7b1fa2;
  }
  
  button:disabled {
    background-color: #cccccc;
    cursor: not-allowed;
  }
  
  .available-rooms {
    background-color: white;
    border-radius: 8px;
    padding: 1.5rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  }
  
  .no-rooms {
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
  
  .room-info h3 {
    margin: 0 0 0.5rem 0;
    color: #333;
  }
  
  .room-details {
    display: flex;
    gap: 1rem;
    color: #666;
    font-size: 0.9rem;
  }
  
  .status {
    color: #4caf50;
    font-weight: 500;
  }
  
  .status.in-progress {
    color: #ff9800;
  }
  
  .join-btn {
    padding: 0.5rem 1.5rem;
  }
</style>
