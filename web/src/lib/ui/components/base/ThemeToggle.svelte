<script lang="ts">
  import { DropdownMenu } from 'bits-ui';
  import { Button } from '$lib/ui/components';
  import { theme } from '$lib/ui/stores/theme.svelte';
  import { Sun, Moon, Monitor, Check } from 'lucide-svelte';
  import { cn, floatingMotionClass } from '$lib/ui/utils';

  interface Props {
    class?: string;
  }

  let { class: className = '' }: Props = $props();

  type ThemeMode = 'light' | 'dark' | 'system';

  const options: { value: ThemeMode; label: string; icon: typeof Sun }[] = [
    { value: 'light', label: 'Light', icon: Sun },
    { value: 'dark', label: 'Dark', icon: Moon },
    { value: 'system', label: 'System', icon: Monitor }
  ];
</script>

<DropdownMenu.Root>
  <DropdownMenu.Trigger>
    {#snippet child({ props: triggerProps })}
      <Button
        {...triggerProps}
        variant="outline"
        class={cn(
          'flex h-11 w-11 items-center justify-center rounded-full p-0 text-gray-700 transition-all active:scale-95 dark:text-gray-400',
          className
        )}
      >
        <Sun size={20} class="theme-icon-light hidden" />
        <Moon size={20} class="theme-icon-dark hidden" />
        <Monitor size={20} class="theme-icon-system hidden" />
        <span class="sr-only">Toggle theme</span>
      </Button>
    {/snippet}
  </DropdownMenu.Trigger>

  <DropdownMenu.Content
    class={cn(
      'z-50 mt-1 w-40 overflow-hidden rounded-lg border border-gray-300 bg-white p-1 shadow-theme-lg dark:border-gray-700 dark:bg-gray-900',
      floatingMotionClass
    )}
    align="end"
    sideOffset={4}
  >
    <DropdownMenu.RadioGroup
      value={theme.current}
      onValueChange={(mode) => theme.set(mode as ThemeMode)}
    >
      {#each options as option (option.value)}
        <DropdownMenu.RadioItem value={option.value}>
          {#snippet child({ props, checked })}
            <Button
              {...props}
              variant="ghost"
              size="sm"
              class={cn(
                'w-full items-center justify-between font-medium',
                checked && 'text-brand-600 dark:text-brand-400'
              )}
            >
              <div class="flex items-center gap-2">
                <option.icon
                  size={16}
                  class={cn(checked && 'text-brand-500 dark:text-brand-400')}
                />
                <span>{option.label}</span>
              </div>
              {#if checked}
                <Check size={16} class="shrink-0 text-brand-500 dark:text-brand-400" />
              {/if}
            </Button>
          {/snippet}
        </DropdownMenu.RadioItem>
      {/each}
    </DropdownMenu.RadioGroup>
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
