// SignalR connection manager for game hub
import { writable, get } from 'svelte/store';
import auth from '$lib/stores/authStore';
import { browser } from '$app/environment';
import API_CONFIG from '$lib/config/api-config';
import * as SignalR from '@microsoft/signalr';
import type { 
  JoinRoomResultDto, 
  InitialGameStatusDto, 
  PrivatePlayerUpdateDto,
  CardRequirementsDto,
  PublicPlayerUpdateDto
} from '$lib/types/game-types';

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
  onJoinedRoom?: (joinRoomResult: JoinRoomResultDto) => void;
  onUserJoined?: (playerEmail: string) => void;
  onRoundStarted?: (initialGameStatus: InitialGameStatusDto) => void;
  onNextTurn?: (playerEmail: string) => void;
  onPlayerDrewCard?: (playerEmail: string) => void;
  onGameStartError?: (error: string) => void;
  onPrivatePlayerUpdate?: (playerUpdate: PrivatePlayerUpdateDto) => void;
  onDrawCardError?: (error: string) => void;
  onCardRequirements?: (requirements: CardRequirementsDto) => void;
  onPlayCardError?: (error: string) => void;
  // New MessageFactory events
  onPlayCard?: (data: { player: string; cardType: number }) => void;
  onGuessCard?: (data: { invoker: string; cardType: number; target: string }) => void;
  onPeekCard?: (data: { invoker: string; target: string }) => void;
  onShowCard?: (data: { invoker: string; target: string, cardType: number }) => void;
  onCompareCards?: (data: { invoker: string; target: string }) => void;
  onComparisonTie?: (data: { invoker: string; target: string }) => void;
  onDiscardCard?: (data: { target: string; cardType: number }) => void;
  onDrawCard?: (data: { player: string }) => void;
  onCardReturnedToDeck?: (data: { player: string; cardCount: number }) => void;
  onPlayerEliminated?: (data: { player: string }) => void;
  onSwitchCards?: (data: { invoker: string; target: string }) => void;
  onPlayerProtected?: (data: { player: string }) => void;
  onChooseCard?: (data: { player: string }) => void;
  // Player update events
  onPublicPlayerUpdate?: (data: PublicPlayerUpdateDto) => void;
  // Other game events
  onCardChoiceSubmitted?: (playerUpdate: PublicPlayerUpdateDto) => void;
  onCardChoiceError?: (error: string) => void;
  onMandatoryCardPlay?: (message: string, requiredCardType: number) => void;
  onRoundWinners?: (winners: string[]) => void;
  onBonusPoints?: (players: string[]) => void;
  onGameOver?: (winners: string[]) => void;
}

// TODO: acabei de jogar um principe, escolhi a mim mesmo para descartar, mas o jogo não atualizou corretamente a minha nova carta, apenas quando comprei do deck no próximo turno

// Handler registration API
let registeredHandlers: SignalRHandlers = {
  onJoinedRoom: undefined,
  onUserJoined: undefined,
  onRoundStarted: undefined,
  onNextTurn: undefined,
  onPlayerDrewCard: undefined,
  onGameStartError: undefined,
  onPrivatePlayerUpdate: undefined,
  onDrawCardError: undefined,
  onCardRequirements: undefined,
  onPlayCardError: undefined,
  // New MessageFactory events
  onPlayCard: undefined,
  onGuessCard: undefined,
  onShowCard: undefined,
  onCompareCards: undefined,
  onComparisonTie: undefined,
  onDiscardCard: undefined,
  onDrawCard: undefined,
  onCardReturnedToDeck: undefined,
  onPlayerEliminated: undefined,
  onSwitchCards: undefined,
  onPlayerProtected: undefined,
  onChooseCard: undefined,
  // Player update events
  onPublicPlayerUpdate: undefined,
  // Other game events
  onCardChoiceSubmitted: undefined,
  onCardChoiceError: undefined,
  onMandatoryCardPlay: undefined,
  onRoundWinners: undefined,
  onBonusPoints: undefined,
  onGameOver: undefined
};

