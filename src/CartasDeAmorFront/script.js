// Configuration
const API_BASE_URL = 'http://localhost:5149/api'; // Adjust this to match your backend URL
const HUB_URL = 'http://localhost:5149/gameHub'; // Adjust this to match your backend URL

// Global variables
let accessToken = null;
let currentUser = null;
let currentRoom = null;
let signalRConnection = null;

// Card types mapping
const CARD_TYPES = {
    0: 'Spy',
    1: 'Guard', 
    2: 'Priest',
    3: 'Baron',
    4: 'Servant',
    5: 'Prince',
    6: 'Chanceller',
    7: 'King',
    8: 'Countess',
    9: 'Princess'
};

// DOM Elements
const authSection = document.getElementById('auth-section');
const lobbySection = document.getElementById('lobby-section');
const gameSection = document.getElementById('game-section');
const userInfo = document.getElementById('user-info');
const usernameDisplay = document.getElementById('username-display');
const statusMessages = document.getElementById('status-messages');

// Initialize app
document.addEventListener('DOMContentLoaded', function() {
    initializeApp();
    setupEventListeners();
    
    // Check if user is already logged in
    const savedToken = localStorage.getItem('accessToken');
    const savedUser = localStorage.getItem('currentUser');
    
    if (savedToken && savedUser) {
        accessToken = savedToken;
        currentUser = JSON.parse(savedUser);
        showLobby();
    }
});

function initializeApp() {
    console.log('Initializing Love Letter Game Frontend');
}

function setupEventListeners() {
    // Auth tab switching
    document.getElementById('login-tab').addEventListener('click', () => switchAuthTab('login'));
    document.getElementById('register-tab').addEventListener('click', () => switchAuthTab('register'));
    
    // Auth forms
    document.getElementById('login-form-element').addEventListener('submit', handleLogin);
    document.getElementById('register-form-element').addEventListener('submit', handleRegister);
    
    // Logout
    document.getElementById('logout-btn').addEventListener('click', handleLogout);
    
    // Room management
    document.getElementById('create-room-form').addEventListener('submit', handleCreateRoom);
    document.getElementById('join-room-form').addEventListener('submit', handleJoinRoom);
    document.getElementById('leave-room-btn').addEventListener('click', handleLeaveRoom);
    document.getElementById('delete-room-btn').addEventListener('click', handleDeleteRoom);
    
    // Game controls
    document.getElementById('start-game-btn').addEventListener('click', handleStartGame);
    document.getElementById('back-to-lobby-btn').addEventListener('click', handleBackToLobby);
}

// Auth functions
function switchAuthTab(tab) {
    const loginTab = document.getElementById('login-tab');
    const registerTab = document.getElementById('register-tab');
    const loginForm = document.getElementById('login-form');
    const registerForm = document.getElementById('register-form');
    
    if (tab === 'login') {
        loginTab.classList.add('active');
        registerTab.classList.remove('active');
        loginForm.style.display = 'block';
        registerForm.style.display = 'none';
    } else {
        registerTab.classList.add('active');
        loginTab.classList.remove('active');
        registerForm.style.display = 'block';
        loginForm.style.display = 'none';
    }
}

