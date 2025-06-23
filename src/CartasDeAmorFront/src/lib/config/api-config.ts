// API configuration for the application
// This file centralizes all API endpoint configurations
import { PUBLIC_API_BASE_URL } from '$env/static/public';

const API_CONFIG = {
  // Base API URL will be handled by nginx proxy in Docker
  baseUrl: PUBLIC_API_BASE_URL,
  
  // Auth endpoints  
  auth: {
    login: `${PUBLIC_API_BASE_URL}/account/login`,
    register: `${PUBLIC_API_BASE_URL}/account/create`,
    logout: `${PUBLIC_API_BASE_URL}/account/logout`,
    deleteAccount: (email: string) => `${PUBLIC_API_BASE_URL}/account/${email}`
  },
  
  // Game endpoints
  game: {
    rooms: `${PUBLIC_API_BASE_URL}/gameroom`,
    create: `${PUBLIC_API_BASE_URL}/gameroom`,
    delete: (roomId: string) => `${PUBLIC_API_BASE_URL}/gameroom/${roomId}`,
  },
  
  // SignalR hub - Use relative path to work with nginx proxy
  signalR: {
    gameHub: `/gameHub`,
  }
  
  // Add more endpoint configurations as needed
};

export default API_CONFIG;
