<script lang="ts">
  import { goto } from '$app/navigation';
  import GeoPattern from 'geopattern';
  import { cn } from '$lib/ui/utils';
  import { Pencil } from 'lucide-svelte';
  import { Button } from '$lib/ui/components';

  let { title, id, href = '#', editHref, yourRole, class: className } = $props<{
    title: string;
    id: string;
    href?: string;
    editHref?: string;
    yourRole?: string;
    class?: string;
  }>();

  let pattern = $derived(GeoPattern.generate(id));
</script>

<div
  class={cn(
    "group relative flex h-32 w-full flex-col overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm sm:h-40 dark:border-gray-800 dark:bg-gray-900",
    "transition-all duration-200 hover:shadow-md hover:border-brand-500/30",
    "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white dark:focus-visible:ring-offset-gray-950",
    className
  )}
>
  <a
    {href}
    class="flex flex-col flex-1 hover:-translate-y-1 active:scale-[0.98] transition-all duration-200"
  >
    <div
      class="pattern-layer flex-1 w-full opacity-80 transition-all duration-300 group-hover:opacity-100 will-change-[background-position,transform]"
      style="background-image: {pattern.toDataUrl()};"
    ></div>
    <div class="p-3.5 bg-white dark:bg-gray-900 border-t border-gray-100 dark:border-gray-800 z-10 relative">
      <h3 class="text-gray-900 dark:text-white/90 font-semibold text-sm truncate transition-colors group-hover:text-brand-500 dark:group-hover:text-brand-400">{title}</h3>
    </div>
  </a>
  {#if editHref && yourRole?.toLowerCase() === 'owner'}
    <div class="absolute top-3 right-3 z-20">
      <Button
        variant="ghost"
        size="xs"
        startIcon={Pencil}
        class="shrink-0 bg-white/50 backdrop-blur-md border border-white/20 shadow-sm hover:bg-white/70 dark:bg-gray-900/50 dark:border-white/10 dark:hover:bg-gray-900/80"
        aria-label="Edit board"
        href={editHref}
      />
    </div>
  {/if}
</div>

<style>
  .pattern-layer {
    background-size: 110%;
    background-position: 50% 50%;
    transition: transform 360ms ease, opacity 300ms ease;
  }

  .group:hover .pattern-layer {
    transform: scale(1.05);
    animation: pattern-drift 15s ease-in-out infinite;
  }

  @keyframes pattern-drift {
    0% { background-position: 50% 50%; }
    25% { background-position: 55% 45%; }
    50% { background-position: 50% 60%; }
    75% { background-position: 45% 55%; }
    100% { background-position: 50% 50%; }
  }
</style>
