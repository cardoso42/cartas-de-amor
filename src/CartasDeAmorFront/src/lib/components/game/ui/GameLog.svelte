<script context="module" lang="ts">
  export interface LogEntry {
    id: string;
    timestamp: Date;
    message: string;
    type: 'info' | 'success' | 'warning' | 'error';
  }
</script>

<script lang="ts">
  import { createEventDispatcher, afterUpdate, onMount, onDestroy } from 'svelte';
  import { _ } from 'svelte-i18n';

  export let logEntries: LogEntry[] = [];
  export let isCollapsed: boolean = false;

  const dispatch = createEventDispatcher<{
    toggle: { collapsed: boolean };
    clear: void;
  }>();

  let logContainer: HTMLElement;
  let isMounted = false;
  let previousLogCount = 0;
  let userScrolling = false;

  // Local storage key
  const COLLAPSE_STATE_KEY = 'gameLogCollapsed';

  onMount(() => {
    isMounted = true;
    
    // Load collapse state from localStorage
    const savedState = localStorage.getItem(COLLAPSE_STATE_KEY);
    if (savedState !== null) {
      isCollapsed = JSON.parse(savedState);
    }

    // Auto-scroll to bottom when new entries are added (newest entries appear at bottom)
    if (logContainer) {
      logContainer.scrollTop = logContainer.scrollHeight;
    }
  });

  onDestroy(() => {
    isMounted = false;
  });

  // Watch for changes in log entries to auto-scroll
  $: if (isMounted && !isCollapsed && logContainer && logEntries && logEntries.length > 0) {
    // Only auto-scroll if new entries were added (not when user is scrolling up)
    if (logEntries.length > previousLogCount && !userScrolling) {
      // Use setTimeout to ensure DOM is updated
      setTimeout(() => {
        try {
          if (logContainer && logContainer.scrollHeight !== undefined) {
            logContainer.scrollTop = logContainer.scrollHeight;
          }
        } catch (error) {
          console.warn('GameLog scroll error:', error);
        }
      }, 0);
    }
    previousLogCount = logEntries.length;
  }

  // Handle scroll events to detect when user is manually scrolling
  function handleScroll() {
    if (!logContainer) return;
    
    const { scrollTop, scrollHeight, clientHeight } = logContainer;
    const isAtBottom = scrollTop + clientHeight >= scrollHeight - 5; // 5px tolerance
    
    // If user is not at the bottom, they're scrolling up to view older logs
    userScrolling = !isAtBottom;
  }

  function toggleCollapse() {
    isCollapsed = !isCollapsed;
    localStorage.setItem(COLLAPSE_STATE_KEY, JSON.stringify(isCollapsed));
    dispatch('toggle', { collapsed: isCollapsed });
  }

  function clearLog() {
    dispatch('clear');
  }

  function formatTime(date: Date): string {
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' });
  }

  function getLogTypeIcon(type: LogEntry['type']): string {
    switch (type) {
      case 'info': return 'info';
      case 'success': return 'check_circle';
      case 'warning': return 'warning';
      case 'error': return 'error';
      default: return 'info';
    }
  }

  function getLogTypeClass(type: LogEntry['type']): string {
    switch (type) {
      case 'info': return 'log-info';
      case 'success': return 'log-success';
      case 'warning': return 'log-warning';
      case 'error': return 'log-error';
      default: return 'log-info';
    }
  }
</script>