async function handleLogin(e) {
    e.preventDefault();
    
    const username = document.getElementById('login-username').value;
    const password = document.getElementById('login-password').value;
    
    try {
        showLoading(e.target);
        
        const response = await fetch(`${API_BASE_URL}/account/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                username: username,
                password: password
            })
        });
        
        if (response.ok) {
            const data = await response.json();
            accessToken = data.accessToken;
            currentUser = { username: username };
            
            // Debug: decode JWT token to see claims
            try {
                const tokenParts = accessToken.split('.');
                if (tokenParts.length === 3) {
                    const payload = JSON.parse(atob(tokenParts[1]));
                    console.log('JWT Token payload:', payload);
                }
            } catch (e) {
                console.error('Failed to decode JWT token:', e);
            }
            
            // Save to localStorage
            localStorage.setItem('accessToken', accessToken);
            localStorage.setItem('currentUser', JSON.stringify(currentUser));
            
            showMessage('Login successful!', 'success');
            await showLobby();
        } else {
            const error = await response.json();
            showMessage(error.message || 'Login failed', 'error');
        }
    } catch (error) {
        console.error('Login error:', error);
        showMessage('Network error during login', 'error');
    } finally {
        hideLoading(e.target);
    }
}

async function handleRegister(e) {
    e.preventDefault();
    
    const username = document.getElementById('register-username').value;
    const email = document.getElementById('register-email').value;
    const password = document.getElementById('register-password').value;
    
    try {
        showLoading(e.target);
        
        const response = await fetch(`${API_BASE_URL}/account/create`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                username: username,
                email: email,
                password: password
            })
        });
        
        if (response.ok) {
            const data = await response.json();
            accessToken = data.accessToken;
            currentUser = { username: username, email: email };
            
            // Save to localStorage
            localStorage.setItem('accessToken', accessToken);
            localStorage.setItem('currentUser', JSON.stringify(currentUser));
            
            showMessage('Account created successfully!', 'success');
            showLobby();
        } else {
            const error = await response.json();
            showMessage(error.message || 'Registration failed', 'error');
        }
    } catch (error) {
        console.error('Registration error:', error);
        showMessage('Network error during registration', 'error');
    } finally {
        hideLoading(e.target);
    }
}

function handleLogout() {
    accessToken = null;
    currentUser = null;
    currentRoom = null;
    
    // Clear localStorage
    localStorage.removeItem('accessToken');
    localStorage.removeItem('currentUser');
    
    // Disconnect SignalR
    if (signalRConnection) {
        signalRConnection.stop();
        signalRConnection = null;
    }
    
    showMessage('Logged out successfully', 'info');
    showAuth();
}

// Room management functions
async function handleCreateRoom(e) {
    e.preventDefault();
    
    const roomName = document.getElementById('room-name').value;
    const password = document.getElementById('room-password').value;
    
    try {
        showLoading(e.target);
        
        const response = await fetch(`${API_BASE_URL}/gameroom`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`
            },
            body: JSON.stringify({
                roomName: roomName,
                password: password || null
            })
        });
        
        if (response.ok) {
            const roomId = await response.json();
            currentRoom = {
                id: roomId,
                name: roomName,
                isHost: true
            };
            
            showMessage('Room created successfully!', 'success');
            updateRoomDisplay();
            await connectToSignalR();
            await joinSignalRRoom(roomId);
            
            // Clear form
            document.getElementById('create-room-form').reset();
        } else {
            const error = await response.json();
            showMessage(error.message || 'Failed to create room', 'error');
        }
    } catch (error) {
        console.error('Create room error:', error);
        showMessage('Network error while creating room', 'error');
    } finally {
        hideLoading(e.target);
    }
}

async function handleJoinRoom(e) {
    e.preventDefault();
    
    const roomId = document.getElementById('join-room-id').value;
    
    try {
        showLoading(e.target);
        
        // For now, we'll just connect to SignalR and join the room
        // In a real implementation, you might want to validate the room exists first
        currentRoom = {
            id: roomId,
            name: 'Joined Room',
            isHost: false
        };
        
        await connectToSignalR();
        await joinSignalRRoom(roomId);
        
        showMessage('Joined room successfully!', 'success');
        updateRoomDisplay();
        
        // Clear form
        document.getElementById('join-room-form').reset();
    } catch (error) {
        console.error('Join room error:', error);
        showMessage('Failed to join room', 'error');
    } finally {
        hideLoading(e.target);
    }
}

async function handleLeaveRoom() {
    if (!currentRoom) return;
    
    try {
        if (signalRConnection) {
            await signalRConnection.invoke('LeaveRoom', currentRoom.id);
        }
        
        currentRoom = null;
        updateRoomDisplay();
        showMessage('Left room successfully', 'info');
    } catch (error) {
        console.error('Leave room error:', error);
        showMessage('Error leaving room', 'error');
    }
}

