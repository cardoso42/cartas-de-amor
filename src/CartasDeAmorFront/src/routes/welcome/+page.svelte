<script lang="ts">
  import { goto } from '$app/navigation';
  import AuthGuard from '$lib/components/AuthGuard.svelte';

  function handleLogin() {
    goto('/login');
  }
</script>

<svelte:head>
  <title>Cartas de Amor</title>
</svelte:head>

<AuthGuard requireAuth={false}>
  <div class="container">
  <div class="home-content">
    <h1 class="title">Cartas de Amor</h1>
    <button class="login-button" on:click={handleLogin}>Login</button>
  </div>
</div>
</AuthGuard>

<style>
  .container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 2rem;
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
