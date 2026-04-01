<script lang="ts">
  import { page } from '$app/state';
  import '../app.css';
  import favicon from '$lib/assets/favicon.svg';
  import { AppHeader, ErrorModal } from '$lib/ui/components';
  import { errorStore } from '$lib/ui/stores/error';
  import type { AppError } from '$lib/core/types/app.js';
  import { theme } from '$lib/ui/stores/theme';
  import { onDestroy, onMount } from 'svelte';
  import { pwaInfo } from 'virtual:pwa-info';
  let { children, data } = $props();

  onDestroy(() => {});
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
</svelte:head>

<div class="relative flex min-h-screen flex-col bg-white dark:bg-gray-900">
  {#if !isPublicRoute}
    <a
      href="#main-content"
      class="pointer-events-none absolute top-4 left-1/2 z-50 -translate-x-1/2 -translate-y-[150%] rounded-full border border-gray-200 bg-white px-5 py-2.5 text-sm font-semibold text-brand-600 opacity-0 shadow-xl transition-all duration-300 focus:pointer-events-auto focus:translate-y-0 focus:opacity-100 focus:outline-none focus:ring-2 focus:ring-brand-500 focus:ring-offset-2 dark:border-gray-800 dark:bg-gray-900 dark:text-brand-400 dark:focus:ring-offset-gray-950"
    >
      Skip to main content
    </a>

    <AppHeader
      onMenuToggle={() => (isSidebarOpen = !isSidebarOpen)}
      {isSidebarOpen}
      user={data?.user}
    />
  {/if}

  <main id="main-content" class="flex grow flex-col focus:outline-none" tabindex="-1">
    {@render children()}

    <ErrorModal bind:isOpen={showErrorModal} bind:errors={modalErrors} />
  </main>
</div>
