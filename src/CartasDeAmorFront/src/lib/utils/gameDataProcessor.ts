import type { InitialGameStatusDto, CardType } from '$lib/types/game-types';

/**
 * PlayerStatus enum values (must match backend)
 */
export enum PlayerStatus {
  Active = 0,
  Protected = 1,
  Eliminated = 2,
  Disconnected = 3,
  Abandoned = 4
}

/**
 * Derive protection status from PlayerStatus enum
 */
export function isPlayerProtected(status: number): boolean {
  return status === PlayerStatus.Protected;
}

/**
 * Derive elimination status from PlayerStatus enum
 */
export function isPlayerEliminated(status: number): boolean {
  return status === PlayerStatus.Eliminated;
}

export interface ProcessedPlayer {
  id: number;
  name: string;
  email: string;
  isLocalPlayer: boolean;
  position: number;
  tokens: number;
  cards: CardType[];
  cardsInHand: number;
  playedCards: number[];
  isProtected: boolean;
  isCurrentTurn: boolean;
  isEliminated: boolean;
}

/**
 * Process raw game data into a format suitable for display
 */
export function processGameData(
  status: InitialGameStatusDto, 
  userEmail: string, 
  turnPlayer: string,
  localPlayerName: string,
  localPlayerPlayedCards: number[],
  eliminatedPlayers: Set<string> = new Set()
): ProcessedPlayer[] {
  const processedPlayers: ProcessedPlayer[] = [];
  
  // Add local player at position 0 (bottom)
  processedPlayers.push({
    id: 0,
    name: localPlayerName, // Use the username from user store
    email: userEmail,
    isLocalPlayer: true,
    position: 0,
    tokens: status.score || 0,
    cards: status.yourCards || [],
    cardsInHand: (status.yourCards || []).length,
    playedCards: localPlayerPlayedCards, // Use the tracked local player played cards
    isProtected: status.isProtected !== undefined ? status.isProtected : false,
    isCurrentTurn: userEmail === turnPlayer,
    isEliminated: eliminatedPlayers.has(userEmail) // Check local elimination state
  });
  
  // Add other players in order around the table
  (status.otherPlayersPublicData || []).forEach((player, index) => {
    processedPlayers.push({
      id: index + 1,
      name: player.username || player.userEmail.split('@')[0], // Use proper username or fallback to email username
      email: player.userEmail,
      isLocalPlayer: false,
      position: index + 1,
      tokens: player.score || 0,
      cards: [], // Other players' cards are hidden
      cardsInHand: player.cardsInHand || 1,
      playedCards: player.playedCards || [], // Include played cards for display
      isProtected: player.isProtected !== undefined ? player.isProtected : isPlayerProtected(player.status || 0),
      isCurrentTurn: player.userEmail === turnPlayer,
      isEliminated: eliminatedPlayers.has(player.userEmail) || isPlayerEliminated(player.status || 0)
    });
  });
  
  return processedPlayers;
}
