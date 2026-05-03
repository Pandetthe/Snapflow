<script lang="ts" generics="T extends string">
  import type { Icon as IconType } from 'lucide-svelte';
  import { cn } from '$lib/ui/utils';

  interface Option {
    value: T;
    label: string;
    icon?: typeof IconType;
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
    md: { button: 'px-4 py-2.5 text-sm gap-2', iconClass: 'size-[18px]' },
  };

  let {
    options,
    value = $bindable(),
    onValueChange,
    size = 'sm',
    class: className
  }: Props = $props();

  function select(v: T) {
    value = v;
    onValueChange?.(v);
  }
</script>

<div
  class={cn('w-full rounded-lg border border-gray-300 dark:border-gray-700', className)}
  style="display: grid; grid-template-columns: repeat({options.length}, minmax(0, 1fr));"
  role="group"
>
  {#each options as opt, i}
    {@const Icon = opt.icon}
    {@const isActive = value === opt.value}
    {@const isFirst = i === 0}
    {@const isLast = i === options.length - 1}
    <button
      type="button"
      onclick={() => select(opt.value)}
      class={cn(
        'relative inline-flex w-full cursor-pointer items-center justify-center font-medium whitespace-nowrap transition focus-visible:z-10 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white dark:focus-visible:ring-offset-gray-950 active:scale-95',
        sizeClasses[size].button,
        isFirst && 'rounded-l-lg',
        isLast && 'rounded-r-lg',
        i > 0 && 'border-l border-gray-300 dark:border-gray-700',
        isActive
          ? 'bg-brand-500 text-white'
          : 'bg-white text-gray-600 hover:bg-gray-50 hover:text-gray-900 dark:bg-gray-900/50 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-gray-100'
      )}
    >
      {#if Icon}
        <span class={cn('flex shrink-0 [&>svg]:h-full [&>svg]:w-full', sizeClasses[size].iconClass)}>
          <Icon />
        </span>
      {/if}
      {opt.label}
    </button>
  {/each}
</div>
