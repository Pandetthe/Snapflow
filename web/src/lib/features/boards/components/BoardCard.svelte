<script lang="ts">
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
    "group relative flex h-36 w-full flex-col overflow-hidden rounded-2xl border border-gray-200/70 bg-white shadow-sm transition-all duration-300 hover:-translate-y-1 hover:shadow-xl hover:shadow-brand-500/10 dark:border-gray-800 dark:bg-gray-900",
    className
  )}
>
  <a
    {href}
    class="flex flex-col flex-1 active:scale-[0.98] transition-transform duration-200"
  >
    <div
      class="pattern-layer flex-1 w-full opacity-80 transition-all duration-500 group-hover:opacity-100 will-change-[background-position,transform]"
      style="background-image: {pattern.toDataUrl()};"
    ></div>
    
    <!-- Rozmyte szkło (Glassmorphism) na dolnym pasku -->
    <div class="absolute bottom-0 left-0 right-0 p-4 backdrop-blur-md bg-white/90 border-t border-white/20 dark:bg-gray-900/90 dark:border-gray-700/50 z-10 transition-colors">
      <h3 class="text-gray-900 dark:text-white font-bold tracking-tight text-sm truncate transition-colors group-hover:text-brand-600 dark:group-hover:text-brand-400">
        {title}
      </h3>
    </div>
  </a>
  
  {#if editHref && yourRole?.toLowerCase() === 'owner'}
    <div class="absolute top-3 right-3 z-20 opacity-0 transition-opacity duration-200 group-hover:opacity-100">
      <Button
        variant="ghost"
        size="xs"
        startIcon={Pencil}
        class="h-8 w-8 shrink-0 rounded-full bg-white/70 backdrop-blur-md border border-white/40 shadow-sm hover:bg-white dark:bg-gray-900/70 dark:border-gray-700 dark:hover:bg-gray-800 text-gray-700 dark:text-gray-200"
        aria-label="Edit board"
        href={editHref}
      />
    </div>
  {/if}
</div>

<style>
  .pattern-layer {
    background-size: 120%;
    background-position: 50% 50%;
  }
  .group:hover .pattern-layer {
    transform: scale(1.08);
    animation: pattern-drift 20s ease-in-out infinite;
  }
  @keyframes pattern-drift {
    0% { background-position: 50% 50%; }
    33% { background-position: 60% 40%; }
    66% { background-position: 40% 60%; }
    100% { background-position: 50% 50%; }
  }
</style>