async function handleDeleteRoom() {
    if (!currentRoom || !currentRoom.isHost) return;
    
    if (!confirm('Are you sure you want to delete this room?')) return;
    
    try {
        const response = await fetch(`${API_BASE_URL}/gameroom/${currentRoom.id}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${accessToken}`
            }
        });
        
        if (response.ok) {
            currentRoom = null;
            updateRoomDisplay();
            showMessage('Room deleted successfully', 'success');
        } else {
            const error = await response.json();
            showMessage(error.message || 'Failed to delete room', 'error');
        }
    } catch (error) {
        console.error('Delete room error:', error);
        showMessage('Network error while deleting room', 'error');
    }
}

// Game functions
async function handleStartGame() {
    if (!currentRoom || !currentRoom.isHost) {
        showMessage('Only the room host can start the game', 'error');
        return;
    }
    
    try {
        // Ensure SignalR connection is established
        if (!signalRConnection || signalRConnection.state !== signalR.HubConnectionState.Connected) {
            await connectToSignalR();
        }
        
        if (signalRConnection && signalRConnection.state === signalR.HubConnectionState.Connected) {
            await signalRConnection.invoke('StartGame', currentRoom.id);
            showMessage('Starting game...', 'info');
        } else {
            throw new Error('Could not establish SignalR connection');
        }
    } catch (error) {
        console.error('Start game error:', error);
        showMessage(`Failed to start game: ${error.message}`, 'error');
    }
}

function handleBackToLobby() {
    showLobby();
    clearGameLog();
}

async function handlePlayCard(cardType) {
    if (!currentRoom) return;
    
    try {
        if (signalRConnection) {
            await signalRConnection.invoke('PlayCard', currentRoom.id, parseInt(cardType));
            addGameMessage(`You played: ${CARD_TYPES[cardType]}`, 'info');
        }
    } catch (error) {
        console.error('Play card error:', error);
        showMessage('Failed to play card', 'error');
    }
}

// SignalR functions
async function connectToSignalR() {
    if (signalRConnection && signalRConnection.state === signalR.HubConnectionState.Connected) {
        return;
    }
    
    if (!accessToken) {
        throw new Error('No access token available');
    }
    
    // Debug: log token (first 50 chars for security)
    console.log('Connecting to SignalR with token:', accessToken.substring(0, 50) + '...');
    
    try {
        signalRConnection = new signalR.HubConnectionBuilder()
            .withUrl(`${HUB_URL}?access_token=${accessToken}`)
            .configureLogging(signalR.LogLevel.Debug) // Enable SignalR debug logging
            .build();
        
        // Set up event handlers
        setupSignalRHandlers();
        
        await signalRConnection.start();
        console.log('SignalR Connected successfully');
        showMessage('Connected to game server', 'success');
    } catch (error) {
        console.error('SignalR connection error:', error);
        showMessage('Failed to connect to game server', 'error');
        throw error;
    }
}

function setupSignalRHandlers() {
    if (!signalRConnection) return;
    
    signalRConnection.on('GameStarted', () => {
        showMessage('Game started!', 'success');
        showGame();
        addGameMessage('Game has started!', 'success');
    });
    
    signalRConnection.on('PlayerCard', (card) => {
        console.log('Received player card:', card);
        updatePlayerCard(card);
        addGameMessage(`Your card: ${CARD_TYPES[card]}`, 'info');
    });
    
    signalRConnection.on('GameStartError', (error) => {
        console.error('Game start error from server:', error);
        showMessage(`Failed to start game: ${error}`, 'error');
    });
    
    signalRConnection.on('UserNotAuthenticated', () => {
        console.error('User not authenticated on SignalR');
        showMessage('Authentication failed. Please login again.', 'error');
        // Force logout and redirect to login
        handleLogout();
    });
    
    signalRConnection.onclose(() => {
        console.log('SignalR Disconnected');
        showMessage('Disconnected from game server', 'error');
    });
}

async function joinSignalRRoom(roomId) {
    if (signalRConnection && signalRConnection.state === signalR.HubConnectionState.Connected) {
        try {
            await signalRConnection.invoke('JoinRoom', roomId);
            console.log(`Joined room: ${roomId}`);
        } catch (error) {
            console.error('Error joining room:', error);
            throw error;
        }
    }
}

