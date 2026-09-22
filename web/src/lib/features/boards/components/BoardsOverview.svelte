<script lang="ts">
  import { onMount, tick } from 'svelte';
  import BoardCard from '$lib/features/boards/components/BoardCard.svelte';
  import BoardCardSkeleton from '$lib/features/boards/components/BoardCardSkeleton.svelte';
  import PublicBoards from '$lib/features/boards/components/PublicBoards.svelte';
  import LastRefreshed from '$lib/features/boards/components/LastRefreshed.svelte';
  import type { PublicBoardDto } from '$lib/features/boards/types/boards.api';
  import { recentBoards } from '$lib/features/boards/stores/recent.svelte';
  import { Button, EmptyState, Input, Skeleton } from '$lib/ui/components';
  import { History, Folders, Plus, Search } from 'lucide-svelte';
  import { fade, slide } from 'svelte/transition';
  import { placeholderOut, slideReveal } from '$lib/ui/utils';
  import { morphPlaceholders, type MorphPair } from '$lib/features/boards/animations/skeletonMorph';

  type BoardData = { id: number; title: string; yourRole?: string };

  let {
    boards,
    publicBoards,
    refreshTime,
    user
  }: {
    boards: BoardData[];
    publicBoards: PublicBoardDto[];
    refreshTime: string;
    user: { id: number; userName: string };
  } = $props();

  $effect(() => {
    recentBoards.configure(user.id);
  });

  const recentlyVisited = $derived(
    recentBoards.current
      .map((boardId) => boards.find((b: BoardData) => b.id === Number(boardId)))
      .filter((board): board is BoardData => board !== undefined)
  );

  let searchQuery = $state('');
  let filteredBoards = $derived(
    boards.filter((b: BoardData) => b.title.toLowerCase().includes(searchQuery.toLowerCase()))
  );
  const serverRendered =
    typeof document === 'undefined' || document.querySelector('[data-boards-skeleton]') !== null;
  let loadPhase = $state<'loading' | 'morphing' | 'ready'>(serverRendered ? 'loading' : 'ready');
  let placeholder = $state<HTMLElement>();
  let content = $state<HTMLElement>();
  let instantReveal = $state(false);

  const BOARDS_MORPH_MS = 260;

  const QUICK_LOAD_MS = 250;

  onMount(async () => {
    if (loadPhase === 'ready') return;

    const firstPaint = performance.getEntriesByName('first-contentful-paint')[0];
    const skeletonShownFor = firstPaint ? performance.now() - firstPaint.startTime : Infinity;
    const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (reduceMotion || skeletonShownFor < QUICK_LOAD_MS) {
      instantReveal = true;
      loadPhase = 'ready';
      return;
    }

    loadPhase = 'morphing';
    await tick();
    if (!placeholder || !content) {
      loadPhase = 'ready';
      return;
    }

    const pairs: MorphPair[] = ['title', 'subtitle', 'search', 'new'].map((part) => [
      placeholder!.querySelector<HTMLElement>(`[data-morph="${part}"]`),
      content!.querySelector<HTMLElement>(`[data-morph="${part}"]`)
    ]);
    const cards = content.querySelectorAll<HTMLElement>('[data-board-card]');
    placeholder.querySelectorAll<HTMLElement>('[data-morph="card"]').forEach((card, index) => {
      pairs.push([card, cards[index]]);
    });

    await morphPlaceholders(pairs, BOARDS_MORPH_MS);
    loadPhase = 'ready';
  });
</script>

