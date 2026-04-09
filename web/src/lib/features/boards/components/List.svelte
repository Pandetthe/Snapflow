<script lang="ts">
  import { flip } from 'svelte/animate';
  import { dragHandleZone, dragHandle, TRIGGERS } from 'svelte-dnd-action';
  import type { DndEvent } from 'svelte-dnd-action';
  import { getContext } from 'svelte';
  import type { BoardsHub } from '$lib/features/boards/hub/boards.hub';
  import Card from './Card.svelte';
  import { ScrollArea } from 'bits-ui';
  import { errorStore } from '$lib/ui/stores/error';
  import { triggerHaptic } from '$lib/ui/utils';
  import { Button } from '$lib/ui/components';
  import { GripVertical, Pencil, Plus } from 'lucide-svelte';
  import type { GetBoardByIdResponse } from '$lib/features/boards/types/boards.api';

  let { list = $bindable(), swimlaneId }: { list: GetBoardByIdResponse.ListDto; swimlaneId: number } = $props();

  const getHub = getContext<() => BoardsHub | null>('hub');
  const hub = $derived(getHub());
  const getBoardState = getContext<() => string>('boardState');
  const boardState = $derived(getBoardState());

  interface BoardUI {
    openListModal: (swimlaneId: number, list?: GetBoardByIdResponse.ListDto) => void;
    openCardModal: (listId: number, card?: GetBoardByIdResponse.CardDto) => void;
  }
  const ui = getContext<BoardUI>('ui');

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
  class="flex h-full max-h-full min-h-0 min-w-[220px] shrink-0 flex-col overflow-hidden rounded-lg bg-transparent px-1 transition-all duration-200 focus-within:shadow-sm"
>
  <div class="group mb-2 flex items-start justify-between gap-2 rounded-lg border border-gray-200/80 bg-gray-50/95 px-3 py-1.5 dark:border-gray-700/70 dark:bg-gray-700/45">
    <div class="min-w-0 flex-1">
      <div class="flex items-center gap-2">
        <div
          use:dragHandle
          class="list-drag-handle show-on-hover touch-none rounded-md p-1 text-gray-400 transition-all duration-200 hover:bg-white/70 hover:text-gray-600 focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none dark:hover:bg-gray-700 dark:text-gray-500 dark:hover:text-gray-300 dark:focus-visible:ring-offset-gray-800 {boardState === 'connected' ? 'cursor-move' : 'cursor-not-allowed opacity-50'}"
        >
          <GripVertical class="h-4 w-4" />
        </div>
        <h3 class="min-w-0 flex-1 text-sm font-bold wrap-break-word text-gray-900 dark:text-white">
          {list.title}
        </h3>
      </div>
      <p class="mt-0.5 pl-8 text-xs text-gray-500 dark:text-gray-400">
        {list.cards.length} card{list.cards.length !== 1 ? 's' : ''}
      </p>
    </div>

    <Button
      type="button"
      variant="ghost"
      size="xs"
      onclick={() => ui.openListModal(swimlaneId, list)}
      disabled={boardState !== 'connected'}
      aria-label="Edit list"
      startIcon={Pencil}
      class="show-on-hover h-7 w-7 min-w-0 rounded-md p-0 text-gray-400 hover:bg-gray-100 hover:text-gray-600 dark:hover:bg-gray-700 dark:hover:text-gray-300"
      title="Edit list"
    >
      <span class="sr-only">Edit list</span>
    </Button>
  </div>

  <ScrollArea.Root class="list-scroll-area relative flex-1 overflow-hidden" type="auto">
    <ScrollArea.Viewport class="h-full w-full rounded-[inherit]">
      <div class="flex h-full min-h-0 flex-col pb-3">
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
          class="flex flex-col gap-2 min-h-[50px] flex-1"
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

        <div class="relative z-10 mt-2 mb-2">
          <Button
            type="button"
            variant="outline"
            startIcon={Plus}
            onclick={() => ui.openCardModal(list.id)}
            disabled={boardState !== 'connected'}
            aria-label="Add card"
            class="add-card-button h-9 w-full justify-center border-dashed border-gray-300 bg-white/70 px-3 text-gray-600 hover:border-gray-400 hover:bg-white dark:border-gray-600 dark:bg-gray-800/40 dark:text-gray-300 dark:hover:bg-gray-700"
          >
            <span class="sr-only">Add card</span>
          </Button>
        </div>
      </div>
    </ScrollArea.Viewport>
    <ScrollArea.Scrollbar
      orientation="vertical"
      class="z-20 flex w-2 touch-none bg-transparent p-px transition-colors duration-200 select-none hover:bg-black/5 dark:hover:bg-white/5"
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
    opacity: 0.5;
    background: var(--color-blue-50) !important;
    border: 1px dashed var(--color-blue-400) !important;
  }

  :global(.dark .card-ghost) {
    background: var(--color-blue-900/20) !important;
    border-color: var(--color-blue-500) !important;
  }

  :global(.card-chosen) {
    cursor: grabbing !important;
  }

  :global(.card-drag) {
    box-shadow:
      0 10px 15px -3px rgba(0, 0, 0, 0.1),
      0 4px 6px -2px rgba(0, 0, 0, 0.05) !important;
    opacity: 0.95 !important;
    transform: rotate(0.5deg);
    background: var(--color-white) !important;
    border: 1px solid var(--color-gray-200) !important;
  }

  :global(.dark .card-drag) {
    background: var(--color-gray-800) !important;
    border-color: var(--color-gray-700) !important;
    box-shadow:
      0 10px 15px -3px rgba(0, 0, 0, 0.3),
      0 4px 6px -2px rgba(0, 0, 0, 0.2) !important;
  }
</style>
