<script lang="ts">
  import type { Icon as IconType } from 'lucide-svelte';
  import type { Snippet } from 'svelte';
  import { cn } from '$lib/ui/utils';

  let {
    icon: Icon,
    title,
    description,
    class: className,
    children
  }: {
    icon: typeof IconType;
    title: string;
    description: string;
    class?: string;
    children?: Snippet;
  } = $props();
</script>

<div
  class={cn('flex flex-col items-center justify-center px-6 py-12 text-center sm:py-16', className)}
>
  <div class="relative mb-5">
    <div
      class="absolute inset-0 rounded-2xl bg-brand-300/60 blur-xl dark:bg-brand-500/30"
      aria-hidden="true"
    ></div>
    <div
      class="relative flex size-14 items-center justify-center rounded-2xl bg-white text-brand-500 shadow-sm ring-1 ring-brand-100 dark:bg-brand-500/15 dark:text-brand-400 dark:ring-brand-500/20"
    >
      <Icon class="size-7" />
    </div>
  </div>
  <h3 class="mb-1.5 text-base font-semibold text-gray-800 dark:text-white/90">{title}</h3>
  <p class="max-w-sm text-sm text-gray-500 dark:text-gray-400">{description}</p>
  {#if children}
    <div class="mt-6">
      {@render children()}
    </div>
  {/if}
</div>
