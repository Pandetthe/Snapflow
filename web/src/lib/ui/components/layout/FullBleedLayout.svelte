<script lang="ts">
  import type { Snippet } from 'svelte';

  let {
    children
  }: {
    children: Snippet;
  } = $props();
</script>

<div class="flex w-full min-w-0 flex-1 flex-col items-stretch justify-start overflow-x-clip">
  {@render children()}
</div>
