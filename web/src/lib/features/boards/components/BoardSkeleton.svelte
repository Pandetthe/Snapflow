<script lang="ts" module>
  export interface SkeletonList {
    width: number;
    height: number;
    leaving?: boolean;
  }

  export interface SkeletonBand {
    height: number;
    lists: SkeletonList[];
    leaving?: boolean;
  }

  const PLACEHOLDER: SkeletonBand[] = [0, 1].map(() => ({
    height: 229,
    lists: [0, 1, 2].map(() => ({ width: 220, height: 160 }))
  }));

  function towards(layout: SkeletonBand[]): SkeletonBand[] {
    return Array.from({ length: Math.max(layout.length, PLACEHOLDER.length) }, (_, bandIndex) => {
      const target = layout[bandIndex];
      const from = PLACEHOLDER[bandIndex];

      if (!target) {
        return {
          height: 0,
          leaving: true,
          lists: from.lists.map((list) => ({ ...list, width: 0, leaving: true }))
        };
      }

      const fromLists = from?.lists ?? [];
      return {
        height: target.height,
        lists: Array.from(
          { length: Math.max(target.lists.length, fromLists.length) },
          (_, listIndex) =>
            target.lists[listIndex] ?? {
              width: 0,
              height: fromLists[listIndex].height,
              leaving: true
            }
        )
      };
    });
  }
</script>

<script lang="ts">
  import { Skeleton } from '$lib/ui/components';

  let { layout = null }: { layout?: SkeletonBand[] | null } = $props();

  const bands = $derived(layout ? towards(layout) : PLACEHOLDER);
</script>

<div class="flex flex-col" aria-busy="true" aria-label="Loading board">
  {#each bands as band, bandIndex (bandIndex)}
    <div
      class="board-skeleton-band flex flex-col overflow-hidden border-b border-gray-200 dark:border-gray-700/60"
      data-leaving={band.leaving || undefined}
      style:--band-height="{band.height}px"
    >
      <div class="flex h-11 shrink-0 items-center gap-1.5 bg-gray-50 px-3 dark:bg-gray-800/70">
        <Skeleton class="h-4 w-32" />
        <Skeleton class="ml-auto h-5 w-6 rounded-full" />
      </div>
      <div
        class="flex min-h-0 flex-1 items-start gap-3 overflow-hidden bg-white/60 px-3 py-3 dark:bg-gray-900/40"
      >
        {#each band.lists as list, listIndex (listIndex)}
          <div
            class="board-skeleton-list flex shrink-0 flex-col overflow-hidden rounded-xl border border-gray-200/80 bg-gray-50 shadow-sm dark:border-gray-700/50 dark:bg-gray-900/40"
            data-leaving={list.leaving || undefined}
            style="--list-width: {list.width}px; --list-height: {list.height}px"
          >
            <div
              class="flex h-10 shrink-0 items-center border-b border-gray-200 bg-gray-100/80 px-2 dark:border-gray-700/60 dark:bg-gray-800/90"
            >
              <Skeleton class="h-3 w-24" />
            </div>
            <div class="flex flex-col gap-1.5 p-2">
              {#each [0, 1] as card (card)}
                <div
                  class="flex flex-col gap-2 rounded-lg border border-gray-200/90 bg-white p-2 shadow-sm dark:border-gray-700/60 dark:bg-gray-800"
                >
                  <Skeleton class="h-3 w-3/4" />
                  <Skeleton class="h-3 w-1/2" />
                </div>
              {/each}
            </div>
          </div>
        {/each}
      </div>
    </div>
  {/each}
</div>
