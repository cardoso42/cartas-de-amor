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
 * Card action requirements enum matching the backend enum
 */
export enum CardActionRequirements {
  None = 0,
  SelectPlayer = 1,
  SelectCardType = 2
}

/**
 * Card requirements DTO interface
 */
export interface CardRequirementsDto {
  cardType: CardType;
  requirements: CardActionRequirements[];
  possibleTargets: string[];
  possibleCardTypes: CardType[];
  message?: string;
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
  playedCards?: CardType[]; // Add played cards for displaying card play history
}

/**
 * Initial game status DTO interface
 * Matches the C# GameStatusDto class
 */
export interface GameStatusDto {
  otherPlayersPublicData: PublicPlayerUpdateDto[];
  yourData: PrivatePlayerUpdateDto;
  allPlayersInOrder: string[];
  firstPlayerIndex: number;
  cardsRemainingInDeck: number;
}

/**
 * Private player update DTO interface
 */
export interface PrivatePlayerUpdateDto {
  userEmail: string;
  status: number; // PlayerStatus enum
  holdingCards: CardType[];
  playedCards: CardType[];
  score: number;
}

/**
 * Public player update DTO interface
 */
export interface PublicPlayerUpdateDto {
  userEmail: string;
  username: string;
  status: number; // PlayerStatus enum
  playedCards: CardType[];
  holdingCardsCount: number;
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

/**
 * Card play DTO interface
 */
export interface CardPlayDto {
  cardType: CardType;
  targetPlayerEmail?: string | null;
  targetCardType?: CardType | null;
}

/**
 * Card action result DTO interface
 */
export interface CardActionResultDto {
  invoker?: PublicPlayerUpdateDto;
  target?: PublicPlayerUpdateDto;
}
