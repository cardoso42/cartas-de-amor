import { init, register } from 'svelte-i18n';

// Register the locales
register('en', () => import('./locales/en.json'));
register('es', () => import('./locales/es.json'));
register('pt', () => import('./locales/pt.json'));

// Initialize the i18n library
init({
  fallbackLocale: 'en',
  initialLocale: 'en', // Set a default locale for SSR
  loadingDelay: 200,
  formats: {
    number: {
      en: {
        style: 'decimal',
        minimumFractionDigits: 0,
        maximumFractionDigits: 2
      }
    },
    date: {
      en: {
        day: 'numeric',
        month: 'short',
        year: 'numeric'
      },
      pt: {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
      }
    },
    time: {
      en: {
        hour: 'numeric',
        minute: '2-digit'
      }
    }
  }
});

export { _, waitLocale } from 'svelte-i18n'; 