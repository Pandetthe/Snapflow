<script lang="ts">
  import { flip } from 'svelte/animate';
  import { fade } from 'svelte/transition';
  import { dragHandleZone, type DndEvent, SOURCES, TRIGGERS } from 'svelte-dnd-action';
  import Swimlane from '$lib/features/boards/components/Swimlane.svelte';
  import BoardSkeleton, {
    type SkeletonBand
  } from '$lib/features/boards/components/BoardSkeleton.svelte';
  import SwimlaneModal from '$lib/features/boards/components/SwimlaneModal.svelte';
  import ListModal from '$lib/features/boards/components/ListModal.svelte';
  import CardModal from '$lib/features/boards/components/CardModal.svelte';
  import { setContext, tick, untrack } from 'svelte';
  import { setBoardUI } from '$lib/features/boards/context/board.context';
  import { BoardsHub } from '$lib/features/boards/hub/boards.hub';
  import { createBoardState } from '$lib/features/boards/composables/boardState.svelte';
  import { errorStore } from '$lib/ui/stores/error.svelte';
  import { recentBoards } from '$lib/features/boards/stores/recent';
  import type { GetBoardByIdResponse } from '$lib/features/boards/types/boards.api';
  import { Button, FullBleedLayout, GoBackButton, LoadingDots } from '$lib/ui/components';
  import { placeholderOut, triggerHaptic } from '$lib/ui/utils';
  import { Folders, Pencil, Plus, Loader2 } from 'lucide-svelte';
  import { LAYOUT_FLIP_MS, layoutFlip } from '$lib/features/boards/animations/motion';
  import '$lib/features/boards/styles/board-dnd.css';

  let { data } = $props();

  const bs = untrack(() => createBoardState(data.board, data.members, data.user.id));

  $effect(() => {
    recentBoards.configure(data.user.id);
  });

  let hub = $state<BoardsHub | null>(null);

  let loadPhase = $state<'loading' | 'morphing' | 'ready'>('loading');
  let skeletonLayout = $state<SkeletonBand[] | null>(null);
  let boardContent = $state<HTMLElement>();
  let instantReveal = $state(false);
  let skeletonShownAt = performance.now();

  const BOARD_MORPH_MS = 260;

  const QUICK_LOAD_MS = 250;

  $effect(() => {
    if (!bs.hasSnapshot) {
      untrack(() => {
        loadPhase = 'loading';
        skeletonLayout = null;
        instantReveal = false;
        skeletonShownAt = performance.now();
      });
      return;
    }
    if (untrack(() => loadPhase) === 'loading') untrack(() => morphIntoBoard());
  });

  async function morphIntoBoard() {
    const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (reduceMotion || performance.now() - skeletonShownAt < QUICK_LOAD_MS) {
      instantReveal = true;
      loadPhase = 'ready';
      return;
    }

    loadPhase = 'morphing';
    await tick();
    if (!boardContent) {
      loadPhase = 'ready';
      return;
    }
    skeletonLayout = measureBoardLayout(boardContent);
    await new Promise((resolve) => setTimeout(resolve, BOARD_MORPH_MS));
    if (loadPhase === 'morphing') loadPhase = 'ready';
  }

  function measureBoardLayout(root: HTMLElement): SkeletonBand[] {
    return [...root.querySelectorAll<HTMLElement>('[data-board-slot="swimlane"]')].map((slot) => ({
      height: slot.offsetHeight,
      lists: [...slot.querySelectorAll<HTMLElement>('[data-list-id]')].map((list) => ({
        width: list.offsetWidth,
        height: list.offsetHeight
      }))
    }));
  }

  let swimlaneModalOpen = $state(false);
  let editingSwimlane: GetBoardByIdResponse.SwimlaneDto | undefined = $state(undefined);

  let listModalOpen = $state(false);
  let targetSwimlaneId: number | null = $state(null);
  let editingList: GetBoardByIdResponse.ListDto | undefined = $state(undefined);

  let cardModalOpen = $state(false);
  let targetListId: number | null = $state(null);
  let editingCard: GetBoardByIdResponse.CardDto | undefined = $state(undefined);

  setBoardUI({
    openSwimlaneModal: (swimlane?: GetBoardByIdResponse.SwimlaneDto) => {
      editingSwimlane = swimlane;
      swimlaneModalOpen = true;
    },
    openListModal: (swimlaneId: number, list?: GetBoardByIdResponse.ListDto) => {
      targetSwimlaneId = swimlaneId;
      editingList = list;
      listModalOpen = true;
    },
    openCardModal: (listId: number, card?: GetBoardByIdResponse.CardDto) => {
      targetListId = listId;
      editingCard = card;
      cardModalOpen = true;
    }
  });

  $effect(() => {
    recentBoards.add(data.board.id);
  });

  $effect(() => {
    const boardId = data.board.id;
    const connection = new BoardsHub(boardId);

    untrack(() => {
      if (bs.board.id !== boardId) bs.reset(data.board, data.members);
      hub = connection;
      bs.registerHubEvents(connection);
    });

    connection.start().catch((err) => {
      if (hub !== connection) return;
      bs.connectionState = 'disconnected';
      if (err instanceof Error) {
        errorStore.addError(err.name, err.message);
      } else {
        errorStore.addError('Web.WebSocketConnectionProblem', 'Failed to connect to board hub');
      }
    });

    return () => {
      connection.stop();
      if (hub === connection) hub = null;
    };
  });

  setContext('hub', () => hub);
  setContext('board', () => bs.board);
  setContext('boardState', () => bs.connectionState);
  setContext('canManageSwimlanes', () => bs.canManageSwimlanes);
  setContext('canManageLists', () => bs.canManageLists);
  setContext('canManageCards', () => bs.canManageCards);
  setContext('canAssignTags', () => bs.canAssignTags);
  setContext('recentMove', bs.getRecentMove);
  setContext('isInFlight', bs.isInFlight);
  setContext('isNew', bs.isNew);
  setContext('isLeaving', bs.isLeaving);

  // Swimlane picked up with the keyboard, shown as selected until it is dropped.
  let keyboardMovedSwimlaneId = $state<number | null>(null);

  function handleSwimlaneConsider(e: CustomEvent<DndEvent<GetBoardByIdResponse.SwimlaneDto>>) {
    bs.board.swimlanes = [...e.detail.items];
    if (e.detail.info.source === SOURCES.KEYBOARD)
      keyboardMovedSwimlaneId = Number(e.detail.info.id);
  }

  async function handleSwimlaneFinalize(
    e: CustomEvent<DndEvent<GetBoardByIdResponse.SwimlaneDto>>
  ) {
    bs.board.swimlanes = [...e.detail.items];
    keyboardMovedSwimlaneId = null;
    const { info } = e.detail;
    if (info.trigger === TRIGGERS.DROPPED_INTO_ZONE) {
      triggerHaptic('success');
      const id = Number(info.id);
      const index = bs.board.swimlanes.findIndex((s) => s.id === id);
      if (index === -1) return;

      const nextItem = bs.board.swimlanes[index + 1];
      const beforeId = nextItem ? nextItem.id : null;

      const res = await hub?.moveSwimlane({ id, beforeId });
      if (res?.ok) {
        const moved = bs.board.swimlanes.find((s) => s.id === id);
        if (moved) moved.rank = res.value.rank;
        bs.sortSwimlanes();
        bs.board.swimlanes = [...bs.board.swimlanes];
      } else {
        errorStore.addError('Web.MoveSwimlaneFailed', 'Failed to move swimlane');
        bs.sortSwimlanes();
        bs.board.swimlanes = [...bs.board.swimlanes];
      }
    }
  }
