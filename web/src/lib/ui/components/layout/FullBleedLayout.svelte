<script lang="ts">
  import type { Snippet } from 'svelte';

  let {
    children
  }: {
    children: Snippet;
  } = $props();
</script>

<div class="flex w-full flex-1 min-w-0 flex-col items-stretch justify-start overflow-x-clip">
  {@render children()}
</div>