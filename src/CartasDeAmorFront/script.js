// Configuration
const API_BASE_URL = 'http://localhost:5149/api'; // Adjust this to match your backend URL
const HUB_URL = 'http://localhost:5149/gameHub'; // Adjust this to match your backend URL

// Global variables
let accessToken = null;
let currentUser = null;
let currentRoom = null;
let signalRConnection = null;
let playerCards = []; // Store current player's cards

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
    document.getElementById('copy-room-id-btn').addEventListener('click', handleCopyRoomId);
    
    // Game controls
    document.getElementById('start-game-btn').addEventListener('click', handleStartGame);
    document.getElementById('back-to-lobby-btn').addEventListener('click', handleBackToLobby);
    document.getElementById('draw-card-btn').addEventListener('click', handleDrawCard);
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
    playerCards = []; // Clear player's cards
    
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

async function handleCopyRoomId() {
    if (!currentRoom) return;
    
    try {
        await navigator.clipboard.writeText(currentRoom.id);
        showMessage('Room ID copied to clipboard!', 'success');
        
        // Visual feedback: briefly change button text
        const button = document.getElementById('copy-room-id-btn');
        const originalText = button.textContent;
        button.textContent = '✓';
        setTimeout(() => {
            button.textContent = originalText;
        }, 1500);
    } catch (error) {
        console.error('Failed to copy room ID:', error);
        
        // Fallback for browsers that don't support clipboard API
        const textArea = document.createElement('textarea');
        textArea.value = currentRoom.id;
        document.body.appendChild(textArea);
        textArea.select();
        try {
            document.execCommand('copy');
            showMessage('Room ID copied to clipboard!', 'success');
        } catch (fallbackError) {
            console.error('Fallback copy failed:', fallbackError);
            showMessage('Failed to copy room ID. Please copy manually: ' + currentRoom.id, 'error');
        }
        document.body.removeChild(textArea);
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
    playerCards = []; // Clear player's cards when leaving game
    showLobby();
    clearGameLog();
}

async function handleDrawCard() {
    if (!currentRoom) {
        showMessage('No active room', 'error');
        return;
    }
    
    try {
        if (signalRConnection && signalRConnection.state === signalR.HubConnectionState.Connected) {
            await signalRConnection.invoke('DrawCard', currentRoom.id);
            addGameMessage('Attempting to draw a card...', 'info');
        } else {
            showMessage('Not connected to game server', 'error');
        }
    } catch (error) {
        console.error('Draw card error:', error);
        showMessage('Failed to draw card', 'error');
    }
}

async function handlePlayCard(cardType) {
    if (!currentRoom) return;
    
    // Check if player has this card
    const cardTypeInt = parseInt(cardType);
    if (!playerCards.includes(cardTypeInt)) {
        showMessage("You don't have that card", 'error');
        return;
    }
    
    try {
        if (signalRConnection) {
            await signalRConnection.invoke('PlayCard', currentRoom.id, cardTypeInt);
            addGameMessage(`You attempted to play: ${CARD_TYPES[cardTypeInt]}`, 'info');
            
            // Optimistically remove the card from hand (will be corrected by server response if needed)
            const cardIndex = playerCards.indexOf(cardTypeInt);
            if (cardIndex > -1) {
                playerCards.splice(cardIndex, 1);
                generateCardButtons(); // Update the UI
            }
        }
    } catch (error) {
        console.error('Play card error:', error);
        showMessage('Failed to play card', 'error');
    }
}

function handleCardInputRequest(cardType, requirements) {
    console.log('Card requires input:', cardType, requirements);
    
    const cardName = CARD_TYPES[cardType];
    const requirementNames = requirements.map(req => {
        switch(req) {
            case 1: return 'Select Player';
            case 2: return 'Select Card Type';
            case 3: return 'Select Card';
            case 4: return 'Draw Card';
            default: return 'Unknown';
        }
    });
    
    showCardInputModal(cardType, requirements, cardName, requirementNames);
}

function updateGameStatus(gameStatus) {
    console.log('Updating game status:', gameStatus);
    
    // Extract properties from InitialGameStatusDto
    const playersStatus = gameStatus.OtherPlayersPublicData || gameStatus.otherPlayersPublicData || [];
    const yourCards = gameStatus.YourCards || gameStatus.yourCards || [];
    const isProtected = gameStatus.IsProtected !== undefined ? gameStatus.IsProtected : gameStatus.isProtected;
    const firstPlayerIndex = gameStatus.FirstPlayerIndex !== undefined ? gameStatus.FirstPlayerIndex : gameStatus.firstPlayerIndex;
    const currentPlayerScore = gameStatus.Score !== undefined ? gameStatus.Score : gameStatus.score;
    const players = gameStatus.AllPlayersInOrder || gameStatus.allPlayersInOrder || [];
    
    // Update player's cards (now it's an array of cards)
    if (yourCards && yourCards.length > 0) {
        console.log('Your cards:', yourCards);
        playerCards = yourCards; // Store cards globally
        
        // If player has multiple cards, show the first one in the UI
        // In a complete implementation, you'd display all cards
        updatePlayerCard(yourCards[0]);
        
        // Display all cards in hand
        const cardNames = yourCards.map(cardType => CARD_TYPES[cardType] || 'Unknown');
        addGameMessage(`Your cards: ${cardNames.join(', ')}`, 'info');
        
        // Update card buttons to show only playable cards
        generateCardButtons();
    } else if (gameStatus.YourCards !== undefined || gameStatus.yourCards !== undefined) {
        // Cards array was provided but empty - player has no cards
        playerCards = [];
        addGameMessage('No cards in hand', 'info');
        generateCardButtons();
    }
    
    // Update protection status
    if (isProtected !== undefined) {
        const protectionMsg = isProtected ? 'You are protected this turn' : 'Your protection has ended';
        if (isProtected) {
            addGameMessage(protectionMsg, 'success');
        }
    }
    
    // Update current player's score
    if (currentPlayerScore !== undefined) {
        addGameMessage(`Your score: ${currentPlayerScore}`, 'info');
    }
    
    // Update player list display
    if (playersStatus && playersStatus.length > 0) {
        console.log('Other players:', playersStatus);
        currentGamePlayers = playersStatus; // Store players for modal use
        updatePlayersList(playersStatus);
    }

    console.log('First player index:', firstPlayerIndex);
    console.log('All players:', players);
    
    // Show whose turn it is
    let isMyTurn = false;
    if (players && players.length > 0 && firstPlayerIndex !== undefined) {
        if (firstPlayerIndex >= 0 && firstPlayerIndex < players.length) {
            const currentPlayer = players[firstPlayerIndex];
            // Find the username of the current player using playersStatus
            let username = '';
            const playerObj = playersStatus.find(p => (p.UserEmail || p.userEmail) === currentPlayer);
            if (playerObj) {
                username = playerObj.Username || playerObj.username || '';
                addGameMessage(`It's ${username}'s turn`, 'info');
            } else {
                isMyTurn = true;
                addGameMessage("It's your turn!", 'success');
            }
        }
    }

    updateDrawCardButton(isMyTurn);
}

function updatePlayersList(players) {
    // For now, just log the players. In a full implementation, you would update the UI
    console.log('Players in game:', players);
    
    let playersInfo = players.map(p => {
        // Handle both camelCase and PascalCase property names
        const username = p.Username || p.username;
        const cardsInHand = p.CardsInHand !== undefined ? p.CardsInHand : p.cardsInHand;
        const score = p.Score !== undefined ? p.Score : p.score;
        const isProtected = p.IsProtected !== undefined ? p.IsProtected : p.isProtected;
        
        return `${username} (Cards: ${cardsInHand}, Score: ${score}${isProtected ? ', Protected' : ''})`;
    }).join(', ');
    
    addGameMessage(`Players: ${playersInfo}`, 'info');
}

// Modal functions
let currentGamePlayers = []; // Store current game players for modal use

function showCardInputModal(cardType, requirements, cardName, requirementNames) {
    const modal = document.getElementById('card-input-modal');
    const modalTitle = document.getElementById('modal-title');
    const modalDescription = document.getElementById('modal-description');
    const inputContainer = document.getElementById('input-container');
    const cardForm = document.getElementById('card-input-form');
    
    // Set modal title and description
    modalTitle.textContent = `${cardName} Card Action`;
    modalDescription.textContent = `${cardName} requires: ${requirementNames.join(', ')}`;
    
    // Clear previous inputs
    inputContainer.innerHTML = '';
    
    // Create inputs based on requirements
    requirements.forEach((requirement, index) => {
        const inputGroup = document.createElement('div');
        inputGroup.className = 'input-group';
        
        switch(requirement) {
            case 1: // Select Player
                createPlayerSelector(inputGroup, index);
                break;
            case 2: // Select Card Type
                createCardTypeSelector(inputGroup, index);
                break;
            case 3: // Select Card
                createCardSelector(inputGroup, index);
                break;
            case 4: // Draw Card (no input needed)
                createInfoDisplay(inputGroup, 'You will draw a card', 'draw-card');
                break;
            default:
                createInfoDisplay(inputGroup, 'Unknown requirement', 'unknown');
                break;
        }
        
        inputContainer.appendChild(inputGroup);
    });
    
    // Show modal
    modal.style.display = 'flex';
    
    // Handle form submission
    cardForm.onsubmit = (e) => {
        e.preventDefault();
        handleModalSubmit(cardType, requirements);
    };
    
    // Setup modal close handlers
    setupModalCloseHandlers();
}

function createPlayerSelector(container, index) {
    const label = document.createElement('label');
    label.textContent = 'Select a player:';
    container.appendChild(label);
    
    const playersDiv = document.createElement('div');
    playersDiv.className = 'players-selection';
    
    // Get players from the last game status (excluding yourself)
    const availablePlayers = currentGamePlayers.filter(p => {
        const userEmail = p.UserEmail || p.userEmail;
        return userEmail !== (currentUser?.email || currentUser?.username);
    });
    
    if (availablePlayers.length === 0) {
        const noPlayersMsg = document.createElement('p');
        noPlayersMsg.textContent = 'No other players available';
        noPlayersMsg.style.color = '#999';
        playersDiv.appendChild(noPlayersMsg);
    } else {
        availablePlayers.forEach((player, playerIndex) => {
            const playerOption = document.createElement('div');
            playerOption.className = 'player-option';
            
            const radio = document.createElement('input');
            radio.type = 'radio';
            radio.name = `player-select-${index}`;
            radio.value = player.UserEmail || player.userEmail;
            radio.id = `player-${index}-${playerIndex}`;
            
            const labelText = document.createElement('label');
            labelText.htmlFor = `player-${index}-${playerIndex}`;
            
            // Handle both camelCase and PascalCase property names
            const username = player.Username || player.username;
            const isProtected = player.IsProtected !== undefined ? player.IsProtected : player.isProtected;
            labelText.textContent = `${username}${isProtected ? ' (Protected)' : ''}`;
            
            playerOption.appendChild(radio);
            playerOption.appendChild(labelText);
            
            // Make the whole div clickable
            playerOption.addEventListener('click', () => {
                radio.checked = true;
                // Remove selected class from all options
                playersDiv.querySelectorAll('.player-option').forEach(opt => 
                    opt.classList.remove('selected'));
                // Add selected class to clicked option
                playerOption.classList.add('selected');
            });
            
            playersDiv.appendChild(playerOption);
        });
    }
    
    container.appendChild(playersDiv);
}

function createCardTypeSelector(container, index) {
    const label = document.createElement('label');
    label.textContent = 'Select a card type:';
    container.appendChild(label);
    
    const select = document.createElement('select');
    select.name = `card-type-${index}`;
    select.required = true;
    
    // Add default option
    const defaultOption = document.createElement('option');
    defaultOption.value = '';
    defaultOption.textContent = 'Choose a card type...';
    select.appendChild(defaultOption);
    
    // Add card type options (excluding Spy which is 0)
    Object.entries(CARD_TYPES).forEach(([value, name]) => {
        if (parseInt(value) > 0) { // Exclude Spy
            const option = document.createElement('option');
            option.value = value;
            option.textContent = name;
            select.appendChild(option);
        }
    });
    
    container.appendChild(select);
}

function createCardSelector(container, index) {
    const label = document.createElement('label');
    label.textContent = 'Select a card:';
    container.appendChild(label);
    
    const select = document.createElement('select');
    select.name = `card-${index}`;
    select.required = true;
    
    // Add default option
    const defaultOption = document.createElement('option');
    defaultOption.value = '';
    defaultOption.textContent = 'Choose a card...';
    select.appendChild(defaultOption);
    
    // Add all card options
    Object.entries(CARD_TYPES).forEach(([value, name]) => {
        const option = document.createElement('option');
        option.value = value;
        option.textContent = name;
        select.appendChild(option);
    });
    
    container.appendChild(select);
}

function createInfoDisplay(container, message, className) {
    const infoDiv = document.createElement('div');
    infoDiv.className = `info-display ${className}`;
    infoDiv.textContent = message;
    infoDiv.style.padding = '1rem';
    infoDiv.style.backgroundColor = '#f8f9ff';
    infoDiv.style.border = '2px solid #667eea';
    infoDiv.style.borderRadius = '6px';
    infoDiv.style.color = '#667eea';
    infoDiv.style.textAlign = 'center';
    container.appendChild(infoDiv);
}

function handleModalSubmit(cardType, requirements) {
    const inputs = [];
    const inputContainer = document.getElementById('input-container');
    let hasError = false;
    
    requirements.forEach((requirement, index) => {
        switch(requirement) {
            case 1: // Select Player
                const selectedPlayer = inputContainer.querySelector(`input[name="player-select-${index}"]:checked`);
                if (selectedPlayer) {
                    inputs.push(selectedPlayer.value);
                } else {
                    showMessage('Please select a player', 'error');
                    hasError = true;
                }
                break;
            case 2: // Select Card Type
                const cardTypeSelect = inputContainer.querySelector(`select[name="card-type-${index}"]`);
                if (cardTypeSelect && cardTypeSelect.value) {
                    inputs.push(parseInt(cardTypeSelect.value));
                } else {
                    showMessage('Please select a card type', 'error');
                    hasError = true;
                }
                break;
            case 3: // Select Card
                const cardSelect = inputContainer.querySelector(`select[name="card-${index}"]`);
                if (cardSelect && cardSelect.value) {
                    inputs.push(parseInt(cardSelect.value));
                } else {
                    showMessage('Please select a card', 'error');
                    hasError = true;
                }
                break;
            case 4: // Draw Card (no input needed)
                inputs.push(null); // Placeholder for draw card action
                break;
            default:
                inputs.push(null);
                break;
        }
    });
    
    // Return early if there are validation errors
    if (hasError) {
        return;
    }
    
    // Close modal
    hideCardInputModal();
    
    // Send the card input to the server
    if (signalRConnection) {
        signalRConnection.invoke('InformCardInput', currentRoom.id, cardType, inputs)
            .then(() => {
                showMessage(`${CARD_TYPES[cardType]} played with selected inputs`, 'success');
            })
            .catch(error => {
                console.error('Error sending card input:', error);
                showMessage('Failed to send card input', 'error');
            });
    }
}

function setupModalCloseHandlers() {
    const modal = document.getElementById('card-input-modal');
    const closeBtn = document.getElementById('modal-close-btn');
    const cancelBtn = document.getElementById('modal-cancel-btn');
    
    // Close button handler
    closeBtn.onclick = hideCardInputModal;
    
    // Cancel button handler
    cancelBtn.onclick = hideCardInputModal;
    
    // Click outside modal to close
    modal.onclick = (e) => {
        if (e.target === modal) {
            hideCardInputModal();
        }
    };
    
    // Escape key to close
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && modal.style.display === 'flex') {
            hideCardInputModal();
        }
    });
}

