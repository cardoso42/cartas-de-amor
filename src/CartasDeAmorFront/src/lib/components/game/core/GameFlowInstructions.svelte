<!-- GameFlowInstructions.svelte -->
<script lang="ts">
  // Props
  export let gameFlowState: 'idle' | 'showing_rules' | 'selecting_player' | 'selecting_card_type' = 'idle';
  export let currentStep: number = 0;
  export let totalSteps: number = 0;
</script>

{#if gameFlowState !== 'idle'}
  <div class="flow-instructions">
    {#if gameFlowState === 'selecting_player'}
      <div class="instruction-bubble">
        <strong>Step {currentStep + 1} of {totalSteps}:</strong> Click on a player to target them
      </div>
    {:else if gameFlowState === 'selecting_card_type'}
      <div class="instruction-bubble">
        <strong>Step {currentStep + 1} of {totalSteps}:</strong> Click on a card type to select it
      </div>
    {/if}
  </div>
{/if}

<style>
  .flow-instructions {
    position: absolute;
    top: 20px;
    left: 50%;
    transform: translateX(-50%);
    z-index: 20;
  }
  
  .instruction-bubble {
    background: rgba(0, 0, 0, 0.9);
    color: white;
    padding: 0.75rem 1.5rem;
    border-radius: 20px;
    border: 2px solid #ffd700;
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.5);
    text-align: center;
    font-size: 0.9rem;
    animation: instructionPulse 2s ease-in-out infinite;
  }
  
  @keyframes instructionPulse {
    0%, 100% { 
      box-shadow: 0 4px 15px rgba(0, 0, 0, 0.5);
    }
    50% { 
      box-shadow: 0 4px 20px rgba(255, 215, 0, 0.3);
    }
  }
</style>
