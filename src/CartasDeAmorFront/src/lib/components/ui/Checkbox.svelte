<!-- Checkbox.svelte -->
<script lang="ts">
  import { createEventDispatcher } from 'svelte';

  export let id: string = '';
  export let name: string = '';
  export let label: string = '';
  export let checked: boolean = false;
  export let disabled: boolean = false;
  export let indeterminate: boolean = false;
  export let size: 'small' | 'medium' | 'large' = 'medium';

  const dispatch = createEventDispatcher<{
    change: { checked: boolean };
  }>();

  function handleChange(event: Event) {
    const target = event.target as HTMLInputElement;
    checked = target.checked;
    dispatch('change', { checked });
  }
</script>

<div class="checkbox-field" class:disabled>
  <label class="checkbox-container" for={id}>
    <input
      type="checkbox"
      {id}
      {name}
      {disabled}
      {indeterminate}
      bind:checked
      on:change={handleChange}
      class="checkbox-input"
    />
    <span 
      class="checkbox-custom" 
      class:small={size === 'small'}
      class:medium={size === 'medium'}
      class:large={size === 'large'}
      class:checked
      class:indeterminate
    >
      {#if checked && !indeterminate}
        <svg class="check-icon" viewBox="0 0 20 20" fill="currentColor">
          <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd" />
        </svg>
      {:else if indeterminate}
        <svg class="indeterminate-icon" viewBox="0 0 20 20" fill="currentColor">
          <path fill-rule="evenodd" d="M3 10a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1z" clip-rule="evenodd" />
        </svg>
      {/if}
    </span>
    {#if label}
      <span class="checkbox-label">{label}</span>
    {/if}
  </label>
</div>

<style>
  .checkbox-field {
    display: inline-flex;
    align-items: flex-start;
  }

  .checkbox-container {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    cursor: pointer;
    user-select: none;
  }

  .checkbox-field.disabled .checkbox-container {
    cursor: not-allowed;
    opacity: 0.6;
  }

  .checkbox-input {
    position: absolute;
    opacity: 0;
    pointer-events: none;
  }

  .checkbox-custom {
    display: flex;
    align-items: center;
    justify-content: center;
    border: 2px solid #d1d5db;
    border-radius: 4px;
    background-color: white;
    transition: all var(--transition-speed);
    flex-shrink: 0;
  }

  .checkbox-custom.small {
    width: 16px;
    height: 16px;
  }

  .checkbox-custom.medium {
    width: 20px;
    height: 20px;
  }

  .checkbox-custom.large {
    width: 24px;
    height: 24px;
  }

  .checkbox-custom.checked,
  .checkbox-custom.indeterminate {
    background-color: var(--primary-color);
    border-color: var(--primary-color);
    color: white;
  }

  .checkbox-input:focus + .checkbox-custom {
    box-shadow: 0 0 0 3px rgba(156, 39, 176, 0.1);
  }

  .checkbox-input:disabled + .checkbox-custom {
    background-color: #f9fafb;
    border-color: #d1d5db;
  }

  .check-icon,
  .indeterminate-icon {
    width: 12px;
    height: 12px;
  }

  .checkbox-custom.small .check-icon,
  .checkbox-custom.small .indeterminate-icon {
    width: 10px;
    height: 10px;
  }

  .checkbox-custom.large .check-icon,
  .checkbox-custom.large .indeterminate-icon {
    width: 14px;
    height: 14px;
  }

  .checkbox-label {
    color: var(--text-dark);
    font-size: 0.9rem;
    line-height: 1.4;
  }
</style>
