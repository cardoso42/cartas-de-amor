<script lang="ts">
  import { CardType, type InitialGameStatusDto } from '$lib/types/game-types';
  import { user } from '$lib/stores/userStore';
  
  // Props from parent component
  export let gameStatus: InitialGameStatusDto;
  export let currentUserEmail: string;
  
  // Get local player data (the player at this client)
  let localPlayerName = '';
  user.subscribe(state => {
    localPlayerName = state.username || currentUserEmail.split('@')[0];
  });
  
  // Process the game data into display format
  $: players = gameStatus ? processGameData(gameStatus, currentUserEmail) : [];
  $: totalPlayers = players.length;
  $: anglePerPlayer = totalPlayers > 1 ? 360 / totalPlayers : 0;
  
  function processGameData(status: InitialGameStatusDto, userEmail: string) {
    const processedPlayers = [];
    
    // Add local player at position 0 (bottom)
    processedPlayers.push({
      id: 0,
      name: localPlayerName, // Use the username from user store
      email: userEmail,
      isLocalPlayer: true,
      position: 0,
      tokens: status.score || 0,
      cards: status.yourCards || [],
      cardsInHand: (status.yourCards || []).length,
      isProtected: status.isProtected || false
    });
    
    // Add other players in order around the table
    (status.otherPlayersPublicData || []).forEach((player, index) => {
      processedPlayers.push({
        id: index + 1,
        name: player.username || player.userEmail.split('@')[0], // Use proper username or fallback to email username
        email: player.userEmail,
        isLocalPlayer: false,
        position: index + 1,
        tokens: player.score || 0,
        cards: [], // Other players' cards are hidden
        cardsInHand: player.cardsInHand || 1,
        isProtected: player.isProtected || false
      });
    });
    
    return processedPlayers;
  }
  
  function getPlayerPosition(playerIndex: number, totalPlayers: number) {
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
  
  function getCardName(cardType: CardType): string {
    const cardNames = {
      [CardType.Spy]: 'Spy',
      [CardType.Guard]: 'Guard',
      [CardType.Priest]: 'Priest', 
      [CardType.Baron]: 'Baron',
      [CardType.Handmaid]: 'Handmaid',
      [CardType.Prince]: 'Prince',
      [CardType.Chanceller]: 'Chanceller',
      [CardType.King]: 'King',
      [CardType.Countess]: 'Countess',
      [CardType.Princess]: 'Princess'
    };
    return cardNames[cardType] || 'Unknown';
  }
</script>

<div class="game-container">
  <div class="table-container">
    <!-- The wooden circular table -->
    <div class="table">
      <!-- Card deck in the center -->
      <div class="deck-area">
        <div class="card-deck">
          <div class="deck-cards">
            <!-- Deck cards stack -->
            <div class="deck-card"></div>
            <div class="deck-card"></div>
            <div class="deck-card"></div>
          </div>
          <div class="deck-label">Deck</div>
        </div>
      </div>
      
      <!-- Players positioned around the table -->
      {#each players as player, index (player.id)}
        {@const position = getPlayerPosition(index, totalPlayers)}
        {@const x = Math.cos((position.angle - 90) * Math.PI / 180) * position.distance}
        {@const y = Math.sin((position.angle - 90) * Math.PI / 180) * position.distance}
        <div 
          class="player-area"
          class:local-player={player.isLocalPlayer}
          style="
            left: 50%;
            top: 50%;
            transform: translate(calc(-50% + {x}px), calc(-50% + {y}px));
          "
        >
          <!-- Player name -->
          <div class="player-name" class:protected={player.isProtected}>
            {player.name}
            {#if player.isProtected}
              <span class="protection-icon">🛡️</span>
            {/if}
          </div>
          
          <!-- Player tokens display (hearts underneath name) -->
          <div class="player-tokens">
            {#each Array(player.tokens) as _, i}
              <div class="token">♥</div>
            {/each}
          </div>
          
          <!-- Player's hand (face down for others, face up for local player) -->
          <div class="player-hand">
            {#if player.isLocalPlayer}
              <!-- Local player sees their cards face up -->
              {#each player.cards as card}
                <div class="card player-card face-up">
                  <div class="card-content">
                    <div class="card-number">{card}</div>
                    <div class="card-name">{getCardName(card)}</div>
                  </div>
                </div>
              {/each}
            {:else}
              <!-- Other players' cards are face down -->
              {#each Array(player.cardsInHand || 1) as _, i}
                <div class="card player-card face-down"></div>
              {/each}
            {/if}
          </div>
        </div>
      {/each}
      
      <!-- Played cards positioned on the table surface in front of each player -->
      <!-- TODO: Add played cards when implementing card playing functionality -->
      <!-- {#each players as player, index (player.id)}
        {@const position = getPlayerPosition(index, totalPlayers)}
        {@const playedCardDistance = 150}
        {@const playedX = Math.cos((position.angle - 90) * Math.PI / 180) * playedCardDistance}
        {@const playedY = Math.sin((position.angle - 90) * Math.PI / 180) * playedCardDistance}
        <div 
          class="played-cards-area"
          style="
            left: 50%;
            top: 50%;
            transform: translate(calc(-50% + {playedX}px), calc(-50% + {playedY}px));
          "
        >
          <div class="card played-card face-up">
            <div class="card-content">
              <div class="card-number">2</div>
              <div class="card-name">Priest</div>
            </div>
          </div>
        </div>
      {/each} -->
    </div>
  </div>
</div>

<style>
  .game-container {
    width: 100%;
    height: 80vh;
    display: flex;
    justify-content: center;
    align-items: center;
    background: linear-gradient(135deg, #3e2723 0%, #5d4037 50%, #3e2723 100%);
    border-radius: 12px;
    padding: 2rem;
  }
  
  .table-container {
    position: relative;
    width: 600px;
    height: 600px;
  }
  
  .table {
    position: relative;
    width: 100%;
    height: 100%;
    background: radial-gradient(circle, #d4a574 10%, #8b4513 40%, #654321 70%, #4a2c17 100%);
    border-radius: 50%;
    box-shadow: 
      inset 0 0 0 8px #a0522d,
      inset 0 0 0 12px #8b4513,
      inset 0 0 40px rgba(101, 67, 33, 0.8),
      0 20px 40px rgba(0, 0, 0, 0.4),
      0 0 0 2px #5d4037;
    
    /* Enhanced wood grain effect */
    background-image: 
      repeating-linear-gradient(
        30deg,
        transparent,
        transparent 1px,
        rgba(139, 69, 19, 0.15) 1px,
        rgba(139, 69, 19, 0.15) 3px
      ),
      repeating-linear-gradient(
        120deg,
        transparent,
        transparent 2px,
        rgba(160, 82, 45, 0.1) 2px,
        rgba(160, 82, 45, 0.1) 4px
      );
  }
  
  .deck-area {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    z-index: 10;
  }
  
  .card-deck {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.5rem;
  }
  
  .deck-cards {
    position: relative;
    width: 60px;
    height: 84px;
  }
  
  .deck-card {
    position: absolute;
    width: 60px;
    height: 84px;
    background-image: url('/images/card-back.png'), linear-gradient(135deg, #1565c0 0%, #0d47a1 100%);
    background-size: cover;
    background-position: center;
    background-repeat: no-repeat;
    border-radius: 8px;
    border: 2px solid #0d47a1;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
  }
  
  .deck-card:nth-child(1) {
    top: 0;
    left: 0;
  }
  
  .deck-card:nth-child(2) {
    top: -2px;
    left: -1px;
  }
  
  .deck-card:nth-child(3) {
    top: -4px;
    left: -2px;
  }
  
  .deck-label {
    color: white;
    font-size: 0.8rem;
    font-weight: bold;
    text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.8);
  }
  
  /* Player positioning around the circular table */
  .player-area {
    position: absolute;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.5rem;
    pointer-events: auto;
  }
  
  .player-name {
    color: white;
    font-weight: bold;
    font-size: 0.9rem;
    text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.8);
    background: rgba(0, 0, 0, 0.3);
    padding: 0.25rem 0.75rem;
    border-radius: 12px;
    min-width: 80px;
    text-align: center;
  }
  
  .local-player .player-name {
    background: rgba(156, 39, 176, 0.7);
    color: white;
  }
  
  .player-name.protected {
    background: rgba(255, 193, 7, 0.8);
    color: #333;
    border: 2px solid #ffc107;
  }
  
  .local-player .player-name.protected {
    background: rgba(255, 193, 7, 0.9);
    color: #333;
    border: 2px solid #ffc107;
  }
  
  .protection-icon {
    margin-left: 0.25rem;
    font-size: 0.8rem;
  }
  
  .player-tokens {
    display: flex;
    gap: 2px;
    justify-content: center;
    align-items: center;
    flex-wrap: wrap;
    max-width: 80px;
  }
  
  .token {
    color: #ff6b9d;
    font-size: 1rem;
    text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.8);
    animation: pulse 2s infinite;
  }
  
  @keyframes pulse {
    0%, 100% { opacity: 1; }
    50% { opacity: 0.8; }
  }
  
  .player-hand {
    display: flex;
    gap: 4px;
  }
  
  .card {
    width: 50px;
    height: 70px;
    border-radius: 6px;
    border: 2px solid #333;
    box-shadow: 0 2px 6px rgba(0, 0, 0, 0.3);
    transition: transform 0.2s ease;
  }
  
  .player-card {
    transform: scale(1.1);
  }
  
  .player-card:hover {
    transform: scale(1.2);
    z-index: 5;
  }
  
  .face-up {
    background: linear-gradient(135deg, #fafafa 0%, #e0e0e0 100%);
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    padding: 4px;
  }
  
  .face-down {
    background-image: url('/images/card-back.png');
    background-size: cover;
    background-position: center;
    background-repeat: no-repeat;
    position: relative;
  }
  
  /* Fallback styling when image fails to load */
  .face-down {
    background-color: #1565c0;
    background-image: url('/images/card-back.png'), linear-gradient(135deg, #1565c0 0%, #0d47a1 100%);
  }
  
  .card-content {
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    height: 100%;
  }
  
  .card-number {
    font-size: 1.1rem;
    font-weight: bold;
    color: #9c27b0;
    text-align: center;
  }
  
  .card-name {
    font-size: 0.6rem;
    font-weight: bold;
    color: #333;
    text-align: center;
    line-height: 1;
  }
  
  .played-cards {
    display: flex;
    gap: 2px;
    flex-wrap: wrap;
    justify-content: center;
    max-width: 100px;
  }
  
  .played-cards-area {
    position: absolute;
    display: flex;
    gap: 4px;
    flex-wrap: wrap;
    justify-content: center;
    z-index: 5;
  }
  
  .played-card {
    width: 40px;
    height: 56px;
    background: linear-gradient(135deg, #fafafa 0%, #e0e0e0 100%);
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    padding: 3px;
  }
  
  .played-card .card-number {
    font-size: 0.8rem;
  }
  
  .played-card .card-name {
    font-size: 0.4rem;
  }
  
  /* Responsive design for smaller screens */
  @media (max-width: 768px) {
    .table-container {
      width: 400px;
      height: 400px;
    }
    
    .token {
      font-size: 0.9rem;
    }
    
    .card {
      width: 40px;
      height: 56px;
    }
    
    .played-card {
      width: 32px;
      height: 45px;
    }
  }
</style>
