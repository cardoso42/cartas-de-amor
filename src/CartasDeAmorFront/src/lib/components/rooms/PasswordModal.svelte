<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  
  export let open: boolean;
  
  let password = '';
  
  // Custom directive to focus the input field when modal opens
  function focusOnMount(node: HTMLElement) {
    setTimeout(() => node.focus(), 0);
    return {};
  }
  
  const dispatch = createEventDispatcher<{
    join: { password: string },
    cancel: void
  }>();
  
  function handleSubmit() {
    dispatch('join', { password });
  }
  
  function handleCancel() {
    dispatch('cancel');
    password = '';
  }
</script>

{#if open}
  <div class="modal-overlay">
    <div class="modal">
      <h3>Enter Password</h3>
      <p>This room requires a password to join.</p>
      <form on:submit|preventDefault={handleSubmit}>
        <input 
          type="text" 
          placeholder="Room password" 
          bind:value={password}
          use:focusOnMount
        />
        <div class="modal-actions">
          <button type="button" class="secondary small" on:click={handleCancel}>Cancel</button>
          <button type="submit" class="small">Join Room</button>
        </div>
      </form>
    </div>
  </div>
{/if}

<style>
  .modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(0, 0, 0, 0.5);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 100;
  }
  
  .modal {
    background-color: white;
    border-radius: 8px;
    padding: 1.5rem;
    width: 90%;
    max-width: 400px;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.2);
  }
  
  .modal h3 {
    margin-top: 0;
    color: #7b1fa2;
  }
  
  .modal input {
    width: 100%;
    margin-bottom: 1rem;
  }
  
  .modal-actions {
    display: flex;
    justify-content: flex-end;
    gap: 1rem;
  }
  
  /* Button styles now come from global buttons.css */
</style>
