<!-- AnimationToggle.svelte -->
<script lang="ts">
	import settings from '$lib/stores/settingsStore';

	// Subscribe to settings to get current state
	$: animationsEnabled = $settings.animationsEnabled;

	function toggleAnimations() {
		settings.toggleAnimations();
	}
</script>

<button
	class="animation-toggle"
	on:click={toggleAnimations}
	title={animationsEnabled ? 'Disable animations' : 'Enable animations'}
	aria-label={animationsEnabled ? 'Disable animations' : 'Enable animations'}
>
	<span class="toggle-label">Animations</span>
	{#if animationsEnabled}
		<span class="toggle-label toggle-on">ON</span>
		/
		<span class="toggle-label toggle-off">OFF</span>
	{:else}
		<span class="toggle-label toggle-off">ON</span>
		/
		<span class="toggle-label toggle-on">OFF</span>
	{/if}
</button>

<style>
	.animation-toggle {
		display: flex;
		align-items: center;
		gap: 0.5rem;
		background: none;
		border: 2px solid rgba(255, 255, 255, 0.3);
		color: white;
		padding: 0.5rem 0.75rem;
		border-radius: 6px;
		cursor: pointer;
		font-size: 0.875rem;
		font-weight: 500;
		transition: all 0.2s ease;
		white-space: nowrap;
	}

	.animation-toggle:hover {
		background: rgba(255, 255, 255, 0.1);
		border-color: rgba(255, 255, 255, 0.5);
	}

	.animation-toggle:focus {
		outline: 2px solid #ffd700;
		outline-offset: 2px;
	}

	.toggle-icon {
		font-size: 1rem;
		line-height: 1;
	}

	.toggle-label {
		font-size: 0.875rem;
	}

	/* Mobile responsive */
	@media (max-width: 768px) {
		.animation-toggle {
			padding: 0.375rem 0.5rem;
			font-size: 0.8rem;
		}

		.toggle-label {
			display: none;
		}

		.toggle-icon {
			font-size: 1.25rem;
		}
	}

	.toggle-on {
		color: white;
	}

	.toggle-off {
		color: #cd93d7;
	}
</style>
