<script lang="ts">
  import { onMount } from 'svelte';
  import AuthGuard from '$lib/components/AuthGuard.svelte';
  import { goto } from '$app/navigation';

  let formData = {
    username: '',
    email: '',
    password: '',
    confirmPassword: ''
  };

  let isLoading = false;
  let errorMessage = '';
  let successMessage = '';

  async function handleRegister() {
    // Reset messages
    errorMessage = '';
    successMessage = '';
    
    // Basic validation
    if (!formData.username || !formData.email || !formData.password) {
      errorMessage = 'Please fill in all required fields';
      return;
    }
    
    if (formData.password !== formData.confirmPassword) {
      errorMessage = 'Passwords do not match';
      return;
    }
    
    isLoading = true;
    
    try {
      // In a real implementation, this would call an API to register the user
      // For now, we're just simulating a successful registration
      await new Promise(resolve => setTimeout(resolve, 1000)); // Simulate API call
      
      successMessage = 'Registration successful! Redirecting to login...';
      
      // Redirect to login page after 2 seconds
      setTimeout(() => {
        goto('/login');
      }, 2000);
      
    } catch (error) {
      errorMessage = 'An error occurred during registration. Please try again.';
      console.error('Registration error:', error);
    } finally {
      isLoading = false;
    }
  }

  onMount(() => {
    // Any initialization code can go here
  });
</script>

<svelte:head>
  <title>Register | Love Letter</title>
</svelte:head>

<AuthGuard requireAuth={false} redirectTo="/dashboard">
  <div class="register-container">
    <div class="register-card">
      <div class="register-header">
        <h1>Create Account</h1>
        <p>Join Love Letter and start playing!</p>
      </div>
      
      {#if errorMessage}
        <div class="message error">
          {errorMessage}
        </div>
      {/if}
      
      {#if successMessage}
        <div class="message success">
          {successMessage}
        </div>
      {/if}
      
      <form on:submit|preventDefault={handleRegister} class="register-form">
        <div class="form-group">
          <label for="username">Username*</label>
          <input 
            type="text" 
            id="username" 
            placeholder="Choose a username" 
            bind:value={formData.username}
            required
          />
        </div>
        
        <div class="form-group">
          <label for="email">Email Address*</label>
          <input 
            type="email" 
            id="email" 
            placeholder="Enter your email" 
            bind:value={formData.email}
            required
          />
        </div>
        
        <div class="form-group">
          <label for="password">Password*</label>
          <input 
            type="password" 
            id="password" 
            placeholder="Create a password" 
            bind:value={formData.password}
            required
            minlength="8"
          />
          <small>Password must be at least 8 characters</small>
        </div>
        
        <div class="form-group">
          <label for="confirmPassword">Confirm Password*</label>
          <input 
            type="password" 
            id="confirmPassword" 
            placeholder="Confirm your password" 
            bind:value={formData.confirmPassword}
            required
          />
        </div>
        
        <button type="submit" class="register-button" disabled={isLoading}>
          {isLoading ? 'Creating Account...' : 'Register'}
        </button>
      </form>
      
      <div class="login-link">
        Already have an account? <a href="/login">Login here</a>
      </div>
    </div>
  </div>
</AuthGuard>

<style>
  .register-container {
    display: flex;
    justify-content: center;
    align-items: center;
    padding: 2rem 1rem;
  }
  
  .register-card {
    background-color: white;
    border-radius: 8px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    padding: 2rem;
    width: 100%;
    max-width: 500px;
  }
  
  .register-header {
    text-align: center;
    margin-bottom: 2rem;
  }
  
  .register-header h1 {
    color: #9c27b0;
    margin-bottom: 0.5rem;
  }
  
  .register-header p {
    color: #666;
  }
  
  .message {
    padding: 1rem;
    border-radius: 4px;
    margin-bottom: 1.5rem;
    font-weight: 500;
  }
  
  .message.error {
    background-color: #ffebee;
    color: #c62828;
    border: 1px solid #ef9a9a;
  }
  
  .message.success {
    background-color: #e8f5e9;
    color: #2e7d32;
    border: 1px solid #a5d6a7;
  }
  
  .register-form {
    display: flex;
    flex-direction: column;
    gap: 1.5rem;
  }
  
  .form-group {
    display: flex;
    flex-direction: column;
  }
  
  label {
    font-weight: 500;
    margin-bottom: 0.5rem;
    color: #333;
  }
  
  input {
    padding: 0.75rem;
    border: 1px solid #ddd;
    border-radius: 4px;
    font-size: 1rem;
  }
  
  input:focus {
    border-color: #9c27b0;
    outline: none;
    box-shadow: 0 0 0 2px rgba(156, 39, 176, 0.2);
  }
  
  small {
    color: #666;
    margin-top: 0.25rem;
    font-size: 0.85rem;
  }
  
  .register-button {
    background-color: #9c27b0;
    color: white;
    border: none;
    border-radius: 4px;
    padding: 0.75rem;
    font-size: 1rem;
    font-weight: 500;
    cursor: pointer;
    margin-top: 1rem;
    transition: background-color 0.3s;
  }
  
  .register-button:hover {
    background-color: #7b1fa2;
  }
  
  .register-button:disabled {
    background-color: #e1bee7;
    cursor: not-allowed;
  }
  
  .login-link {
    text-align: center;
    margin-top: 1.5rem;
    color: #666;
  }
  
  .login-link a {
    color: #9c27b0;
    text-decoration: none;
    font-weight: 500;
  }
  
  .login-link a:hover {
    text-decoration: underline;
  }
</style>
