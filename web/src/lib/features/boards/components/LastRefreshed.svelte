<script lang="ts">
  import { onDestroy, onMount } from 'svelte';
  import { invalidate } from '$app/navigation';
  import { Clock3 } from '@lucide/svelte';

  let { refreshTime }: { refreshTime: string } = $props();

  const refreshedAt = $derived(new Date(refreshTime));
  let intervalId: ReturnType<typeof setInterval>;

  function formatTime(date: Date) {
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' });
  }

  onMount(() => {
    intervalId = setInterval(() => invalidate('/api/boards'), 60000);
  });

  onDestroy(() => {
    clearInterval(intervalId);
  });
</script>

<div class="mt-12 flex items-center justify-center gap-2 text-xs text-gray-500 dark:text-gray-400">
  <Clock3 class="h-3 w-3" />
  <span>Last refreshed: <span class="font-medium">{formatTime(refreshedAt)}</span></span>
</div>
