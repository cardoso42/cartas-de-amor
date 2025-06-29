<script lang="ts">
	import AuthGuard from '$lib/components/auth/AuthGuard.svelte';
	import { onMount } from 'svelte';
	import {
		getCurrentUserProfile,
		deleteAccount,
		updateAccount,
		type UserData
	} from '$lib/services/userService';
	import { goto } from '$app/navigation';
	import auth from '$lib/stores/authStore';
	import user from '$lib/stores/userStore';
	import { _ } from 'svelte-i18n';

	// User data state
	let userData: UserData = {
		username: '',
		email: '',
		joinedDate: ''
	};

	let message = '';
	let messageType = '';
	let showDeleteConfirm = false;
	let isEditingUsername = false;
	let newUsername = '';
	let isUpdatingUsername = false;

	async function confirmDeleteAccount() {
		if (userData.email) {
			const result = await deleteAccount(userData.email);

			if (result.success) {
				// Logout the user
				auth.logout();

				// Redirect to home page
				goto('/');
			} else {
				message = result.message;
				messageType = 'error';
				showDeleteConfirm = false;
			}
		}
	}

	function toggleDeleteConfirm() {
		showDeleteConfirm = !showDeleteConfirm;
	}

	function startEditingUsername() {
		isEditingUsername = true;
		newUsername = userData.username;
	}

	function cancelEditingUsername() {
		isEditingUsername = false;
		newUsername = '';
		message = '';
		messageType = '';
	}

	async function saveUsername() {
		if (!newUsername.trim()) {
			message = $_('common.usernameCannotBeEmpty');
			messageType = 'error';
			return;
		}

		if (newUsername === userData.username) {
			message = $_('common.noChangesMade');
			messageType = 'error';
			return;
		}

		isUpdatingUsername = true;

		try {
			const result = await updateAccount(userData.email, newUsername.trim());

			if (result.success) {
				// Update the local user data
				userData.username = newUsername.trim();

				// Update the user store to keep it in sync across tabs
				user.updateUser({ username: newUsername.trim() });

				message = result.message;
				messageType = 'success';
				isEditingUsername = false;
				newUsername = '';
			} else {
				message = result.message;
				messageType = 'error';
			}
		} catch (error) {
			message = $_('common.unexpectedError');
			messageType = 'error';
			console.error('Username update error:', error);
		} finally {
			isUpdatingUsername = false;
		}
	}

	onMount(() => {
		// First check if user store has data
		const storeUserData = user.getUserData();

		if (storeUserData && storeUserData.email && storeUserData.username) {
			// Use data from user store
			userData = {
				username: storeUserData.username,
				email: storeUserData.email,
				joinedDate: '' // We don't store joinedDate in user store, get from JWT
			};

			// Get joinedDate from JWT token
			const jwtProfile = getCurrentUserProfile();
			if (jwtProfile) {
				userData.joinedDate = jwtProfile.joinedDate;
			}
		} else {
			// Fallback to JWT token data
			const profile = getCurrentUserProfile();

			if (profile) {
				userData = profile;
				// Also update the user store with this data
				user.setUser({
					id: profile.email, // Using email as ID since we don't have a separate ID
					email: profile.email,
					username: profile.username
				});
			} else {
				// If profile couldn't be loaded, show error
				message = $_('common.couldNotLoadProfile');
				messageType = 'error';
			}
		}
	});
</script>

<svelte:head>
	<title>{$_('navigation.profile')} | {$_('app.name')}</title>
</svelte:head>

