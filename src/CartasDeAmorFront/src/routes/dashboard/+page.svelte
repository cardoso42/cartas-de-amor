<script lang="ts">
  import { logout } from '$lib/services/authService';
  import AuthGuard from '$lib/components/AuthGuard.svelte';

  function handleLogout() {
    logout();
  }
</script>

<svelte:head>
  <title>Dashboard | Love Letter</title>
</svelte:head>

<AuthGuard requireAuth={true} redirectTo="/welcome">
  <div class="dashboard-container">
    <h1>Welcome to Love Letter</h1>
    
    <div class="dashboard-grid">
      <div class="dashboard-card">
        <div class="card-icon">🎮</div>
        <h2>Play Game</h2>
        <p>Join a game room or create a new one to play with friends.</p>
        <a href="/rooms" class="dashboard-button">Game Lobby</a>
      </div>
      
      <div class="dashboard-card">
        <div class="card-icon">👤</div>
        <h2>My Profile</h2>
        <p>View and edit your profile information and game statistics.</p>
        <a href="/profile" class="dashboard-button">View Profile</a>
      </div>
      
      <div class="dashboard-card">
        <div class="card-icon">📜</div>
        <h2>Game Rules</h2>
        <p>Review the rules and learn how to play Love Letter.</p>
        <a href="/rules" class="dashboard-button">Read Rules</a>
      </div>
      
      <div class="dashboard-card">
        <div class="card-icon">🏆</div>
        <h2>Leaderboard</h2>
        <p>Check the top players and see where you stand.</p>
        <a href="/leaderboard" class="dashboard-button disabled">Coming Soon</a>
      </div>
    </div>
  </div>
</AuthGuard>

<style>
  .dashboard-container {
    max-width: 1200px;
    margin: 0 auto;
  }
  
  h1 {
    color: #9c27b0;
    text-align: center;
    margin-bottom: 2rem;
  }
  
  .dashboard-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
    gap: 1.5rem;
    margin-top: 2rem;
  }
  
  .dashboard-card {
    background-color: white;
    border-radius: 8px;
    padding: 2rem;
    text-align: center;
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
    transition: transform 0.2s, box-shadow 0.2s;
  }
  
  .dashboard-card:hover {
    transform: translateY(-5px);
    box-shadow: 0 8px 16px rgba(0, 0, 0, 0.1);
  }
  
  .card-icon {
    font-size: 2.5rem;
    margin-bottom: 1rem;
  }
  
  .dashboard-card h2 {
    color: #7b1fa2;
    margin-bottom: 0.5rem;
  }
  
  .dashboard-card p {
    color: #666;
    margin-bottom: 1.5rem;
  }
  
  .dashboard-button {
    display: inline-block;
    background-color: #9c27b0;
    color: white;
    text-decoration: none;
    padding: 0.75rem 1.5rem;
    border-radius: 4px;
    font-weight: 500;
    transition: background-color 0.3s;
  }
  
  .dashboard-button:hover {
    background-color: #7b1fa2;
  }
  
  .dashboard-button.disabled {
    background-color: #e0e0e0;
    color: #9e9e9e;
    cursor: not-allowed;
  }
</style>
