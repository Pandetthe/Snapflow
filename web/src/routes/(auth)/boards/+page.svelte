<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import { invalidate } from '$app/navigation';
  import BoardCard from '$lib/features/boards/components/BoardCard.svelte';
  import BoardCardSkeleton from '$lib/features/boards/components/BoardCardSkeleton.svelte';
  import { recentBoards } from '$lib/features/boards/stores/recent';
  import { Button, FullLayout, Input, Skeleton } from '$lib/ui/components';
  import { Clock3, History, Folders, Plus } from 'lucide-svelte';
  import { slide, fade } from 'svelte/transition';

  let { data } = $props();
  let intervalId: NodeJS.Timeout;
  let refreshTime = $derived(new Date(data.refreshTime));

  type BoardData = { id: number; title: string; yourRole?: string };

  $effect(() => {
    recentBoards.configure(data.user.id);
  });

  let searchQuery = $state('');
  let filteredBoards = $derived(
    data.boards.filter((b: BoardData) => b.title.toLowerCase().includes(searchQuery.toLowerCase()))
  );
  let isMounted = $state(false);
  onMount(() => {
    isMounted = true;
  });

  function formatTime(date: Date) {
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' });
  }

  onMount(() => {
    intervalId = setInterval(() => invalidate('/api/boards'), 60000);
  });

  onDestroy(() => {
    clearInterval(intervalId);
  });
</script>

<svelte:head>
  <title>Snapflow | Boards</title>
</svelte:head>

<FullLayout>
  <div class="relative w-full flex-1 pb-20 sm:pb-6">
    {#if isMounted}
      <div in:fade={{ duration: 400 }}>
        <div class="mb-6 flex flex-col gap-4 sm:mb-8 sm:flex-row sm:items-center sm:justify-between">
          <div class="space-y-1">
            <h1 class="text-2xl font-bold tracking-tight text-gray-900 sm:text-3xl dark:text-white">
              Hi {data.user.userName}!
            </h1>
            <p class="text-sm text-gray-500 dark:text-gray-400">
              Manage your projects and collaborate with your team.
            </p>
          </div>
          <div class="flex items-center gap-3">
            <div class="relative w-full sm:w-64">
              <Input
                type="search"
                placeholder="Search boards..."
                bind:value={searchQuery}
                class="h-10"
              />
            </div>
            <Button
              variant="primary"
              size="md"
              haptic="light"
              startIcon={Plus}
              class="hidden sm:inline-flex"
              href="/boards/new"
            >
              New Board
            </Button>
          </div>
        </div>

        {#if $recentBoards.length > 0}
          <div class="mb-8 lg:mb-10" transition:slide={{ duration: 400 }}>
            <h2
              class="mb-4 flex items-center gap-2 text-xs font-bold tracking-widest text-gray-500 uppercase dark:text-gray-400"
            >
              <History class="h-3.5 w-3.5" />
              Recently visited
            </h2>
            <div
              class="grid grid-cols-1 items-stretch gap-3 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 2xl:grid-cols-5 sm:gap-5 xl:gap-6"
            >
              {#each $recentBoards as boardId}
                {@const board = data.boards.find((b: BoardData) => b.id === Number(boardId))}
                {#if board}
                  <BoardCard
                    title={board.title}
                    id={board.id.toString()}
                    href={`/boards/${board.id}`}
                    editHref={`/boards/${board.id}/edit`}
                    yourRole={(board as BoardData).yourRole}
                  />
                {/if}
              {/each}
            </div>
          </div>
        {/if}

        <h2
          class="mb-4 flex items-center gap-2 text-xs font-bold tracking-widest text-gray-500 uppercase dark:text-gray-400"
        >
          <span class="flex items-center gap-2">
            <Folders class="h-3.5 w-3.5" />
            Your boards
          </span>
        </h2>

        {#if data.boards.length === 0}
          <div
            class="flex flex-col items-center justify-center rounded-2xl border-2 border-dashed border-gray-200 px-4 py-12 text-center dark:border-gray-800 sm:py-20"
          >
            <div
              class="mb-5 flex h-16 w-16 items-center justify-center rounded-full bg-gray-50 dark:bg-gray-800/30"
            >
              <Folders class="h-8 w-8 text-gray-400" />
            </div>
            <h3 class="mb-2 text-xl font-semibold text-gray-900 dark:text-white">No boards found</h3>
            <p class="mb-8 max-w-sm text-sm text-gray-500 dark:text-gray-400">
              {searchQuery
                ? `No boards match "${searchQuery}". Try a different search term.`
                : 'Get started by creating your first board to organize your tasks and projects.'}
            </p>
            {#if !searchQuery}
              <Button href="/boards/new" variant="primary" startIcon={Plus}
                >Create First Board</Button
              >
            {/if}
          </div>
        {:else}
          <div
            class="grid grid-cols-1 items-stretch gap-3 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 2xl:grid-cols-5 sm:gap-5 xl:gap-6"
          >
            {#each filteredBoards as board}
              <BoardCard
                title={board.title}
                id={board.id.toString()}
                href={`/boards/${board.id}`}
                editHref={`/boards/${board.id}/edit`}
                yourRole={(board as BoardData).yourRole}
              />
            {/each}
          </div>
        {/if}

        <div
          class="mt-12 flex items-center justify-center gap-2 text-xs text-gray-500 dark:text-gray-400"
        >
          <Clock3 class="h-3 w-3" />
          <span>Last refreshed: <span class="font-medium">{formatTime(refreshTime)}</span></span>
        </div>
      </div>
    {:else}
      <div class="flex flex-col gap-8">
        <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div class="space-y-3">
            <Skeleton class="h-9 w-48" />
            <Skeleton class="h-4 w-64" />
          </div>
          <div class="flex items-center gap-3">
            <Skeleton class="h-10 w-full rounded-lg sm:w-64" />
            <Skeleton class="hidden h-11 w-35 rounded-lg sm:block" />
          </div>
        </div>

        <div class="space-y-4">
          <Skeleton class="h-4 w-32" />
          <div
            class="grid grid-cols-1 items-stretch gap-3 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 2xl:grid-cols-5 sm:gap-5 xl:gap-6"
          >
            <BoardCardSkeleton />
            <BoardCardSkeleton />
            <BoardCardSkeleton />
            <BoardCardSkeleton />
            <BoardCardSkeleton />
            <BoardCardSkeleton />
          </div>
        </div>
      </div>
    {/if}

    <Button
      variant="primary"
      size="lg"
      startIcon={Plus}
      haptic="light"
      class="fixed right-4 bottom-4 z-40 h-14 w-14 min-w-14 rounded-full p-0 shadow-lg sm:hidden"
      href="/boards/new"
      aria-label="Create board"
    >
      <span class="sr-only">Create board</span>
    </Button>
  </div>
</FullLayout>
