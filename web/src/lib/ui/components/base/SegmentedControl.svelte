<script lang="ts" generics="T extends string">
  import { ToggleGroup } from 'bits-ui';
  import type { LucideIcon } from '@lucide/svelte';
  import { cn } from '$lib/ui/utils';

  interface Option {
    value: T;
    label: string;
    icon?: LucideIcon;
  }

  interface Props {
    options: Option[];
    value?: T;
    onValueChange?: (value: T) => void;
    size?: 'xs' | 'sm' | 'md';
    class?: string;
  }

  const sizeClasses = {
    xs: { button: 'px-2 py-1 text-xs gap-1', iconClass: 'size-3.5' },
    sm: { button: 'px-3 py-2 text-sm gap-1.5', iconClass: 'size-4' },
    md: { button: 'px-4 py-2.5 text-sm gap-2', iconClass: 'size-4.5' }
  };

  let {
    options,
    value = $bindable(),
    onValueChange,
    size = 'sm',
    class: className
  }: Props = $props();

  function select(next: string) {
    if (!next) return;
    value = next as T;
    onValueChange?.(next as T);
  }
</script>

<ToggleGroup.Root
  type="single"
  bind:value={() => value ?? '', select}
  class={cn('grid w-full rounded-lg border border-gray-300 dark:border-gray-700', className)}
  style="grid-template-columns: repeat({options.length}, minmax(0, 1fr));"
>
  {#each options as opt, i (opt.value)}
    {@const Icon = opt.icon}
    <ToggleGroup.Item
      value={opt.value}
      class={cn(
        'relative inline-flex w-full cursor-pointer items-center justify-center font-medium whitespace-nowrap transition focus-visible:z-10 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500 active:scale-95',
        'bg-white text-gray-600 hover:bg-gray-50 hover:text-gray-900 dark:bg-gray-900/50 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-gray-100',
        'data-[state=on]:bg-brand-500 data-[state=on]:text-white data-[state=on]:hover:bg-brand-500 data-[state=on]:hover:text-white',
        sizeClasses[size].button,
        i === 0 && 'rounded-l-lg',
        i === options.length - 1 && 'rounded-r-lg',
        i > 0 && 'border-l border-gray-300 dark:border-gray-700'
      )}
    >
      {#if Icon}
        <span
          class={cn('flex shrink-0 [&>svg]:h-full [&>svg]:w-full', sizeClasses[size].iconClass)}
        >
          <Icon />
        </span>
      {/if}
      {opt.label}
    </ToggleGroup.Item>
  {/each}
</ToggleGroup.Root>
