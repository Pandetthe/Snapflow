<script lang="ts">
  import BoardCard from '$lib/features/boards/components/BoardCard.svelte';
  import type { PublicBoardDto } from '$lib/features/boards/types/boards.api';
  import { EmptyState } from '$lib/ui/components';
  import { Globe } from '@lucide/svelte';

  let { boards }: { boards: PublicBoardDto[] } = $props();
</script>

<section>
  <h2
    class="mb-4 flex items-center gap-2 text-xs font-bold tracking-widest text-gray-500 uppercase dark:text-gray-400"
  >
    <Globe class="h-3.5 w-3.5" />
    Public boards
  </h2>

  {#if boards.length === 0}
    <EmptyState
      icon={Globe}
      title="No public boards yet"
      description="Boards made public by their owners will show up here for everyone to see."
    />
  {:else}
    <div
      class="grid grid-cols-1 items-stretch gap-3 sm:grid-cols-2 sm:gap-5 lg:grid-cols-3 xl:grid-cols-4 xl:gap-6 2xl:grid-cols-5"
    >
      {#each boards as board (board.id)}
        <BoardCard title={board.title} id={board.id.toString()} />
      {/each}
    </div>
  {/if}
</section>
