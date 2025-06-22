import { writable } from 'svelte/store';
import { browser } from '$app/environment';

// Define the type for our settings store
interface SettingsStore {
  animationsEnabled: boolean;
  isLoaded: boolean;
}

// Create initial state with default values
const initialState: SettingsStore = {
  animationsEnabled: true, // Default to enabled
  isLoaded: false
};

// Create the writable store
const settingsStore = writable<SettingsStore>(initialState);

// Create derived store with methods to handle settings actions
export const settings = {
  ...settingsStore,
  setAnimationsEnabled: (enabled: boolean) => {
    if (browser) {
      localStorage.setItem('settings.animationsEnabled', JSON.stringify(enabled));
    }
    settingsStore.update(state => ({
      ...state,
      animationsEnabled: enabled
    }));
  },
  toggleAnimations: () => {
    settingsStore.update(state => {
      const newEnabled = !state.animationsEnabled;
      if (browser) {
        localStorage.setItem('settings.animationsEnabled', JSON.stringify(newEnabled));
      }
      return {
        ...state,
        animationsEnabled: newEnabled
      };
    });
  },
  getAnimationsEnabled: (): boolean => {
    let currentEnabled = true;
    settingsStore.subscribe(state => {
      currentEnabled = state.animationsEnabled;
    })();
    return currentEnabled;
  },
  // Synchronize settings from localStorage at app initialization
  synchronize: () => {
    if (browser) {
      const animationsEnabled = localStorage.getItem('settings.animationsEnabled');
      
      let enabled = true; // Default value
      if (animationsEnabled !== null) {
        try {
          enabled = JSON.parse(animationsEnabled);
        } catch (error) {
          console.error('Error parsing animations setting from localStorage:', error);
          // Invalid JSON in localStorage, use default and save it
          localStorage.setItem('settings.animationsEnabled', JSON.stringify(true));
        }
      } else {
        // No setting found, save default
        localStorage.setItem('settings.animationsEnabled', JSON.stringify(true));
      }
      
      settingsStore.update(state => ({
        ...state,
        animationsEnabled: enabled,
        isLoaded: true
      }));
      
      return true;
    }
    return false;
  }
};

export default settings;
