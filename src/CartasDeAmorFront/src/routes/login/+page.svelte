<script lang="ts">
  import Login from '$lib/components/auth/Login.svelte';
  import AuthGuard from '$lib/components/auth/AuthGuard.svelte';
</script>

<svelte:head>
  <title>Login - Love Letter</title>
</svelte:head>

<AuthGuard requireAuth={false} redirectTo="/dashboard">
  <Login />
</AuthGuard>
