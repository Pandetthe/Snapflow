<script lang="ts">
  import { Menu, X, LogOut, Folders, User as UserIcon, ExternalLink, Sun, Moon, Monitor, Github } from 'lucide-svelte';
  import { page } from '$app/state';
  import { ThemeToggle, UserMenu, GithubButton, Button, UserAvatar } from '$lib/ui/components';
  import { AuthService } from '$lib/features/auth/api/auth';
  import { apiClient } from '$lib/core/api.client';
  import { errorStore } from '$lib/ui/stores/error.svelte';
  import { theme } from '$lib/ui/stores/theme';
  import { cn } from '$lib/ui/utils';
  import type { User } from '$lib/features/users/api/users';
  import { avatarBust } from '$lib/features/users/avatarBust.svelte';
  import { Dialog } from 'bits-ui';

  interface Props {
    onMenuToggle?: () => void;
    isSidebarOpen?: boolean;
    user: User | null;
  }

  let { onMenuToggle, isSidebarOpen = false, user = null }: Props = $props();
  let mobileMenuOpen = $state(false);

  const authService = new AuthService(apiClient);

  type ThemeMode = 'light' | 'dark' | 'system';

  const themeOptions: { value: ThemeMode; label: string; icon: typeof Sun }[] = [
    { value: 'light', label: 'Light', icon: Sun },
    { value: 'dark', label: 'Dark', icon: Moon },
    { value: 'system', label: 'System', icon: Monitor }
  ];

  const currentTheme = $derived(themeOptions.find((o) => o.value === $theme) ?? themeOptions[2]);

  const navItems = [
    { href: '/boards', icon: Folders, text: 'Boards' },
    { href: '/profile', icon: UserIcon, text: 'Edit profile' }
  ];

  /*
    Mobile menu: every entry is the same 44px row (16px icon, 14px text) with its icon on the logo's edge,
    separated into groups by the same divider. The selected page and theme share one highlight.
  */
  const rowClass =
    'flex h-11 w-full items-center justify-start gap-3 rounded-lg px-3 text-sm font-medium text-gray-700 dark:text-gray-300';
  const selectedClass = 'bg-gray-100 text-gray-900 dark:bg-white/10 dark:text-white';

  function isCurrent(href: string) {
    return page.url.pathname === href || page.url.pathname.startsWith(`${href}/`);
  }

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