// UI functions
function showAuth() {
    authSection.style.display = 'block';
    lobbySection.style.display = 'none';
    gameSection.style.display = 'none';
    userInfo.style.display = 'none';
}

async function showLobby() {
    authSection.style.display = 'none';
    lobbySection.style.display = 'block';
    gameSection.style.display = 'none';
    userInfo.style.display = 'flex';
    
    if (currentUser) {
        usernameDisplay.textContent = currentUser.username;
    }
    
    updateRoomDisplay();
    
    // Establish SignalR connection when entering lobby
    try {
        await connectToSignalR();
    } catch (error) {
        console.error('Failed to connect to SignalR on lobby entry:', error);
        // Don't show error message here as it might be confusing to user
    }
}

function showGame() {
    authSection.style.display = 'none';
    lobbySection.style.display = 'none';
    gameSection.style.display = 'block';
    userInfo.style.display = 'flex';
    
    if (currentRoom) {
        document.getElementById('game-room-name').textContent = currentRoom.name;
    }
    
    generateCardButtons();
}

function updateRoomDisplay() {
    const currentRoomDiv = document.getElementById('current-room');
    
    if (currentRoom) {
        currentRoomDiv.style.display = 'block';
        document.getElementById('current-room-name').textContent = currentRoom.name;
        document.getElementById('current-room-id').textContent = currentRoom.id;
        
        // Show/hide buttons based on host status
        document.getElementById('start-game-btn').style.display = currentRoom.isHost ? 'inline-block' : 'none';
        document.getElementById('delete-room-btn').style.display = currentRoom.isHost ? 'inline-block' : 'none';
    } else {
        currentRoomDiv.style.display = 'none';
    }
}

function updatePlayerCard(cardType) {
    const playerCardElement = document.getElementById('player-card');
    if (playerCardElement) {
        playerCardElement.textContent = CARD_TYPES[cardType] || 'Unknown';
    }
}

function generateCardButtons() {
    const cardButtonsContainer = document.getElementById('card-buttons');
    cardButtonsContainer.innerHTML = '';
    
    // Generate buttons for all card types (in a real game, this would be based on the player's hand)
    Object.entries(CARD_TYPES).forEach(([value, name]) => {
        const button = document.createElement('button');
        button.className = 'card-btn';
        button.textContent = name;
        button.onclick = () => handlePlayCard(value);
        cardButtonsContainer.appendChild(button);
    });
}

function addGameMessage(message, type = 'info') {
    const messagesContainer = document.getElementById('game-messages');
    const messageElement = document.createElement('div');
    messageElement.className = `message ${type}`;
    messageElement.textContent = `${new Date().toLocaleTimeString()}: ${message}`;
    messagesContainer.appendChild(messageElement);
    messagesContainer.scrollTop = messagesContainer.scrollHeight;
}

function clearGameLog() {
    const messagesContainer = document.getElementById('game-messages');
    if (messagesContainer) {
        messagesContainer.innerHTML = '';
    }
}

function showMessage(message, type = 'info') {
    const messageElement = document.createElement('div');
    messageElement.className = `status-message ${type}`;
    messageElement.textContent = message;
    
    statusMessages.appendChild(messageElement);
    
    // Auto-remove after 5 seconds
    setTimeout(() => {
        if (messageElement.parentNode) {
            messageElement.parentNode.removeChild(messageElement);
        }
    }, 5000);
}

function showLoading(element) {
    element.classList.add('loading');
}

function hideLoading(element) {
    element.classList.remove('loading');
}

// Utility functions
function formatGuid(guid) {
    return guid.substring(0, 8);
}

// Error handling
window.addEventListener('error', (event) => {
    console.error('Global error:', event.error);
    showMessage('An unexpected error occurred', 'error');
});

// Handle page reload/close
window.addEventListener('beforeunload', () => {
    if (signalRConnection) {
        signalRConnection.stop();
    }
});
