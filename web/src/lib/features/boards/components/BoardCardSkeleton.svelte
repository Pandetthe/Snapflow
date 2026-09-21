<script lang="ts">
  import { cn } from '$lib/ui/utils';
  import Skeleton from '$lib/ui/components/base/Skeleton.svelte';

  let { class: className = '' } = $props<{ class?: string }>();
</script>

<div
  class={cn(
    'relative flex h-32 w-full flex-col overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm sm:h-40 dark:border-gray-800 dark:bg-gray-900',
    className
  )}
>
  <div class="w-full flex-1 bg-gray-50/50 dark:bg-gray-800/20">
    <Skeleton class="h-full w-full rounded-none opacity-50" />
  </div>
  <div class="border-t border-gray-100 bg-white p-3.5 dark:border-gray-800 dark:bg-gray-900">
    <Skeleton class="h-4 w-3/4" />
  </div>
</div>
