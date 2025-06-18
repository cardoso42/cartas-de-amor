<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  
  export let isCreating = false;
  export let error = '';
  
  let roomName = '';
  let roomPassword = '';
  
  const dispatch = createEventDispatcher<{
    create: { roomName: string, roomPassword: string | null }
  }>();
  
  function handleSubmit() {
    if (!roomName.trim()) return;
    
    dispatch('create', { 
      roomName,
      roomPassword: roomPassword || null
    });
    
    // Reset form (in case the creation fails)
    roomName = '';
    roomPassword = '';
  }
</script>

<div class="create-room">
  <h2>Create New Room</h2>
  
  {#if error}
    <div class="error-message">
      <p>{error}</p>
    </div>
  {/if}
  
  <form on:submit|preventDefault={handleSubmit}>
    <div class="form-group">
      <input 
        type="text" 
        placeholder="Enter room name" 
        bind:value={roomName}
        disabled={isCreating}
        required
      />
      <input 
        type="password" 
        placeholder="Password (optional)" 
        bind:value={roomPassword}
        disabled={isCreating}
      />
      <button type="submit" disabled={isCreating || !roomName.trim()}>
        {isCreating ? 'Creating...' : 'Create Room'}
      </button>
    </div>
  </form>
</div>

<style>
  .create-room {
    background-color: white;
    border-radius: 8px;
    padding: 1.5rem;
    margin-bottom: 2rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  }
  
  h2 {
    color: #7b1fa2;
    margin-bottom: 1rem;
  }
  
  form {
    width: 100%;
  }
  
  .form-group {
    display: flex;
    gap: 1rem;
    flex-wrap: wrap;
  }
  
  @media (max-width: 768px) {
    .form-group {
      flex-direction: column;
    }
  }
  
  input {
    flex: 1;
    padding: 0.75rem;
    border: 1px solid #ddd;
    border-radius: 4px;
    font-size: 1rem;
    min-width: 0;
  }
  
  /* Button styles are now defined globally in +layout.svelte */
  
  .error-message {
    background-color: #ffebee;
    color: #c62828;
    padding: 1rem;
    border-radius: 4px;
    margin-bottom: 1rem;
  }
  
  .error-message p {
    margin: 0;
  }
</style>
