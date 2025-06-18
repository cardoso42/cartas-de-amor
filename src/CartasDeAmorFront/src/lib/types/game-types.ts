// TypeScript interfaces for game-related DTOs
// These should match the C# models from the backend

/**
 * Card types enum matching the backend CardType enum
 */
export enum CardType {
  Guard = 0,
  Priest = 1,
  Baron = 2,
  Handmaid = 3,
  Prince = 4,
  King = 5,
  Countess = 6,
  Princess = 7
}

/**
 * Player status DTO interface
 */
export interface PlayerStatusDto {
  userEmail: string;
  username: string;
  isProtected: boolean;
  score: number;
  playedCards: CardType[];
  isEliminated: boolean;
}

/**
 * Initial game status DTO interface
 * Matches the C# InitialGameStatusDto class
 */
export interface InitialGameStatusDto {
  otherPlayersPublicData: PlayerStatusDto[];
  yourCards: CardType[];
  isProtected: boolean;
  allPlayersInOrder: string[];
  firstPlayerIndex: number;
  score: number;
}

/**
 * Private player update DTO interface
 */
export interface PrivatePlayerUpdateDto {
  holdingCards: CardType[];
  isProtected: boolean;
  score: number;
}

/**
 * Public player update DTO interface
 */
export interface PublicPlayerUpdateDto {
  userEmail: string;
  isProtected: boolean;
  playedCards: CardType[];
  isEliminated: boolean;
  score: number;
}

/**
 * Game status DTO interface
 */
export interface JoinRoomResultDto {
  roomId: string;
  roomName: string;
  playerId: number;
  hostEmail: string;
  players: string[];
}
