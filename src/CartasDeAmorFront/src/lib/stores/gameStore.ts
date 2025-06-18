import { writable } from 'svelte/store';

interface GameState {
  roomId: string | null;
  roomName?: string;
  players: string[];
  isRoomOwner: boolean;
  hostEmail?: string;
}

export const gameStore = writable<GameState>({
  roomId: null,
  roomName: undefined,
  players: [],
  isRoomOwner: false,
  hostEmail: undefined
});
