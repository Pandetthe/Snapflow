<script lang="ts">
  import { flip } from 'svelte/animate';
  import {
    dragHandle,
    SHADOW_ITEM_MARKER_PROPERTY_NAME,
    SOURCES,
    TRIGGERS
  } from 'svelte-dnd-action';
  import type { DndEvent } from 'svelte-dnd-action';
  import { boardZone } from '$lib/features/boards/actions/boardZone';
  import { dragHandles } from '$lib/features/boards/stores/dragHandles';
  import List from './List.svelte';
  import { getContext, tick } from 'svelte';
  import { getBoardUI } from '$lib/features/boards/context/board.context';
  import { BoardsHub } from '$lib/features/boards/hub/boards.hub';
  import type { GetBoardByIdResponse } from '$lib/features/boards/types/boards.api';
  import { errorStore } from '$lib/ui/stores/error.svelte';
  import { Button } from '$lib/ui/components';
  import { ScrollArea } from 'bits-ui';
  import { triggerHaptic } from '$lib/ui/utils';
  import { GripVertical, Pencil, Plus } from 'lucide-svelte';
  import type {
    GetRecentMove,
    IsInFlight,
    IsLeaving,
    IsNew
  } from '$lib/features/boards/composables/boardState.svelte';
  import { LAYOUT_FLIP_MS, layoutFlip } from '$lib/features/boards/animations/motion';
  import {
    forgetDraggedList,
    holdListZoneHeights,
    measureDraggedList,
    releaseListZoneHeights,
    sizeDraggedList
  } from '$lib/features/boards/animations/zoneHeights';
  import {
    dragPointerX,
    forgetDragPointer,
    trackDragPointer
  } from '$lib/features/boards/dragPointer';
  import MovedByIndicator from './MovedByIndicator.svelte';
  import { storedToCss } from '$lib/features/boards/sizes';

  let { swimlane = $bindable() }: { swimlane: GetBoardByIdResponse.SwimlaneDto } = $props();

  const getHub = getContext<() => BoardsHub | null>('hub');
  const hub = $derived(getHub());
  const getBoardState = getContext<() => string>('boardState');
  const boardState = $derived(getBoardState());
  const getCanManageSwimlanes = getContext<() => boolean>('canManageSwimlanes');
  const canManageSwimlanes = $derived(getCanManageSwimlanes());
  const getCanManageLists = getContext<() => boolean>('canManageLists');
  const canManageLists = $derived(getCanManageLists());
  const getRecentMove = getContext<GetRecentMove | undefined>('recentMove');
  const recentMove = $derived(getRecentMove?.('swimlane', swimlane.id));
  const getIsInFlight = getContext<IsInFlight | undefined>('isInFlight');
  const inFlight = $derived(getIsInFlight?.('swimlane', swimlane.id) ?? false);
  const isNew = getContext<IsNew | undefined>('isNew');
  const isLeaving = getContext<IsLeaving | undefined>('isLeaving');

  const ui = getBoardUI();

  // List picked up with the keyboard, shown as selected until it is dropped.
  let keyboardMovedListId = $state<number | null>(null);

  let listZoneEl = $state<HTMLElement | null>(null);

  // The drop slot of the dragged list is in this swimlane.
  const receivingList = $derived(
    swimlane.lists.some(
      (l) => (l as unknown as Record<string, unknown>)[SHADOW_ITEM_MARKER_PROPERTY_NAME]
    )
  );

  // An empty zone spans the whole swimlane. It keeps that width while a list hovers it: shrinking to the
  // drop slot would move the zone out from under the cursor, dropping the slot and growing back in a loop.
  const fillsSwimlane = $derived(
    swimlane.lists.every(
      (l) => (l as unknown as Record<string, unknown>)[SHADOW_ITEM_MARKER_PROPERTY_NAME]
    )
  );

  function handleListConsider(e: CustomEvent<DndEvent<GetBoardByIdResponse.ListDto>>) {
    swimlane.lists = e.detail.items;
    const { info } = e.detail;
    if (info.source === SOURCES.KEYBOARD) keyboardMovedListId = Number(info.id);
    if (info.trigger === TRIGGERS.DRAG_STARTED) {
      measureDraggedList(Number(info.id));
      holdListZoneHeights();
      trackDragPointer();
    }
    if (info.trigger === TRIGGERS.DRAG_STOPPED) endListDrag();
  }

  function endListDrag() {
    keyboardMovedListId = null;
    forgetDraggedList();
    releaseListZoneHeights();
    forgetDragPointer();
  }

  /** The list of this swimlane that a drop at `x` belongs before, or null for the end of the row. */
  function listAfter(x: number | null) {
    if (x === null) return null;
    for (const list of swimlane.lists) {
      const el = listZoneEl?.querySelector(`[data-list-id="${list.id}"]`);
      if (!el) continue;
      const rect = el.getBoundingClientRect();
      if (x < rect.left + rect.width / 2) return list.id;
    }
    return null;
  }

  async function moveListHere(list: GetBoardByIdResponse.ListDto, beforeId: number | null) {
    const at =
      beforeId === null
        ? swimlane.lists.length
        : swimlane.lists.findIndex((l) => l.id === beforeId);
    const next = [...swimlane.lists];
    next.splice(at === -1 ? next.length : at, 0, list);
    swimlane.lists = next;

    const res = await hub?.moveList({ id: list.id, swimlaneId: swimlane.id, beforeId });

    if (res && res.ok) {
      const moved = swimlane.lists.find((l) => l.id === list.id);
      if (moved && res.value?.rank) moved.rank = res.value.rank;
    } else {
      errorStore.addError('Web.MoveListFailed', 'Failed to move list');
    }
    swimlane.lists.sort((a, b) => a.rank.localeCompare(b.rank));
    swimlane.lists = [...swimlane.lists];
  }

  // The header bar takes a list like the swimlane's body does: the list it holds while one is dragged over
  // it is the drop slot, and on drop the list joins this swimlane where the pointer points along the bar.
  let headerDrop = $state<GetBoardByIdResponse.ListDto[]>([]);

  function handleHeaderConsider(e: CustomEvent<DndEvent<GetBoardByIdResponse.ListDto>>) {
    headerDrop = e.detail.items;
  }

  async function handleHeaderFinalize(e: CustomEvent<DndEvent<GetBoardByIdResponse.ListDto>>) {
    const { info } = e.detail;
    const id = Number(info.id);
    const dropped = e.detail.items.find((l) => l.id === id);
    headerDrop = [];

    if (info.trigger !== TRIGGERS.DROPPED_INTO_ZONE || !dropped) return;

    // Read where the drop landed before ending the drag, which forgets the pointer.
    const beforeId = listAfter(dragPointerX());
    if (info.source === SOURCES.POINTER) endListDrag();

    triggerHaptic('success');
    // The zone the list came from is told right after this handler; putting the list back only once that has
    // happened keeps its swimlane, when the list came from this one, from moving the same list a second time.
    await tick();
    await moveListHere(dropped, beforeId);
  }

  $effect(() => {
    if (receivingList && listZoneEl) sizeDraggedList(listZoneEl);
  });

  async function handleListFinalize(e: CustomEvent<DndEvent<GetBoardByIdResponse.ListDto>>) {
    swimlane.lists = e.detail.items;
    const { info } = e.detail;
    if (info.source === SOURCES.POINTER) endListDrag();
    else if (info.trigger === TRIGGERS.DROPPED_INTO_ANOTHER) keyboardMovedListId = null;

    if (
      info.trigger === TRIGGERS.DROPPED_INTO_ZONE ||
      info.trigger === TRIGGERS.DROPPED_INTO_ANOTHER
    ) {
      triggerHaptic('success');
      const id = Number(info.id);

      const index = swimlane.lists.findIndex((l) => l.id === id);
      if (index === -1) {
        return;
      }

      const nextItem = swimlane.lists[index + 1];
      const beforeId = nextItem ? nextItem.id : null;

      let res = await hub?.moveList({ id, swimlaneId: swimlane.id, beforeId });

      if (res && res.ok) {
        const movedItem = swimlane.lists.find((l) => l.id === id);
        if (movedItem && res.value?.rank) {
          movedItem.rank = res.value.rank;
        }
        swimlane.lists.sort((a, b) => a.rank.localeCompare(b.rank));
        swimlane.lists = [...swimlane.lists];
      } else {
        errorStore.addError('Web.MoveListFailed', 'Failed to move list');
        swimlane.lists.sort((a, b) => a.rank.localeCompare(b.rank));
        swimlane.lists = [...swimlane.lists];
      }
    }
  }
