<script module lang="ts">
  export type StatusTone = 'success' | 'neutral' | 'warning' | 'danger';
</script>

<script lang="ts">
  import { cn } from '$lib/ui/utils';
  import type { Snippet } from 'svelte';

  let {
    tone = 'neutral',
    class: className,
    children
  }: {
    tone?: StatusTone;
    class?: string;
    children: Snippet;
  } = $props();

  const toneClasses: Record<StatusTone, string> = {
    success: 'bg-success-50 text-success-600 dark:bg-success-500/10 dark:text-success-400',
    neutral: 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400',
    warning: 'bg-warning-50 text-warning-700 dark:bg-warning-500/10 dark:text-warning-400',
    danger: 'bg-error-50 text-error-600 dark:bg-error-500/10 dark:text-error-400'
  };
</script>

<span
  class={cn(
    'inline-flex shrink-0 items-center rounded-full px-2 py-0.5 text-[10px] font-medium whitespace-nowrap sm:text-xs',
    toneClasses[tone],
    className
  )}
>
  {@render children()}
</span>
