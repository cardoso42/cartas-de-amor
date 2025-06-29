<script lang="ts">
  import { page } from '$app/stores';
  import { auth } from '$lib/stores/authStore';
  import { goto } from '$app/navigation';
  import { logout } from '$lib/services/authService';
  import AnimationToggle from '$lib/components/ui/AnimationToggle.svelte';
  import { gameStore } from '$lib/stores/gameStore';

  // Track authentication state
  let isAuthenticated = false;
  
  // Track game state to determine if user is in a game
  let isInGame = false;
  let currentRoomId: string | null = null;

  // Subscribe to auth store
  auth.subscribe((state) => {
    isAuthenticated = state.isAuthenticated;
  });

  // Subscribe to game store to track when user joins/leaves a game
  gameStore.subscribe((state) => {
    isInGame = !!state.roomId;
    currentRoomId = state.roomId;
  });

	// Handle logout
	function handleLogout() {
		logout();
		goto('/login');
	}
</script>

<nav class="navbar">
  <div class="navbar-container">
		<div class="navbar-logo">
			{#if isAuthenticated}
				<a href="/dashboard">Cartas de Amor</a>
			{:else}
				<a href="/welcome">Cartas de Amor</a>
			{/if}
		</div>
    
    <div class="navbar-links">
      {#if isAuthenticated}
        <!-- Links for authenticated users -->
        <a href="/dashboard" class:active={$page.url.pathname === '/dashboard'}>Home</a>
        {#if isInGame && currentRoomId}
          <a href="/game/{currentRoomId}" class:active={$page.url.pathname.startsWith('/game/')}>Current Game</a>
        {:else}
          <a href="/rooms" class:active={$page.url.pathname === '/rooms'}>Game Lobby</a>
        {/if}
        <a href="/profile" class:active={$page.url.pathname === '/profile'}>My Profile</a>
        <a href="/rules" class:active={$page.url.pathname === '/rules'}>Game Rules</a>
        <AnimationToggle />
        <button class="navbar-button" on:click={handleLogout}>Logout</button>
      {:else}
        <!-- Links for non-authenticated users -->
        <a href="/welcome" class:active={$page.url.pathname === '/welcome'}>Home</a>
        <a href="/rules" class:active={$page.url.pathname === '/rules'}>Game Rules</a>
        <a href="/login" class:active={$page.url.pathname === '/login'}>Login</a>
        <a href="/register" class:active={$page.url.pathname === '/register'}>Register</a>
      {/if}
    </div>
  </div>
</nav>

<style>
	.navbar {
		background-color: #9c27b0; /* Purple theme for Love Letter */
		position: sticky;
		top: 0;
		width: 100%;
		z-index: 1000;
		box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
	}

	.navbar-container {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 0.8rem 2rem;
		max-width: 1200px;
		margin: 0 auto;
	}

	.navbar-logo a {
		color: white;
		font-size: 1.5rem;
		font-weight: bold;
		text-decoration: none;
	}

	.navbar-links {
		display: flex;
		align-items: center;
		gap: 1.5rem;
	}

	.navbar-links a {
		color: rgba(255, 255, 255, 0.85);
		text-decoration: none;
		font-weight: 500;
		transition: color 0.3s;
	}

	.navbar-links a:hover,
	.navbar-links a.active {
		color: white;
	}

	.navbar-button {
		background-color: rgba(255, 255, 255, 0.2) !important;
		padding: 0.5rem 1rem !important;
	}

	.navbar-button:hover {
		background-color: rgba(255, 255, 255, 0.3) !important;
	}

	/* Responsive adjustments */
	@media (max-width: 768px) {
		.navbar-container {
			flex-direction: column;
			padding: 0.5rem;
		}

		.navbar-logo {
			margin-bottom: 0.5rem;
		}

		.navbar-links {
			flex-wrap: wrap;
			justify-content: center;
			gap: 1rem;
		}
	}
</style>