function hideCardInputModal() {
    const modal = document.getElementById('card-input-modal');
    modal.style.display = 'none';
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
    
    signalRConnection.on('GameStarted', (gameStatus) => {
        playerCards = []; // Clear old cards when a new game starts
        showMessage('Game started!', 'success');
        showGame();
        addGameMessage('Game has started!', 'success');
        
        // Process the initial game status data received from server
        if (gameStatus) {
            console.log('Received initial game status:', gameStatus);
            updateGameStatus(gameStatus);
        }
    });
    
    signalRConnection.on('PlayerCard', (card) => {
        console.log('Received player card:', card);
        // Add the card to player's hand if not already present
        if (!playerCards.includes(card)) {
            playerCards.push(card);
        }
        updatePlayerCard(card);
        addGameMessage(`Your card: ${CARD_TYPES[card]}`, 'info');
        // Update the card buttons to reflect the new hand
        generateCardButtons();
    });
    
    signalRConnection.on('GameStartError', (error) => {
        console.error('Game start error from server:', error);
        showMessage(`Failed to start game: ${error}`, 'error');
    });
    
    signalRConnection.on('RequestCardInput', (cardType, requirements) => {
        console.log('Card requires additional input:', cardType, requirements);
        handleCardInputRequest(cardType, requirements);
    });
    
    signalRConnection.on('CardPlayed', (userEmail, cardType) => {
        console.log('Card played by:', userEmail, 'Card:', cardType);
        addGameMessage(`${userEmail} played: ${CARD_TYPES[cardType]}`, 'info');
    });
    
    signalRConnection.on('GameStatusUpdated', (gameStatus) => {
        console.log('Game status updated:', gameStatus);
        updateGameStatus(gameStatus);
    });
    
    signalRConnection.on('PlayCardError', (error) => {
        console.error('Play card error from server:', error);
        showMessage(`Card play failed: ${error}`, 'error');
    });
    
    signalRConnection.on('CardDrawn', (drawnCard) => {
        console.log('Card drawn:', drawnCard);
        // Add the drawn card to player's hand
        if (!playerCards.includes(drawnCard)) {
            playerCards.push(drawnCard);
        }
        updatePlayerCard(drawnCard);
        addGameMessage(`You drew: ${CARD_TYPES[drawnCard]}`, 'success');
        generateCardButtons(); // Update the UI
    });
    
    signalRConnection.on('PlayerDrewCard', (userEmail) => {
        console.log('Player drew card:', userEmail);
        addGameMessage(`${userEmail} drew a card`, 'info');
    });
    
    signalRConnection.on('DrawCardError', (error) => {
        console.error('Draw card error from server:', error);
        showMessage(`Draw card failed: ${error}`, 'error');
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
    // Initialize draw card button as disabled until we know whose turn it is
    updateDrawCardButton(false);
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

function updateDrawCardButton(isMyTurn) {
    const drawCardBtn = document.getElementById('draw-card-btn');
    if (drawCardBtn) {
        drawCardBtn.disabled = !isMyTurn;
        if (isMyTurn) {
            drawCardBtn.textContent = 'Draw Card';
            drawCardBtn.title = 'Click to draw a new card';
        } else {
            drawCardBtn.textContent = 'Draw Card (Not Your Turn)';
            drawCardBtn.title = 'Wait for your turn to draw a card';
        }
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
    
    // Generate buttons based on the player's actual cards
    if (playerCards && playerCards.length > 0) {
        playerCards.forEach((cardType, index) => {
            const button = document.createElement('button');
            button.className = 'card-btn';
            button.textContent = CARD_TYPES[cardType] || 'Unknown';
            button.onclick = () => handlePlayCard(cardType);
            cardButtonsContainer.appendChild(button);
        });
    } else {
        // Show message if no cards available
        const messageDiv = document.createElement('div');
        messageDiv.className = 'no-cards-message';
        messageDiv.textContent = 'No cards in hand';
        messageDiv.style.padding = '1rem';
        messageDiv.style.color = '#666';
        messageDiv.style.textAlign = 'center';
        cardButtonsContainer.appendChild(messageDiv);
    }
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
