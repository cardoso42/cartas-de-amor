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
    - [Card Result Events](#card-result-events)
      - [CardResult-ShowCard](#cardresult-showcard)
      - [CardResult-PlayerEliminated](#cardresult-playereliminated)
      - [CardResult-SwitchCards](#cardresult-switchcards)
      - [CardResult-DiscardAndDrawCard](#cardresult-discardanddrawcard)
      - [CardResult-ProtectionGranted](#cardresult-protectiongranted)
      - [CardResult-ChooseCard](#cardresult-choosecard)
    - [Game State Events](#game-state-events)
      - [RoundStarted](#roundstarted)
      - [RoundWinners](#roundwinners)
      - [BonusPoints](#bonuspoints)
      - [GameOver](#gameover)
      - [NextTurn](#nextturn)
      - [PlayerDrewCard](#playerdrewcard)
      - [PrivatePlayerUpdate](#privateplayerupdate)
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
  - `PrivatePlayerUpdate`: Sent to the player who drew the card with their updated hand
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
  - `CardResult-{result}`: Sent to all players with the result of the card play (see Card Result Events section below)
  - `PrivatePlayerUpdate`: Sent to the player who played the card with their updated status
  - `PrivatePlayerUpdate`: Sent to the target player (if any) with their updated status
  - `ChooseCard`: Sent to the player when they need to choose a card after playing
  - `NextTurn`: Sent to all players with the email of the next player (if game advances)
  - `RoundWinners`: Sent to all players with a list of emails of the round winners (when a round ends)
  - `BonusPoints`: Sent to all players with a list of emails of the bonus points receivers (when a round ends)
  - `GameOver`: Sent to all players with a list of emails of the game winners (when a game ends)
  - Various error events: `MandatoryCardPlay`, `CardRequirements`, `PlayCardError`
- **Notes**:
  - Resolves the effects of playing a card
  - May trigger other game state changes like round end

#### Submit Card Choice
- **Method**: `SubmitCardChoice`
- **Parameters**:
  - `roomId` (Guid): The ID of the room
  - `keepCardType` (CardType): The card type the player wants to keep
  - `returnCardTypes` (List<CardType>): The card types the player wants to return
- **Server Events**:
  - `CardChoiceSubmitted`: Sent to all players with the player's public update
  - `PrivatePlayerUpdate`: Sent to the player with their updated status
  - `NextTurn`: Sent to all players with the email of the next player
  - `CardChoiceError`: Sent to the caller if there's an error
- **Notes**:
  - Used after certain card effects that require the player to choose between multiple cards
  - Advances the game to the next player's turn

### Card Result Events

When a card is played using the `PlayCard` method, one of the following events will be emitted based on the result of the card action. All these events send a `CardActionResultDto` object containing information about the action result.

#### CardResult-ShowCard
- **Event**: `CardResult-ShowCard` 
- **Description**: Sent when one player shows a card to another player
- **Data**: CardActionResultDto with invoker and target player information

#### CardResult-PlayerEliminated
- **Event**: `CardResult-PlayerEliminated`
- **Description**: Sent when a player is eliminated from the round
- **Data**: CardActionResultDto with invoker and target player information (target has status=2 if they were eliminated)

#### CardResult-SwitchCards
- **Event**: `CardResult-SwitchCards`
- **Description**: Sent when two players switch cards
- **Data**: CardActionResultDto with invoker and target player information

#### CardResult-DiscardAndDrawCard
- **Event**: `CardResult-DiscardAndDrawCard`
- **Description**: Sent when a player discards and draws a new card
- **Data**: CardActionResultDto with invoker information including played cards

#### CardResult-ProtectionGranted
- **Event**: `CardResult-ProtectionGranted`
- **Description**: Sent when a player gains protection for a turn
- **Data**: CardActionResultDto with invoker information

#### CardResult-ChooseCard
- **Event**: `CardResult-ChooseCard`
- **Description**: Sent when a player needs to choose a card (triggers the `ChooseCard` event to the player)
- **Data**: CardActionResultDto with invoker information and card type

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

#### PrivatePlayerUpdate
- **Event**: `PrivatePlayerUpdate`
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
- **CardActionResultDto**: Result of playing a card
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
