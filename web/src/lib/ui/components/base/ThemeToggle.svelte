<script lang="ts">
  import { DropdownMenu } from 'bits-ui';
  import { Button } from '$lib/ui/components';
  import { theme } from '$lib/ui/stores/theme';
  import { Sun, Moon, Monitor, ChevronDown, Check } from 'lucide-svelte';
  import { cn } from '$lib/ui/utils';

  interface Props {
    showLabel?: boolean;
    class?: string;
  }

  let { showLabel = false, class: className = '' }: Props = $props();

  function setTheme(newTheme: 'light' | 'dark' | 'system') {
    theme.set(newTheme);
  }
</script>

<DropdownMenu.Root>
  <DropdownMenu.Trigger>
    {#snippet child({ props: triggerProps })}
      <Button
        {...triggerProps}
        variant="outline"
        class={cn(
          "flex items-center transition-all active:scale-95",
          showLabel ? "w-full justify-between" : "h-11 w-11 justify-center rounded-full p-0 text-gray-700 dark:text-gray-400",
          className
        )}
      >
        {#if showLabel}
          <div class="flex items-center gap-2">
            <Sun size={18} class="text-gray-500 dark:text-gray-400 hidden theme-icon-light" />
            <Moon size={18} class="text-gray-500 dark:text-gray-400 hidden theme-icon-dark" />
            <Monitor size={18} class="text-gray-500 dark:text-gray-400 hidden theme-icon-system" />
            <span>Theme</span>
          </div>
          <ChevronDown size={18} class="text-gray-500 dark:text-gray-400" />
        {:else}
          <Sun size={20} class="hidden theme-icon-light" />
          <Moon size={20} class="hidden theme-icon-dark" />
          <Monitor size={20} class="hidden theme-icon-system" />
          <span class="sr-only">Toggle theme</span>
        {/if}
      </Button>
    {/snippet}
  </DropdownMenu.Trigger>

  <DropdownMenu.Content
    class={cn(
      "z-50 mt-1 origin-top overflow-hidden rounded-lg border border-gray-300 bg-white p-1 shadow-theme-lg will-change-[opacity,transform] data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 dark:border-gray-700 dark:bg-gray-900",
      showLabel ? "w-(--bits-dropdown-menu-anchor-width) min-w-(--bits-dropdown-menu-anchor-width)" : "w-40"
    )}
    align="end"
    sideOffset={4}
  >
    <DropdownMenu.Item onclick={() => setTheme('light')}>
      {#snippet child({ props })}
        <Button
          {...props}
          variant="ghost"
          size="sm"
          class={cn(
            "w-full justify-between items-center font-medium",
            $theme === 'light' ? "text-brand-600 dark:text-brand-400" : ""
          )}
        >
          <div class="flex items-center gap-2">
            <Sun size={16} class={cn($theme === 'light' && "text-brand-500 dark:text-brand-400")} />
            <span>Light</span>
          </div>
          {#if $theme === 'light'}
            <Check size={16} class="shrink-0 text-brand-500 dark:text-brand-400" />
          {/if}
        </Button>
      {/snippet}
    </DropdownMenu.Item>
    <DropdownMenu.Item onclick={() => setTheme('dark')}>
      {#snippet child({ props })}
        <Button
          {...props}
          variant="ghost"
          size="sm"
          class={cn(
            "w-full justify-between items-center font-medium",
            $theme === 'dark' ? "text-brand-600 dark:text-brand-400" : ""
          )}
        >
          <div class="flex items-center gap-2">
            <Moon size={16} class={cn($theme === 'dark' && "text-brand-500 dark:text-brand-400")} />
            <span>Dark</span>
          </div>
          {#if $theme === 'dark'}
            <Check size={16} class="shrink-0 text-brand-500 dark:text-brand-400" />
          {/if}
        </Button>
      {/snippet}
    </DropdownMenu.Item>
    <DropdownMenu.Item onclick={() => setTheme('system')}>
      {#snippet child({ props })}
        <Button
          {...props}
          variant="ghost"
          size="sm"
          class={cn(
            "w-full justify-between items-center font-medium",
            $theme === 'system' ? "text-brand-600 dark:text-brand-400" : ""
          )}
        >
          <div class="flex items-center gap-2">
            <Monitor size={16} class={cn($theme === 'system' && "text-brand-500 dark:text-brand-400")} />
            <span>System</span>
          </div>
          {#if $theme === 'system'}
            <Check size={16} class="shrink-0 text-brand-500 dark:text-brand-400" />
          {/if}
        </Button>
      {/snippet}
    </DropdownMenu.Item>
  </DropdownMenu.Content>
</DropdownMenu.Root>

<style>
  :global(html[data-theme='light'] .theme-icon-light) {
    display: block !important;
  }
  :global(html[data-theme='dark'] .theme-icon-dark) {
    display: block !important;
  }
  :global(html[data-theme='system'] .theme-icon-system) {
    display: block !important;
  }
</style>
