# Love Letter Game: Use Cases

This document provides a comprehensive overview of all the use cases (actions and operations) that users can perform in the Love Letter game application.

## Table of Contents

- [Player Account Management](#player-account-management)
- [Game Room Management](#game-room-management)
- [Game Session Management](#game-session-management)
- [Game Actions](#game-actions)
- [Card Effects](#card-effects)
- [End-Game Conditions](#end-game-conditions)

## Player Account Management

### Create Account

**Description**: Players can create a new account to access the game.

**Flow**:
1. Player enters username, email, and password
2. System validates the data
3. System creates a new user account
4. System generates and returns an access token
5. Player is now authenticated and can access the game features

**Requirements**:
- Unique email address
- Valid password

**Error Cases**:
- 409 Conflict if the account already exists
- 500 Server Error for unexpected issues

### Login

**Description**: Existing players can authenticate with their credentials.

**Flow**:
1. Player enters email and password
2. System validates credentials
3. System generates an authentication token
4. Player is logged in and can access game features

**Requirements**:
- Registered email and correct password

**Error Cases**:
- 401 Unauthorized if credentials are invalid
- 500 Server Error for unexpected issues

### Delete Account

**Description**: Players can permanently remove their account from the system.

**Flow**:
1. Player requests account deletion
2. System verifies the player is authenticated
3. System deletes the account and associated data
4. Player receives confirmation

**Requirements**:
- Valid authentication token
- Only account owner can delete their account

**Error Cases**:
- 401 Unauthorized if not properly authenticated
- 403 Forbidden if trying to delete someone else's account
- 404 Not Found if account doesn't exist
- 500 Server Error for unexpected issues

## Game Room Management

### Create Room

**Description**: Players can create a new game room to host a game session.

**Flow**:
1. Player submits room name and optional password
2. System creates a new game room with the player as host
3. System returns the room ID
4. Player is automatically joined to the room

**Requirements**:
- Player must be authenticated
- Valid room name

**Error Cases**:
- 400 Bad Request for invalid input
- 500 Server Error for unexpected issues

### Join Room

**Description**: Players can join an existing game room.

**Flow**:
1. Player selects a room to join and enters password if required
2. System validates the player can join (room not full, game not in progress)
3. System adds the player to the room
4. Other players in the room are notified

**Requirements**:
- Valid room ID
- Room password if room is password-protected
- Room must not be full (max 6 players)
- Game must not be in progress

**Error Cases**:
- Room not found
- Incorrect password
- Room is full
- Game already in progress

### Leave Room

**Description**: Players can leave a game room they have joined.

**Flow**:
1. Player requests to leave a room
2. System removes player from the room
3. Other players in the room are notified
4. If host leaves, room ownership may transfer or room may be deleted

**Requirements**:
- Player must be in the room

**Error Cases**:
- Room not found
- Player not in the room

### Reconnect To Room

**Description**: Players can reconnect to a room after a temporary disconnection.

**Flow**:
1. Player requests to reconnect to a previously joined room
2. System verifies player was part of the room
3. System reconnects player to the room
4. Player resumes their game in progress if applicable

**Requirements**:
- Player must have been in the room before disconnection

**Error Cases**:
- Room not found
- Player was not previously in the room

## Game Session Management

### Start Game

**Description**: The host can start a game when enough players have joined.

**Flow**:
1. Host initiates the game start
2. System validates minimum player count (2+)
3. System initializes the game (shuffles deck, assigns cards)
4. Each player receives their initial game state
5. System announces the first player's turn

**Requirements**:
- Only the host can start the game
- At least 2 players must be in the room
- Maximum 6 players allowed

**Error Cases**:
- Not enough players
- Too many players
- Player is not the host

### Game State Transitions

**Description**: The game progresses through various states.

**States**:
1. **WaitingForPlayers**: Initial state, before the game starts
2. **WaitingForDraw**: At the beginning of a player's turn, waiting for card draw
3. **WaitingForPlay**: After drawing, waiting for card play
4. **Finished**: Game is completed

**Transitions**:
- WaitingForPlayers → WaitingForDraw (when game starts)
- WaitingForDraw → WaitingForPlay (after drawing a card)
- WaitingForPlay → WaitingForDraw (after playing a card, next player's turn)
- WaitingForPlay → Finished (when game ends)

## Game Actions

### Draw Card

**Description**: Players draw a card at the beginning of their turn.

**Flow**:
1. Player requests to draw a card
2. System validates it's the player's turn and in the correct state
3. System draws a card from the deck
4. Player receives the card in their hand
5. System transitions to WaitingForPlay state

**Requirements**:
- Must be the player's turn
- Game must be in WaitingForDraw state

**Error Cases**:
- Not the player's turn
- Wrong game state
- Deck is empty (triggers round end)

### Get Card Requirements

**Description**: Player checks what's required to play a specific card.

**Flow**:
1. Player queries the requirements for a card type
2. System returns requirements (target player selection, card type guess, etc.)
3. Player uses this information to make their play decision

**Requirements**:
- Player must have the card in hand
- Game must be in WaitingForPlay state

**Error Cases**:
- Invalid card type
- Not the player's turn

### Play Card

**Description**: Players play a card from their hand, triggering its effect.

**Flow**:
1. Player selects a card to play and provides required parameters
2. System validates the play is legal and requirements are met
3. System resolves the card's effect
4. All players are notified of the play and its result
5. If needed, system may request additional choices from the player
6. System advances to next player's turn or ends the round

**Requirements**:
- Must be the player's turn
- Game must be in WaitingForPlay state
- Card must be in player's hand
- Required parameters must be provided as needed (target player, card guess)

**Error Cases**:
- Not the player's turn
- Missing requirements for card
- Mandatory card play violation (e.g., must play Countess when holding King/Prince)

### Submit Card Choice

**Description**: After certain card effects (like Chancellor), players must choose which card to keep.

**Flow**:
1. Player receives multiple cards to choose from
2. Player submits which card to keep and which to return
3. System updates player's hand accordingly
4. System advances to next player's turn

**Requirements**:
- Must have previously played a card that triggered choices
- Must be the player's turn
- Must choose valid cards (ones they currently hold)

**Error Cases**:
- Invalid card selection
- Not in a choice state

## Card Effects

### Card Actions

The game revolves around 10 different card types, each with unique abilities:

1.  **Spy (0)**: If the player still has the Spy at the end of the round, they get a bonus point
    - Requires: none
    - Bonus point condition at end of round

2. **Guard (1)**: Guess another player's card; if correct, they're eliminated
   - Requires: target player, target card type guess
   
3. **Priest (2)**: Look at another player's hand
   - Requires: target player
   
4. **Baron (3)**: Compare hands with another player; lower value is eliminated
   - Requires: target player
   
5. **Handmaid (4)**: Player is protected until their next turn
   - Requires: none
   
6. **Prince (5)**: Force a player (including yourself) to discard their hand and draw a new card
   - Requires: target player
   - Special case: If target discards Princess, they're eliminated
   
7.  **Chancellor (6)**: Draw two additional cards and choose which to keep
   - Requires: none
   - Triggers card choice afterward
   -    
8. **King (7)**: Trade hands with another player
   - Requires: target player
   
9. **Countess (8)**: No effect, but must be played if holding King or Prince
   - Requires: none
   
10. **Princess (9)**: If played or discarded, player is eliminated
   - Requires: none
   

### Card Results Events

When a card is played, one of these result events is triggered:

1. **ShowCard**: Player shows their card to another player
2. **PlayerEliminated**: A player is eliminated from the round
3. **SwitchCards**: Two players swap cards
4. **DiscardAndDrawCard**: Player discards their hand and draws a new card
5. **ProtectionGranted**: Player is protected until their next turn
6. **ChooseCard**: Player must choose between multiple cards

## End-Game Conditions

### Round End Conditions

A round can end in several ways:

1. **Single Player Remaining**: All other players are eliminated
2. **Deck Depleted**: No more cards in the deck

**Flow**:
1. System determines round is over
2. System identifies round winner(s)
3. System awards points to winners
4. System awards bonus points (e.g., for holding the Spy)
5. If any player has reached target score, game ends; otherwise, new round begins

**Round Winner Determination**:
- Last player standing, or
- Highest card value among remaining players (ties possible)

### Game End Conditions

The game ends when a player reaches the target score, which depends on player count:
- 2 players: 6 points
- 3 players: 5 points
- 4 players: 4 points
- 5-6 players: 3 points

**Flow**:
1. System checks if any player has reached the target score
2. If yes, system declares game winner(s)

## Exceptional Flows

### Disconnection Handling

**Description**: System handles player disconnections gracefully.

**Flow**:
1. Player disconnects (intentionally or due to connection issues)
2. System marks player as disconnected but keeps them in game
3. Game can continue if enough players remain
4. Player can reconnect later and resume play

### Error Handling

The system provides several error events to handle exceptional situations:

1. **DrawCardError**: Issues when drawing a card
2. **GameStartError**: Problems when starting a game
3. **PlayCardError**: Issues when playing a card
4. **CardChoiceError**: Problems when submitting card choices
5. **MandatoryCardPlay**: When player must play a specific card
6. **UserNotAuthenticated**: Authentication failures

Each error includes appropriate messaging to help players understand and resolve issues.
