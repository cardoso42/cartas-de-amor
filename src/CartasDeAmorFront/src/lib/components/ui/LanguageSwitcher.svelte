<script lang="ts">
  import { locale } from 'svelte-i18n';
  import { onMount } from 'svelte';
  import { browser } from '$app/environment';

  const languages = [
    { code: 'en', name: 'English', flag: '🇺🇸' },
    { code: 'es', name: 'Español', flag: '🇪🇸' },
    { code: 'pt', name: 'Português', flag: '🇧🇷' }
  ];

  let isOpen = false;
  let dropdownRef: HTMLElement;

  function changeLanguage(langCode: string) {
    $locale = langCode;
    isOpen = false;
    // Store the preference in localStorage
    if (browser) {
      localStorage.setItem('preferredLanguage', langCode);
    }
  }

  function toggleDropdown() {
    isOpen = !isOpen;
  }

  function handleClickOutside(event: MouseEvent) {
    if (dropdownRef && !dropdownRef.contains(event.target as Node)) {
      isOpen = false;
    }
  }

  // Load preferred language from localStorage on mount
  onMount(() => {
    if (browser) {
      const savedLanguage = localStorage.getItem('preferredLanguage');
      if (savedLanguage && languages.some(lang => lang.code === savedLanguage)) {
        $locale = savedLanguage;
      }
      
      // Add click outside listener
      document.addEventListener('click', handleClickOutside);
      
      return () => {
        document.removeEventListener('click', handleClickOutside);
      };
    }
  });

  // Get current language info
  $: currentLanguage = languages.find(lang => lang.code === $locale) || languages[0];
</script>

<div class="language-switcher" bind:this={dropdownRef}>
  <button
    class="language-dropdown-trigger"
    on:click={toggleDropdown}
    title={currentLanguage.name}
    aria-label="Select language"
  >
    <span class="flag">{currentLanguage.flag}</span>
    <span class="dropdown-arrow" class:open={isOpen}>▼</span>
  </button>
  
  {#if isOpen}
    <div class="language-dropdown">
      {#each languages as language}
        <button
          class="language-option"
          class:active={$locale === language.code}
          on:click={() => changeLanguage(language.code)}
          title={language.name}
        >
          <span class="flag">{language.flag}</span>
        </button>
      {/each}
    </div>
  {/if}
</div>

<style>
  .language-switcher {
    position: relative;
    display: flex;
    align-items: center;
  }

  .language-dropdown-trigger {
    display: flex;
    align-items: center;
    gap: 0.25rem;
    padding: 0.5rem;
    border: 1px solid rgba(255, 255, 255, 0.2);
    border-radius: 0.5rem;
    background: rgba(255, 255, 255, 0.1);
    color: white;
    cursor: pointer;
    transition: all 0.2s ease;
    font-size: 0.875rem;
  }

  .language-dropdown-trigger:hover {
    background: rgba(255, 255, 255, 0.2);
    border-color: rgba(255, 255, 255, 0.3);
  }

  .dropdown-arrow {
    font-size: 0.75rem;
    transition: transform 0.2s ease;
  }

  .dropdown-arrow.open {
    transform: rotate(180deg);
  }

  .language-dropdown {
    position: absolute;
    top: 100%;
    right: 0;
    margin-top: 0.25rem;
    background: white;
    border: 1px solid #e0e0e0;
    border-radius: 0.5rem;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    z-index: 1000;
    min-width: 60px;
    overflow: hidden;
  }

  .language-option {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 100%;
    padding: 0.75rem;
    border: none;
    background: white;
    cursor: pointer;
    transition: background-color 0.2s ease;
    font-size: 1.2rem;
  }

  .language-option:hover {
    background-color: #f8f9fa;
  }

  .language-option.active {
    background-color: var(--primary-light);
    color: var(--primary-dark);
  }

  .flag {
    font-size: 1.2rem;
  }

  /* Mobile responsiveness */
  @media (max-width: 768px) {
    .language-dropdown-trigger {
      padding: 0.4rem;
    }
    
    .language-dropdown {
      min-width: 50px;
    }
    
    .language-option {
      padding: 0.6rem;
    }
  }
</style> 