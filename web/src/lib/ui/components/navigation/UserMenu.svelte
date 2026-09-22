<script lang="ts">
  import { ChevronDown, User as UserIcon, LogOut, SlidersHorizontal } from '@lucide/svelte';
  import { DropdownMenu } from 'bits-ui';
  import { Button, UserAvatar } from '$lib/ui/components';
  import { floatingMotionClass } from '$lib/ui/utils';
  import type { User } from '$lib/features/users/api/users';
  import { avatarBust } from '$lib/features/users/avatarBust.svelte';

  interface Props {
    user: User | null;
    handleSignOut?: () => void;
  }

  let { user, handleSignOut }: Props = $props();

  const menuItems = [
    { href: '/profile', icon: UserIcon, text: 'Edit profile' },
    { href: '/settings', icon: SlidersHorizontal, text: 'Settings' }
  ];
</script>

<div class="relative">
  {#if user}
    <DropdownMenu.Root>
      <DropdownMenu.Trigger>
        {#snippet child({ props: triggerProps })}
          <button
            {...triggerProps}
            class="group flex cursor-pointer items-center gap-3 rounded-lg p-1 transition-all hover:bg-gray-100 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500 active:scale-95 dark:hover:bg-gray-800"
          >
            <UserAvatar
              src={user.avatarUrl ? `${user.avatarUrl}?v=${avatarBust.count}` : null}
              name={user.userName || user.email || 'User'}
              size={40}
              class="ring-2 ring-gray-100 transition-all group-focus-visible:ring-brand-500 dark:ring-gray-800"
            />

            <div class="hidden text-left sm:block">
              <p class="flex items-center gap-1 text-sm font-medium text-gray-900 dark:text-white">
                {user.userName}
                <ChevronDown
                  size={18}
                  class="text-gray-400 transition-transform group-data-[state=open]:rotate-180"
                />
              </p>
            </div>
          </button>
        {/snippet}
      </DropdownMenu.Trigger>

      <DropdownMenu.Content
        class="z-50 mt-1 min-w-56 overflow-hidden rounded-lg border border-gray-300 bg-white p-1 shadow-theme-lg dark:border-gray-700 dark:bg-gray-900 {floatingMotionClass}"
        align="end"
        sideOffset={4}
      >
        <div class="mb-1 border-b border-gray-200 px-3 py-2 dark:border-gray-800">
          <span class="block text-sm font-semibold text-gray-900 dark:text-white">
            {user.userName}
          </span>
          <span class="block truncate text-xs text-gray-500 dark:text-gray-400">
            {user.email}
          </span>
        </div>

        <div class="space-y-0.5">
          {#each menuItems as item (item.href)}
            <DropdownMenu.Item>
              {#snippet child({ props: itemProps })}
                <Button
                  {...itemProps}
                  variant="ghost"
                  size="sm"
                  href={item.href}
                  class="w-full justify-start font-medium"
                  startIcon={item.icon}
                >
                  {item.text}
                </Button>
              {/snippet}
            </DropdownMenu.Item>
          {/each}
        </div>

        <div class="mt-1 border-t border-gray-200 pt-1 dark:border-gray-800">
          <DropdownMenu.Item onSelect={handleSignOut}>
            {#snippet child({ props: itemProps })}
              <Button
                {...itemProps}
                variant="ghost"
                size="sm"
                class="w-full justify-start font-medium"
                startIcon={LogOut}
              >
                Sign out
              </Button>
            {/snippet}
          </DropdownMenu.Item>
        </div>
      </DropdownMenu.Content>
    </DropdownMenu.Root>
  {:else}
    <div class="flex items-center gap-3">
      <Button variant="outline" href="/sign-in">Sign in</Button>
      <Button variant="outline" href="/sign-up">Sign up</Button>
    </div>
  {/if}
</div>

<style>
  :global(.text-theme-sm) {
    font-size: 0.875rem;
    line-height: 1.25rem;
  }
</style>
