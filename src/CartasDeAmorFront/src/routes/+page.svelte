<script lang="ts">
  import { logout } from '$lib/services/authService';
  import { auth } from '$lib/stores/authStore';

  function handleLogout() {
    logout();
  }
</script>

<svelte:head>
  <title>Home - Love Letter</title>
</svelte:head>

{#if $auth.isAuthenticated}
  <div class="container">
    <header>
      <h1>Welcome to Love Letter</h1>
      <button class="logout-button" on:click={handleLogout}>Logout</button>
    </header>
    
    <main>
      <p>You are successfully logged in. This is your dashboard.</p>
    </main>
  </div>
{/if}

<style>
  .container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 2rem;
  }
  
  header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 2rem;
  }
  
  h1 {
    color: #333;
  }
  
  .logout-button {
    background-color: #f44336;
    color: white;
    border: none;
    padding: 0.5rem 1rem;
    border-radius: 4px;
    cursor: pointer;
    font-size: 0.9rem;
    font-weight: 500;
    transition: background-color 0.2s;
  }
  
  .logout-button:hover {
    background-color: #d32f2f;
  }
</style>