function attachEventHandlers(connection: SignalR.HubConnection) {
  // Remove existing handlers to prevent duplicates
  connection.off('JoinedRoom');
  connection.off('UserJoined');
  connection.off('RoundStarted');
  connection.off('NextTurn');
  connection.off('PlayerDrewCard');
  connection.off('GameStartError');
  connection.off('PlayerUpdatePrivate');
  connection.off('DrawCardError');
  connection.off('CardRequirements');
  connection.off('PlayCardError');
  // New MessageFactory events
  connection.off('PlayCard');
  connection.off('GuessCard');
  connection.off('PeekCard');
  connection.off('ShowCard');
  connection.off('CompareCards');
  connection.off('ComparisonTie');
  connection.off('DiscardCard');
  connection.off('DrawCard');
  connection.off('CardReturnedToDeck');
  connection.off('PlayerEliminated');
  connection.off('SwitchCards');
  connection.off('PlayerProtected');
  connection.off('ChooseCard');
  // Player update events
  connection.off('PublicPlayerUpdate');
  // Other game events
  connection.off('CardChoiceSubmitted');
  connection.off('CardChoiceError');
  connection.off('MandatoryCardPlay');
  connection.off('RoundWinners');
  connection.off('BonusPoints');
  connection.off('GameOver');
  
  // Add new handlers
  connection.on('JoinedRoom', (joinRoomResult: JoinRoomResultDto) => registeredHandlers.onJoinedRoom?.(joinRoomResult));
  connection.on('UserJoined', (playerEmail: string) => registeredHandlers.onUserJoined?.(playerEmail));
  connection.on('RoundStarted', (initialGameStatus: InitialGameStatusDto) => registeredHandlers.onRoundStarted?.(initialGameStatus));
  connection.on('NextTurn', (playerEmail: string) => registeredHandlers.onNextTurn?.(playerEmail));
  connection.on('PlayerDrewCard', (playerEmail: string) => registeredHandlers.onPlayerDrewCard?.(playerEmail));
  connection.on('GameStartError', (error: string) => registeredHandlers.onGameStartError?.(error));
  connection.on('PlayerUpdatePrivate', (playerUpdate: PrivatePlayerUpdateDto) => registeredHandlers.onPrivatePlayerUpdate?.(playerUpdate));
  connection.on('DrawCardError', (error: string) => registeredHandlers.onDrawCardError?.(error));
  connection.on('CardRequirements', (requirements: CardRequirementsDto) => registeredHandlers.onCardRequirements?.(requirements));
  connection.on('PlayCardError', (error: string) => registeredHandlers.onPlayCardError?.(error));
  // New MessageFactory events
  connection.on('PlayCard', (data: { player: string; cardType: number }) => registeredHandlers.onPlayCard?.(data));
  connection.on('GuessCard', (data: { invoker: string; cardType: number; target: string }) => registeredHandlers.onGuessCard?.(data));
  connection.on('PeekCard', (data: { invoker: string; target: string }) => registeredHandlers.onPeekCard?.(data));
  connection.on('ShowCard', (data: { invoker: string; target: string, cardType: number }) => registeredHandlers.onShowCard?.(data));
  connection.on('CompareCards', (data: { invoker: string; target: string }) => registeredHandlers.onCompareCards?.(data));
  connection.on('ComparisonTie', (data: { invoker: string; target: string }) => registeredHandlers.onComparisonTie?.(data));
  connection.on('DiscardCard', (data: { target: string; cardType: number }) => registeredHandlers.onDiscardCard?.(data));
  connection.on('DrawCard', (data: { player: string }) => registeredHandlers.onDrawCard?.(data));
  connection.on('CardReturnedToDeck', (data: { player: string; cardCount: number }) => registeredHandlers.onCardReturnedToDeck?.(data));
  connection.on('PlayerEliminated', (data: { player: string }) => registeredHandlers.onPlayerEliminated?.(data));
  connection.on('SwitchCards', (data: { invoker: string; target: string }) => registeredHandlers.onSwitchCards?.(data));
  connection.on('PlayerProtected', (data: { player: string }) => registeredHandlers.onPlayerProtected?.(data));
  connection.on('ChooseCard', (data: { player: string }) => registeredHandlers.onChooseCard?.(data));
  // Player update events
  connection.on('PublicPlayerUpdate', (data: PublicPlayerUpdateDto) => registeredHandlers.onPublicPlayerUpdate?.(data));
  // Other game events
  connection.on('CardChoiceSubmitted', (playerUpdate: PublicPlayerUpdateDto) => registeredHandlers.onCardChoiceSubmitted?.(playerUpdate));
  connection.on('CardChoiceError', (error: string) => registeredHandlers.onCardChoiceError?.(error));
  connection.on('MandatoryCardPlay', (message: string, requiredCardType: number) => registeredHandlers.onMandatoryCardPlay?.(message, requiredCardType));
  connection.on('RoundWinners', (winners: string[]) => registeredHandlers.onRoundWinners?.(winners));
  connection.on('BonusPoints', (players: string[]) => registeredHandlers.onBonusPoints?.(players));
  connection.on('GameOver', (winners: string[]) => registeredHandlers.onGameOver?.(winners));
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
  
  async disconnect() {
    const state = get(signalRStore);
    
    if (state.connection) {
      try {
        console.log('Disconnecting SignalR connection');
        await state.connection.stop();
        signalRStore.update(s => ({ 
          ...s, 
          connection: null, 
          status: 'disconnected',
          currentRoomId: null,
          error: null
        }));
        return true;
      } catch (error) {
        console.error('Error disconnecting SignalR:', error);
        return false;
      }
    }
    
    return true;
  },
  
  async initialize() {
    if (!browser) return null;

    try {
      // First, close any existing connection
      const currentState = get(signalRStore);
      if (currentState.connection) {
        console.log('Closing existing SignalR connection before creating new one');
        try {
          await currentState.connection.stop();
        } catch (error) {
          console.warn('Error stopping existing connection:', error);
        }
      }

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

      // Clean up connection when page is unloaded
      if (browser) {
        const handleBeforeUnload = () => {
          if (connection && connection.state === SignalR.HubConnectionState.Connected) {
            connection.stop();
          }
        };
        window.addEventListener('beforeunload', handleBeforeUnload);
      }

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
    
    // If we already have a connection, ensure it's properly connected
    if (connection && connection.state === SignalR.HubConnectionState.Connected) {
      // Use existing connection
    } else {
      // Need to create new connection (either no connection or connection is not connected)
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
    
    // If we already have a connected connection, use it
    if (connection && connection.state === SignalR.HubConnectionState.Connected) {
      // Use existing connection
    } else {
      // Need to create new connection (either no connection or connection is not connected)
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
    
    if (state.connection && state.connection.state === SignalR.HubConnectionState.Connected) {
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
    
    if (state.connection && state.connection.state === SignalR.HubConnectionState.Connected) {
      try {
        await state.connection.invoke('DrawCard', roomId);
        return true;
      } catch (error) {
        console.error('Error drawing card:', error);
        throw error;
      }
    }
    
    return false;
  },

  async getCardRequirements(roomId: string, cardType: number) {
    const state = get(signalRStore);
    
    if (state.connection && state.connection.state === SignalR.HubConnectionState.Connected) {
      try {
        await state.connection.invoke('GetCardRequirements', roomId, cardType);
        return true;
      } catch (error) {
        console.error('Error getting card requirements:', error);
        throw error;
      }
    }
    
    return false;
  },

  async playCard(roomId: string, cardPlayDto: { cardType: number; targetPlayerEmail?: string | null; targetCardType?: number | null }) {
    const state = get(signalRStore);
    
    if (state.connection && state.connection.state === SignalR.HubConnectionState.Connected) {
      try {
        await state.connection.invoke('PlayCard', roomId, cardPlayDto);
        return true;
      } catch (error) {
        console.error('Error playing card:', error);
        throw error;
      }
    }
    
    return false;
  },

  async submitCardChoice(roomId: string, keepCardType: number, returnCardTypes: number[]) {
    const state = get(signalRStore);
    
    if (state.connection && state.connection.state === SignalR.HubConnectionState.Connected) {
      try {
        await state.connection.invoke('SubmitCardChoice', roomId, keepCardType, returnCardTypes);
        return true;
      } catch (error) {
        console.error('Error submitting card choice:', error);
        throw error;
      }
    }
    
    return false;
  }
};
