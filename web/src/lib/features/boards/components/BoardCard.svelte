<script lang="ts">
  import GeoPattern from 'geopattern';
  import { resolve } from '$app/paths';
  import { cn } from '$lib/ui/utils';
  import { Pencil } from 'lucide-svelte';
  import { Button } from '$lib/ui/components';

  let {
    title,
    id,
    yourRole,
    class: className
  } = $props<{
    title: string;
    id: string;
    yourRole?: string;
    class?: string;
  }>();

  let pattern = $derived(GeoPattern.generate(id));

  const canEditBoard = $derived(
    yourRole?.toLowerCase() === 'owner' || yourRole?.toLowerCase() === 'admin'
  );
</script>

<div
  data-board-card
  class={cn(
    'group relative flex h-32 w-full flex-col overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm sm:h-40 dark:border-gray-800 dark:bg-gray-900',
    'transition-all duration-200 hover:border-brand-500/30 hover:shadow-md',
    'focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500',
    className
  )}
>
  <a
    href={resolve(`/boards/${id}`)}
    class="flex flex-1 flex-col transition-all duration-200 hover:-translate-y-1 active:scale-[0.98]"
  >
    <div
      class="pattern-layer w-full flex-1 opacity-80 transition-all duration-300 will-change-[background-position,transform] group-hover:opacity-100"
      style="background-image: {pattern.toDataUrl()};"
    ></div>
    <div
      class="relative z-10 border-t border-gray-100 bg-white p-3.5 dark:border-gray-800 dark:bg-gray-900"
    >
      <h3
        class="truncate text-sm font-semibold text-gray-900 transition-colors group-hover:text-brand-500 dark:text-white/90 dark:group-hover:text-brand-400"
      >
        {title}
      </h3>
    </div>
  </a>
  {#if canEditBoard}
    <div class="absolute top-3 right-3 z-20">
      <Button
        variant="ghost"
        size="xs"
        startIcon={Pencil}
        class="shrink-0 border border-white/20 bg-white/50 shadow-sm backdrop-blur-md hover:bg-white/70 dark:border-white/10 dark:bg-gray-900/50 dark:hover:bg-gray-900/80"
        aria-label="Edit board"
        href={resolve(`/boards/${id}/edit`)}
      />
    </div>
  {/if}
</div>

<style>
  .pattern-layer {
    background-size: 110%;
    background-position: 50% 50%;
    transition:
      transform 360ms ease,
      opacity 300ms ease;
  }

  .group:hover .pattern-layer {
    transform: scale(1.05);
    animation: pattern-drift 15s ease-in-out infinite;
  }

  @keyframes pattern-drift {
    0% {
      background-position: 50% 50%;
    }
    25% {
      background-position: 55% 45%;
    }
    50% {
      background-position: 50% 60%;
    }
    75% {
      background-position: 45% 55%;
    }
    100% {
      background-position: 50% 50%;
    }
  }
</style>
