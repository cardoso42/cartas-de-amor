<script lang="ts">
  import AuthGuard from '$lib/components/AuthGuard.svelte';
  import { onMount } from 'svelte';
  
  // Sample user data (would be fetched from an API in a real implementation)
  let userData = {
    username: 'Player1',
    email: 'player1@example.com',
    joinedDate: '2025-03-15',
    totalGames: 24,
    wins: 10,
    avatar: null
  };
  
  // Form data for profile updates
  let formData = {
    username: userData.username,
    email: userData.email,
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };
  
  let isEditing = false;
  let message = '';
  let messageType = '';
  
  function toggleEdit() {
    isEditing = !isEditing;
    
    if (!isEditing) {
      // Reset form if cancelling
      formData.username = userData.username;
      formData.email = userData.email;
      formData.currentPassword = '';
      formData.newPassword = '';
      formData.confirmPassword = '';
    }
  }
  
  function saveProfile() {
    // Password change validation
    if (formData.newPassword) {
      if (formData.newPassword !== formData.confirmPassword) {
        message = 'New passwords do not match';
        messageType = 'error';
        return;
      }
      
      if (!formData.currentPassword) {
        message = 'Current password is required to set a new password';
        messageType = 'error';
        return;
      }
    }
    
    // In a real implementation, this would call an API to update the profile
    userData = {
      ...userData,
      username: formData.username,
      email: formData.email
    };
    
    message = 'Profile updated successfully';
    messageType = 'success';
    isEditing = false;
    
    // Clear message after 3 seconds
    setTimeout(() => {
      message = '';
      messageType = '';
    }, 3000);
  }
  
  onMount(() => {
    // In a real implementation, this would fetch user data from the server
    console.log('Fetching user profile data...');
  });
</script>

<svelte:head>
  <title>My Profile | Love Letter</title>
</svelte:head>

<AuthGuard requireAuth={true} redirectTo="/login">
  <div class="profile-container">
    <h1>My Profile</h1>
    
    {#if message}
      <div class="message {messageType}">
        {message}
      </div>
    {/if}
    
    <div class="profile-card">
      <div class="profile-header">
        <div class="avatar-container">
          <!-- Default avatar if none uploaded -->
          <div class="avatar">
            {userData.username.charAt(0).toUpperCase()}
          </div>
          
          {#if isEditing}
            <button class="avatar-upload-btn" title="Upload avatar (not implemented)">
              Change
            </button>
          {/if}
        </div>
        
        <div class="stats">
          <div class="stat">
            <span class="stat-value">{userData.totalGames}</span>
            <span class="stat-label">Games</span>
          </div>
          <div class="stat">
            <span class="stat-value">{userData.wins}</span>
            <span class="stat-label">Wins</span>
          </div>
          <div class="stat">
            <span class="stat-value">{Math.round((userData.wins / userData.totalGames) * 100)}%</span>
            <span class="stat-label">Win Rate</span>
          </div>
        </div>
      </div>
      
      <div class="profile-content">
        {#if isEditing}
          <div class="form">
            <div class="form-group">
              <label for="username">Username</label>
              <input 
                type="text" 
                id="username" 
                bind:value={formData.username} 
                required
              />
            </div>
            
            <div class="form-group">
              <label for="email">Email</label>
              <input 
                type="email" 
                id="email" 
                bind:value={formData.email} 
                required
              />
            </div>
            
            <div class="form-group">
              <label for="currentPassword">Current Password</label>
              <input 
                type="password" 
                id="currentPassword" 
                bind:value={formData.currentPassword} 
              />
            </div>
            
            <div class="form-group">
              <label for="newPassword">New Password</label>
              <input 
                type="password" 
                id="newPassword" 
                bind:value={formData.newPassword} 
              />
            </div>
            
            <div class="form-group">
              <label for="confirmPassword">Confirm New Password</label>
              <input 
                type="password" 
                id="confirmPassword" 
                bind:value={formData.confirmPassword} 
              />
            </div>
            
            <div class="form-actions">
              <button class="secondary" on:click={toggleEdit}>Cancel</button>
              <button on:click={saveProfile}>Save Changes</button>
            </div>
          </div>
        {:else}
          <div class="profile-info">
            <div class="info-row">
              <span class="info-label">Username</span>
              <span class="info-value">{userData.username}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Email</span>
              <span class="info-value">{userData.email}</span>
            </div>
            <div class="info-row">
              <span class="info-label">Member Since</span>
              <span class="info-value">{new Date(userData.joinedDate).toLocaleDateString()}</span>
            </div>
            
            <button on:click={toggleEdit}>Edit Profile</button>
          </div>
        {/if}
      </div>
    </div>
  </div>
</AuthGuard>

<style>
  .profile-container {
    max-width: 700px;
    margin: 0 auto;
  }
  
  h1 {
    color: #9c27b0;
    margin-bottom: 2rem;
  }
  
  .message {
    padding: 1rem;
    border-radius: 4px;
    margin-bottom: 1rem;
    font-weight: 500;
  }
  
  .message.success {
    background-color: #e8f5e9;
    color: #2e7d32;
    border: 1px solid #a5d6a7;
  }
  
  .message.error {
    background-color: #ffebee;
    color: #c62828;
    border: 1px solid #ef9a9a;
  }
  
  .profile-card {
    background-color: white;
    border-radius: 8px;
    overflow: hidden;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  }
  
  .profile-header {
    background-color: #9c27b0;
    color: white;
    padding: 2rem;
    display: flex;
    align-items: center;
    justify-content: space-between;
  }
  
  .avatar-container {
    position: relative;
  }
  
  .avatar {
    width: 80px;
    height: 80px;
    border-radius: 50%;
    background-color: #7b1fa2;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 2rem;
    font-weight: bold;
    border: 3px solid white;
  }
  
  .avatar-upload-btn {
    position: absolute;
    bottom: 0;
    right: 0;
    background-color: rgba(255, 255, 255, 0.9);
    color: #333;
    border: none;
    border-radius: 12px;
    padding: 0.3rem 0.6rem;
    font-size: 0.8rem;
    cursor: pointer;
  }
  
  .stats {
    display: flex;
    gap: 2rem;
  }
  
  .stat {
    display: flex;
    flex-direction: column;
    align-items: center;
  }
  
  .stat-value {
    font-size: 1.5rem;
    font-weight: bold;
  }
  
  .stat-label {
    font-size: 0.875rem;
    opacity: 0.8;
  }
  
  .profile-content {
    padding: 2rem;
  }
  
  .info-row {
    display: flex;
    margin-bottom: 1rem;
    padding-bottom: 0.5rem;
    border-bottom: 1px solid #eee;
  }
  
  .info-label {
    flex: 1;
    color: #666;
    font-weight: 500;
  }
  
  .info-value {
    flex: 2;
  }
  
  .form-group {
    margin-bottom: 1.5rem;
  }
  
  label {
    display: block;
    margin-bottom: 0.5rem;
    color: #555;
    font-weight: 500;
  }
  
  input {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #ddd;
    border-radius: 4px;
    font-size: 1rem;
  }
  
  .form-actions {
    display: flex;
    gap: 1rem;
    justify-content: flex-end;
    margin-top: 2rem;
  }
  
  /* Button styles now come from global styles */
</style>