<div class="relative w-full flex-1 pb-20 sm:pb-6">
  <div class="grid grid-cols-[minmax(0,1fr)]">
    {#if loadPhase !== 'loading'}
      <div
        bind:this={content}
        class="col-start-1 row-start-1 min-w-0 transition-opacity duration-150 ease-flow"
        class:opacity-0={loadPhase === 'morphing'}
        inert={loadPhase === 'morphing'}
      >
        <div
          class="mb-6 flex flex-col gap-4 sm:mb-8 sm:flex-row sm:items-center sm:justify-between"
        >
          <div class="space-y-1">
            <h1
              class="w-fit text-2xl font-bold tracking-tight text-gray-900 sm:text-3xl dark:text-white"
              data-morph="title"
            >
              Hi {user.userName}!
            </h1>
            <p class="w-fit text-sm text-gray-500 dark:text-gray-400" data-morph="subtitle">
              Manage your projects and collaborate with your team.
            </p>
          </div>
          <div class="flex items-center gap-3">
            <div class="relative w-full sm:w-64" data-morph="search">
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
              data-morph="new"
            >
              New board
            </Button>
          </div>
        </div>

        {#if recentlyVisited.length > 0}
          <div
            class="mb-8 border-b border-gray-200 pb-10 lg:mb-10 lg:pb-12 dark:border-gray-800"
            transition:slide={slideReveal}
          >
            <h2
              class="mb-4 flex items-center gap-2 text-xs font-bold tracking-widest text-gray-500 uppercase dark:text-gray-400"
            >
              <History class="h-3.5 w-3.5" />
              Recently visited
            </h2>
            <div
              class="grid grid-cols-1 items-stretch gap-3 sm:grid-cols-2 sm:gap-5 lg:grid-cols-3 xl:grid-cols-4 xl:gap-6 2xl:grid-cols-5"
            >
              {#each recentlyVisited as board (board.id)}
                <BoardCard title={board.title} id={board.id.toString()} yourRole={board.yourRole} />
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

        {#if boards.length === 0}
          <EmptyState
            icon={Folders}
            title="No boards yet"
            description="Create your first board to start organizing your team's work."
          >
            <Button href="/boards/new" variant="primary" startIcon={Plus} haptic="light">
              Create board
            </Button>
          </EmptyState>
        {:else if filteredBoards.length === 0}
          <EmptyState
            icon={Search}
            title="No matching boards"
            description={`Nothing matches "${searchQuery}". Try a different search term.`}
          />
        {:else}
          <div
            class="grid grid-cols-1 items-stretch gap-3 sm:grid-cols-2 sm:gap-5 lg:grid-cols-3 xl:grid-cols-4 xl:gap-6 2xl:grid-cols-5"
          >
            {#each filteredBoards as board (board.id)}
              <BoardCard
                title={board.title}
                id={board.id.toString()}
                yourRole={(board as BoardData).yourRole}
              />
            {/each}
          </div>
        {/if}

        <div class="mt-10 border-t border-gray-200 pt-8 lg:mt-12 lg:pt-10 dark:border-gray-800">
          <PublicBoards boards={publicBoards} />
        </div>

        <LastRefreshed {refreshTime} />
      </div>
    {/if}
    {#if loadPhase !== 'ready'}
      <div
        bind:this={placeholder}
        class="col-start-1 row-start-1 flex min-w-0 flex-col gap-8"
        aria-hidden="true"
        data-boards-skeleton
        out:fade={instantReveal ? { duration: 0 } : placeholderOut}
      >
        <div class="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div class="space-y-3">
            <Skeleton class="h-9 w-48" data-morph="title" />
            <Skeleton class="h-4 w-64" data-morph="subtitle" />
          </div>
          <div class="flex items-center gap-3">
            <Skeleton class="h-10 w-full rounded-lg sm:w-64" data-morph="search" />
            <Skeleton class="hidden h-11 w-35 rounded-lg sm:block" data-morph="new" />
          </div>
        </div>

        <div class="space-y-4">
          <Skeleton class="h-4 w-32" />
          <div
            class="grid grid-cols-1 items-stretch gap-3 sm:grid-cols-2 sm:gap-5 lg:grid-cols-3 xl:grid-cols-4 xl:gap-6 2xl:grid-cols-5"
          >
            <div data-morph="card"><BoardCardSkeleton /></div>
            <div data-morph="card"><BoardCardSkeleton /></div>
            <div data-morph="card"><BoardCardSkeleton /></div>
            <div data-morph="card"><BoardCardSkeleton /></div>
            <div data-morph="card"><BoardCardSkeleton /></div>
            <div data-morph="card"><BoardCardSkeleton /></div>
          </div>
        </div>
      </div>
    {/if}
  </div>

  <Button
    variant="primary"
    size="lg"
    startIcon={Plus}
    haptic="light"
    class="fixed right-4 bottom-4 z-30 h-14 w-14 min-w-14 rounded-full p-0 shadow-lg sm:hidden"
    href="/boards/new"
    aria-label="Create board"
  >
    <span class="sr-only">Create board</span>
  </Button>
</div>
