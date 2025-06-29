<script lang="ts">
	import { goto } from '$app/navigation';
	import AuthGuard from '$lib/components/auth/AuthGuard.svelte';
	import { _ } from 'svelte-i18n';

	function handleLogin() {
		goto('/login');
	}
	function handleRegister() {
		goto('/register');
	}
</script>

<svelte:head>
	<title>{$_('app.name')}</title>
</svelte:head>

<AuthGuard requireAuth={false}>
	<div class="container">
		<div class="home-content">
			<h1 class="title">{$_('app.name')}</h1>
			<div class="home-buttons">
				<button class="success" on:click={handleLogin}>{$_('auth.login')}</button>
				<button class="success" on:click={handleRegister}>{$_('auth.register')}</button>
			</div>
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
	
	.home-buttons {
		display: flex;
		flex-direction: column;
		gap: 1em;
		justify-content: space-between;
	}
</style>
