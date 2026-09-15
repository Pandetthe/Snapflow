<script lang="ts">
  import { page } from '$app/state';
  import { onNavigate } from '$app/navigation';
  import '../app.css';
  import favicon from '$lib/assets/favicon.svg';
  import { AppHeader, ErrorModal, NoticeModal } from '$lib/ui/components';
  import { errorStore } from '$lib/ui/stores/error.svelte';
  import { noticeStore, type AppNotice } from '$lib/ui/stores/notice.svelte';
  import type { AppError } from '$lib/core/types/app.js';
  import { theme } from '$lib/ui/stores/theme';
  import { onMount, untrack } from 'svelte';
  import { pwaInfo } from 'virtual:pwa-info';
  let { children, data } = $props();

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

  // One transition for every page change (View Transitions API, styled in app.css); no-op where unsupported.
  onNavigate((navigation) => {
    if (!document.startViewTransition) return;

    return new Promise((resolve) => {
      document.startViewTransition(async () => {
        resolve();
        await navigation.complete;
      });
    });
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

  // The modal keeps its errors after it closes, so it does not empty itself while it animates
  // away; a new batch replaces them here instead. Errors arriving while it is open pile on.
  $effect(() => {
    if (errorStore.errors.length > 0) {
      const incoming = errorStore.errors;
      untrack(() => {
        modalErrors = showErrorModal ? [...modalErrors, ...incoming] : [...incoming];
        showErrorModal = true;
      });
      errorStore.reset();
    }
  });

  let showNoticeModal = $state(false);
  let currentNotice = $state<AppNotice | undefined>(undefined);

  // Shown one at a time, and from the layout, so a notice survives the navigation that raised it.
  $effect(() => {
    if (noticeStore.notices.length > 0) {
      currentNotice = noticeStore.notices[0];
      showNoticeModal = true;
      noticeStore.reset();
    }
  });

  let isSidebarOpen = $state(false);
</script>

<svelte:head>
  <link rel="icon" href={favicon} />
  {@html webManifestLink}
</svelte:head>

<div class="relative flex min-h-screen flex-col bg-white dark:bg-gray-900">
  {#if !isPublicRoute}
    <a
      href="#main-content"
      class="pointer-events-none absolute top-4 left-1/2 z-50 -translate-x-1/2 -translate-y-[150%] rounded-full border border-gray-200 bg-white px-5 py-2.5 text-sm font-semibold text-brand-600 opacity-0 shadow-xl transition-all duration-300 focus:pointer-events-auto focus:translate-y-0 focus:opacity-100 focus:outline-2 focus:outline-brand-500 focus:outline-offset-2 dark:border-gray-800 dark:bg-gray-900 dark:text-brand-400"
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

    <NoticeModal bind:isOpen={showNoticeModal} bind:notice={currentNotice} />
  </main>
</div>
