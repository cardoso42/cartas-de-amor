<script lang="ts">
  import Login from '$lib/components/auth/Login.svelte';
  import AuthGuard from '$lib/components/auth/AuthGuard.svelte';
  import { _ } from 'svelte-i18n';
</script>

<svelte:head>
  <title>{$_('auth.login')} - {$_('app.name')}</title>
</svelte:head>

<AuthGuard requireAuth={false} redirectTo="/dashboard">
  <Login />
</AuthGuard>
