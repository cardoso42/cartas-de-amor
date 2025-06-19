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

// Define handler types
interface SignalRHandlers {
  onJoinedRoom?: (joinRoomResult: unknown) => void;
  onUserJoined?: (playerEmail: string) => void;
  onRoundStarted?: (initialGameStatus: unknown) => void;
  onNextTurn?: (playerEmail: string) => void;
  onPlayerDrewCard?: (playerEmail: string) => void;
  onGameStartError?: (error: string) => void;
  onPrivatePlayerUpdate?: (playerUpdate: unknown) => void;
  onDrawCardError?: (error: string) => void;
}

// Handler registration API
let registeredHandlers: SignalRHandlers = {
  onJoinedRoom: undefined,
  onUserJoined: undefined,
  onRoundStarted: undefined,
  onNextTurn: undefined,
  onPlayerDrewCard: undefined,
  onGameStartError: undefined,
  onPrivatePlayerUpdate: undefined,
  onDrawCardError: undefined
};

function attachEventHandlers(connection: SignalR.HubConnection) {
  // Remove existing handlers to prevent duplicates
  connection.off('JoinedRoom');
  connection.off('UserJoined');
  connection.off('RoundStarted');
  connection.off('NextTurn');
  connection.off('PlayerDrewCard');
  connection.off('GameStartError');
  connection.off('PrivatePlayerUpdate');
  connection.off('DrawCardError');
  
  // Add new handlers
  connection.on('JoinedRoom', (joinRoomResult: unknown) => registeredHandlers.onJoinedRoom?.(joinRoomResult));
  connection.on('UserJoined', (playerEmail: string) => registeredHandlers.onUserJoined?.(playerEmail));
  connection.on('RoundStarted', (initialGameStatus: unknown) => registeredHandlers.onRoundStarted?.(initialGameStatus));
  connection.on('NextTurn', (playerEmail: string) => registeredHandlers.onNextTurn?.(playerEmail));
  connection.on('PlayerDrewCard', (playerEmail: string) => registeredHandlers.onPlayerDrewCard?.(playerEmail));
  connection.on('GameStartError', (error: string) => registeredHandlers.onGameStartError?.(error));
  connection.on('PrivatePlayerUpdate', (playerUpdate: unknown) => registeredHandlers.onPrivatePlayerUpdate?.(playerUpdate));
  connection.on('DrawCardError', (error: string) => registeredHandlers.onDrawCardError?.(error));
}

// Create exported object with methods
export const signalR = {
  subscribe: signalRStore.subscribe,
  registerHandlers(handlers: SignalRHandlers) {
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
  },

  async drawCard(roomId: string) {
    const state = get(signalRStore);
    
    if (state.connection) {
      try {
        await state.connection.invoke('DrawCard', roomId);
        return true;
      } catch (error) {
        console.error('Error drawing card:', error);
        throw error;
      }
    }
    
    return false;
  }
};
