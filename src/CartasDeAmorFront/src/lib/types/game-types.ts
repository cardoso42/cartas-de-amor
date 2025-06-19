// TypeScript interfaces for game-related DTOs
// These should match the C# models from the backend

/**
 * Card types enum matching the backend CardType enum
 */
export enum CardType {
  Spy = 0,
  Guard = 1,
  Priest = 2,
  Baron = 3,
  Handmaid = 4,
  Prince = 5,
  Chanceller = 6,
  King = 7,
  Countess = 8,
  Princess = 9
}

/**
 * Player status DTO interface
 */
export interface PlayerStatusDto {
  userEmail: string;
  username: string;
  status: number; // PlayerStatus enum
  isProtected: boolean;
  score: number;
  cardsInHand: number;
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
