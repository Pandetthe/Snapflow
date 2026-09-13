<script lang="ts">
  import { flip } from 'svelte/animate';
  import { dragHandle, dragHandleZone, SHADOW_ITEM_MARKER_PROPERTY_NAME, TRIGGERS } from 'svelte-dnd-action';
  import type { DndEvent } from 'svelte-dnd-action';
  import List from './List.svelte';
  import { getContext } from 'svelte';
  import { getBoardUI } from '$lib/features/boards/context/board.context';
  import { BoardsHub } from '$lib/features/boards/hub/boards.hub';
  import type { GetBoardByIdResponse } from '$lib/features/boards/types/boards.api';
  import { errorStore } from '$lib/ui/stores/error.svelte';
  import { Button } from '$lib/ui/components';
  import { ScrollArea } from 'bits-ui';
  import { triggerHaptic } from '$lib/ui/utils';
  import { GripVertical, Pencil, Plus } from 'lucide-svelte';

  let { swimlane = $bindable() }: { swimlane: GetBoardByIdResponse.SwimlaneDto } = $props();

  const getHub = getContext<() => BoardsHub | null>('hub');
  const hub = $derived(getHub());
  const getBoard = getContext<() => GetBoardByIdResponse.BoardDto>('board');
  const getBoardState = getContext<() => string>('boardState');
  const boardState = $derived(getBoardState());
  const getCanManageSwimlanes = getContext<() => boolean>('canManageSwimlanes');
  const canManageSwimlanes = $derived(getCanManageSwimlanes());
  const getCanManageLists = getContext<() => boolean>('canManageLists');
  const canManageLists = $derived(getCanManageLists());

  const ui = getBoardUI();

  function handleListConsider(e: CustomEvent<DndEvent<GetBoardByIdResponse.ListDto>>) {
    swimlane.lists = e.detail.items;
  }

  async function handleListFinalize(e: CustomEvent<DndEvent<GetBoardByIdResponse.ListDto>>) {
    swimlane.lists = e.detail.items;
    const { info } = e.detail;

    if (info.trigger === TRIGGERS.DROPPED_INTO_ZONE || info.trigger === TRIGGERS.DROPPED_INTO_ANOTHER) {
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
  role="group"
  style:height={swimlane.height ? `${swimlane.height}px` : undefined}
  class="group/swimlane flex flex-col border-b border-gray-200 dark:border-gray-700/60 {swimlane.height
    ? ''
    : 'flex-1 min-h-[180px]'}"
>
  <!-- Header band — always visible, even when collapsed during drag -->
  <div class="swimlane-header flex h-11 shrink-0 items-center gap-2 bg-gray-50 px-3 dark:bg-gray-800/70">
    {#if canManageSwimlanes}
      <div
        use:dragHandle
        class="touch-none rounded p-1 text-gray-400 transition-colors duration-150 hover:bg-gray-200 hover:text-gray-600 focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:outline-none dark:hover:bg-gray-700 dark:text-gray-500 dark:hover:text-gray-300 {boardState === 'connected' ? 'cursor-grab' : 'cursor-not-allowed opacity-40'}"
        aria-label="Drag swimlane"
      >
        <GripVertical class="h-4 w-4" />
      </div>
    {/if}

    <h2 class="min-w-0 flex-1 truncate text-sm font-semibold text-gray-800 dark:text-gray-100">
      {swimlane.title}
    </h2>

    <span class="shrink-0 rounded-full bg-gray-200 px-2 py-0.5 text-[11px] font-medium tabular-nums text-gray-500 dark:bg-gray-700 dark:text-gray-400">
      {swimlane.lists.length}
    </span>

    {#if canManageSwimlanes}
      <Button
        type="button"
        variant="ghost"
        size="xs"
        disabled={boardState !== 'connected'}
        onclick={() => ui.openSwimlaneModal(swimlane)}
        startIcon={Pencil}
        class="h-7 w-7 min-w-0 shrink-0 rounded p-0 text-gray-400 opacity-0 transition-opacity duration-150 hover:bg-gray-200 hover:text-gray-600 group-hover/swimlane:opacity-100 dark:hover:bg-gray-700 dark:hover:text-gray-300"
        title="Edit swimlane"
      >
        <span class="sr-only">Edit swimlane</span>
      </Button>
    {/if}
  </div>

  <!-- Content area — hidden when this swimlane is being dragged -->
  <ScrollArea.Root class="swimlane-content swimlane-scroll-area relative flex-1 overflow-hidden bg-white/60 dark:bg-gray-900/40" type="auto">
    <ScrollArea.Viewport class="h-full w-full rounded-[inherit]">
      <div class="flex h-full px-3 py-3">
        <section
          use:dragHandleZone={{
            items: swimlane.lists,
            flipDurationMs: 150,
            type: 'lists',
            dropTargetStyle: {},
            useCursorForDetection: true,
            zoneTabIndex: -1,
            dragDisabled: boardState !== 'connected'
          }}
          onconsider={handleListConsider}
          onfinalize={handleListFinalize}
          class="flex h-full items-stretch gap-3"
        >
          {#each swimlane.lists as list, index (list.id)}
            <div
              class="relative z-20 flex min-h-0 self-stretch rounded-xl transition-shadow duration-200 focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none dark:focus-visible:ring-offset-gray-900"
              animate:flip={{ duration: 150 }}
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
  :global(.swimlane-scroll-area [data-scroll-area-viewport] > [data-scroll-area-content]) {
    height: 100%;
  }

  /* Collapse swimlane to header-only band while dragging */
  :global(.swimlane-drag .swimlane-content),
  :global(.swimlane-ghost .swimlane-content) {
    display: none !important;
  }

  :global(.swimlane-drag),
  :global(.swimlane-ghost) {
    height: auto !important;
    flex: none !important;
    min-height: 0 !important;
  }

  :global(.list-ghost) {
    opacity: 0.45;
    background: var(--color-brand-50) !important;
    border: 2px dashed var(--color-brand-300) !important;
    border-radius: 0.75rem !important;
  }

  :global(.dark .list-ghost) {
    background: color-mix(in srgb, var(--color-brand-500) 12%, transparent) !important;
    border-color: var(--color-brand-600) !important;
  }

  :global(.list-chosen) {
    cursor: grabbing !important;
  }

  :global(.list-drag) {
    box-shadow:
      0 20px 30px -8px rgba(70, 95, 255, 0.15),
      0 8px 12px -4px rgba(0, 0, 0, 0.08) !important;
    opacity: 0.97 !important;
    transform: rotate(0.8deg) scale(1.01);
    border-radius: 0.75rem !important;
  }

  :global(.dark .list-drag) {
    box-shadow:
      0 20px 30px -8px rgba(0, 0, 0, 0.4),
      0 8px 12px -4px rgba(0, 0, 0, 0.3) !important;
  }
</style>
