<script lang="ts">
  import AuthGuard from '$lib/components/AuthGuard.svelte';
  import { onMount } from 'svelte';
  import { 
    getCurrentUserProfile, 
    deleteAccount, 
    type UserData 
  } from '$lib/services/userService';
  import { goto } from '$app/navigation';
  import auth from '$lib/stores/authStore';
  
  // User data state
  let userData: UserData = {
    username: '',
    email: '',
    joinedDate: '',
  };
  
  let message = '';
  let messageType = '';
  let showDeleteConfirm = false;
  
  async function confirmDeleteAccount() {
    if (userData.email) {
      const result = await deleteAccount(userData.email);
      
      if (result.success) {
        // Logout the user
        auth.logout();
        
        // Redirect to home page
        goto('/');
      } else {
        message = result.message;
        messageType = 'error';
        showDeleteConfirm = false;
      }
    }
  }
  
  function toggleDeleteConfirm() {
    showDeleteConfirm = !showDeleteConfirm;
  }
  
  function logout() {
    auth.logout();
    goto('/login');
  }
  
  onMount(() => {
    // Fetch the user profile data
    const profile = getCurrentUserProfile();
    
    if (profile) {
      userData = profile;
    } else {
      // If profile couldn't be loaded, show error
      message = 'Could not load profile data. Please try again later.';
      messageType = 'error';
    }
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
        </div>
      </div>
      
      <div class="profile-content">
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
              <span class="info-value">{userData.joinedDate ? new Date(userData.joinedDate).toLocaleDateString() : 'N/A'}</span>
            </div>
            
            <div class="button-group">
              <button class="danger" on:click={toggleDeleteConfirm}>Delete Account</button>
            </div>
            
            {#if showDeleteConfirm}
              <div class="delete-confirmation">
                <p>Are you sure you want to delete your account? This action cannot be undone.</p>
                <div class="button-group">
                  <button class="secondary" on:click={toggleDeleteConfirm}>Cancel</button>
                  <button class="danger" on:click={confirmDeleteAccount}>Confirm Delete</button>
                </div>
              </div>
            {/if}
          </div>
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
  
  .form-actions {
    display: flex;
    gap: 1rem;
    justify-content: flex-end;
    margin-top: 2rem;
  }
  
  .button-group {
    display: flex;
    gap: 1rem;
    margin-top: 1.5rem;
  }
  
  .delete-confirmation {
    margin-top: 1.5rem;
    padding: 1rem;
    border: 1px solid #ffcdd2;
    background-color: #ffebee;
    border-radius: 4px;
    color: #c62828;
  }
  
  .danger {
    background-color: #f44336;
    color: white;
  }
  
  .danger:hover {
    background-color: #d32f2f;
  }
</style>
