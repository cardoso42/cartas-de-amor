<!-- WoodenTable.svelte -->
<div class="table-container">
  <div class="table">
    <slot />
  </div>
</div>

<style>
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

  /* Responsive design for smaller screens */
  @media (max-width: 768px) {
    .table-container {
      width: 400px;
      height: 400px;
    }
  }
</style>
