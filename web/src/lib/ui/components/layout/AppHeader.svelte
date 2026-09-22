<script lang="ts">
  import {
    Menu,
    X,
    LogOut,
    Folders,
    User as UserIcon,
    ExternalLink,
    Sun,
    Moon,
    Monitor,
    SlidersHorizontal
  } from '@lucide/svelte';
  import { page } from '$app/state';
  import { resolve } from '$app/paths';
  import {
    ThemeToggle,
    UserMenu,
    GithubButton,
    GithubIcon,
    Button,
    UserAvatar
  } from '$lib/ui/components';
  import { AuthService } from '$lib/features/auth/api/auth';
  import { apiClient } from '$lib/core/api.client';
  import { errorStore } from '$lib/ui/stores/error.svelte';
  import { theme } from '$lib/ui/stores/theme.svelte';
  import { cn } from '$lib/ui/utils';
  import type { User } from '$lib/features/users/api/users';
  import { avatarBust } from '$lib/features/users/avatarBust.svelte';
  import { Dialog, ToggleGroup } from 'bits-ui';

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

  const currentTheme = $derived(
    themeOptions.find((o) => o.value === theme.current) ?? themeOptions[2]
  );

  const navItems = [
    { href: '/', icon: Folders, text: 'Boards' },
    { href: '/profile', icon: UserIcon, text: 'Edit profile' },
    { href: '/settings', icon: SlidersHorizontal, text: 'Settings' }
  ];

  const rowClass =
    'flex h-11 w-full items-center justify-start gap-3 rounded-lg px-3 text-sm font-medium text-gray-700 dark:text-gray-300';
  const selectedClass = 'bg-gray-100 text-gray-900 dark:bg-white/10 dark:text-white';

  function isCurrent(href: string) {
    if (href === '/') return page.url.pathname === '/' || page.url.pathname.startsWith('/boards/');
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
  <div class="mx-3 my-2 h-px bg-gray-200 dark:bg-gray-800"></div>
{/snippet}

<header
  class="sticky top-0 z-40 flex w-full flex-col border-b border-gray-200 bg-white [view-transition-name:app-header] dark:border-gray-800 dark:bg-gray-900"
>
  <Dialog.Root open={isMobileMenuOpen()} onOpenChange={handleMenuToggle}>
    <div class="flex w-full items-center justify-between gap-2 px-6 py-2.5 sm:py-4 lg:px-8">
      <a
        href={resolve('/')}
        class="flex shrink-0 items-center gap-2 rounded-md transition-all duration-200 outline-none focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500"
      >
        <span class="text-xl font-bold text-gray-900 sm:text-2xl dark:text-white">Snapflow</span>
      </a>

      <div class="hidden min-w-0 items-center justify-end gap-2 md:flex">
        <div class="flex shrink-0 items-center gap-2">
          <GithubButton />
          {#if !user}
            <ThemeToggle />
          {/if}
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
        class="relative flex h-10 w-10 min-w-10 items-center justify-center rounded-full border-gray-200 bg-white p-0 text-gray-700 hover:bg-gray-50 active:scale-95 md:hidden dark:border-gray-800 dark:bg-gray-900 dark:text-gray-400 dark:hover:bg-gray-800"
        aria-label={isMobileMenuOpen() ? 'Close mobile menu' : 'Open mobile menu'}
        aria-expanded={isMobileMenuOpen()}
        aria-controls="mobile-header-menu"
      >
        <div
          class="absolute flex items-center justify-center transition-all duration-200 {isMobileMenuOpen()
            ? 'scale-0 rotate-90 opacity-0'
            : 'scale-100 rotate-0 opacity-100'}"
        >
          <Menu size={20} />
        </div>
        <div
          class="absolute flex items-center justify-center transition-all duration-200 {isMobileMenuOpen()
            ? 'scale-100 rotate-0 opacity-100'
            : 'scale-0 -rotate-90 opacity-0'}"
        >
          <X size={20} />
        </div>
      </Button>
    </div>

    <Dialog.Overlay
      class="absolute top-full left-0 z-30 h-screen w-full bg-gray-900/40 backdrop-blur-sm data-[state=closed]:duration-200 data-[state=closed]:ease-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:duration-300 data-[state=open]:ease-flow data-[state=open]:animate-in data-[state=open]:fade-in-0 md:hidden dark:bg-gray-900/60"
    />

    <Dialog.Content
      id="mobile-header-menu"
      class="absolute top-full left-0 z-40 max-h-[calc(100dvh-4rem)] w-full overflow-y-auto border-b border-gray-200 bg-white px-3 py-2 shadow-xl data-[state=closed]:duration-200 data-[state=closed]:ease-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:slide-out-to-top-2 data-[state=open]:duration-300 data-[state=open]:ease-flow data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=open]:slide-in-from-top-2 md:hidden dark:border-gray-800 dark:bg-gray-900"
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
            <p class="truncate text-sm font-semibold text-gray-900 dark:text-white">
              {user.userName}
            </p>
            <p class="truncate text-xs text-gray-500 dark:text-gray-400">{user.email}</p>
          </div>
        </div>
      {:else}
        <div class="grid grid-cols-2 gap-2 px-3 py-1">
          <Button
            variant="primary"
            size="sm"
            href="/sign-in"
            class="h-11"
            onclick={closeMobileMenu}
          >
            Sign in
          </Button>
          <Button
            variant="outline"
            size="sm"
            href="/sign-up"
            class="h-11"
            onclick={closeMobileMenu}
          >
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
              startIcon={GithubIcon}
              class={rowClass}
            >
              <span class="flex-1 text-left">GitHub</span>
              <ExternalLink size={14} class="text-gray-400 dark:text-gray-500" />
            </Button>
          </li>
          {#if !user}
            <li class={rowClass}>
              <currentTheme.icon size={16} />
              <span class="flex-1">Theme</span>
              <ToggleGroup.Root
                type="single"
                bind:value={() => theme.current, (mode) => mode && theme.set(mode as ThemeMode)}
                aria-label="Theme"
                class="flex items-center gap-0.5 rounded-lg border border-gray-200 p-0.5 dark:border-gray-800"
              >
                {#each themeOptions as option (option.value)}
                  <ToggleGroup.Item
                    value={option.value}
                    aria-label={option.label}
                    class="flex h-8 w-8 cursor-pointer items-center justify-center rounded-md text-gray-500 transition hover:text-gray-900 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500 active:scale-95 data-[state=on]:bg-gray-100 data-[state=on]:text-gray-900 dark:text-gray-400 dark:hover:text-white dark:data-[state=on]:bg-white/10 dark:data-[state=on]:text-white"
                  >
                    <option.icon size={16} />
                  </ToggleGroup.Item>
                {/each}
              </ToggleGroup.Root>
            </li>
          {/if}
        </ul>
      </nav>

      {#if user}
        {@render divider()}

        <Button
          variant="ghost"
          size="sm"
          onclick={handleSignOut}
          startIcon={LogOut}
          class={rowClass}
        >
          Sign out
        </Button>
      {/if}
    </Dialog.Content>
  </Dialog.Root>
</header>
