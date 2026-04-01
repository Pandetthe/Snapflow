<script lang="ts">
  import { Menu, X, LogOut } from 'lucide-svelte';
  import { ThemeToggle, UserMenu, GithubButton, Button } from '$lib/ui/components';
  import { AuthService } from '$lib/features/auth/api/auth';
  import { apiClient } from '$lib/core/api.client';
  import { errorStore } from '$lib/ui/stores/error';
  import type { User } from '$lib/features/users/api/users';
  import { Dialog } from 'bits-ui';

  interface Props {
    onMenuToggle?: () => void;
    isSidebarOpen?: boolean;
    user: User | null;
  }

  let { onMenuToggle, isSidebarOpen = false, user = null }: Props = $props();
  let mobileMenuOpen = $state(false);

  const authService = new AuthService(apiClient);

  async function handleSignOut() {
    try {
      const response = await authService.signOut();
      if (response.ok) {
        closeMobileMenu();
        window.location.href = '/';
      } else {
        errorStore.addError(null, 'Problem with connection to the server');
      }
    } catch (err) {
      if (err instanceof Error) {
        if (err.message === 'Failed to fetch') {
          errorStore.addError('Web.ConnectionProblem', 'Problem with connection to the server');
        } else {
          errorStore.addError(err.name, err.message);
        }
      } else {
        errorStore.addError(null, 'Unknown error occurred during sign out');
      }
    }
  }

  function isMobileMenuOpen() {
    return onMenuToggle ? isSidebarOpen : mobileMenuOpen;
  }

  function handleMenuToggle(isOpen: boolean) {
    if (onMenuToggle) {
      if (isOpen !== isSidebarOpen) onMenuToggle();
      return;
    }
    mobileMenuOpen = isOpen;
  }

  function toggleMobileMenu() {
    handleMenuToggle(!isMobileMenuOpen());
  }

  function closeMobileMenu() {
    handleMenuToggle(false);
  }
</script>

<header
  class="sticky top-0 z-40 flex w-full flex-col border-b border-gray-200 bg-white dark:border-gray-800 dark:bg-gray-900"
>
  <Dialog.Root open={isMobileMenuOpen()} onOpenChange={handleMenuToggle}>
    <div class="flex w-full items-center justify-between gap-2 px-5 py-3 sm:px-6 sm:py-4 lg:px-8">
      <a
      href="/"
      class="flex shrink-0 items-center gap-2 rounded-md outline-none transition-all duration-200 focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 dark:focus-visible:ring-offset-gray-950"
    >
      <span class="text-2xl font-bold text-gray-900 dark:text-white">Snapflow</span>
    </a>

    <div class="hidden min-w-0 items-center justify-end gap-2 md:flex">
      <div class="flex shrink-0 items-center gap-2">
        <GithubButton />
        <ThemeToggle />
      </div>

      <div
        class="flex shrink-0 items-center border-l border-gray-200 pl-2 sm:pl-4 dark:border-gray-800"
      >
        <UserMenu {handleSignOut} {user} />
      </div>
    </div>

    <Button
      variant="outline"
      onclick={toggleMobileMenu}
      class="relative md:hidden h-11 w-11 p-0 rounded-full flex items-center justify-center border-gray-200 bg-white text-gray-700 hover:bg-gray-50 active:scale-95 dark:border-gray-800 dark:bg-gray-900 dark:text-gray-400 dark:hover:bg-gray-800"
      aria-label={isMobileMenuOpen() ? 'Close mobile menu' : 'Open mobile menu'}
      aria-expanded={isMobileMenuOpen()}
      aria-controls="mobile-header-menu"
    >
      <div
        class="absolute flex items-center justify-center transition-all duration-200 {isMobileMenuOpen() ? 'rotate-90 scale-0 opacity-0' : 'rotate-0 scale-100 opacity-100'}"
      >
        <Menu size={20} />
      </div>
      <div
        class="absolute flex items-center justify-center transition-all duration-200 {isMobileMenuOpen() ? 'rotate-0 scale-100 opacity-100' : '-rotate-90 scale-0 opacity-0'}"
      >
        <X size={20} />
      </div>
    </Button>
  </div>

  <Dialog.Overlay
    class="absolute top-full left-0 z-30 h-screen w-full bg-gray-900/40 backdrop-blur-sm md:hidden dark:bg-gray-900/60 duration-200 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:animate-in data-[state=open]:fade-in-0"
  />

  <Dialog.Content
    class="absolute top-full left-0 z-40 w-full border-b border-gray-200 bg-white px-5 pt-3 pb-5 shadow-xl md:hidden dark:border-gray-800 dark:bg-gray-900 sm:px-6 duration-250 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:slide-out-to-top-4 data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=open]:slide-in-from-top-4"
  >
    <Dialog.Title class="sr-only">Mobile menu</Dialog.Title>
    <div
      class="mb-3 rounded-xl border border-gray-200 bg-white p-3 dark:border-gray-800 dark:bg-gray-900"
    >
        <UserMenu {handleSignOut} {user} mobile onAction={closeMobileMenu} />
      </div>

      <div class="grid {user ? 'grid-cols-3' : 'grid-cols-2'} gap-2 rounded-xl border border-gray-200 bg-white p-3 dark:border-gray-800 dark:bg-gray-900">
        <ThemeToggle showLabel />
        <GithubButton showLabel onclick={closeMobileMenu} />
        {#if user}
          <div class="text-gray-700 dark:text-gray-300">
            <Button
              variant="outline"
              onclick={handleSignOut}
              class="w-full justify-start font-medium"
              startIcon={LogOut}
            >
              Sign out
            </Button>
          </div>
        {/if}
      </div>
  </Dialog.Content>
</Dialog.Root>
</header>
