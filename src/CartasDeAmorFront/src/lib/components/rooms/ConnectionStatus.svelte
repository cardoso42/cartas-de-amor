<script lang="ts">
  export let status: string;
</script>

<div class="connection-status" class:connected={status === 'connected'}>
  <span class="status-dot"></span>
  <span class="status-text">
    {#if status === 'connected'}
      Connected to game server
    {:else if status === 'connecting'}
      Connecting...
    {:else if status === 'error'}
      Connection error
    {:else}
      Disconnected
    {/if}
  </span>
</div>

<style>
  .connection-status {
    position: fixed;
    bottom: 1rem;
    left: 50%;
    transform: translateX(-50%);
    background-color: rgba(255, 255, 255, 0.9);
    padding: 0.5rem 1rem;
    border-radius: 2rem;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    font-size: 0.8rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    z-index: 10;
  }
  
  .status-dot {
    width: 0.8rem;
    height: 0.8rem;
    border-radius: 50%;
    background-color: #f44336;
  }
  
  .connected .status-dot {
    background-color: #4caf50;
  }
  
  .status-text {
    color: #333;
  }
</style>
