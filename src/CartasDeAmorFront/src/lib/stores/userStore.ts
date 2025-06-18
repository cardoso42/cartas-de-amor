import { writable } from 'svelte/store';
import { browser } from '$app/environment';

// Define the type for our user store
interface UserStore {
  id: string | null;
  email: string | null;
  username: string | null;
  isLoaded: boolean;
}

// Create initial state with default values
const initialState: UserStore = {
  id: null,
  email: null,
  username: null,
  isLoaded: false
};

// Create the writable store
const userStore = writable<UserStore>(initialState);

// Create derived store with methods to handle user actions
export const user = {
  ...userStore,
  setUser: (userData: { id: string; email: string; username: string }) => {
    if (browser) {
      localStorage.setItem('userData', JSON.stringify(userData));
    }
    userStore.update(state => ({
      ...state,
      id: userData.id,
      email: userData.email,
      username: userData.username,
      isLoaded: true
    }));
  },
  updateUser: (updates: Partial<Pick<UserStore, 'email' | 'username'>>) => {
    userStore.update(state => {
      const newState = { ...state, ...updates };
      if (browser) {
        const { id, email, username } = newState;
        if (id && email && username) {
          localStorage.setItem('userData', JSON.stringify({ id, email, username }));
        }
      }
      return newState;
    });
  },
  clearUser: () => {
    if (browser) {
      localStorage.removeItem('userData');
    }
    userStore.update(state => ({
      ...state,
      id: null,
      email: null,
      username: null,
      isLoaded: false
    }));
  },
  getUserData: (): Pick<UserStore, 'id' | 'email' | 'username'> | null => {
    let currentUser: Pick<UserStore, 'id' | 'email' | 'username'> | null = null;
    userStore.subscribe(state => {
      if (state.id && state.email && state.username) {
        currentUser = {
          id: state.id,
          email: state.email,
          username: state.username
        };
      }
    })();
    return currentUser;
  },
  // Synchronize user data from localStorage at app initialization
  synchronize: () => {
    if (browser) {
      const userData = localStorage.getItem('userData');
      if (userData) {
        try {
          const parsed = JSON.parse(userData);
          userStore.update(state => ({
            ...state,
            id: parsed.id || null,
            email: parsed.email || null,
            username: parsed.username || null,
            isLoaded: true
          }));
          return true;
        } catch (error) {
          console.error('Error parsing user data from localStorage:', error);

          // Invalid JSON in localStorage, clear it
          localStorage.removeItem('userData');
          userStore.update(state => ({
            ...state,
            isLoaded: true
          }));
        }
      } else {
        userStore.update(state => ({
          ...state,
          isLoaded: true
        }));
      }
    }
    return false;
  }
};

export default user;