</script>

<div
  data-id={swimlane.id}
  data-swimlane-id={swimlane.id}
  data-board-item="swimlane"
  role="group"
  class:flight-hidden={inFlight}
  class:flight-settle={recentMove?.pop && !inFlight}
  style:height={swimlane.height ? storedToCss(swimlane.height) : undefined}
  data-fixed-height={swimlane.height ? true : undefined}
  class="group/swimlane relative flex flex-col border-b border-gray-200 dark:border-gray-700/60 {swimlane.height
    ? ''
    : 'min-h-45 flex-1'}"
>
  <MovedByIndicator move={recentMove} rounded="rounded-none" />

  <!-- Header band — always visible, even when collapsed during drag -->
  <div
    class="swimlane-header board-item-bar relative flex h-11 shrink-0 items-center gap-1.5 bg-gray-50 px-3 dark:bg-gray-800/70"
  >
    <!-- Takes a list dropped on the bar; empty and invisible until one is dragged over it -->
    <div
      use:boardZone={{
        useHandle: false,
        items: headerDrop,
        flipDurationMs: LAYOUT_FLIP_MS,
        type: 'lists',
        dropTargetStyle: {},
        dropTargetClasses: [],
        morphDisabled: true,
        useCursorForDetection: true,
        zoneTabIndex: -1,
        dragDisabled: true
      }}
      onconsider={handleHeaderConsider}
      onfinalize={handleHeaderFinalize}
      data-board-zone="lists-header"
      class="pointer-events-none absolute inset-0 z-20"
      aria-hidden="true"
    >
      {#each headerDrop as list (list.id)}
        <div class="relative h-full w-full" data-board-slot="list" animate:flip={layoutFlip}></div>
      {/each}
    </div>
    {#if canManageSwimlanes && $dragHandles === 'hidden'}
      <!-- No grip to grab, so the bar itself picks the swimlane up; its buttons sit above this -->
      <div
        use:dragHandle
        class="absolute inset-0 touch-none focus-visible:outline-none"
        aria-label="Drag swimlane"
      ></div>
    {/if}

    {#if canManageSwimlanes && $dragHandles !== 'hidden'}
      <div
        use:dragHandle
        class="board-drag-handle board-control touch-none focus-visible:outline-none {boardState ===
        'connected'
          ? 'cursor-grab'
          : 'cursor-not-allowed opacity-40'}"
        aria-label="Drag swimlane"
      >
        <GripVertical class="h-3.5 w-3.5" />
      </div>
    {/if}

    <!-- The title and the count stay under the bar's drag surface, so the bar can be grabbed by them -->
    <h2 class="min-w-0 flex-1 truncate text-sm font-semibold text-gray-800 dark:text-gray-100">
      {swimlane.title}
    </h2>

    <span class="board-count" title="Lists">{swimlane.lists.length}</span>

    {#if canManageSwimlanes}
      <Button
        type="button"
        variant="ghost"
        size="xs"
        disabled={boardState !== 'connected'}
        onclick={() => ui.openSwimlaneModal(swimlane)}
        startIcon={Pencil}
        class="board-control relative z-10"
        title="Edit swimlane"
      >
        <span class="sr-only">Edit swimlane</span>
      </Button>
    {/if}
  </div>

  <!-- Content area — hidden when this swimlane is being dragged -->
  <!-- Flex column so the viewport gets a definite height and an empty list zone can fill the swimlane -->
  <ScrollArea.Root
    class="swimlane-content swimlane-scroll-area relative flex flex-1 flex-col overflow-hidden bg-white/60 dark:bg-gray-900/40"
    type="auto"
  >
    <ScrollArea.Viewport class="flex min-h-0 w-full flex-1 flex-col rounded-[inherit]">
      <div class="flex min-h-0 flex-1 px-3 py-3">
        <!--
          The drop area reaches under "Add list" via padding cancelled by a negative margin, so dropping
          right after the last list (or into an empty swimlane) is easy to hit without changing the layout.
        -->
        <section
          use:boardZone={{
            // Always by a handle: with the grip hidden the list's bar is the handle, so dragging from the
            // body cannot fight the cards inside it.
            useHandle: true,
            items: swimlane.lists,
            flipDurationMs: LAYOUT_FLIP_MS,
            type: 'lists',
            dropTargetStyle: {},
            dropTargetClasses: ['board-drop-target'],
            // Lists keep their own size; morphing the clone to the growing drop slot made it shrink and grow back.
            morphDisabled: true,
            useCursorForDetection: true,
            zoneTabIndex: -1,
            dragDisabled: boardState !== 'connected'
          }}
          onconsider={handleListConsider}
          onfinalize={handleListFinalize}
          bind:this={listZoneEl}
          data-board-zone="lists"
          data-empty={swimlane.lists.length === 0 || undefined}
          data-receiving={receivingList || undefined}
          data-fill={fillsSwimlane || undefined}
          class="flex min-h-9 items-stretch gap-3 self-stretch {fillsSwimlane
            ? '-mr-[100%] w-full'
            : '-mr-28 pr-28'}"
        >
          {#each swimlane.lists as list, index (list.id)}
            <div
              class="relative z-20 flex min-h-0 self-stretch rounded-xl outline-none"
              class:board-enter={isNew?.('list', list.id)}
              class:board-leave={isLeaving?.('list', list.id)}
              data-board-slot="list"
              data-selected={keyboardMovedListId === list.id || undefined}
              animate:flip={layoutFlip}
            >
              <List bind:list={swimlane.lists[index]} swimlaneId={swimlane.id} />
            </div>
          {/each}
        </section>

        {#if canManageLists}
          <div class="relative z-10 ml-3 self-start">
            <Button
              type="button"
              variant="ghost"
              startIcon={Plus}
              disabled={boardState !== 'connected'}
              onclick={() => ui.openListModal(swimlane.id)}
              aria-label="Add list"
              class="add-list-button h-9 justify-start gap-1.5 rounded-lg px-3 text-xs font-medium text-gray-400 hover:bg-gray-100 hover:text-gray-600 dark:text-gray-500 dark:hover:bg-gray-700/60 dark:hover:text-gray-300"
            >
              Add list
            </Button>
          </div>
        {/if}
      </div>
    </ScrollArea.Viewport>
    <ScrollArea.Scrollbar
      orientation="horizontal"
      class="flex h-2 touch-none flex-col bg-transparent p-0.5 transition-colors duration-200 select-none hover:bg-black/5 dark:hover:bg-white/5"
    >
      <ScrollArea.Thumb
        class="relative flex-1 rounded-full bg-gray-300 transition-colors duration-200 hover:bg-gray-400 dark:bg-gray-600 dark:hover:bg-gray-500"
      />
    </ScrollArea.Scrollbar>
  </ScrollArea.Root>
</div>

<style>
  /* A flex column instead of height: 100%, which cannot resolve against the viewport's flexed height */
  :global(.swimlane-scroll-area [data-scroll-area-viewport] > [data-scroll-area-content]) {
    display: flex;
    flex-direction: column;
  }

  :global(
    [data-fixed-height]
      > .swimlane-scroll-area
      [data-scroll-area-viewport]
      > [data-scroll-area-content]
  ) {
    flex: 1 1 0;
    min-height: 0;
  }
</style>
