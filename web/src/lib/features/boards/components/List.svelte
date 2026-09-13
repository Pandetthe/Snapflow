<script lang="ts">
  import { flip } from 'svelte/animate';
  import { dragHandleZone, dragHandle, TRIGGERS } from 'svelte-dnd-action';
  import type { DndEvent } from 'svelte-dnd-action';
  import { getContext } from 'svelte';
  import { getBoardUI } from '$lib/features/boards/context/board.context';
  import type { BoardsHub } from '$lib/features/boards/hub/boards.hub';
  import Card from './Card.svelte';
  import { ScrollArea } from 'bits-ui';
  import { errorStore } from '$lib/ui/stores/error.svelte';
  import { triggerHaptic } from '$lib/ui/utils';
  import { Button } from '$lib/ui/components';
  import { GripVertical, Pencil, Plus } from 'lucide-svelte';
  import type { GetBoardByIdResponse } from '$lib/features/boards/types/boards.api';

  let { list = $bindable(), swimlaneId }: { list: GetBoardByIdResponse.ListDto; swimlaneId: number } = $props();

  const getHub = getContext<() => BoardsHub | null>('hub');
  const hub = $derived(getHub());
  const getBoardState = getContext<() => string>('boardState');
  const boardState = $derived(getBoardState());
  const getCanManageLists = getContext<() => boolean>('canManageLists');
  const canManageLists = $derived(getCanManageLists());
  const getCanManageCards = getContext<() => boolean>('canManageCards');
  const canManageCards = $derived(getCanManageCards());

  const ui = getBoardUI();

  function handleCardConsider(e: CustomEvent<DndEvent<GetBoardByIdResponse.CardDto>>) {
    list.cards = e.detail.items;
  }

  async function handleCardFinalize(e: CustomEvent<DndEvent<GetBoardByIdResponse.CardDto>>) {
    list.cards = e.detail.items;
    const { info } = e.detail;
    if (info.trigger === TRIGGERS.DROPPED_INTO_ZONE || info.trigger === TRIGGERS.DROPPED_INTO_ANOTHER) {
      triggerHaptic('success');
      const id = Number(info.id);
      const index = list.cards.findIndex((c) => c.id === id);
      const nextItem = list.cards[index + 1];
      const beforeId = nextItem ? nextItem.id : null;

      let res = await hub?.moveCard({ id, listId: list.id, beforeId });
      if (res && res.ok) {
        const movedItem = list.cards.find((c) => c.id === id);
        if (movedItem) movedItem.rank = res.value.rank;
        list.cards.sort((a, b) => a.rank.localeCompare(b.rank));
        list.cards = [...list.cards];
      } else {
        errorStore.addError('Web.MoveCardFailed', 'Failed to move card');
        list.cards.sort((a, b) => a.rank.localeCompare(b.rank));
        list.cards = [...list.cards];
      }
    }
  }
</script>

<div
  role="group"
  style:width={list.width ? `${list.width}px` : 'auto'}
  class="group/list flex h-full max-h-full min-h-0 min-w-[220px] shrink-0 flex-col overflow-hidden rounded-xl border border-gray-200/80 bg-gray-50 shadow-sm transition-shadow duration-200 dark:border-gray-700/50 dark:bg-gray-900/40 dark:shadow-black/20"