<AuthGuard requireAuth={true} redirectTo="/login">
	<div class="profile-container">
		<h1>{$_('navigation.profile')}</h1>

		{#if message}
			<div class="message {messageType}">
				{message}
			</div>
		{/if}

		<div class="profile-card">
			<div class="profile-header">
				<div class="avatar-container">
					<!-- Default avatar if none uploaded -->
					<div class="avatar">
						{userData.username.charAt(0).toUpperCase()}
					</div>
				</div>
			</div>

			<div class="profile-content">
				<div class="profile-info">
					<div class="info-row">
						<span class="info-label">{$_('auth.username')}</span>
						{#if !isEditingUsername}
							<div class="info-value-container">
								<span class="info-value">{userData.username}</span>
								<button
									class="edit-button"
									on:click={startEditingUsername}
									title={$_('common.editUsername')}
								>
									<span class="material-icons">edit</span>
								</button>
							</div>
						{:else}
							<div class="edit-username-container">
								<input
									type="text"
									bind:value={newUsername}
									class="username-input"
									disabled={isUpdatingUsername}
								/>
								<div class="edit-buttons">
									<button
										class="save-button"
										on:click={saveUsername}
										disabled={isUpdatingUsername}
									>
										{isUpdatingUsername
											? $_('common.saving')
											: $_('common.save')}
									</button>
									<button
										class="cancel-button"
										on:click={cancelEditingUsername}
										disabled={isUpdatingUsername}
									>
										{$_('common.cancel')}
									</button>
								</div>
							</div>
						{/if}
					</div>
					<div class="info-row">
						<span class="info-label">{$_('auth.email')}</span>
						<span class="info-value">{userData.email}</span>
					</div>
					<div class="info-row">
						<span class="info-label">{$_('common.memberSince')}</span>
						<span class="info-value"
							>{userData.joinedDate
								? new Date(userData.joinedDate).toLocaleDateString()
								: 'N/A'}</span
						>
					</div>

					<div class="button-group">
						<button class="danger" on:click={toggleDeleteConfirm}
							>{$_('common.deleteAccount')}</button
						>
					</div>

					{#if showDeleteConfirm}
						<div class="delete-confirmation">
							<p>{$_('common.deleteAccountConfirm')}</p>
							<div class="button-group">
								<button class="primary" on:click={toggleDeleteConfirm}
									>{$_('common.cancel')}</button
								>
								<button class="danger" on:click={confirmDeleteAccount}
									>{$_('common.confirm')}</button
								>
							</div>
						</div>
					{/if}
				</div>
			</div>
		</div>
	</div>
</AuthGuard>

<style>
	.profile-container {
		max-width: 700px;
		margin: 0 auto;
	}

	h1 {
		color: #9c27b0;
		margin-bottom: 2rem;
	}

	.message {
		padding: 1rem;
		border-radius: 4px;
		margin-bottom: 1rem;
		font-weight: 500;
	}

	.message.success {
		background-color: #e8f5e9;
		color: #2e7d32;
		border: 1px solid #a5d6a7;
	}

	.message.error {
		background-color: #ffebee;
		color: #c62828;
		border: 1px solid #ef9a9a;
	}

	.profile-card {
		background-color: white;
		border-radius: 8px;
		overflow: hidden;
		box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
	}

	.profile-header {
		background-color: #9c27b0;
		color: white;
		padding: 2rem;
		display: flex;
		align-items: center;
		justify-content: space-between;
	}

	.avatar-container {
		position: relative;
	}

	.avatar {
		width: 80px;
		height: 80px;
		border-radius: 50%;
		background-color: #7b1fa2;
		display: flex;
		align-items: center;
		justify-content: center;
		font-size: 2rem;
		font-weight: bold;
		border: 3px solid white;
	}

	.avatar-upload-btn {
		position: absolute;
		bottom: 0;
		right: 0;
		background-color: rgba(255, 255, 255, 0.9);
		color: #333;
		border: none;
		border-radius: 12px;
		padding: 0.3rem 0.6rem;
		font-size: 0.8rem;
		cursor: pointer;
	}

	.stats {
		display: flex;
		gap: 2rem;
	}

	.stat {
		display: flex;
		flex-direction: column;
		align-items: center;
	}

	.stat-value {
		font-size: 1.5rem;
		font-weight: bold;
	}

	.stat-label {
		font-size: 0.875rem;
		opacity: 0.8;
	}

	.profile-content {
		padding: 2rem;
	}

	.info-row {
		display: flex;
		margin-bottom: 1rem;
		padding-bottom: 0.5rem;
		border-bottom: 1px solid #eee;
	}

	.info-label {
		flex: 1;
		color: #666;
		font-weight: 500;
	}

	.info-value {
		flex: 2;
	}

	.info-value-container {
		flex: 2;
		display: flex;
		align-items: center;
		gap: 0.5rem;
	}

	.edit-button {
		background-color: #9c27b0;
		border: none;
		cursor: pointer;
		padding: 0.5rem;
		border-radius: 4px;
		color: white;
		transition: all 0.2s;
		display: flex;
		align-items: center;
		justify-content: center;
	}

	.edit-button:hover {
		background-color: #7b1fa2;
		transform: scale(1.1);
	}

	.edit-button .material-icons {
		font-size: 1.2rem;
	}

	.edit-username-container {
		flex: 2;
		display: flex;
		flex-direction: column;
		gap: 0.5rem;
	}

	.username-input {
		padding: 0.5rem;
		border: 1px solid #ddd;
		border-radius: 4px;
		font-size: 1rem;
	}

	.username-input:focus {
		outline: none;
		border-color: #9c27b0;
	}

	.edit-buttons {
		display: flex;
		gap: 0.5rem;
	}

	.save-button {
		background-color: #9c27b0;
		color: white;
		border: none;
		padding: 0.5rem 1rem;
		border-radius: 4px;
		cursor: pointer;
		font-size: 0.875rem;
	}

	.save-button:hover:not(:disabled) {
		background-color: #7b1fa2;
	}

	.save-button:disabled {
		opacity: 0.6;
		cursor: not-allowed;
	}

	.cancel-button {
		background-color: #f44336;
		color: #666;
		border: 1px solid #ddd;
		padding: 0.5rem 1rem;
		border-radius: 4px;
		cursor: pointer;
		font-size: 0.875rem;
	}

	.cancel-button:hover:not(:disabled) {
		background-color: #eeeeee;
	}

	.cancel-button:disabled {
		opacity: 0.6;
		cursor: not-allowed;
	}

	.form-group {
		margin-bottom: 1.5rem;
	}

	.form-actions {
		display: flex;
		gap: 1rem;
		justify-content: flex-end;
		margin-top: 2rem;
	}

	.button-group {
		display: flex;
		gap: 1rem;
		margin-top: 1.5rem;
	}

	.delete-confirmation {
		margin-top: 1.5rem;
		padding: 1rem;
		border: 1px solid #ffcdd2;
		background-color: #ffebee;
		border-radius: 4px;
		color: #c62828;
	}

	.primary {
		background-color: #9c27b0;
		color: white;
		padding: 0.75rem 1.5rem;
		border: none;
		border-radius: 4px;
		cursor: pointer;
		font-size: 1rem;
		font-weight: 500;
		display: flex;
		align-items: center;
		gap: 0.5rem;
		transition: all 0.2s ease;
	}

	.primary:hover {
		background-color: #7b1fa2;
		transform: translateY(-1px);
	}

	.danger {
		background-color: #f44336;
		color: white;
		padding: 0.75rem 1.5rem;
		border: none;
		border-radius: 4px;
		cursor: pointer;
		font-size: 1rem;
		font-weight: 500;
		transition: all 0.2s ease;
	}

	.danger:hover {
		background-color: #d32f2f;
		transform: translateY(-1px);
	}
</style>
