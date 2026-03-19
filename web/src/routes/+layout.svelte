<script lang="ts">
  import { page } from '$app/state';
  import '../app.css';
  import favicon from '$lib/assets/favicon.svg';
  import { AppHeader } from '$lib/ui/components';
  import { AuthService } from '$lib/features/auth/api/auth';
  import { apiClient } from '$lib/core/api.client';
  import ErrorModal from '$lib/ui/components/ErrorModal.svelte';
  import { errorStore } from '$lib/ui/stores/error';
  import type { AppError } from '$lib/core/types/app.js';
  import { theme } from '$lib/ui/stores/theme';
  import { onDestroy, onMount } from 'svelte';
  import { pwaInfo } from 'virtual:pwa-info';

  let { children, data } = $props();
  let authService = new AuthService(apiClient);

  onMount(async () => {
    if (pwaInfo) {
      const { registerSW } = await import('virtual:pwa-register');
      registerSW({
        immediate: true,
        onRegistered(r) {
          console.info(`SW Registered: ${r}`);
        },
        onRegisterError(error) {
          console.error('SW registration error', error);
        }
      });
    }
  });

  const themeColor = $derived($theme === 'dark' ? '#111827' : '#f9fafb');
  const webManifestLink = $derived(pwaInfo ? pwaInfo.webManifest.linkTag : '');

  const publicRoutes = [
    '/sign-in',
    '/sign-up',
    '/forgot-password',
    '/reset-password',
    '/email-confirmed'
  ];
  const isPublicRoute = $derived(publicRoutes.includes(page.url.pathname));

  let showErrorModal = $state(false);
  let modalErrors = $state([] as AppError[]);

  let unsubscribe: () => void;
  onMount(() => {
    unsubscribe = errorStore.subscribe((errors) => {
      if (errors && errors.length > 0) {
        modalErrors = [...modalErrors, ...errors];
        showErrorModal = true;
        errorStore.reset();
      }
    });
  });

  let isSidebarOpen = $state(false);

  onDestroy(() => unsubscribe?.());
</script>

<svelte:head>
  <link rel="icon" href={favicon} />
  {@html webManifestLink}
  <meta name="theme-color" content={themeColor} />
  <meta name="apple-mobile-web-app-capable" content="yes" />
  <meta name="apple-mobile-web-app-status-bar-style" content="default" />
  <link rel="apple-touch-icon" href={favicon} />
</svelte:head>

<div class="relative flex min-h-screen flex-col bg-gray-100 dark:bg-gray-900">
  {#if !isPublicRoute}
    <AppHeader
      onMenuToggle={() => (isSidebarOpen = !isSidebarOpen)}
      {isSidebarOpen}
      user={data?.user}
    />
  {/if}

  <main class="flex grow flex-col">
    {@render children()}

    <ErrorModal bind:isOpen={showErrorModal} bind:errors={modalErrors} />
  </main>
</div>
