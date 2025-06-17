import { writable } from 'svelte/store';
import { browser } from '$app/environment';

// Define the type for our auth store
interface AuthStore {
  token: string | null;
  isAuthenticated: boolean;
}

// Create initial state
const initialState: AuthStore = {
  token: browser ? localStorage.getItem('authToken') : null,
  isAuthenticated: browser ? !!localStorage.getItem('authToken') : false
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
  }
};

export default auth;
