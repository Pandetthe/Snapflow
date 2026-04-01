<script lang="ts">
  import type { Snippet } from 'svelte';

  let {
    children
  }: {
    children: Snippet;
  } = $props();
</script>

<div class="flex flex-1 flex-col items-center justify-center px-4 py-8 sm:px-6 lg:px-8">
  {@render children()}
</div>