<!-- Input.svelte - Reusable input component -->
<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  import { _ } from 'svelte-i18n';
  
  // Props
  export let type: 'text' | 'email' | 'password' | 'number' | 'tel' | 'url' | 'search' = 'text';
  export let value = '';
  export let placeholder = '';
  export let label = '';
  export let id = '';
  export let required = false;
  export let disabled = false;
  export let readonly = false;
  export let error = '';
  export let success = '';
  export let helpText = '';
  export let size: 'sm' | 'md' | 'lg' = 'md';
  export let fullWidth = false;
  
  const dispatch = createEventDispatcher<{
    input: Event;
    change: Event;
    focus: FocusEvent;
    blur: FocusEvent;
  }>();
  
  // Generate unique ID if not provided
  $: inputId = id || `input-${Math.random().toString(36).substr(2, 9)}`;
  
  $: inputClasses = [
    'form-input',
    `input-${size}`,
    fullWidth && 'input-full-width',
    error && 'error',
    success && 'success',
    disabled && 'disabled',
    readonly && 'readonly'
  ].filter(Boolean).join(' ');
  
  function handleInput(event: Event) {
    const target = event.target as HTMLInputElement;
    value = target.value;
    dispatch('input', event);
  }
  
  function handleChange(event: Event) {
    dispatch('change', event);
  }
  
  function handleFocus(event: FocusEvent) {
    dispatch('focus', event);
  }
  
  function handleBlur(event: FocusEvent) {
    dispatch('blur', event);
  }
</script>

<div class="input-wrapper" class:full-width={fullWidth}>
  {#if label}
    <label for={inputId} class="form-label">
      {label}
      {#if required}
        <span class="form-required" aria-label={$_('common.required')}>*</span>
      {/if}
    </label>
  {/if}
  
  <input
    {type}
    id={inputId}
    class={inputClasses}
    {value}
    {placeholder}
    {required}
    {disabled}
    {readonly}
    on:input={handleInput}
    on:change={handleChange}
    on:focus={handleFocus}
    on:blur={handleBlur}
    {...$$restProps}
  />
  
  {#if helpText && !error && !success}
    <div class="form-help">{helpText}</div>
  {/if}
  
  {#if error}
    <div class="form-error" role="alert">{error}</div>
  {/if}
  
  {#if success}
    <div class="form-success" role="status">{success}</div>
  {/if}
</div>

<style>
  .input-wrapper {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
  }
  
  .input-wrapper.full-width {
    width: 100%;
  }
  
  .form-input {
    padding: 0.75rem 1rem;
    border: 2px solid #e0e0e0;
    border-radius: var(--border-radius);
    font-size: 1rem;
    font-family: inherit;
    transition: border-color var(--transition-speed) ease, box-shadow var(--transition-speed) ease;
    background-color: white;
    color: var(--text-dark);
  }
  
  .form-input:focus {
    outline: none;
    border-color: var(--primary-color);
    box-shadow: 0 0 0 3px rgba(156, 39, 176, 0.1);
  }
  
  .form-input::placeholder {
    color: var(--text-muted);
    opacity: 1;
  }
  
  /* Sizes */
  .input-sm {
    padding: 0.5rem 0.75rem;
    font-size: 0.875rem;
  }
  
  .input-lg {
    padding: 1rem 1.25rem;
    font-size: 1.125rem;
  }
  
  /* States */
  .form-input.error {
    border-color: #dc3545;
    box-shadow: 0 0 0 3px rgba(220, 53, 69, 0.1);
  }
  
  .form-input.success {
    border-color: #28a745;
    box-shadow: 0 0 0 3px rgba(40, 167, 69, 0.1);
  }
  
  .form-input.disabled {
    background-color: #f8f9fa;
    color: var(--text-muted);
    cursor: not-allowed;
    opacity: 0.8;
  }
  
  .form-input.readonly {
    background-color: #f8f9fa;
    cursor: default;
  }
  
  .input-full-width {
    width: 100%;
  }
  
  .form-label {
    font-weight: 600;
    color: var(--text-dark);
    font-size: 0.875rem;
    display: flex;
    align-items: center;
    gap: 0.25rem;
  }
  
  .form-required {
    color: #dc3545;
    font-weight: bold;
  }
  
  .form-help {
    font-size: 0.875rem;
    color: var(--text-muted);
  }
  
  .form-error {
    color: #dc3545;
    font-size: 0.875rem;
    font-weight: 500;
  }
  
  .form-success {
    color: #28a745;
    font-size: 0.875rem;
    font-weight: 500;
  }
  
  /* Dark theme support */
  @media (prefers-color-scheme: dark) {
    .form-input {
      background-color: #2d3748;
      border-color: #4a5568;
      color: #e2e8f0;
    }
    
    .form-input:focus {
      border-color: var(--primary-light);
      box-shadow: 0 0 0 3px rgba(225, 190, 231, 0.1);
    }
    
    .form-input.disabled,
    .form-input.readonly {
      background-color: #1a202c;
    }
  }
</style>