>
  <!-- List header -->
  <div class="flex shrink-0 items-center gap-1.5 border-b border-gray-200 bg-gray-100/80 px-2.5 py-2 dark:border-gray-700/60 dark:bg-gray-800/90">
    {#if canManageLists}
      <div
        use:dragHandle
        class="list-drag-handle touch-none rounded p-1 text-gray-400 opacity-0 transition-all duration-150 hover:bg-gray-200 hover:text-gray-600 focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:outline-none group-hover/list:opacity-100 dark:hover:bg-gray-700 dark:text-gray-500 dark:hover:text-gray-300 {boardState === 'connected' ? 'cursor-grab' : 'cursor-not-allowed opacity-30'}"
      >
        <GripVertical class="h-3.5 w-3.5" />
      </div>
    {/if}

    <h3 class="min-w-0 flex-1 truncate text-xs font-semibold text-gray-700 dark:text-gray-200">
      {list.title}
    </h3>

    <span class="shrink-0 rounded-full bg-gray-100 px-1.5 py-0.5 text-[10px] font-medium tabular-nums text-gray-500 dark:bg-gray-700/80 dark:text-gray-400">
      {list.cards.length}
    </span>

    {#if canManageLists}
      <Button
        type="button"
        variant="ghost"
        size="xs"
        onclick={() => ui.openListModal(swimlaneId, list)}
        disabled={boardState !== 'connected'}
        aria-label="Edit list"
        startIcon={Pencil}
        class="h-6 w-6 min-w-0 shrink-0 rounded p-0 text-gray-400 opacity-0 transition-opacity duration-150 hover:bg-gray-200 hover:text-gray-600 group-hover/list:opacity-100 dark:hover:bg-gray-700 dark:hover:text-gray-300"
        title="Edit list"
      >
        <span class="sr-only">Edit list</span>
      </Button>
    {/if}
  </div>

  <!-- Cards area -->
  <ScrollArea.Root class="list-scroll-area relative flex-1 overflow-hidden" type="auto">
    <ScrollArea.Viewport class="h-full w-full rounded-[inherit]">
      <div class="flex h-full min-h-0 flex-col p-2">
        <section
          use:dragHandleZone={{
            items: list.cards,
            flipDurationMs: 150,
            type: 'cards',
            dropTargetStyle: {},
            useCursorForDetection: true,
            zoneTabIndex: -1,
            dragDisabled: boardState !== 'connected'
          }}
          onconsider={handleCardConsider}
          onfinalize={handleCardFinalize}
          class="flex min-h-[40px] flex-1 flex-col gap-1.5"
        >
          {#each list.cards as card (card.id)}
            <div
              animate:flip={{ duration: 150 }}
              class="relative z-20 rounded-lg transition-shadow duration-200 focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none dark:focus-visible:ring-offset-gray-900"
            >
              <Card {card} listId={list.id} />
            </div>
          {/each}
        </section>

        {#if canManageCards}
          <div class="relative z-10 mt-1.5">
            <Button
              type="button"
              variant="ghost"
              startIcon={Plus}
              onclick={() => ui.openCardModal(list.id)}
              disabled={boardState !== 'connected'}
              aria-label="Add card"
              class="add-card-button h-9 justify-start gap-1.5 rounded-lg px-3 text-xs font-medium text-gray-400 hover:bg-gray-100 hover:text-gray-600 dark:text-gray-500 dark:hover:bg-gray-700/60 dark:hover:text-gray-300"
            >
              Add card
            </Button>
          </div>
        {/if}
      </div>
    </ScrollArea.Viewport>
    <ScrollArea.Scrollbar
      orientation="vertical"
      class="z-20 flex w-1.5 touch-none bg-transparent p-px transition-colors duration-200 select-none hover:bg-black/5 dark:hover:bg-white/5"
    >
      <ScrollArea.Thumb
        class="relative flex-1 rounded-full bg-gray-300 transition-colors duration-200 hover:bg-gray-400 dark:bg-gray-600 dark:hover:bg-gray-500"
      />
    </ScrollArea.Scrollbar>
  </ScrollArea.Root>
</div>

<style>
  :global(.list-scroll-area [data-scroll-area-viewport] > [data-scroll-area-content]) {
    height: 100%;
  }

  :global(.card-ghost) {
    opacity: 0.4;
    background: var(--color-brand-50) !important;
    border: 1.5px dashed var(--color-brand-300) !important;
    border-radius: 0.5rem !important;
    box-shadow: none !important;
  }

  :global(.dark .card-ghost) {
    background: color-mix(in srgb, var(--color-brand-500) 10%, transparent) !important;
    border-color: var(--color-brand-600) !important;
  }

  :global(.card-chosen) {
    cursor: grabbing !important;
  }

  :global(.card-drag) {
    box-shadow:
      0 12px 20px -4px rgba(70, 95, 255, 0.12),
      0 4px 8px -2px rgba(0, 0, 0, 0.08) !important;
    opacity: 0.97 !important;
    transform: rotate(1deg) scale(1.02);
    border-radius: 0.5rem !important;
  }

  :global(.dark .card-drag) {
    box-shadow:
      0 12px 20px -4px rgba(0, 0, 0, 0.4),
      0 4px 8px -2px rgba(0, 0, 0, 0.3) !important;
  }
</style>
