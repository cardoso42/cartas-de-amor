<!-- Select.svelte -->
<script lang="ts">
  import { createEventDispatcher } from 'svelte';

  export let id: string = '';
  export let name: string = '';
  export let label: string = '';
  export let value: string = '';
  export let options: { value: string; label: string; disabled?: boolean }[] = [];
  export let placeholder: string = 'Select an option';
  export let required: boolean = false;
  export let disabled: boolean = false;
  export let error: string = '';

  const dispatch = createEventDispatcher<{
    change: { value: string };
  }>();

  function handleChange(event: Event) {
    const target = event.target as HTMLSelectElement;
    value = target.value;
    dispatch('change', { value });
  }
</script>

<div class="select-field">
  {#if label}
    <label for={id} class="select-label" class:required>
      {label}
    </label>
  {/if}
  
  <div class="select-wrapper" class:error={!!error} class:disabled>
    <select 
      {id} 
      {name} 
      {required} 
      {disabled}
      bind:value
      on:change={handleChange}
      class="select-input"
    >
      {#if placeholder}
        <option value="" disabled selected={!value}>{placeholder}</option>
      {/if}
      {#each options as option}
        <option value={option.value} disabled={option.disabled}>
          {option.label}
        </option>
      {/each}
    </select>
    <div class="select-arrow">
      <svg width="12" height="8" viewBox="0 0 12 8" fill="none">
        <path d="M1 1L6 6L11 1" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
      </svg>
    </div>
  </div>
  
  {#if error}
    <span class="error-message">{error}</span>
  {/if}
</div>

<style>
  .select-field {
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
  }

  .select-label {
    font-weight: 500;
    color: var(--text-dark);
    font-size: 0.9rem;
  }

  .select-label.required::after {
    content: ' *';
    color: #ef4444;
  }

  .select-wrapper {
    position: relative;
    display: flex;
    align-items: center;
  }

  .select-input {
    width: 100%;
    padding: 0.75rem 2.5rem 0.75rem 0.75rem;
    border: 2px solid #e5e7eb;
    border-radius: var(--border-radius);
    font-size: 1rem;
    background-color: white;
    transition: all var(--transition-speed);
    appearance: none;
    cursor: pointer;
  }

  .select-input:focus {
    outline: none;
    border-color: var(--primary-color);
    box-shadow: 0 0 0 3px rgba(156, 39, 176, 0.1);
  }

  .select-input:disabled {
    background-color: #f9fafb;
    border-color: #d1d5db;
    cursor: not-allowed;
    color: #9ca3af;
  }

  .select-wrapper.error .select-input {
    border-color: #ef4444;
  }

  .select-wrapper.error .select-input:focus {
    border-color: #ef4444;
    box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.1);
  }

  .select-arrow {
    position: absolute;
    right: 0.75rem;
    color: #6b7280;
    pointer-events: none;
    transition: transform var(--transition-speed);
  }

  .select-wrapper.disabled .select-arrow {
    color: #9ca3af;
  }

  .select-input:focus + .select-arrow {
    transform: rotate(180deg);
  }

  .error-message {
    color: #ef4444;
    font-size: 0.8rem;
    margin-top: 0.25rem;
  }
</style>
