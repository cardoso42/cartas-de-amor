<script lang="ts">
  import { logout } from '$lib/services/authService';
  import AuthGuard from '$lib/components/auth/AuthGuard.svelte';
  import { _ } from 'svelte-i18n';

  function handleLogout() {
    logout();
  }
</script>

<svelte:head>
  <title>{$_('navigation.dashboard')} | {$_('app.name')}</title>
</svelte:head>

<AuthGuard requireAuth={true} redirectTo="/welcome">
  <div class="dashboard-container">
    <h1>{$_('common.welcome')} {$_('app.name')}</h1>
    
    <div class="dashboard-grid">
      <div class="dashboard-card">
        <div class="card-icon">🎮</div>
        <h2>{$_('common.playGame')}</h2>
        <p>{$_('common.playGameDescription')}</p>
        <a href="/rooms" class="button">{$_('navigation.rooms')}</a>
      </div>
      
      <div class="dashboard-card">
        <div class="card-icon">👤</div>
        <h2>{$_('navigation.profile')}</h2>
        <p>{$_('common.profileDescription')}</p>
        <a href="/profile" class="button">{$_('common.viewProfile')}</a>
      </div>
      
      <div class="dashboard-card">
        <div class="card-icon">📜</div>
        <h2>{$_('navigation.rules')}</h2>
        <p>{$_('common.rulesDescription')}</p>
        <a href="/rules" class="button">{$_('common.readRules')}</a>
      </div>
      
      <!-- <div class="dashboard-card"> -->
      <!--   <div class="card-icon">🏆</div> -->
      <!--   <h2>Leaderboard</h2> -->
      <!--   <p>Check the top players and see where you stand.</p> -->
      <!--   <a href="/leaderboard" class="button disabled">Coming Soon</a> -->
      <!-- </div> -->
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
  
  /* Button styles now come from global button.css */
</style>
