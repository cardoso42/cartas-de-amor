# Love Letter Game API Documentation

This document provides comprehensive documentation for the public interfaces of the Love Letter game backend application. The documentation is based on the controllers and SignalR hub methods.

## Table of Contents
- [Love Letter Game API Documentation](#love-letter-game-api-documentation)
  - [Table of Contents](#table-of-contents)
  - [REST API Endpoints](#rest-api-endpoints)
    - [Account Controller](#account-controller)
      - [Create Account](#create-account)
      - [Login](#login)
      - [Delete Account](#delete-account)
    - [Game Room Controller](#game-room-controller)
      - [Create Room](#create-room)
      - [Get All Rooms](#get-all-rooms)
      - [Delete Room](#delete-room)
  - [SignalR Game Hub](#signalr-game-hub)
    - [Game Room Management](#game-room-management)
      - [Join Room](#join-room)
      - [Leave Room](#leave-room)
      - [Reconnect To Room](#reconnect-to-room)
    - [Game Actions](#game-actions)
      - [Start Game](#start-game)
      - [Draw Card](#draw-card)
      - [Get Card Requirements](#get-card-requirements)
      - [Play Card](#play-card)
      - [Submit Card Choice](#submit-card-choice)
    - [MessageFactory Game Events](#messagefactory-game-events)
      - [PlayCard](#playcard)
      - [GuessCard](#guesscard)
      - [ShowCard](#showcard)
      - [CompareCards](#comparecards)
      - [ComparisonTie](#comparisontie)
      - [DiscardCard](#discardcard)
      - [DrawCard](#drawcard)
      - [PlayerEliminated](#playereliminated)
      - [SwitchCards](#switchcards)
      - [PlayerProtected](#playerprotected)
      - [ChooseCard](#choosecard)
      - [PublicPlayerUpdate](#publicplayerupdate)
    - [Game State Events](#game-state-events)
      - [RoundStarted](#roundstarted)
      - [RoundWinners](#roundwinners)
      - [BonusPoints](#bonuspoints)
      - [GameOver](#gameover)
      - [NextTurn](#nextturn)
      - [PlayerDrewCard](#playerdrewcard)
      - [PlayerUpdatePrivate](#PlayerUpdatePrivate)
      - [CardChoiceSubmitted](#cardchoicesubmitted)
    - [Error Events](#error-events)
      - [DrawCardError](#drawcarderror)
      - [GameStartError](#gamestarterror)
      - [PlayCardError](#playcarderror)
      - [CardChoiceError](#cardchoiceerror)
      - [MandatoryCardPlay](#mandatorycardplay)
      - [UserNotAuthenticated](#usernotauthenticated)
    - [Connection Management](#connection-management)
      - [OnDisconnectedAsync](#ondisconnectedasync)
  - [Data Transfer Objects (DTOs)](#data-transfer-objects-dtos)
  - [Error Handling](#error-handling)

## REST API Endpoints

### Account Controller

Base path: `/api/Account`

#### Create Account
- **Endpoint**: `POST /api/Account/create`
- **Authorization**: None
- **Request Body**:
  ```json
  {
    "username": "string",
    "email": "string",
    "password": "string"
  }
  ```
- **Response**:
  - 200 OK:
    ```json
    {
      "accessToken": "string",
      "message": "Account created successfully."
    }
    ```
  - 409 Conflict: Account already exists
  - 500 Server Error: Unexpected error

#### Login
- **Endpoint**: `POST /api/Account/login`
- **Authorization**: None
- **Request Body**:
  ```json
  {
    "email": "string",
    "password": "string"
  }
  ```
- **Response**:
  - 200 OK:
    ```json
    {
      "username": "string",
      "email": "string",
      "token": "string",
      "expiration": "DateTime",
      "message": "Login successful"
    }
    ```
  - 401 Unauthorized: Invalid credentials
  - 500 Server Error: Unexpected error

#### Delete Account
- **Endpoint**: `DELETE /api/Account/{email}`
- **Authorization**: Bearer token
- **Parameters**:
  - `email` (path): The email of the account to delete
- **Response**:
  - 200 OK:
    ```json
    {
      "message": "Account deleted successfully."
    }
    ```
  - 401 Unauthorized: User not properly authenticated
  - 403 Forbidden: Trying to delete someone else's account
  - 404 Not Found: Account not found
  - 500 Server Error: Unexpected error

### Game Room Controller

Base path: `/api/GameRoom`

#### Create Room
- **Endpoint**: `POST /api/GameRoom`
- **Authorization**: Bearer token
- **Request Body**:
  ```json
  {
    "roomName": "string",
    "password": "string" // Optional
  }
  ```
- **Response**:
  - 200 OK: Room ID (GUID)
  - 400 Bad Request: Invalid input
  - 500 Server Error: Unexpected error

#### Get All Rooms
- **Endpoint**: `GET /api/GameRoom`
- **Authorization**: Bearer token
- **Response**:
  - 200 OK: Array of game rooms
    ```json
    [
      {
        "id": "guid",
        "roomName": "string",
        "ownerEmail": "string",
        "hasPassword": "boolean",
        "currentPlayers": "integer",
        "createdAt": "dateTime"
      }
    ]
    ```
  - 500 Server Error: Unexpected error

#### Delete Room
- **Endpoint**: `DELETE /api/GameRoom/{roomId}`
- **Authorization**: Bearer token
- **Parameters**:
  - `roomId` (path): The ID of the room to delete
- **Response**:
  - 204 No Content: Room deleted successfully
  - 404 Not Found: Room not found or user not authorized
  - 500 Server Error: Unexpected error

## SignalR Game Hub

The Game Hub provides real-time communication for the Love Letter game. Connect to `/GameHub` with an authorization bearer token in the request header.

**Note**: The backend uses MessageFactory to generate specific, granular events for each card effect instead of generic CardResult events. This provides better real-time feedback and more accurate game state communication to all players.

### Game Room Management

#### Join Room
- **Method**: `JoinRoom`
- **Parameters**:
  - `roomId` (Guid): The ID of the room to join
  - `password` (string, optional): The password for the room if it is password-protected
- **Server Events**:
  - None directly from this method
- **Notes**:
  - Adds the user to the specified room
  - Maps the user's connection ID to their email for future reference

#### Leave Room
- **Method**: `LeaveRoom`
- **Parameters**:
  - `roomId` (Guid): The ID of the room to leave
- **Server Events**:
  - None directly from this method
- **Notes**:
  - Removes the user from the specified room
  - Updates the connection mapping

#### Reconnect To Room
- **Method**: `ReconnectToRoom`
- **Parameters**:
  - `roomId` (Guid): The ID of the room to reconnect to
- **Server Events**:
  - None directly from this method
- **Notes**:
  - Re-adds the user's connection to the room after a disconnect/reconnect
  - Does not rejoin the game, only reconnects the SignalR connection

### Game Actions

#### Start Game
- **Method**: `StartGame`
- **Parameters**:
  - `roomId` (Guid): The ID of the room where the game will start
- **Server Events**:
  - `RoundStarted`: Sent to each player with their personal game status
  - `NextTurn`: Sent to all players with the email of the player whose turn is next
- **Notes**:
  - Only the host can start the game
  - Requires at least 2 players in the room

#### Draw Card
- **Method**: `DrawCard`
- **Parameters**:
  - `roomId` (Guid): The ID of the room where the player will draw a card
- **Server Events**:
  - `PlayerUpdatePrivate`: Sent to the player who drew the card with their updated hand
  - `PlayerDrewCard`: Sent to all players notifying that a player drew a card
  - `DrawCardError`: Sent to the caller if there's an error
- **Notes**:
  - Can only be called by the player whose turn it is
  - Required before playing a card each turn

#### Get Card Requirements
- **Method**: `GetCardRequirements`
- **Parameters**:
  - `roomId` (Guid): The ID of the room
  - `cardType` (CardType): The type of card to get requirements for
- **Server Events**:
  - `CardRequirements`: Requirements for playing the specified card
- **Notes**:
  - Used to determine what additional information is needed to play a card (target player, card type guess, etc.)

#### Play Card
- **Method**: `PlayCard`
- **Parameters**:
  - `roomId` (Guid): The ID of the room
  - `cardPlayDto` (CardPlayDto): Information about the card being played, including:
    - `cardType`: The type of card being played
    - `targetPlayerEmail`: The email of the target player (if required)
    - `targetCardType`: The card type being guessed or targeted (if required)
- **Server Events**:
  - `PlayCard`: Sent to all players with information about the card being played
  - Card-specific events based on the card's effect:
    - `GuessCard`: Guard card guess attempt
    - `ShowCard`: Priest card show effect  
    - `CompareCards`: Baron card comparison
    - `ComparisonTie`: Baron card tie result
    - `DiscardCard`: Card discard notifications
    - `DrawCard`: Card draw notifications
    - `PlayerEliminated`: Player elimination
    - `SwitchCards`: King card effect
    - `PlayerProtected`: Servant card protection
    - `ChooseCard`: Chancellor card choice prompt
  - `PublicPlayerUpdate`: Sent to all players with updated public player information
  - `PlayerUpdatePrivate`: Sent to the player who played the card with their updated status
  - `PlayerUpdatePrivate`: Sent to the target player (if any) with their updated status
  - `NextTurn`: Sent to all players with the email of the next player (if game advances)
  - `RoundWinners`: Sent to all players with a list of emails of the round winners (when a round ends)
  - `BonusPoints`: Sent to all players with a list of emails of the bonus points receivers (when a round ends)
  - `GameOver`: Sent to all players with a list of emails of the game winners (when a game ends)
  - Various error events: `MandatoryCardPlay`, `CardRequirements`, `PlayCardError`
- **Notes**:
  - Resolves the effects of playing a card using the new MessageFactory event system
  - May trigger other game state changes like round end

#### Submit Card Choice
- **Method**: `SubmitCardChoice`
- **Parameters**:
  - `roomId` (Guid): The ID of the room
  - `keepCardType` (CardType): The card type the player wants to keep
  - `returnCardTypes` (List<CardType>): The card types the player wants to return
- **Server Events**:
  - `CardChoiceSubmitted`: Sent to all players with the player's public update
  - `PlayerUpdatePrivate`: Sent to the player with their updated status
  - `NextTurn`: Sent to all players with the email of the next player
  - `CardChoiceError`: Sent to the caller if there's an error
- **Notes**:
  - Used after certain card effects that require the player to choose between multiple cards
  - Advances the game to the next player's turn

### MessageFactory Game Events

When a card is played using the `PlayCard` method, the backend uses MessageFactory to generate specific events based on the card's effect. These events provide detailed information about what happened during card play.

#### PlayCard
- **Event**: `PlayCard`
- **Description**: Sent to all players when any card is played
- **Data**: `{ Player: string, CardType: number }`
- **Example**: `{ "Player": "player@example.com", "CardType": 1 }`

#### GuessCard  
- **Event**: `GuessCard`
- **Description**: Sent when a Guard card is played to guess another player's card
- **Data**: `{ Invoker: string, CardType: number, Target: string }`
- **Example**: `{ "Invoker": "player1@example.com", "CardType": 2, "Target": "player2@example.com" }`

#### ShowCard
- **Event**: `ShowCard`
- **Description**: Sent when a Priest card is played to look at another player's card
- **Data**: `{ Invoker: string, Target: string }`
- **Example**: `{ "Invoker": "player1@example.com", "Target": "player2@example.com" }`

#### CompareCards
- **Event**: `CompareCards`
- **Description**: Sent when a Baron card is played to compare cards
- **Data**: `{ Invoker: string, Target: string }`
- **Example**: `{ "Invoker": "player1@example.com", "Target": "player2@example.com" }`

#### ComparisonTie
- **Event**: `ComparisonTie`
- **Description**: Sent when a Baron card comparison results in a tie
- **Data**: `{ Invoker: string, Target: string }`
- **Example**: `{ "Invoker": "player1@example.com", "Target": "player2@example.com" }`

#### DiscardCard
- **Event**: `DiscardCard`
- **Description**: Sent when a player discards a card (Prince effect)
- **Data**: `{ Target: string, CardType: number }`
- **Example**: `{ "Target": "player@example.com", "CardType": 3 }`

#### DrawCard
- **Event**: `DrawCard`
- **Description**: Sent when a player draws a card (Chancellor effect)
- **Data**: `{ Player: string }`
- **Example**: `{ "Player": "player@example.com" }`

#### PlayerEliminated
- **Event**: `PlayerEliminated`
- **Description**: Sent when a player is eliminated from the round
- **Data**: `{ Player: string }`
- **Example**: `{ "Player": "player@example.com" }`

#### SwitchCards
- **Event**: `SwitchCards`
- **Description**: Sent when two players switch cards (King effect)
- **Data**: `{ Invoker: string, Target: string }`
- **Example**: `{ "Invoker": "player1@example.com", "Target": "player2@example.com" }`

#### PlayerProtected
- **Event**: `PlayerProtected`
- **Description**: Sent when a player gains protection (Servant effect)
- **Data**: `{ Player: string }`
- **Example**: `{ "Player": "player@example.com" }`

#### ChooseCard
- **Event**: `ChooseCard`
- **Description**: Sent to a specific player when they need to choose cards (Chancellor effect)
- **Data**: `{ Player: string }`
- **Example**: `{ "Player": "player@example.com" }`

#### PublicPlayerUpdate
- **Event**: `PublicPlayerUpdate`
- **Description**: Sent to all players with public player state updates
- **Data**: PublicPlayerUpdateDto with player's public information
- **Example**: `{ "UserEmail": "player@example.com", "Status": 1, "HoldingCardsCount": 1, "PlayedCards": [2], "Score": 3 }`

### Game State Events

These events inform players about changes in the game state.

#### RoundStarted
- **Event**: `RoundStarted`
- **Description**: Sent to players when a new round begins
- **Data**: InitialGameStatusDto containing the initial game state for that specific player

#### RoundWinners
- **Event**: `RoundWinners`
- **Description**: Sent to all players when a round ends
- **Data**: List of player emails who won the round

#### BonusPoints
- **Event**: `BonusPoints`
- **Description**: Sent to all players when bonus points are awarded
- **Data**: List of player emails who received bonus points

#### GameOver
- **Event**: `GameOver`
- **Description**: Sent to all players when the game ends
- **Data**: List of player emails who won the game

#### NextTurn
- **Event**: `NextTurn`
- **Description**: Sent to all players when it's the next player's turn
- **Data**: Email of the player whose turn is next

#### PlayerDrewCard
- **Event**: `PlayerDrewCard`
- **Description**: Sent to all players when a player draws a card
- **Data**: Email of the player who drew a card

#### PlayerUpdatePrivate
- **Event**: `PlayerUpdatePrivate`
- **Description**: Sent to a specific player with their private game state
- **Data**: PrivatePlayerUpdateDto with player's cards and status

#### CardChoiceSubmitted
- **Event**: `CardChoiceSubmitted`
- **Description**: Sent to all players when a player submits their card choice
- **Data**: PublicPlayerUpdateDto with the player's public game state

### Error Events

These events are sent when errors occur during game actions.

#### DrawCardError
- **Event**: `DrawCardError`
- **Description**: Sent when there's an error drawing a card
- **Data**: Error message string

#### GameStartError
- **Event**: `GameStartError`
- **Description**: Sent when there's an error starting the game
- **Data**: Error message string

#### PlayCardError
- **Event**: `PlayCardError`
- **Description**: Sent when there's an error playing a card
- **Data**: Error message string

#### CardChoiceError
- **Event**: `CardChoiceError`
- **Description**: Sent when there's an error submitting a card choice
- **Data**: Error message string

#### MandatoryCardPlay
- **Event**: `MandatoryCardPlay`
- **Description**: Sent when a player must play a specific card
- **Data**: Error message string and the required card type

#### UserNotAuthenticated
- **Event**: `UserNotAuthenticated`
- **Description**: Sent when the user is not properly authenticated
- **Data**: None

### Connection Management

The connection management methods are mostly internal to the SignalR hub and handle the mapping between user emails and connection IDs.

#### OnDisconnectedAsync
- **Notes**:
  - Automatically called when a client disconnects
  - Cleans up the connection mapping
  - Does not remove the player from the game

## Data Transfer Objects (DTOs)

The application uses several DTOs for transferring data between the client and server. The key DTOs include:

- **CreateAccountRequestDto**: Username, email, and password for account creation
- **LoginRequestDto**: Username and password for login
- **LoginResultDto**: Authentication result with token and expiration
- **GameRoomCreationRequestDto**: Name and optional password for a new game room
- **GameRoomDto**: Information about a game room (id, room name, owner email, password status, player count, creation time)
- **CardPlayDto**: Information about a card being played
- **CardActionResultDto**: Legacy DTO, no longer used for main game events (replaced by MessageFactory events)
- **CardRequirementsDto**: Requirements for playing a specific card
- **InitialGameStatusDto**: Initial game state for a player
- **PrivatePlayerUpdateDto**: Private update for a specific player
- **PublicPlayerUpdateDto**: Public update visible to all players

## Error Handling

The API uses standard HTTP status codes and exception messages for REST endpoints. SignalR methods use specific error events with descriptive messages to communicate errors to clients.

Common exceptions include:
- **InvalidOperationException**: For most game rule violations
- **UnauthorizedAccessException**: For authentication failures
- **MandatoryCardPlayViolationException**: When a player must play a specific card
- **CardRequirementsNotMetException**: When requirements for playing a card are not met
