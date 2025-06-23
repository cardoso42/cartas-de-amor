/**
 * Type definitions for Data Transfer Objects (DTOs) used in the API
 * Based on the API documentation
 */

export interface CreateAccountRequestDto {
  username: string;
  email: string;
  password: string;
}

export interface LoginRequestDto {
  email: string;
  password: string;
}

export interface LoginResultDto {
  username: string;
  email: string;
  token: string;
  expiration: string;
  message: string;
}

export interface GameRoomCreationRequestDto {
  roomName: string;
  password?: string;
}

export interface GameRoomDto {
  id: string; // GUID
  roomName: string;
  ownerEmail: string;
  hasPassword: boolean;
  currentPlayers: number;
  createdAt: string;
}

export interface CardPlayDto {
  cardType: number;
  targetPlayerEmail?: string;
  targetCardType?: number;
}
