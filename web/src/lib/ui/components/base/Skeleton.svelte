<script lang="ts">
  import type { HTMLAttributes } from 'svelte/elements';
  import { cn } from '$lib/ui/utils';

  let { class: className = '', ...rest }: HTMLAttributes<HTMLDivElement> = $props();
</script>

<div
  class={cn(
    'relative overflow-hidden rounded-md bg-gray-100 dark:bg-gray-800/80',
    'before:absolute before:inset-0 before:animate-shimmer before:bg-linear-to-r before:from-transparent before:via-white/60 before:to-transparent motion-reduce:before:animate-none dark:before:via-white/5',
    className
  )}
  {...rest}
></div>