{#snippet divider()}
  <!-- Inset like the rows' content, so it ends where the buttons and icons do -->
  <div class="mx-3 my-2 h-px bg-gray-200 dark:bg-gray-800"></div>
{/snippet}

<header
  class="sticky top-0 z-40 flex w-full flex-col border-b border-gray-200 bg-white [view-transition-name:app-header] dark:border-gray-800 dark:bg-gray-900"
>
  <Dialog.Root open={isMobileMenuOpen()} onOpenChange={handleMenuToggle}>
    <div class="flex w-full items-center justify-between gap-2 px-6 py-2.5 sm:py-4 lg:px-8">
      <a
        href="/"
        class="flex shrink-0 items-center gap-2 rounded-md outline-none transition-all duration-200 focus-visible:outline-2 focus-visible:outline-brand-500 focus-visible:outline-offset-2"
      >
        <span class="text-xl font-bold text-gray-900 sm:text-2xl dark:text-white">Snapflow</span>
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
        class="relative md:hidden h-10 w-10 min-w-10 p-0 rounded-full flex items-center justify-center border-gray-200 bg-white text-gray-700 hover:bg-gray-50 active:scale-95 dark:border-gray-800 dark:bg-gray-900 dark:text-gray-400 dark:hover:bg-gray-800"
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
      class="absolute top-full left-0 z-30 h-screen w-full bg-gray-900/40 backdrop-blur-sm md:hidden dark:bg-gray-900/60 data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=open]:duration-300 data-[state=open]:ease-flow data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:duration-200 data-[state=closed]:ease-in"
    />

    <!-- px-3 plus the rows' px-3 puts icons and the avatar on the logo's 24px edge -->
    <Dialog.Content
      id="mobile-header-menu"
      class="absolute top-full left-0 z-40 max-h-[calc(100dvh-4rem)] w-full overflow-y-auto border-b border-gray-200 bg-white px-3 py-2 shadow-xl md:hidden dark:border-gray-800 dark:bg-gray-900 data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=open]:slide-in-from-top-2 data-[state=open]:duration-300 data-[state=open]:ease-flow data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:slide-out-to-top-2 data-[state=closed]:duration-200 data-[state=closed]:ease-in"
    >
      <Dialog.Title class="sr-only">Menu</Dialog.Title>

      {#if user}
        <div class="flex items-center gap-3 px-3 py-2">
          <UserAvatar
            src={user.avatarUrl ? `${user.avatarUrl}?v=${avatarBust.count}` : null}
            name={user.userName || user.email || 'User'}
            size={36}
            class="ring-2 ring-gray-100 dark:ring-gray-800"
          />
          <div class="min-w-0">
            <p class="truncate text-sm font-semibold text-gray-900 dark:text-white">{user.userName}</p>
            <p class="truncate text-xs text-gray-500 dark:text-gray-400">{user.email}</p>
          </div>
        </div>
      {:else}
        <div class="grid grid-cols-2 gap-2 px-3 py-1">
          <Button variant="primary" size="sm" href="/sign-in" class="h-11" onclick={closeMobileMenu}>
            Sign in
          </Button>
          <Button variant="outline" size="sm" href="/sign-up" class="h-11" onclick={closeMobileMenu}>
            Sign up
          </Button>
        </div>
      {/if}

      {@render divider()}

      <nav aria-label="Menu">
        <ul class="space-y-0.5">
          {#if user}
            {#each navItems as item (item.href)}
              <li>
                <Button
                  variant="ghost"
                  size="sm"
                  href={item.href}
                  onclick={closeMobileMenu}
                  startIcon={item.icon}
                  aria-current={isCurrent(item.href) ? 'page' : undefined}
                  class={cn(rowClass, isCurrent(item.href) && selectedClass)}
                >
                  {item.text}
                </Button>
              </li>
            {/each}
          {/if}
          <li>
            <Button
              variant="ghost"
              size="sm"
              href="https://github.com/pandetthe/Snapflow"
              target="_blank"
              rel="noopener noreferrer"
              onclick={closeMobileMenu}
              startIcon={Github}
              class={rowClass}
            >
              <span class="flex-1 text-left">GitHub</span>
              <ExternalLink size={14} class="text-gray-400 dark:text-gray-500" />
            </Button>
          </li>
          <li class={rowClass}>
            <currentTheme.icon size={16} />
            <span class="flex-1">Theme</span>
            <div
              role="radiogroup"
              aria-label="Theme"
              class="flex items-center gap-0.5 rounded-lg border border-gray-200 p-0.5 dark:border-gray-800"
            >
              {#each themeOptions as option (option.value)}
                <button
                  type="button"
                  role="radio"
                  aria-checked={$theme === option.value}
                  aria-label={option.label}
                  onclick={() => theme.set(option.value)}
                  class={cn(
                    'flex h-8 w-8 cursor-pointer items-center justify-center rounded-md text-gray-500 transition hover:text-gray-900 focus-visible:outline-2 focus-visible:outline-brand-500 focus-visible:outline-offset-2 active:scale-95 dark:text-gray-400 dark:hover:text-white',
                    $theme === option.value && selectedClass
                  )}
                >
                  <option.icon size={16} />
                </button>
              {/each}
            </div>
          </li>
        </ul>
      </nav>

      {#if user}
        {@render divider()}

        <Button variant="ghost" size="sm" onclick={handleSignOut} startIcon={LogOut} class={rowClass}>
          Sign out
        </Button>
      {/if}
    </Dialog.Content>
  </Dialog.Root>
</header>
