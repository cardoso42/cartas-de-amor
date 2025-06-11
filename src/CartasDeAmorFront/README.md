# Love Letter Game - Frontend Test Client

This is a simple HTML/CSS/JavaScript frontend client designed to test the functionalities of the Love Letter game backend.

## Features

### Authentication
- **User Registration**: Create new accounts with username, email, and password
- **User Login**: Authenticate existing users
- **JWT Token Management**: Automatic token storage and authentication headers

### Game Room Management
- **Create Room**: Create new game rooms with optional password protection
- **Join Room**: Join existing rooms by Room ID
- **Leave Room**: Leave current room
- **Delete Room**: Delete rooms (host only)

### Real-time Game Communication
- **SignalR Integration**: Real-time communication with the backend
- **Game Start**: Start games (host only)
- **Card Playing**: Play cards during the game
- **Game Events**: Receive real-time game updates

## Setup Instructions

1. **Configure Backend URL**
   - Open `script.js`
   - Update the `API_BASE_URL` and `HUB_URL` constants to match your backend server
   - Default values assume backend is running on `https://localhost:7001`

2. **Start Backend Server**
   - Make sure your .NET backend is running
   - Ensure CORS is properly configured to allow requests from your frontend

3. **Serve Frontend**
   - You can serve the frontend using any web server
   - For development, you can use Python's built-in server:
     ```bash
     cd /home/cardoso42/dev/love-letter/src/CartasDeAmorFront
     python3 -m http.server 8080
     ```
   - Or use Node.js with `npx serve`:
     ```bash
     npx serve -p 8080
     ```

4. **Access the Application**
   - Open your browser and navigate to `http://localhost:8080`

## How to Test

### 1. User Registration/Login
1. Start with the registration tab
2. Fill in username, email, and password
3. Click "Register" to create an account
4. Or switch to "Login" tab if you already have an account

### 2. Room Management
1. After logging in, you'll see the lobby
2. **Create a Room**:
   - Enter a room name
   - Optionally set a password
   - Click "Create Room"
3. **Join a Room**:
   - Enter a Room ID in the "Join Room by ID" section
   - Click "Join Room"

### 3. Game Testing
1. As a room host, click "Start Game" to begin
2. You'll be redirected to the game screen
3. Your card will be displayed
4. Use the card buttons to play cards
5. Game events will appear in the game log

### 4. SignalR Testing
- Real-time events are logged in the game log
- Multiple browser tabs can be used to simulate multiple players
- Connection status is shown via status messages

## API Endpoints Tested

### Account Controller
- `POST /api/account/create` - User registration
- `POST /api/account/login` - User authentication
- `DELETE /api/account/{email}` - Account deletion (can be tested via browser dev tools)

### GameRoom Controller
- `POST /api/gameroom` - Create room
- `DELETE /api/gameroom/{roomId}` - Delete room

### SignalR Hub Methods
- `JoinRoom(roomId)` - Join a game room
- `LeaveRoom(roomId)` - Leave a game room
- `StartGame(roomId)` - Start the game
- `PlayCard(roomId, cardType)` - Play a card
- `ReconnectToRoom(roomId)` - Reconnect to room

## Troubleshooting

### Common Issues

1. **CORS Errors**
   - Ensure your backend has CORS configured for your frontend domain
   - Check browser console for CORS-related errors

2. **SignalR Connection Failures**
   - Verify the Hub URL is correct
   - Check that JWT authentication is working
   - Ensure the backend SignalR hub is properly configured

3. **Authentication Issues**
   - Check if JWT tokens are being properly sent in headers
   - Verify token expiration
   - Clear localStorage and try logging in again

4. **Network Errors**
   - Verify backend server is running
   - Check API URLs in `script.js`
   - Use browser dev tools to inspect network requests

### Browser Developer Tools
- Open F12 Developer Tools
- Check Console tab for errors
- Check Network tab to see API requests/responses
- Check Application tab to view stored tokens

## File Structure

```
CartasDeAmorFront/
├── index.html          # Main HTML structure
├── styles.css          # Styling and responsive design
├── script.js           # JavaScript functionality and API integration
└── README.md          # This documentation
```

## Browser Compatibility

This frontend is compatible with modern browsers that support:
- ES6+ JavaScript features
- CSS Grid and Flexbox
- SignalR JavaScript client
- Local Storage API
- Fetch API

Tested browsers:
- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+
