import { writable } from 'svelte/store';
import { browser } from '$app/environment';

// Define the type for our auth store
interface AuthStore {
  token: string | null;
  isAuthenticated: boolean;
}

// Create initial state with default values
const initialState: AuthStore = {
  token: null,
  isAuthenticated: false
};

// Create the writable store
const authStore = writable<AuthStore>(initialState);

// Create derived store with methods to handle auth actions
export const auth = {
  ...authStore,
  login: (token: string) => {
    if (browser) {
      localStorage.setItem('authToken', token);
    }
    authStore.update(state => ({
      ...state,
      token,
      isAuthenticated: true
    }));
  },
  logout: () => {
    if (browser) {
      localStorage.removeItem('authToken');
    }
    authStore.update(state => ({
      ...state,
      token: null,
      isAuthenticated: false
    }));
  },
  getToken: (): string | null => {
    let currentToken: string | null = null;
    authStore.subscribe(state => {
      currentToken = state.token;
    })();
    return currentToken;
  },
  // New synchronize method to call at key navigation points
  synchronize: () => {
    if (browser) {
      const token = localStorage.getItem('authToken');
      authStore.update(state => ({
        ...state,
        token,
        isAuthenticated: !!token
      }));
      return !!token;
    }
    return false;
  }
};

export default auth;
