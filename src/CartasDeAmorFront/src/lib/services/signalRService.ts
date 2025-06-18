// SignalR connection manager for game hub
import { writable, get } from 'svelte/store';
import auth from '$lib/stores/authStore';
import { browser } from '$app/environment';
import API_CONFIG from '$lib/config/api-config';
import * as SignalR from '@microsoft/signalr';

// Define connection status type
export type ConnectionStatus = 'disconnected' | 'connecting' | 'connected' | 'error';

// Define the store state interface
interface SignalRState {
  connection: SignalR.HubConnection | null;
  status: ConnectionStatus;
  error: string | null;
  currentRoomId: string | null;
}

// Initial state
const initialState: SignalRState = {
  connection: null,
  status: 'disconnected',
  error: null,
  currentRoomId: null
};

// Create the writable store
const signalRStore = writable<SignalRState>(initialState);

// Handler registration API
let registeredHandlers = {
  onJoinedRoom: null,
  onUserJoined: null,
  onRoundStarted: null,
  onNextTurn: null,
  onPlayerDrewCard: null,
  onGameStartError: null
};

function attachEventHandlers(connection) {
  connection.on('JoinedRoom', (...args) => registeredHandlers.onJoinedRoom?.(...args));
  connection.on('UserJoined', (...args) => registeredHandlers.onUserJoined?.(...args));
  connection.on('RoundStarted', (...args) => registeredHandlers.onRoundStarted?.(...args));
  connection.on('NextTurn', (...args) => registeredHandlers.onNextTurn?.(...args));
  connection.on('PlayerDrewCard', (...args) => registeredHandlers.onPlayerDrewCard?.(...args));
  connection.on('GameStartError', (...args) => registeredHandlers.onGameStartError?.(...args));
}

// Create exported object with methods
export const signalR = {
  subscribe: signalRStore.subscribe,
  registerHandlers(handlers) {
    registeredHandlers = { ...registeredHandlers, ...handlers };
    // Attach to current connection if available
    const state = get(signalRStore);
    if (state.connection) {
      attachEventHandlers(state.connection);
    }
  },
  
  async initialize() {
    if (!browser) return null;

    try {
      signalRStore.update(state => ({ ...state, status: 'connecting' }));
      const token = auth.getToken();
      
      if (!token) {
        throw new Error('Authentication token not available');
      }

      const connection = new SignalR.HubConnectionBuilder()
        .withUrl(`${API_CONFIG.signalR.gameHub}?access_token=${token}`, {
          skipNegotiation: false,
          transport: SignalR.HttpTransportType.WebSockets
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 15000, 30000])
        .configureLogging(SignalR.LogLevel.Debug)
        .build();

      // Register connection error handler
      connection.onclose(error => {
        console.error('SignalR connection closed:', error);
        signalRStore.update(state => ({
          ...state,
          status: 'disconnected',
          error: error ? `Connection closed: ${error.message}` : 'Connection closed'
        }));
      });

      // Attach event handlers
      attachEventHandlers(connection);

      try {
        await connection.start();

        signalRStore.update(state => ({ 
          ...state, 
          connection, 
          status: 'connected',
          error: null
        }));
        
        return connection;
      } catch (startError) {
        const errorMessage = startError instanceof Error ? startError.message : 'Unknown error';
        console.error(`SignalR connection start failed: ${errorMessage}`, startError);
        
        // Try to provide more helpful information about the error
        let detailedError = errorMessage;
        if (errorMessage.includes('<!doctype') || errorMessage.includes('Unexpected token')) {
          detailedError = `Server returned HTML instead of JSON. This usually means the endpoint is incorrect or authentication failed. Check API configuration and network traffic.`;
        }
        
        signalRStore.update(state => ({ 
          ...state, 
          status: 'error',
          error: detailedError
        }));
        
        throw new Error(`Failed to connect to SignalR hub: ${detailedError}`);
      }
    } catch (error) {
      console.error('SignalR connection error:', error);
      signalRStore.update(state => ({ 
        ...state, 
        status: 'error',
        error: error instanceof Error ? error.message : 'Unknown error'
      }));
      throw error;
    }
  },

  async joinRoom(roomId: string, password: string | null = null) {
    const state = get(signalRStore);
    let connection = state.connection;
    
    if (!connection) {
      connection = await this.initialize();
    }
    
    if (connection) {
      try {
        await connection.invoke('JoinRoom', roomId, password);
        signalRStore.update(s => ({ ...s, currentRoomId: roomId }));
        return true;
      } catch (error) {
        console.error('Error joining room:', error);
        throw error;
      }
    }
    
    return false;
  },

  async leaveRoom(roomId: string) {
    const state = get(signalRStore);
    
    if (state.connection) {
      try {
        await state.connection.invoke('LeaveRoom', roomId);
        signalRStore.update(s => ({ ...s, currentRoomId: null }));
        return true;
      } catch (error) {
        console.error('Error leaving room:', error);
        return false;
      }
    }
    
    return false;
  },
  
  async reconnectToRoom(roomId: string) {
    const state = get(signalRStore);
    let connection = state.connection;
    
    if (!connection) {
      connection = await this.initialize();
    }
    
    if (connection) {
      try {
        await connection.invoke('ReconnectToRoom', roomId);
        signalRStore.update(s => ({ ...s, currentRoomId: roomId }));
        return true;
      } catch (error) {
        console.error('Error reconnecting to room:', error);
        throw error;
      }
    }
    
    return false;
  },
  
  async startGame(roomId: string) {
    const state = get(signalRStore);
    
    if (state.connection) {
      try {
        await state.connection.invoke('StartGame', roomId);
        return true;
      } catch (error) {
        console.error('Error starting game:', error);
        throw error;
      }
    }
    
    return false;
  }
};
