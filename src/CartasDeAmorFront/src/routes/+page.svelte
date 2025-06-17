<script lang="ts">
  import { logout } from '$lib/services/authService';
  import { auth } from '$lib/stores/authStore';
  import { goto } from '$app/navigation';

  function handleLogout() {
    logout();
  }
  
  function handleLogin() {
    goto('/login');
  }
</script>

<svelte:head>
  <title>Cartas de Amor</title>
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
{:else}
  <div class="container">
    <div class="home-content">
      <h1 class="title">Cartas de Amor</h1>
      <button class="login-button" on:click={handleLogin}>Login</button>
    </div>
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
  
  .home-content {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-height: 80vh;
  }
  
  .title {
    font-size: 3rem;
    margin-bottom: 2rem;
    color: #9c27b0; /* Purple color theme for "Love Letter" */
    text-align: center;
  }
  
  .login-button {
    background-color: #4CAF50;
    color: white;
    border: none;
    padding: 0.8rem 2rem;
    border-radius: 4px;
    cursor: pointer;
    font-size: 1.1rem;
    font-weight: 600;
    transition: background-color 0.3s;
    box-shadow: 0 2px 4px rgba(0,0,0,0.2);
  }
  
  .login-button:hover {
    background-color: #388e3c;
    transform: translateY(-2px);
    box-shadow: 0 4px 8px rgba(0,0,0,0.2);
  }
</style>
