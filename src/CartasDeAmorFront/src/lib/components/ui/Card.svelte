<!-- Card.svelte -->
<script lang="ts">
  export let variant: 'default' | 'elevated' | 'outlined' = 'default';
  export let padding: 'none' | 'small' | 'medium' | 'large' = 'medium';
  export let clickable: boolean = false;
  export let href: string = '';
</script>

<svelte:element
  this={href ? 'a' : 'div'}
  {href}
  class="card"
  class:default={variant === 'default'}
  class:elevated={variant === 'elevated'}
  class:outlined={variant === 'outlined'}
  class:padding-none={padding === 'none'}
  class:padding-small={padding === 'small'}
  class:padding-medium={padding === 'medium'}
  class:padding-large={padding === 'large'}
  class:clickable
  on:click
  on:keydown
  role={clickable ? 'button' : undefined}
  tabindex={clickable ? 0 : undefined}
>
  <slot />
</svelte:element>

<style>
  .card {
    background-color: var(--card-background);
    border-radius: var(--border-radius);
    transition: all var(--transition-speed);
    display: block;
    text-decoration: none;
    color: inherit;
  }

  .card.default {
    box-shadow: var(--card-shadow);
  }

  .card.elevated {
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  }

  .card.outlined {
    border: 1px solid #e5e7eb;
    box-shadow: none;
  }

  .card.padding-none {
    padding: 0;
  }

  .card.padding-small {
    padding: 0.75rem;
  }

  .card.padding-medium {
    padding: 1.5rem;
  }

  .card.padding-large {
    padding: 2rem;
  }

  .card.clickable {
    cursor: pointer;
  }

  .card.clickable:hover {
    transform: translateY(-2px);
    box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
  }

  .card.clickable:focus {
    outline: none;
    box-shadow: 0 0 0 3px rgba(156, 39, 176, 0.1);
  }

  .card.clickable:active {
    transform: translateY(0);
  }
</style>
