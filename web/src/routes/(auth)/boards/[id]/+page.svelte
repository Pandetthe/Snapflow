<script lang="ts">
  import { flip } from 'svelte/animate';
  import { dragHandleZone, type DndEvent } from 'svelte-dnd-action';
  import Swimlane from '$lib/features/boards/components/Swimlane.svelte';
  import SwimlaneModal from '$lib/features/boards/components/SwimlaneModal.svelte';
  import ListModal from '$lib/features/boards/components/ListModal.svelte';
  import CardModal from '$lib/features/boards/components/CardModal.svelte';
  import { onDestroy, onMount, setContext } from 'svelte';
  import { BoardsHub } from '$lib/features/boards/hub/boards.hub';
  import { errorStore } from '$lib/ui/stores/error';
  import { recentBoards } from '$lib/features/boards/stores/recent';
  import type { GetBoardByIdResponse } from '$lib/features/boards/types/boards.api';
  import { Button, FullBleedLayout, GoBackButton, LoadingDots } from '$lib/ui/components';
  import { triggerHaptic } from '$lib/ui/utils';
  import { Folders, Pencil, Plus, Loader2 } from 'lucide-svelte';
  import { fade } from 'svelte/transition';
  import type { Response } from '$lib/core/types/app';

  let { data } = $props();
  let board = $state((() => data.board)());
  let hub = $state<BoardsHub | null>(null);
  let connectionState = $state<'connecting' | 'connected' | 'reconnecting' | 'disconnected'>('connecting');

  $effect(() => {
    recentBoards.configure(data.user.id);
  });

  let swimlaneModalOpen = $state(false);
  let editingSwimlane: GetBoardByIdResponse.SwimlaneDto | undefined = $state(undefined);

  let listModalOpen = $state(false);
  let targetSwimlaneId: number | null = $state(null);
  let editingList: GetBoardByIdResponse.ListDto | undefined = $state(undefined);

  let cardModalOpen = $state(false);
  let targetListId: number | null = $state(null);
  let editingCard: GetBoardByIdResponse.CardDto | undefined = $state(undefined);

  setContext('ui', {
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

  async function handleSwimlaneConfirm(title: string, height: number | null): Promise<Response<any>> {
    if (!hub) {
      return { ok: false, problem: { title: 'Board hub unavailable', detail: 'Please try again.' } };
    }

    if (editingSwimlane) {
      const res = await hub.updateSwimlane({
        id: editingSwimlane.id,
        title,
        height: height
      });
      if (res?.ok) {
        editingSwimlane.title = title;
        editingSwimlane.height = height;
      }
      return res;
    } else {
      const res = await hub.createSwimlane({
        title,
        height: height,
        beforeId: null
      });
      if (res?.ok) {
        if (!board.swimlanes.some((s) => s.id === res.value.id)) {
          board.swimlanes.push({
            id: res.value.id,
            title,
            height,
            rank: res.value.rank,
            lists: []
          });
          sortSwimlanes();
        }
      }
      return res;
    }
  }

  async function handleSwimlaneDelete(id: number): Promise<boolean> {
    if (!hub) {
      triggerHaptic('error');
      errorStore.addError('Web.BoardHubUnavailable', 'Board connection is unavailable');
      return false;
    }

    const res = await hub.deleteSwimlane({ id });
    if (!res.ok) {
      triggerHaptic('error');
      errorStore.addError('Web.DeleteSwimlaneFailed', 'Failed to delete swimlane');
      return false;
    }

    const index = board.swimlanes.findIndex((swimlane) => swimlane.id === id);
    if (index !== -1) {
      board.swimlanes.splice(index, 1);
      board.swimlanes = [...board.swimlanes];
    }

    triggerHaptic('success');
    return true;
  }

  async function handleListConfirm(title: string, width: number | null): Promise<Response<any>> {
    if (!hub) {
      return { ok: false, problem: { title: 'Board hub unavailable', detail: 'Please try again.' } };
    }

    if (editingList) {
      const res = await hub.updateList({
        id: editingList.id,
        title,
        width: width
      });
      if (res?.ok) {
        editingList.title = title;
        editingList.width = width;
      }
      return res;
    } else if (targetSwimlaneId) {
      const res = await hub.createList({
        swimlaneId: targetSwimlaneId,
        title,
        width: width,
        beforeId: null
      });
      if (res?.ok) {
        const swimlane = board.swimlanes.find((s) => s.id === targetSwimlaneId);
        if (swimlane && !swimlane.lists.some((l) => l.id === res.value.id)) {
          swimlane.lists.push({
            id: res.value.id,
            title,
            width,
            rank: res.value.rank,
            cards: []
          });
          sortLists(swimlane);
        }
      }
      return res;
    }

    return {
      ok: false,
      problem: { title: 'Invalid target swimlane', detail: 'Please choose swimlane and try again.' }
    };
  }

  async function handleListDelete(id: number): Promise<boolean> {
    if (!hub) {
      triggerHaptic('error');
      errorStore.addError('Web.BoardHubUnavailable', 'Board connection is unavailable');
      return false;
    }

    const res = await hub.deleteList({ id });
    if (!res.ok) {
      triggerHaptic('error');
      errorStore.addError('Web.DeleteListFailed', 'Failed to delete list');
      return false;
    }

    for (const swimlane of board.swimlanes) {
      const index = swimlane.lists.findIndex((list) => list.id === id);
      if (index !== -1) {
        swimlane.lists.splice(index, 1);
        swimlane.lists = [...swimlane.lists];
        break;
      }
    }

    triggerHaptic('success');
    return true;
  }

  async function handleCardConfirm(title: string, description: string): Promise<Response<any>> {
    if (!hub) {
      return { ok: false, problem: { title: 'Board hub unavailable', detail: 'Please try again.' } };
    }

    if (editingCard) {
      const res = await hub.updateCard({
        id: editingCard.id,
        title,
        description
      });
      if (res?.ok) {
        editingCard.title = title;
        editingCard.description = description;
        // Update audit info if available in the response
        if (res.value.updatedAt) editingCard.updatedAt = res.value.updatedAt;
        if (res.value.updatedBy) editingCard.updatedBy = res.value.updatedBy as any;
      }
      return res;
    } else if (targetListId) {
      const res = await hub.createCard({
        listId: targetListId,
        title,
        description,
        beforeId: null
      });
      if (res?.ok) {
        for (const s of board.swimlanes) {
          const list = s.lists.find((l) => l.id === targetListId);
          if (list && !list.cards.some((c) => c.id === res.value.id)) {
            list.cards.push({
              id: res.value.id,
              title,
              description,
              rank: res.value.rank,
              createdAt: res.value.createdAt,
              createdBy: res.value.createdBy as any,
              updatedAt: null,
              updatedBy: null
            });
            sortCards(list);
            break;
          }
        }
      }
      return res;
    }

    return {
      ok: false,
      problem: { title: 'Invalid target list', detail: 'Please choose list and try again.' }
    };
  }

  async function handleCardDelete(id: number): Promise<boolean> {
    if (!hub) {
      triggerHaptic('error');
      errorStore.addError('Web.BoardHubUnavailable', 'Board connection is unavailable');
      return false;
    }

    const res = await hub.deleteCard({ id });
    if (!res.ok) {
      triggerHaptic('error');
      errorStore.addError('Web.DeleteCardFailed', 'Failed to delete card');
      return false;
    }

    for (const swimlane of board.swimlanes) {
      for (const list of swimlane.lists) {
        const index = list.cards.findIndex((card) => card.id === id);
        if (index !== -1) {
          list.cards.splice(index, 1);
          list.cards = [...list.cards];
          triggerHaptic('success');
          return true;
        }
      }
    }

    triggerHaptic('success');
    return true;
  }

  function openCreateSwimlaneModal() {
    editingSwimlane = undefined;
    swimlaneModalOpen = true;
  }

  function sortAll() {
    board.swimlanes.sort(
      (a: GetBoardByIdResponse.SwimlaneDto, b: GetBoardByIdResponse.SwimlaneDto) =>
        a.rank.localeCompare(b.rank)
    );
    for (const s of board.swimlanes) {
      s.lists.sort((a: GetBoardByIdResponse.ListDto, b: GetBoardByIdResponse.ListDto) =>
        a.rank.localeCompare(b.rank)
      );
      for (const l of s.lists) {
        l.cards.sort((a: GetBoardByIdResponse.CardDto, b: GetBoardByIdResponse.CardDto) =>
          a.rank.localeCompare(b.rank)
        );
      }
    }
  }

  function sortSwimlanes() {
    board.swimlanes.sort(
      (a: GetBoardByIdResponse.SwimlaneDto, b: GetBoardByIdResponse.SwimlaneDto) =>
        a.rank.localeCompare(b.rank)
    );
    board.swimlanes = [...board.swimlanes];
  }

  function sortLists(swimlane: GetBoardByIdResponse.SwimlaneDto) {
    swimlane.lists.sort((a: GetBoardByIdResponse.ListDto, b: GetBoardByIdResponse.ListDto) =>
      a.rank.localeCompare(b.rank)
    );
    swimlane.lists = [...swimlane.lists];
  }

  function sortCards(list: GetBoardByIdResponse.ListDto) {
    list.cards.sort((a: GetBoardByIdResponse.CardDto, b: GetBoardByIdResponse.CardDto) =>
      a.rank.localeCompare(b.rank)
    );
    list.cards = [...list.cards];
  }

  $effect(() => {
    if (data.board.id !== board.id) {
      board = data.board;
      sortAll();
    }

    recentBoards.add(board.id);
  });

  setContext('hub', () => hub);
  setContext('board', () => board);
  onMount(async () => {
    hub = new BoardsHub(data.board.id);

    try {
      await hub.start();

      hub.on('BoardUpdated', (payload) => {
        board.title = payload.title;
        board.description = payload.description;
      });

      hub.on('SwimlaneCreated', (payload) => {
        const existing = board.swimlanes.find((s) => s.id === payload.id);
        if (existing) {
          Object.assign(existing, payload);
          sortSwimlanes();
          return;
        }
        const newSwimlane = { ...payload, lists: [] };
        board.swimlanes.push(newSwimlane);
        sortSwimlanes();
      });

      hub.on('SwimlaneUpdated', (payload) => {
        const index = board.swimlanes.findIndex((s) => s.id === payload.id);
        if (index !== -1) {
          board.swimlanes[index].title = payload.title;
          board.swimlanes[index].height = payload.height;
        }
      });

      hub.on('SwimlaneMoved', (payload) => {
        const index = board.swimlanes.findIndex((s) => s.id === payload.id);
        if (index !== -1 && payload.rank !== board.swimlanes[index].rank) {
          board.swimlanes[index].rank = payload.rank;
          sortSwimlanes();
        }
      });

      hub.on('SwimlaneDeleted', (payload) => {
        const index = board.swimlanes.findIndex((s) => s.id === payload.id);
        if (index !== -1) {
          board.swimlanes.splice(index, 1);
        }
      });

      hub.on('ListCreated', (payload) => {
        const swimlane = board.swimlanes.find((s) => s.id === payload.swimlaneId);
        if (swimlane) {
          const existing = swimlane.lists.find((l) => l.id === payload.id);
          if (existing) {
            Object.assign(existing, payload);
            sortLists(swimlane);
            return;
          }
          const newList = { ...payload, cards: [] };
          swimlane.lists.push(newList);
          sortLists(swimlane);
        }
      });

      hub.on('ListUpdated', (payload) => {
        for (const s of board.swimlanes) {
          const list = s.lists.find((l) => l.id === payload.id);
          if (list) {
            list.title = payload.title;
            break;
          }
        }
      });

      hub.on('ListMoved', (payload) => {
        let movedList: GetBoardByIdResponse.ListDto | null = null;
        for (const s of board.swimlanes) {
          const index = s.lists.findIndex((l) => l.id === payload.id);
          if (index !== -1) {
            [movedList] = s.lists.splice(index, 1);
            s.lists = [...s.lists];
            break;
          }
        }
        const targetSwimlane = board.swimlanes.find((s) => s.id === payload.swimlaneId);
        if (movedList && targetSwimlane) {
          movedList.rank = payload.rank;
          targetSwimlane.lists.push(movedList);
          sortLists(targetSwimlane);
        }
      });

      hub.on('ListDeleted', (payload) => {
        for (const s of board.swimlanes) {
          const index = s.lists.findIndex((l) => l.id === payload.id);
          if (index !== -1) {
            s.lists.splice(index, 1);
            s.lists = [...s.lists];
            break;
          }
        }
      });

      hub.on('CardCreated', (payload) => {
        for (const s of board.swimlanes) {
          const list = s.lists.find((l) => l.id === payload.listId);
          if (list) {
            const existing = list.cards.find((c) => c.id === payload.id);
            if (existing) {
              Object.assign(existing, payload);
              sortCards(list);
              return;
            }
            list.cards.push({
              ...payload,
              // temp
              createdAt: new Date().toISOString(),
              createdBy: {
                id: 1,
                userName: 'John Doe',
                avatarUrl: null
              },
              updatedAt: null,
              updatedBy: null
            });
            sortCards(list);
            break;
          }
        }
      });

      hub.on('CardMoved', (payload) => {
        let movedCard: GetBoardByIdResponse.CardDto | null = null;
        for (const s of board.swimlanes) {
          for (const l of s.lists) {
            const index = l.cards.findIndex((c) => c.id === payload.id);
            if (index !== -1) {
              [movedCard] = l.cards.splice(index, 1);
              l.cards = [...l.cards];
              break;
            }
          }
          if (movedCard) break;
        }
        if (movedCard) {
          for (const s of board.swimlanes) {
            const targetList = s.lists.find((l) => l.id === payload.listId);
            if (targetList) {
              movedCard.rank = payload.rank;
              targetList.cards.push(movedCard);
              sortCards(targetList);
              break;
            }
          }
        }
      });

      hub.on('CardUpdated', (payload) => {
        for (const s of board.swimlanes) {
          for (const l of s.lists) {
            const card = l.cards.find((c) => c.id === payload.id);
            if (card) {
              card.title = payload.title;
              card.description = payload.description;
              return;
            }
          }
        }
      });

      hub.on('CardDeleted', (payload) => {
        for (const s of board.swimlanes) {
          for (const l of s.lists) {
            const index = l.cards.findIndex((c) => c.id === payload.id);
            if (index !== -1) {
              l.cards.splice(index, 1);
              l.cards = [...l.cards];
              return;
            }
          }
        }
      });

      hub.on('BoardDeleted', () => {
        window.location.href = '/boards/';
      });

      hub.onClose((err) => {
        connectionState = 'disconnected';
      });

      hub.onReconnecting((err) => {
        connectionState = 'reconnecting';
      });

      hub.onReconnected((connId) => {
        connectionState = 'connected';
      });

      connectionState = 'connected';
    } catch (err) {
      connectionState = 'disconnected';
      if (err instanceof Error) {
        errorStore.addError(err.name, err.message);
      } else {
        errorStore.addError('Web.WebSocketConnectionProblem', 'Failed to connect to board hub');
      }
    }
  });

  function handleSwimlaneConsider(e: CustomEvent<DndEvent<GetBoardByIdResponse.SwimlaneDto>>) {
    board.swimlanes = [...e.detail.items];
  }

  async function handleSwimlaneFinalize(
    e: CustomEvent<DndEvent<GetBoardByIdResponse.SwimlaneDto>>
  ) {
    board.swimlanes = [...e.detail.items];
    const { info } = e.detail;
    if (info.trigger === 'droppedIntoZone') {
      triggerHaptic('success');
      const id = Number(info.id);
      const index = board.swimlanes.findIndex((s) => s.id === id);
      if (index === -1) {
        return;
      }
      const nextItem = board.swimlanes[index + 1];
      const beforeId = nextItem ? nextItem.id : null;

      let res = await hub?.moveSwimlane({ id, beforeId });
      if (res?.ok) {
        const moved = board.swimlanes.find((s) => s.id === id);
        if (moved) moved.rank = res.value.rank;
        sortSwimlanes();
        board.swimlanes = [...board.swimlanes];
      } else {
        errorStore.addError('Web.MoveSwimlaneFailed', 'Failed to move swimlane');
        sortSwimlanes();
        board.swimlanes = [...board.swimlanes];
      }
    }
  }

  onDestroy(async () => {
    await hub?.stop();
    hub = null;
  });
</script>

<svelte:head>
  <title>Snapflow | {board.title}</title>
</svelte:head>

<FullBleedLayout>
  {#if connectionState !== 'connected'}
    <div class="fixed bottom-4 right-4 z-50 flex h-14 items-center gap-3 rounded-full px-6 text-sm font-medium text-white shadow-lg transition-all dark:shadow-black/40 {connectionState === 'disconnected' ? 'bg-red-600' : 'bg-primary-600 dark:bg-primary-500'}">
      {#if connectionState === 'connecting'}
        <Loader2 class="h-4 w-4 animate-spin" /> <span class="flex items-center gap-0.5">Connecting<LoadingDots /></span>
      {:else if connectionState === 'reconnecting'}
        <Loader2 class="h-4 w-4 animate-spin" /> <span class="flex items-center gap-0.5">Reconnecting<LoadingDots /></span>
      {:else}
        <div class="h-2.5 w-2.5 rounded-full bg-white bg-red-600"></div> <span>Disconnected</span>
      {/if}
    </div>
  {/if}
  <div class="w-full space-y-0 overflow-x-clip pb-12" in:fade={{ duration: 400 }}>
    <div class="relative left-1/2 right-1/2 -ml-[50vw] -mr-[50vw] w-screen border-b border-gray-200 bg-white shadow-sm dark:border-gray-800 dark:bg-gray-900">
      <div class="flex w-full items-center px-8 sm:px-10 lg:px-14 xl:px-20">
        <div class="shrink-0 pr-8 sm:pr-10 lg:pr-14 xl:pr-20">
          <GoBackButton
            href="/boards"
            hideTextOnMobile={true}
          />

        </div>

        <div class="min-w-0 flex-1 py-2 border-x border-gray-200 px-8 sm:px-10 lg:px-14 xl:px-20 dark:border-gray-700">
          <h1 class="truncate text-xl font-bold tracking-tight text-gray-900 dark:text-white">
            {board.title}
          </h1>
          <p class="mt-0.5 truncate text-sm text-gray-500 dark:text-gray-400">
            {board.description?.trim()}
          </p>
        </div>

        <div class="shrink-0 pl-8 sm:pl-10 lg:pl-14 xl:pl-20">
          <Button
            href={`/boards/${board.id}/edit`}
            variant="ghost"
            size="sm"
            class="h-9 min-w-9 justify-center px-2.5 text-gray-600 hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white text-sm sm:h-11 sm:min-w-32 sm:px-4"
            startIcon={Pencil}
            aria-label="Edit board"
          >
            <span class="hidden sm:inline">Edit Board</span>
          </Button>
        </div>
      </div>
    </div>

    <section class="space-y-6">
      {#if board.swimlanes.length === 0}
        <div
          class="mb-6 flex flex-col items-center justify-center rounded-2xl border-2 border-dashed border-gray-200 px-4 py-12 text-center dark:border-gray-700 sm:py-16"
        >
          <div
            class="mb-5 flex h-16 w-16 items-center justify-center rounded-full bg-gray-50 dark:bg-gray-800/40"
          >
            <Folders class="h-8 w-8 text-gray-400" />
          </div>
          <h2 class="mb-2 text-xl font-semibold text-gray-900 dark:text-white">No swimlanes yet</h2>
          <p class="max-w-md text-sm text-gray-500 dark:text-gray-400">
            Start by creating your first swimlane to structure this board.
          </p>
        </div>
      {/if}

      <div class="relative left-1/2 right-1/2 -ml-[50vw] -mr-[50vw] w-screen">
        <section
          use:dragHandleZone={{
            items: board.swimlanes,
            flipDurationMs: 150,
            type: 'swimlanes',
            dropTargetStyle: {},
            useCursorForDetection: true,
            zoneTabIndex: -1,
            zoneItemTabIndex: 0
          }}
          onconsider={handleSwimlaneConsider}
          onfinalize={handleSwimlaneFinalize}
          class="flex flex-col gap-0"
        >
          {#each board.swimlanes as swimlane, index (swimlane.id)}
            <div
              animate:flip={{ duration: 150 }}
              class="relative z-20 w-full transition-shadow duration-200 focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none dark:focus-visible:ring-offset-gray-900"
            >
              <Swimlane bind:swimlane={board.swimlanes[index]} />
            </div>
          {/each}
        </section>

        <div class="relative z-10 overflow-hidden">
          <Button
            type="button"
            variant="outline"
            startIcon={Plus}
            onclick={openCreateSwimlaneModal}
            aria-label="Add swimlane"
            class="mt-1 h-12 w-full justify-center border-dashed border-gray-300 bg-gray-50/60 px-3 text-gray-600 hover:border-gray-400 hover:bg-gray-100 dark:border-gray-700 dark:bg-gray-800/40 dark:text-gray-300 dark:hover:bg-gray-800"
          >
            <span class="sr-only">Add swimlane</span>
          </Button>
        </div>
      </div>
    </section>
  </div>
</FullBleedLayout>

<SwimlaneModal
  bind:open={swimlaneModalOpen}
  swimlane={editingSwimlane}
  onConfirm={handleSwimlaneConfirm}
  onDelete={handleSwimlaneDelete}
/>

<ListModal
  bind:open={listModalOpen}
  list={editingList}
  onConfirm={handleListConfirm}
  onDelete={handleListDelete}
/>

<CardModal
  bind:open={cardModalOpen}
  card={editingCard}
  onConfirm={handleCardConfirm}
  onDelete={handleCardDelete}
/>

<style>
  :global(.swimlane-ghost) {
    opacity: 0.5;
    background: var(--color-gray-300) !important;
    border: 2px dashed var(--color-gray-500) !important;
  }

  :global(.dark .swimlane-ghost) {
    background: var(--color-gray-700) !important;
    border-color: var(--color-gray-400) !important;
  }

  :global(.swimlane-chosen) {
    cursor: grabbing !important;
  }

  :global(.swimlane-drag) {
    box-shadow:
      0 20px 25px -5px rgba(0, 0, 0, 0.2),
      0 10px 10px -5px rgba(0, 0, 0, 0.1) !important;
    opacity: 0.95 !important;
    transform: rotate(0.5deg);
    background: var(--color-white) !important;
    border: 1px solid var(--color-gray-200) !important;
  }

  :global(.dark .swimlane-drag) {
    background: var(--color-gray-800) !important;
    border-color: var(--color-gray-700) !important;
    box-shadow:
      0 20px 25px -5px rgba(0, 0, 0, 0.4),
      0 10px 10px -5px rgba(0, 0, 0, 0.2) !important;
  }
</style>
