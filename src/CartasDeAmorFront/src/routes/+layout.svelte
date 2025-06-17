<script lang="ts">
  import { auth } from '$lib/stores/authStore';
</script>

<div class="app">
  <slot />
</div>

<style>
  :global(body) {
    margin: 0;
    padding: 0;
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen,
      Ubuntu, Cantarell, 'Open Sans', 'Helvetica Neue', sans-serif;
    line-height: 1.6;
    color: #333;
    background-color: #f5f5f5;
  }

  :global(*, *::before, *::after) {
    box-sizing: border-box;
  }
  
  .app {
    min-height: 100vh;
    display: flex;
    flex-direction: column;
  }
</style>