<div class="game-log" class:collapsed={isCollapsed}>
  <div class="log-header">
    {#if !isCollapsed}
      <h3>{$_('game.gameLog')}</h3>
    {/if}
    <div class="log-controls">
      {#if !isCollapsed}
        <button 
          class="log-control-btn" 
          on:click={clearLog} 
          title={$_('game.clearLog')}
          disabled={logEntries.length === 0}
        >
          <span class="material-icons">delete</span>
        </button>
      {/if}
      <button 
        class="log-control-btn" 
        on:click={toggleCollapse} 
        title={isCollapsed ? $_('game.expandLog') : $_('game.collapseLog')}
      >
        <span class="material-icons">{isCollapsed ? 'open_in_full' : 'close_fullscreen'}</span>
      </button>
    </div>
  </div>
  
  {#if !isCollapsed}
    <div class="log-content" bind:this={logContainer} on:scroll={handleScroll}>
      {#if logEntries.length === 0}
        <div class="log-empty">
          <p>{$_('game.noGameEventsYet')}</p>
        </div>
      {:else}
        {#each logEntries.slice().reverse() as entry (entry.id)}
          <div class="log-entry {getLogTypeClass(entry.type)}">
            <span class="material-icons log-icon">{getLogTypeIcon(entry.type)}</span>
            <span class="log-time">{formatTime(entry.timestamp)}</span>
            <span class="log-message">{entry.message}</span>
          </div>
        {/each}
      {/if}
    </div>
  {/if}
</div>

<style>
  .game-log {
    width: 300px;
    max-height: var(--game-table-height, 80vh); /* Match GameTable height */
    background: rgba(0, 0, 0, 0.9);
    border: 2px solid #8b4513;
    border-radius: 8px;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
    display: flex;
    flex-direction: column;
    backdrop-filter: blur(5px);
    transition: all 0.3s ease;
  }

  .game-log.collapsed {
    height: 50px;
    min-height: 50px;
    width: 50px;
    min-width: 50px;
  }

  .log-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 0.75rem 1rem;
    background: rgba(139, 69, 19, 0.8);
    border-radius: 6px 6px 0 0;
    color: white;
    border-bottom: 1px solid rgba(255, 255, 255, 0.2);
    flex-shrink: 0;
  }

  .game-log.collapsed .log-header {
    justify-content: center;
    padding: 0.75rem 0.5rem;
  }

  .log-header h3 {
    margin: 0;
    font-size: 1rem;
    font-weight: 600;
    color: #ffd700;
  }

  .log-controls {
    display: flex;
    gap: 0.5rem;
  }

  .game-log.collapsed .log-controls {
    gap: 0;
  }

  .log-control-btn {
    background: none;
    border: none;
    cursor: pointer;
    padding: 0.25rem;
    border-radius: 4px;
    transition: background-color 0.2s ease;
    opacity: 0.8;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .log-control-btn .material-icons {
    font-size: 1.2rem;
    color: white;
  }

  .log-control-btn:hover {
    background: rgba(255, 255, 255, 0.2);
    opacity: 1;
  }

  .log-control-btn:disabled {
    opacity: 0.4;
    cursor: not-allowed;
  }

  .log-content {
    flex: 1;
    overflow-y: auto;
    padding: 0.5rem;
    min-height: 0;
  }

  .log-content::-webkit-scrollbar {
    width: 6px;
  }

  .log-content::-webkit-scrollbar-track {
    background: rgba(255, 255, 255, 0.1);
    border-radius: 3px;
  }

  .log-content::-webkit-scrollbar-thumb {
    background: rgba(255, 215, 0, 0.6);
    border-radius: 3px;
  }

  .log-content::-webkit-scrollbar-thumb:hover {
    background: rgba(255, 215, 0, 0.8);
  }

  .log-empty {
    text-align: center;
    padding: 2rem 1rem;
    color: rgba(255, 255, 255, 0.6);
    font-style: italic;
  }

  .log-entry {
    display: flex;
    align-items: flex-start;
    gap: 0.5rem;
    padding: 0.5rem;
    margin-bottom: 0.25rem;
    border-radius: 4px;
    font-size: 0.85rem;
    line-height: 1.4;
    animation: slideIn 0.3s ease-out;
  }

  .log-entry:hover {
    background: rgba(255, 255, 255, 0.05);
  }

  .log-icon {
    font-size: 1rem;
    flex-shrink: 0;
    margin-top: 0.1rem;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 20px;
    height: 20px;
  }

  .log-time {
    font-size: 0.75rem;
    color: rgba(255, 255, 255, 0.7);
    flex-shrink: 0;
    font-family: 'Courier New', monospace;
    min-width: 55px;
  }

  .log-message {
    flex: 1;
    word-wrap: break-word;
    font-size: 0.85rem;
  }

  .log-info {
    color: #e3f2fd;
    border-left: 3px solid #2196f3;
  }

  .log-info .log-icon {
    color: #2196f3;
  }

  .log-success {
    color: #e8f5e9;
    border-left: 3px solid #4caf50;
  }

  .log-success .log-icon {
    color: #4caf50;
  }

  .log-warning {
    color: #fff3e0;
    border-left: 3px solid #ff9800;
  }

  .log-warning .log-icon {
    color: #ff9800;
  }

  .log-error {
    color: #ffebee;
    border-left: 3px solid #f44336;
  }

  .log-error .log-icon {
    color: #f44336;
  }

  @keyframes slideIn {
    from {
      opacity: 0;
      transform: translateX(20px);
    }
    to {
      opacity: 1;
      transform: translateX(0);
    }
  }

  /* Responsive design */
  @media (max-width: 1024px) {
    .game-log {
      width: 250px;
    }
    
    .log-message {
      font-size: 0.8rem;
    }
    
    .log-time {
      font-size: 0.7rem;
      min-width: 50px;
    }
  }

  @media (max-width: 768px) {
    .game-log {
      width: 100%;
      max-height: 300px;
      order: 2;
    }

    .game-log.collapsed {
      height: 50px;
      max-height: 50px;
      width: 50px;
      min-width: 50px;
    }

    .log-header h3 {
      font-size: 0.9rem;
    }
  }
</style>
