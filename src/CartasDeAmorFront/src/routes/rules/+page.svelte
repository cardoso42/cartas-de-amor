<script lang="ts">
  import AuthGuard from '$lib/components/AuthGuard.svelte';
</script>

<svelte:head>
  <title>Game Rules | Love Letter</title>
</svelte:head>

<AuthGuard requireAuth={false}>
  <div class="rules-container">
    <h1>Love Letter - Game Rules</h1>
    
    <section>
      <h2>Overview</h2>
      <p>
        In the game Love Letter, players take on the role of potential suitors seeking to deliver 
        their love letter to Princess Annette. The player whose letter reaches the princess wins her heart and the game.
      </p>
    </section>
    
    <section>
      <h2>Components</h2>
      <ul>
        <li>16 Game cards</li>
        <li>4 Reference cards</li>
        <li>Tokens of affection</li>
      </ul>
    </section>
    
    <section>
      <h2>Setup</h2>
      <p>
        1. Shuffle the 16 game cards to form a face-down deck.<br>
        2. Remove the top card from the deck and set it aside face-down. This card will not be used in the round.<br>
        3. Each player draws one card from the deck as their starting hand.
      </p>
    </section>
    
    <section>
      <h2>Gameplay</h2>
      <p>
        Players take turns clockwise around the table. On your turn:<br>
        1. Draw the top card from the deck.<br>
        2. Choose one of your two cards to play and place it face-up in front of you.<br>
        3. Follow the instructions on the played card.<br>
        4. After playing a card, if you are still in the round, you should have exactly one card in hand.
      </p>
    </section>
    
    <section>
      <h2>Card Types</h2>
      <div class="cards-grid">
        <div class="card">
          <h3>8 - Princess</h3>
          <p>If you discard this card, you are out of the round.</p>
        </div>
        <div class="card">
          <h3>7 - Countess</h3>
          <p>If you have this card and the King or Prince in your hand, you must discard this card.</p>
        </div>
        <div class="card">
          <h3>6 - King</h3>
          <p>Trade hands with another player of your choice.</p>
        </div>
        <div class="card">
          <h3>5 - Prince</h3>
          <p>Choose any player (including yourself) to discard their hand and draw a new card.</p>
        </div>
        <div class="card">
          <h3>4 - Handmaid</h3>
          <p>Until your next turn, you are immune to the effects of other players' cards.</p>
        </div>
        <div class="card">
          <h3>3 - Baron</h3>
          <p>Compare hands with another player. The player with the lower-value card is out of the round.</p>
        </div>
        <div class="card">
          <h3>2 - Priest</h3>
          <p>Look at another player's hand.</p>
        </div>
        <div class="card">
          <h3>1 - Guard</h3>
          <p>Name a card (except Guard). If another player has that card, they are out of the round.</p>
        </div>
      </div>
    </section>
    
    <section>
      <h2>End of a Round</h2>
      <p>
        A round ends when:<br>
        - The deck is empty after a player's turn.<br>
        - All players except one are out of the round.<br>
        <br>
        The player still in the round or the player with the highest-value card wins the round and receives a token of affection.
      </p>
    </section>
    
    <section>
      <h2>Winning the Game</h2>
      <p>
        The first player to collect a certain number of tokens (depending on the number of players) wins the game.
      </p>
    </section>
  </div>
</AuthGuard>

<style>
  .rules-container {
    max-width: 800px;
    margin: 0 auto;
    padding: 2rem 1rem;
  }
  
  h1 {
    color: #9c27b0;
    text-align: center;
    margin-bottom: 2rem;
  }
  
  h2 {
    color: #7b1fa2;
    margin-top: 2rem;
    border-bottom: 2px solid #e1bee7;
    padding-bottom: 0.5rem;
  }
  
  section {
    margin-bottom: 2rem;
  }
  
  .cards-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
    gap: 1rem;
  }
  
  .card {
    background-color: white;
    border: 1px solid #e0e0e0;
    border-radius: 8px;
    padding: 1rem;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  }
  
  .card h3 {
    color: #9c27b0;
    margin-top: 0;
  }
</style>
