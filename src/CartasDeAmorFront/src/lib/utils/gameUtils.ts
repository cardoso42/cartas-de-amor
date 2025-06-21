import type { InitialGameStatusDto } from '$lib/types/game-types';

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