</script>

<svelte:head>
  <title>Snapflow | {bs.board.title}</title>
</svelte:head>

<FullBleedLayout>
  {#if bs.connectionState !== 'connected'}
    <div
      class="fixed right-4 bottom-4 z-50 flex h-14 items-center gap-3 rounded-full px-6 text-sm font-medium text-white shadow-lg transition-all dark:shadow-black/40 {bs.connectionState ===
      'disconnected'
        ? 'bg-red-600'
        : 'bg-primary-600 dark:bg-primary-500'}"
    >
      {#if bs.connectionState === 'connecting'}
        <Loader2 class="h-4 w-4 animate-spin" />
        <span class="flex items-center gap-0.5">Connecting<LoadingDots /></span>
      {:else if bs.connectionState === 'reconnecting'}
        <Loader2 class="h-4 w-4 animate-spin" />
        <span class="flex items-center gap-0.5">Reconnecting<LoadingDots /></span>
      {:else}
        <div class="h-2.5 w-2.5 rounded-full bg-red-600"></div>
        <span>Disconnected</span>
      {/if}
    </div>
  {/if}
  <div class="flex w-full flex-1 flex-col overflow-x-clip pb-12" data-board-page>
    <!-- Board header -->
    <div
      class="relative w-full border-b border-gray-200/80 bg-white/95 backdrop-blur-sm dark:border-gray-800 dark:bg-gray-900/95"
    >
      <div class="flex w-full items-center gap-4 px-4 py-2.5 sm:px-6 lg:px-8">
        <GoBackButton href="/boards" hideTextOnMobile={true} />

        <div class="min-w-0 flex-1">
          <h1
            class="truncate text-base font-semibold tracking-tight text-gray-900 sm:text-lg dark:text-white"
          >
            {bs.board.title}
          </h1>
          {#if bs.board.description?.trim()}
            <p class="truncate text-xs text-gray-500 dark:text-gray-400">
              {bs.board.description.trim()}
            </p>
          {/if}
        </div>

        {#if bs.canEditBoard}
          <Button
            href={`/boards/${bs.board.id}/edit`}
            variant="ghost"
            size="sm"
            class="h-8 shrink-0 gap-1.5 rounded-lg px-3 text-xs font-medium text-gray-600 hover:bg-gray-100 sm:h-9 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white"
            startIcon={Pencil}
            aria-label="Edit board"
          >
            <span class="hidden sm:inline">Edit</span>
          </Button>
        {/if}
      </div>
    </div>

    <!-- Swimlanes -->
    <section class="grid flex-1 grid-cols-[minmax(0,1fr)]">
      {#if loadPhase !== 'ready'}
        <div
          class="col-start-1 row-start-1 min-w-0"
          out:fade={instantReveal ? { duration: 0 } : placeholderOut}
        >
          <BoardSkeleton layout={skeletonLayout} />
        </div>
      {/if}
      {#if loadPhase !== 'loading'}
        <div
          bind:this={boardContent}
          class="col-start-1 row-start-1 flex min-w-0 flex-col transition-opacity duration-150 ease-flow"
          class:opacity-0={loadPhase === 'morphing'}
          inert={loadPhase === 'morphing'}
        >
          {#if bs.board.swimlanes.length === 0}
            <div class="flex flex-col items-center justify-center px-4 py-16 text-center">
              <div
                class="mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-gray-100 dark:bg-gray-800"
              >
                <Folders class="h-7 w-7 text-gray-400" />
              </div>
              <h2 class="mb-1.5 text-base font-semibold text-gray-900 dark:text-white">
                No swimlanes yet
              </h2>
              <p class="max-w-xs text-sm text-gray-500 dark:text-gray-400">
                Create your first swimlane to start organizing this board.
              </p>
            </div>
          {/if}

          <div class="relative flex w-full flex-1 flex-col">
            <!-- The drop area reaches under "Add swimlane" via padding cancelled by a negative margin -->
            <section
              use:dragHandleZone={{
                items: bs.board.swimlanes,
                flipDurationMs: LAYOUT_FLIP_MS,
                type: 'swimlanes',
                dropTargetStyle: {},
                dropTargetClasses: ['board-drop-target'],
                // Swimlanes keep their own size while dragged.
                morphDisabled: true,
                useCursorForDetection: true,
                zoneTabIndex: -1,
                zoneItemTabIndex: 0,
                dragDisabled: bs.connectionState !== 'connected'
              }}
              onconsider={handleSwimlaneConsider}
              onfinalize={handleSwimlaneFinalize}
              data-board-zone="swimlanes"
              data-empty={bs.board.swimlanes.length === 0 || undefined}
              class="flex flex-col {bs.canManageSwimlanes ? '-mb-14 pb-14' : ''}"
            >
              {#each bs.board.swimlanes as swimlane, index (swimlane.id)}
                <div
                  animate:flip={layoutFlip}
                  class="relative z-20 w-full outline-none"
                  class:board-enter={bs.isNew('swimlane', swimlane.id)}
                  class:board-leave={bs.isLeaving('swimlane', swimlane.id)}
                  data-board-slot="swimlane"
                  data-selected={keyboardMovedSwimlaneId === swimlane.id || undefined}
                >
                  <Swimlane bind:swimlane={bs.board.swimlanes[index]} />
                </div>
              {/each}
            </section>

            {#if bs.canManageSwimlanes}
              <div class="relative z-10 px-5 py-2">
                <Button
                  type="button"
                  variant="ghost"
                  startIcon={Plus}
                  onclick={() => {
                    editingSwimlane = undefined;
                    swimlaneModalOpen = true;
                  }}
                  disabled={bs.connectionState !== 'connected'}
                  aria-label="Add swimlane"
                  class="h-9 justify-start gap-1.5 rounded-lg px-3 text-xs font-medium text-gray-400 hover:bg-gray-100 hover:text-gray-600 dark:text-gray-500 dark:hover:bg-gray-700/60 dark:hover:text-gray-300"
                >
                  Add swimlane
                </Button>
              </div>
            {/if}
          </div>
        </div>
      {/if}
    </section>
  </div>
</FullBleedLayout>

<SwimlaneModal
  bind:open={swimlaneModalOpen}
  swimlane={editingSwimlane}
  onConfirm={(title, height) => bs.handleSwimlaneConfirm(editingSwimlane, title, height)}
  onDelete={(id) => bs.handleSwimlaneDelete(id)}
/>

<ListModal
  bind:open={listModalOpen}
  list={editingList}
  onConfirm={(title, width) => bs.handleListConfirm(editingList, targetSwimlaneId, title, width)}
  onDelete={(id) => bs.handleListDelete(id)}
/>

<CardModal
  bind:open={cardModalOpen}
  card={editingCard}
  onConfirm={(title, description, tagIds) =>
    bs.handleCardConfirm(editingCard, targetListId, title, description, tagIds)}
  onDelete={(id) => bs.handleCardDelete(id)}
/>
