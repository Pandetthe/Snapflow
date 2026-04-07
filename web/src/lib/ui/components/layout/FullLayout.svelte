<script lang="ts">
  import type { Snippet } from 'svelte';

  let {
    children
  }: {
    children: Snippet;
  } = $props();
</script>

<div
  class="mx-auto flex w-full max-w-[1800px] flex-1 flex-col items-stretch justify-start px-8 py-6 sm:px-10 lg:px-14 xl:px-20"
>
  {@render children()}
</div>