<script lang="ts">
  import Login from '$lib/components/Login.svelte';
  import AuthGuard from '$lib/components/AuthGuard.svelte';
</script>

<svelte:head>
  <title>Login - Love Letter</title>
</svelte:head>

<AuthGuard requireAuth={false} redirectTo="/dashboard">
  <Login />
</AuthGuard>
