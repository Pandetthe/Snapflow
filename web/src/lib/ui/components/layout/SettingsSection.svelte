<script lang="ts">
  import type { LucideIcon } from '@lucide/svelte';
  import type { Snippet } from 'svelte';
  import { cn } from '$lib/ui/utils';

  let {
    icon: Icon,
    title,
    description,
    danger = false,
    class: className,
    children
  }: {
    icon: LucideIcon;
    title: string;
    description: string;
    danger?: boolean;
    class?: string;
    children: Snippet;
  } = $props();
</script>

<section
  class={cn(
    'rounded-2xl border p-5 shadow-sm transition-all duration-200 sm:rounded-3xl sm:p-6',
    danger
      ? 'border-error-100 bg-error-50/20 dark:border-error-900/30 dark:bg-error-950/10'
      : 'border-gray-200 bg-white dark:border-gray-800 dark:bg-gray-900/50',
    className
  )}
>
  <div class="mb-6 flex items-start justify-between gap-4">
    <div class="min-w-0 space-y-1">
      <h2
        class={cn(
          'text-lg font-bold',
          danger ? 'text-error-600 dark:text-error-400' : 'text-gray-900 dark:text-white'
        )}
      >
        {title}
      </h2>
      <p
        class={cn(
          'text-sm',
          danger ? 'text-error-700/60 dark:text-error-400/60' : 'text-gray-600 dark:text-gray-400'
        )}
      >
        {description}
      </p>
    </div>
    <Icon class={cn('h-5 w-5 shrink-0', danger ? 'text-error-500' : 'text-gray-500')} />
  </div>

  {@render children()}
</section>
