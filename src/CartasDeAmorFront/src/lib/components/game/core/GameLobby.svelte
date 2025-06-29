<script lang="ts">
  import { _ } from 'svelte-i18n';
  
  export let players: string[] = [];
  export let userEmail: string = '';
</script>

<div class="game-content">
  <div class="lobby">
    <h2>{$_('rooms.playersInRoom')}</h2>
    {#if players.length > 0}
      <ul class="player-list">
        {#each players as player}
          <li>{player} {player === userEmail ? $_('rooms.you') : ''}</li>
        {/each}
      </ul>
    {:else}
      <p>{$_('rooms.loadingPlayers')}</p>
    {/if}
    <div class="waiting-message">
      {$_('rooms.waitingForGame')}
    </div>
  </div>
</div>

<style>
  .game-content {
    background-color: white;
    border-radius: 8px;
    padding: 2rem;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  }
  
  .lobby {
    padding: 1rem;
  }
  
  h2 {
    color: #7b1fa2;
    margin-top: 0;
    font-size: 1.5rem;
  }
  
  .player-list {
    list-style-type: none;
    padding: 0;
    margin: 1rem 0;
  }
  
  .player-list li {
    padding: 0.5rem 1rem;
    margin-bottom: 0.5rem;
    background-color: #f5f5f5;
    border-radius: 4px;
    display: flex;
    align-items: center;
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
