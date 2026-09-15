<script lang="ts">
  import type { Snippet } from 'svelte';

  let {
    children
  }: {
    children: Snippet;
  } = $props();
</script>

<div
  class="mx-auto flex w-full max-w-450 flex-1 flex-col items-stretch justify-start px-6 py-5 sm:px-10 sm:py-6 lg:px-14 xl:px-20"
>
  {@render children()}
</div>
