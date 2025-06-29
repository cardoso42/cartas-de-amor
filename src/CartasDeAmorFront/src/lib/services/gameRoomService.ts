// Game room service to manage game room operations
import API_CONFIG from '$lib/config/api-config';
import { apiGet, apiPost, apiDelete } from '$lib/utils/apiUtils';

// Define types
export interface GameRoom {
  id: string;
  roomName: string;
  hasPassword: boolean;
  currentPlayers: number;
  ownerEmail: string;
  creationTime: string;
}

// Get all available game rooms
export async function getGameRooms(): Promise<GameRoom[]> {
  try {
    // Using the API utility function to make authenticated GET request
    return await apiGet<GameRoom[]>(`${API_CONFIG.baseUrl}/GameRoom`);
  } catch (error) {
    console.error('Failed to fetch game rooms:', error);
    return [];
  }
}

// Create a new game room
export async function createGameRoom(roomName: string, password?: string | null): Promise<string> {
  try {
    const response = await apiPost<string>(`${API_CONFIG.baseUrl}/GameRoom`, {
      roomName,
      password: password || null
    });
    return response; // Returns room ID
  } catch (error) {
    console.error('Failed to create game room:', error);
    throw error;
  }
}

// Get user's active (unfinished) game rooms
export async function getUserActiveRooms(): Promise<GameRoom[]> {
  try {
    return await apiGet<GameRoom[]>(`${API_CONFIG.baseUrl}/GameRoom/user`);
  } catch (error) {
    console.error('Failed to fetch user active rooms:', error);
    return [];
  }
}

// Delete a game room
export async function deleteGameRoom(roomId: string): Promise<boolean> {
  try {
    await apiDelete(`${API_CONFIG.baseUrl}/GameRoom/${roomId}`);
    return true;
  } catch (error) {
    console.error('Failed to delete game room:', error);
    return false;
  }
}
