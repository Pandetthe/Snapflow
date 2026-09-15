<script lang="ts">
  import { cn } from '$lib/ui/utils';

  let { class: className = '', ...rest } = $props<{
    class?: string;
    [key: string]: any;
  }>();
</script>

<!-- The shimmer animation is a theme animation (--animate-shimmer in app.css), so Tailwind generates before:animate-shimmer -->
<div
  class={cn(
    'relative overflow-hidden rounded-md bg-gray-100 dark:bg-gray-800/80',
    'before:absolute before:inset-0 before:animate-shimmer before:bg-linear-to-r before:from-transparent before:via-white/60 before:to-transparent dark:before:via-white/5 motion-reduce:before:animate-none',
    className
  )}
  {...rest}
></div>
