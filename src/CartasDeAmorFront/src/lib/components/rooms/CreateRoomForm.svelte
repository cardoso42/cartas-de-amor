<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  import { Card, Button, Input } from '$lib/components/ui';
  
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

<Card padding="large">
  <h2>Create New Room</h2>
  
  {#if error}
    <div class="error-display">
      <p>{error}</p>
    </div>
  {/if}
  
  <form on:submit|preventDefault={handleSubmit} class="create-form">
    <div class="form-fields">
      <Input 
        type="text" 
        placeholder="Enter room name" 
        label="Room Name"
        bind:value={roomName}
        disabled={isCreating}
        required
      />
      <Input 
        type="password" 
        placeholder="Password (optional)" 
        label="Room Password"
        bind:value={roomPassword}
        disabled={isCreating}
      />
      <Button 
        type="submit" 
        variant="primary"
        disabled={isCreating || !roomName.trim()}
        loading={isCreating}
      >
        {isCreating ? 'Creating...' : 'Create Room'}
      </Button>
    </div>
  </form>
</Card>

<style>
  h2 {
    color: var(--primary-color);
    margin-bottom: 1rem;
    font-size: 1.5rem;
  }
  
  .error-display {
    background-color: #fef2f2;
    border: 1px solid #fecaca;
    color: #dc2626;
    padding: 0.75rem;
    border-radius: var(--border-radius);
    margin-bottom: 1rem;
  }
  
  .create-form {
    width: 100%;
  }
  
  .form-fields {
    display: flex;
    flex-direction: column;
    gap: 1rem;
    width: 100%;
  }
  
  @media (min-width: 768px) {
    .form-fields {
      flex-direction: row;
      align-items: flex-end;
      flex-wrap: nowrap;
    }
    
    .form-fields :global(.input-field) {
      flex: 0 1 auto;
      width: 180px;
      margin-right: 0.5rem;
    }
    
    .form-fields :global(.input-field):last-of-type {
      margin-right: 1rem;
    }
    
    .form-fields :global(button) {
      flex-shrink: 0;
      width: auto;
      min-width: 120px;
      max-width: 140px;
    }
  }
  
  @media (max-width: 767px) {
    .form-fields :global(button) {
      width: 100%;
    }
  }
</style>
