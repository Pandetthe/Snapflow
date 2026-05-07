<script lang="ts">
  import { flip } from 'svelte/animate';
  import { dragHandleZone, dragHandle, TRIGGERS } from 'svelte-dnd-action';
  import type { DndEvent } from 'svelte-dnd-action';
  import { getContext } from 'svelte';
  import type { BoardsHub } from '$lib/features/boards/hub/boards.hub';
  import Card from './Card.svelte';
  import { ScrollArea } from 'bits-ui';
  import { errorStore } from '$lib/ui/stores/error.svelte';
  import { triggerHaptic } from '$lib/ui/utils';
  import { Button } from '$lib/ui/components';
  import { GripVertical, MoreHorizontal, Plus } from 'lucide-svelte';
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
      
      if (index === -1) return;

      const nextItem = list.cards[index + 1];
      const beforeId = nextItem ? nextItem.id : null;

      let res = await hub?.moveCard({ id, listId: list.id, beforeId });
      if (res && res.ok) {
        const movedItem = list.cards.find((c) => c.id === id);
        if (movedItem) movedItem.rank = res.value.rank;
        list.cards.sort((a, b) => a.rank.localeCompare(b.rank));
        list.cards = [...list.cards];
      } 
      else if (boardState === 'connected') {
        errorStore.addError('Web.MoveCardFailed', 'Failed to move card');
        list.cards.sort((a, b) => a.rank.localeCompare(b.rank));
        list.cards = [...list.cards];
      }
    }
  }
</script>

<div
  role="group"
  style:width={list.width ? `${list.width}px` : '280px'}
  class="flex h-fit max-h-full min-h-0 shrink-0 flex-col overflow-hidden rounded-2xl bg-gray-100/60 pb-2 transition-all duration-200 dark:bg-gray-800/40"
>
  <!-- Minimalistyczny nagłówek -->
  <div class="group flex items-center justify-between gap-2 px-3 py-3">
    <div class="flex flex-1 items-center gap-1.5 min-w-0">
      <div
        use:dragHandle
        class="list-drag-handle rounded text-gray-400 opacity-0 transition-opacity duration-200 group-hover:opacity-100 hover:bg-gray-200 hover:text-gray-700 dark:hover:bg-gray-700 dark:hover:text-gray-300 {boardState === 'connected' ? 'cursor-grab active:cursor-grabbing' : 'cursor-not-allowed opacity-50'}"
      >
        <GripVertical class="h-4 w-4" />
      </div>
      <h3 class="min-w-0 flex-1 truncate text-sm font-semibold tracking-tight text-gray-900 dark:text-gray-100">
        {list.title}
      </h3>
      <span class="ml-1 rounded-full bg-gray-200/70 px-2 py-0.5 text-[10px] font-medium text-gray-600 dark:bg-gray-700/50 dark:text-gray-400">
        {list.cards.length}
      </span>
    </div>

    <Button
      type="button"
      variant="ghost"
      size="xs"
      onclick={() => ui.openListModal(swimlaneId, list)}
      disabled={boardState !== 'connected'}
      class="h-6 w-6 rounded-md p-0 text-gray-400 opacity-0 transition-opacity duration-200 group-hover:opacity-100 hover:bg-gray-200 hover:text-gray-700 dark:hover:bg-gray-700 dark:hover:text-gray-200"
    >
      <MoreHorizontal class="h-4 w-4" />
    </Button>
  </div>

  <!-- Ukryty Scrollbar dzięki bits-ui (brak komponentu ScrollArea.Scrollbar) -->
  <ScrollArea.Root class="relative flex-1 overflow-hidden px-2" type="auto">
    <ScrollArea.Viewport class="h-full w-full rounded-[inherit] hide-scrollbar">
      <div class="flex h-full min-h-0 flex-col">
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
          class="flex flex-col gap-2.5 min-h-[50px] flex-1 pb-2 p-1"
        >
          {#each list.cards as card (card.id)}
            <div
              animate:flip={{ duration: 150 }}
              class="relative z-20 rounded-xl outline-none"
            >
              <Card {card} listId={list.id} />
            </div>
          {/each}
        </section>

        <div class="mt-1 pb-2">
          <Button
            type="button"
            variant="ghost"
            startIcon={Plus}
            onclick={() => ui.openCardModal(list.id)}
            disabled={boardState !== 'connected'}
            class="h-8 w-full justify-start rounded-lg px-2 text-sm font-medium text-gray-500 hover:bg-gray-200/60 hover:text-gray-900 dark:text-gray-400 dark:hover:bg-gray-700/50 dark:hover:text-gray-100 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 dark:focus-visible:ring-offset-gray-900"
          >
            Add a card...
          </Button>
        </div>
      </div>
    </ScrollArea.Viewport>
  </ScrollArea.Root>
</div>

<style>
  :global(.card-ghost) {
    opacity: 0.4;
    background: var(--color-brand-50) !important;
    border: 1px dashed var(--color-brand-400) !important;
    border-radius: 0.75rem !important;
  }
  :global(.dark .card-ghost) {
    background: var(--color-brand-900/20) !important;
    border-color: var(--color-brand-500/50) !important;
  }
  :global(.card-chosen) {
    cursor: grabbing !important;
  }
  :global(.card-drag) {
    box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 8px 10px -6px rgba(0, 0, 0, 0.1) !important;
    opacity: 1 !important;
    transform: rotate(2deg) scale(1.02);
    border-radius: 0.75rem !important;
  }
  :global(.dark .card-drag) {
    box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.4) !important;
  }
</style>