<script lang="ts">
  import { cn } from '$lib/ui/utils';

  let { class: className = '', ...rest } = $props<{
    class?: string;
    [key: string]: any;
  }>();
</script>

<div
  class={cn(
    'relative overflow-hidden rounded-md bg-gray-100 dark:bg-gray-800/80',
    'before:absolute before:inset-0 before:-translate-x-full before:animate-shimmer before:bg-gradient-to-r before:from-transparent before:via-white/20 before:to-transparent dark:before:via-white/5',
    className
  )}
  {...rest}
></div>

<style>
  @keyframes shimmer {
    100% {
      transform: translateX(100%);
    }
  }
  .animate-shimmer {
    animation: shimmer 2s infinite linear;
  }
</style>
