<script lang="ts">
	import { login } from '$lib/services/authService';
	import { goto } from '$app/navigation';
	import { _ } from 'svelte-i18n';

	let email = '';
	let password = '';
	let errorMessage = '';
	let loading = false;

	async function handleSubmit() {
		if (!email || !password) {
			errorMessage = $_('auth.enterEmailPassword');
			return;
		}

		loading = true;
		errorMessage = '';

		try {
			const result = await login(email, password);

			if (result.success) {
				// Redirect to the main page or dashboard after successful login
				goto('/dashboard');
			} else {
				errorMessage = result.message || $_('auth.loginFailed');
			}
		} catch (error) {
			console.error('Login error:', error);
			errorMessage = $_('auth.loginError');
		} finally {
			loading = false;
		}
	}
</script>

<div class="login-container">
	<div class="login-form">
		<h1>{$_('auth.login')}</h1>

		<form on:submit|preventDefault={handleSubmit}>
			{#if errorMessage}
				<div class="error-message">{errorMessage}</div>
			{/if}

			<div class="form-group">
				<label for="email">{$_('auth.email')}</label>
				<input
					type="text"
					id="email"
					bind:value={email}
					disabled={loading}
					placeholder={$_('auth.enterEmail')}
					autocomplete="email"
				/>
			</div>

			<div class="form-group">
				<label for="password">{$_('auth.password')}</label>
				<input
					type="password"
					id="password"
					bind:value={password}
					disabled={loading}
					placeholder={$_('auth.enterPassword')}
					autocomplete="current-password"
				/>
			</div>

			<button type="submit" disabled={loading}>
				{loading ? $_('auth.loggingIn') : $_('auth.login')}
			</button>
		</form>
	</div>
</div>

<style>
	.login-container {
		display: flex;
		justify-content: center;
		align-items: center;
		min-height: 100vh;
		background-color: #f5f5f5;
	}

	.login-form {
		width: 100%;
		max-width: 400px;
		padding: 2rem;
		background-color: white;
		border-radius: 8px;
		box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
	}

	h1 {
		text-align: center;
		margin-bottom: 1.5rem;
		color: #333;
		font-size: 1.8rem;
	}

	.form-group {
		margin-bottom: 1rem;
	}

	label {
		display: block;
		margin-bottom: 0.5rem;
		font-weight: 500;
		color: #555;
	}

	input {
		width: 100%;
		padding: 0.75rem;
		border: 1px solid #ddd;
		border-radius: 4px;
		font-size: 1rem;
	}

	input:focus {
		outline: none;
		border-color: #4a90e2;
		box-shadow: 0 0 0 2px rgba(74, 144, 226, 0.2);
	}

	.error-message {
		background-color: #ffebee;
		color: #d32f2f;
		padding: 0.75rem;
		border-radius: 4px;
		margin-bottom: 1rem;
		font-size: 0.9rem;
	}
</style>
