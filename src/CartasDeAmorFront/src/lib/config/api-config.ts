// API configuration for the application
// This file centralizes all API endpoint configurations
import { PUBLIC_API_BASE_URL } from '$env/static/public';

const API_CONFIG = {
  // Base API URL will be handled by Vite's proxy configuration
  // In development, requests to /api/* will be proxied to the API_URL from .env
  baseUrl: PUBLIC_API_BASE_URL,
  
  // Auth endpoints
  auth: {
    login: `${PUBLIC_API_BASE_URL}/Account/Login`,
    register: `${PUBLIC_API_BASE_URL}/Account/Create`,
    logout: `${PUBLIC_API_BASE_URL}/Account/Logout`
  },
  
  // Game endpoints
  game: {
    rooms: `${PUBLIC_API_BASE_URL}/GameRoom`,
    create: `${PUBLIC_API_BASE_URL}/GameRoom`,
    delete: (roomId: string) => `${PUBLIC_API_BASE_URL}/GameRoom/${roomId}`,
  },
  
  // SignalR hub - Use relative path to work with Vite proxy
  signalR: {
    gameHub: `/gamehub`,
  }
  
  // Add more endpoint configurations as needed
};

export default API_CONFIG;
