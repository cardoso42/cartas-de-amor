import type { InitialGameStatusDto } from '$lib/types/game-types';
import type { ProcessedPlayer } from './gameDataProcessor';

/**
 * Calculate player position around a circular table
 */
export function getPlayerPosition(playerIndex: number, totalPlayers: number) {
  if (totalPlayers === 1) {
    return { angle: 180, distance: 320 }; // Single player at bottom
  }
  
  const baseAngle = 180; // Start at bottom (180 degrees)
  const angleStep = 360 / totalPlayers;
  const angle = (baseAngle + (playerIndex * angleStep)) % 360;
  
  // Distance from center of table
  const distance = 320;
  
  return { angle, distance };
}

/**
 * Get the current turn player email
 */
export function getCurrentTurnPlayer(status: InitialGameStatusDto | null, turnPlayerEmail: string): string {
  if (!status) return '';
  
  // If we have a turn player email from SignalR, use that
  if (turnPlayerEmail) return turnPlayerEmail;
  
  // Otherwise, use the FirstPlayerIndex from the game status
  if (status.allPlayersInOrder && status.firstPlayerIndex >= 0 && status.firstPlayerIndex < status.allPlayersInOrder.length) {
    return status.allPlayersInOrder[status.firstPlayerIndex];
  }
  
  return '';
}

/**
 * Get display name for a player based on email
 */
export function getPlayerDisplayName(email: string, players?: Array<{ email: string; name: string }>): string {
  if (!players) {
    return email.split('@')[0];
  }
  
  const player = players.find(p => p.email === email);
  return player?.name || email.split('@')[0];
}

/**
 * Get played cards area position for a player using perspective-aware positioning
 */
export function getPlayerPlayedCardsPosition(
  playerEmail: string, 
  players: ProcessedPlayer[]
): { x: number; y: number; width: number; height: number } {
  const player = players.find((p: ProcessedPlayer) => p.email === playerEmail);
  
  if (!player) {
    return { x: window.innerWidth / 2, y: window.innerHeight / 2, width: 32, height: 45 };
  }
  
  // Use the player's processed position which is already perspective-aware
  const totalPlayers = players.length;
  const playerPosition = player.position;
  
  // Calculate position using the same logic as PlayerArea but for played cards (closer to center)
  const baseAngle = 180; // Start at bottom (180 degrees)
  const angleStep = 360 / totalPlayers;
  const angle = (baseAngle + (playerPosition * angleStep)) % 360;
  const playedCardDistance = 150; // Closer to center than player position (320)
  
  // Convert to screen coordinates
  const centerX = window.innerWidth / 2;
  const centerY = window.innerHeight / 2;
  const radians = (angle - 90) * Math.PI / 180;
  
  return {
    x: centerX + Math.cos(radians) * playedCardDistance,
    y: centerY + Math.sin(radians) * playedCardDistance,
    width: 32,
    height: 45
  };
}

/**
 * Get player screen position for animations using perspective-aware positioning
 */
export function getPlayerScreenPosition(
  playerEmail: string, 
  players: ProcessedPlayer[]
): { x: number; y: number } {
  const player = players.find((p: ProcessedPlayer) => p.email === playerEmail);
  
  if (!player) {
    return { x: window.innerWidth / 2, y: window.innerHeight / 2 };
  }
  
  // Use the player's processed position which is already perspective-aware
  const totalPlayers = players.length;
  const playerPosition = player.position;
  
  // Calculate position using the same logic as PlayerArea
  const baseAngle = 180; // Start at bottom (180 degrees)
  const angleStep = 360 / totalPlayers;
  const angle = (baseAngle + (playerPosition * angleStep)) % 360;
  const distance = 320;
  
  // Convert to screen coordinates
  const centerX = window.innerWidth / 2;
  const centerY = window.innerHeight / 2;
  const radians = (angle - 90) * Math.PI / 180;
  
  return {
    x: centerX + Math.cos(radians) * distance,
    y: centerY + Math.sin(radians) * distance
  };
}
