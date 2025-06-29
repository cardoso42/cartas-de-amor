<script lang="ts">
  import AuthGuard from '$lib/components/auth/AuthGuard.svelte';
  import { _ } from 'svelte-i18n';
</script>

<svelte:head>
  <title>{$_('navigation.rules')} | {$_('app.name')}</title>
</svelte:head>

<AuthGuard requireAuth={false}>
  <div class="rules-container">
    <h1>{$_('app.name')} - {$_('navigation.rules')}</h1>
    
    <section>
      <h2>{$_('rules.overview')}</h2>
      <p>
        {$_('rules.overviewText')}
      </p>
    </section>
    
    <section>
      <h2>{$_('rules.gameplay')}</h2>
      <p>
        {$_('rules.gameplayText')}
      </p>
    </section>
    
    <section>
      <h2>{$_('rules.cardTypes')}</h2>
      <div class="cards-grid">
        <div class="card">
          <h3>9 - {$_('cards.princess')}</h3>
          <p>{$_('rules.princessDescription')}</p>
        </div>
        <div class="card">
          <h3>8 - {$_('cards.countess')}</h3>
          <p>{$_('rules.countessDescription')}</p>
        </div>
        <div class="card">
          <h3>7 - {$_('cards.king')}</h3>
          <p>{$_('rules.kingDescription')}</p>
        </div>
        <div class="card">
          <h3>6 - {$_('cards.chanceller')}</h3>
          <p>{$_('rules.chancellorDescription')}</p>
        </div>
        <div class="card">
          <h3>5 - {$_('cards.prince')}</h3>
          <p>{$_('rules.princeDescription')}</p>
        </div>
        <div class="card">
          <h3>4 - {$_('cards.handmaid')}</h3>
          <p>{$_('rules.handmaidDescription')}</p>
        </div>
        <div class="card">
          <h3>3 - {$_('cards.baron')}</h3>
          <p>{$_('rules.baronDescription')}</p>
        </div>
        <div class="card">
          <h3>2 - {$_('cards.priest')}</h3>
          <p>{$_('rules.priestDescription')}</p>
        </div>
        <div class="card">
          <h3>1 - {$_('cards.guard')}</h3>
          <p>{$_('rules.guardDescription')}</p>
        </div>
        <div class="card">
          <h3>0 - {$_('cards.spy')}</h3>
          <p>{$_('rules.spyDescription')}</p>
        </div>
      </div>
    </section>
    
    <section>
      <h2>{$_('rules.endOfRound')}</h2>
      <p>
        {$_('rules.endOfRoundText')}
      </p>
    </section>
    
    <section>
      <h2>{$_('rules.winningGame')}</h2>
      <p>
        {$_('rules.winningGameText')}
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
