<!-- Button.svelte - Reusable button component -->
<script lang="ts">
  import { createEventDispatcher } from 'svelte';
  
  // Props
  export let variant: 'primary' | 'secondary' | 'success' | 'danger' | 'warning' | 'info' | 'light' | 'dark' = 'primary';
  export let size: 'sm' | 'md' | 'lg' = 'md';
  export let disabled = false;
  export let loading = false;
  export let type: 'button' | 'submit' | 'reset' = 'button';
  export let fullWidth = false;
  export let outline = false;
  
  const dispatch = createEventDispatcher<{ click: MouseEvent }>();
  
  function handleClick(event: MouseEvent) {
    if (!disabled && !loading) {
      dispatch('click', event);
    }
  }
  
  $: classes = [
    'btn',
    `btn-${variant}`,
    `btn-${size}`,
    outline && 'btn-outline',
    fullWidth && 'btn-full-width',
    loading && 'btn-loading',
    disabled && 'btn-disabled'
  ].filter(Boolean).join(' ');
</script>

<button
  {type}
  class={classes}
  {disabled}
  on:click={handleClick}
  {...$$restProps}
>
  {#if loading}
    <span class="btn-spinner" aria-hidden="true"></span>
  {/if}
  <slot />
</button>

<style>
  .btn {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 0.5rem;
    padding: 0.5rem 1rem;
    border: 2px solid transparent;
    border-radius: var(--border-radius);
    font-weight: 600;
    text-align: center;
    text-decoration: none;
    cursor: pointer;
    transition: all var(--transition-speed) ease;
    position: relative;
    font-family: inherit;
    font-size: 1rem;
    line-height: 1.5;
  }
  
  .btn:focus {
    outline: 2px solid var(--primary-color);
    outline-offset: 2px;
  }
  
  /* Sizes */
  .btn-sm {
    padding: 0.375rem 0.75rem;
    font-size: 0.875rem;
  }
  
  .btn-lg {
    padding: 0.75rem 1.5rem;
    font-size: 1.125rem;
  }
  
  /* Variants */
  .btn-primary {
    background: var(--primary-color);
    color: white;
  }
  
  .btn-primary:hover:not(.btn-disabled) {
    background: var(--primary-dark);
    transform: translateY(-1px);
  }
  
  .btn-secondary {
    background: #6c757d;
    color: white;
  }
  
  .btn-secondary:hover:not(.btn-disabled) {
    background: #5a6268;
    transform: translateY(-1px);
  }
  
  .btn-success {
    background: #28a745;
    color: white;
  }
  
  .btn-success:hover:not(.btn-disabled) {
    background: #218838;
    transform: translateY(-1px);
  }
  
  .btn-danger {
    background: #dc3545;
    color: white;
  }
  
  .btn-danger:hover:not(.btn-disabled) {
    background: #c82333;
    transform: translateY(-1px);
  }
  
  .btn-warning {
    background: #ffc107;
    color: #212529;
  }
  
  .btn-warning:hover:not(.btn-disabled) {
    background: #e0a800;
    transform: translateY(-1px);
  }
  
  .btn-info {
    background: #17a2b8;
    color: white;
  }
  
  .btn-info:hover:not(.btn-disabled) {
    background: #138496;
    transform: translateY(-1px);
  }
  
  .btn-light {
    background: #f8f9fa;
    color: #212529;
    border-color: #dee2e6;
  }
  
  .btn-light:hover:not(.btn-disabled) {
    background: #e2e6ea;
    transform: translateY(-1px);
  }
  
  .btn-dark {
    background: #343a40;
    color: white;
  }
  
  .btn-dark:hover:not(.btn-disabled) {
    background: #23272b;
    transform: translateY(-1px);
  }
  
  /* Outline variants */
  .btn-outline {
    background: transparent;
    border-color: currentColor;
  }
  
  .btn-outline.btn-primary {
    color: var(--primary-color);
  }
  
  .btn-outline.btn-primary:hover:not(.btn-disabled) {
    background: var(--primary-color);
    color: white;
  }
  
  /* States */
  .btn-disabled {
    opacity: 0.6;
    cursor: not-allowed;
    transform: none !important;
  }
  
  .btn-loading {
    pointer-events: none;
  }
  
  .btn-full-width {
    width: 100%;
  }
  
  /* Loading spinner */
  .btn-spinner {
    width: 1rem;
    height: 1rem;
    border: 2px solid transparent;
    border-top: 2px solid currentColor;
    border-radius: 50%;
    animation: spin 1s linear infinite;
  }
  
  @keyframes spin {
    0% { transform: rotate(0deg); }
    100% { transform: rotate(360deg); }
  }
</style>
