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
  CardActionResultDto,
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
  // Card result events
  onCardResultNone?: (cardResult: CardActionResultDto) => void;
  onCardResultShowCard?: (cardResult: CardActionResultDto) => void;
  onCardResultPlayerEliminated?: (cardResult: CardActionResultDto) => void;
  onCardResultSwitchCards?: (cardResult: CardActionResultDto) => void;
  onCardResultDiscardAndDrawCard?: (cardResult: CardActionResultDto) => void;
  onCardResultProtectionGranted?: (cardResult: CardActionResultDto) => void;
  onCardResultChooseCard?: (cardResult: CardActionResultDto) => void;
  // Other game events
  onChooseCard?: (cardType: number) => void;
  onCardChoiceSubmitted?: (playerUpdate: PublicPlayerUpdateDto) => void;
  onCardChoiceError?: (error: string) => void;
  onMandatoryCardPlay?: (message: string, requiredCardType: number) => void;
  onRoundWinners?: (winners: string[]) => void;
  onBonusPoints?: (players: string[]) => void;
  onGameOver?: (winners: string[]) => void;
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
  onDrawCardError: undefined,
  onCardRequirements: undefined,
  onPlayCardError: undefined,
  // Card result events
  onCardResultNone: undefined,
  onCardResultShowCard: undefined,
  onCardResultPlayerEliminated: undefined,
  onCardResultSwitchCards: undefined,
  onCardResultDiscardAndDrawCard: undefined,
  onCardResultProtectionGranted: undefined,
  onCardResultChooseCard: undefined,
  // Other game events
  onChooseCard: undefined,
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
  connection.off('PrivatePlayerUpdate');
  connection.off('DrawCardError');
  connection.off('CardRequirements');
  connection.off('PlayCardError');
  // Card result events
  connection.off('CardResult-None');
  connection.off('CardResult-ShowCard');
  connection.off('CardResult-PlayerEliminated');
  connection.off('CardResult-SwitchCards');
  connection.off('CardResult-DiscardAndDrawCard');
  connection.off('CardResult-ProtectionGranted');
  connection.off('CardResult-ChooseCard');
  // Other game events
  connection.off('ChooseCard');
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
  connection.on('PrivatePlayerUpdate', (playerUpdate: PrivatePlayerUpdateDto) => registeredHandlers.onPrivatePlayerUpdate?.(playerUpdate));
  connection.on('DrawCardError', (error: string) => registeredHandlers.onDrawCardError?.(error));
  connection.on('CardRequirements', (requirements: CardRequirementsDto) => registeredHandlers.onCardRequirements?.(requirements));
  connection.on('PlayCardError', (error: string) => registeredHandlers.onPlayCardError?.(error));
  // Card result events
  connection.on('CardResult-None', (cardResult: CardActionResultDto) => registeredHandlers.onCardResultNone?.(cardResult));
  connection.on('CardResult-ShowCard', (cardResult: CardActionResultDto) => registeredHandlers.onCardResultShowCard?.(cardResult));
  connection.on('CardResult-PlayerEliminated', (cardResult: CardActionResultDto) => registeredHandlers.onCardResultPlayerEliminated?.(cardResult));
  connection.on('CardResult-SwitchCards', (cardResult: CardActionResultDto) => registeredHandlers.onCardResultSwitchCards?.(cardResult));
  connection.on('CardResult-DiscardAndDrawCard', (cardResult: CardActionResultDto) => registeredHandlers.onCardResultDiscardAndDrawCard?.(cardResult));
  connection.on('CardResult-ProtectionGranted', (cardResult: CardActionResultDto) => registeredHandlers.onCardResultProtectionGranted?.(cardResult));
  connection.on('CardResult-ChooseCard', (cardResult: CardActionResultDto) => registeredHandlers.onCardResultChooseCard?.(cardResult));
  // Other game events
  connection.on('ChooseCard', (cardType: number) => registeredHandlers.onChooseCard?.(cardType));
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
  },

  async getCardRequirements(roomId: string, cardType: number) {
    const state = get(signalRStore);
    
    if (state.connection) {
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
    
    if (state.connection) {
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
    
    if (state.connection) {
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
