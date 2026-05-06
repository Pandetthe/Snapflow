<script lang="ts">
  import { flip } from 'svelte/animate';
  import { dragHandle, dragHandleZone, TRIGGERS } from 'svelte-dnd-action';
  import type { DndEvent } from 'svelte-dnd-action';
  import List from './List.svelte';
  import { getContext } from 'svelte';
  import { BoardsHub } from '$lib/features/boards/hub/boards.hub';
  import type { GetBoardByIdResponse } from '$lib/features/boards/types/boards.api';
  import { errorStore } from '$lib/ui/stores/error.svelte';
  import { Button } from '$lib/ui/components';
  import { ScrollArea } from 'bits-ui';
  import { triggerHaptic } from '$lib/ui/utils';
  import { GripVertical, Settings2, Plus } from 'lucide-svelte';

  let { swimlane = $bindable() }: { swimlane: GetBoardByIdResponse.SwimlaneDto } = $props();

  const getHub = getContext<() => BoardsHub | null>('hub');
  const hub = $derived(getHub());
  const getBoardState = getContext<() => string>('boardState');
  const boardState = $derived(getBoardState());

  interface BoardUI {
    openSwimlaneModal: (swimlane?: GetBoardByIdResponse.SwimlaneDto) => void;
    openListModal: (swimlaneId: number, list?: GetBoardByIdResponse.ListDto) => void;
  }
  const ui = getContext<BoardUI>('ui');

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
      if (index === -1) return;

      const nextItem = swimlane.lists[index + 1];
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
  data-id={swimlane.id}
  role="group"
  style:height={swimlane.height ? `${swimlane.height}px` : undefined}
  class="flex flex-col mb-6 {swimlane.height ? '' : 'flex-1'}"
>
  <!-- Czysty nagłówek toru -->
  <div class="group flex items-center gap-3 py-2 px-6">
    <div
      use:dragHandle
      class="rounded p-1 text-gray-300 opacity-0 transition-opacity duration-200 group-hover:opacity-100 hover:bg-gray-100 hover:text-gray-600 dark:hover:bg-gray-800 dark:text-gray-600 dark:hover:text-gray-300 {boardState === 'connected' ? 'cursor-grab active:cursor-grabbing' : 'cursor-not-allowed opacity-50'}"
    >
      <GripVertical class="h-5 w-5" />
    </div>

    <h2 class="text-base font-bold tracking-tight text-gray-900 dark:text-gray-100">
      {swimlane.title}
    </h2>
    <span class="rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-semibold text-gray-500 dark:bg-gray-800 dark:text-gray-400">
      {swimlane.lists.length}
    </span>

    <Button
      type="button"
      variant="ghost"
      size="xs"
      disabled={boardState !== 'connected'}
      onclick={() => ui.openSwimlaneModal(swimlane)}
      class="ml-2 h-7 w-7 rounded-lg p-0 text-gray-400 opacity-0 transition-opacity duration-200 group-hover:opacity-100 hover:bg-gray-100 hover:text-gray-700 dark:hover:bg-gray-800 dark:hover:text-gray-200"
    >
      <Settings2 class="h-4 w-4" />
    </Button>
  </div>

  <!-- Brak ScrollArea.Scrollbar ukrywa suwak! -->
  <ScrollArea.Root class="relative flex-1 overflow-hidden" type="auto">
    <ScrollArea.Viewport class="h-full w-full hide-scrollbar">
      <div class="flex h-full px-6 pb-2 pt-1">
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
          class="flex h-full items-start gap-4"
        >
          {#each swimlane.lists as list, index (list.id)}
            <div
              class="relative z-20 flex min-h-0 self-stretch outline-none"
              animate:flip={{ duration: 150 }}
            >
              <List bind:list={swimlane.lists[index]} swimlaneId={swimlane.id} />
            </div>
          {/each}
        </section>

        <!-- Elegancki przycisk "Add List" -->
        <div class="ml-4 shrink-0 self-start">
          <button
            type="button"
            disabled={boardState !== 'connected'}
            onclick={() => ui.openListModal(swimlane.id)}
            class="flex h-[120px] w-[280px] items-center justify-center gap-2 rounded-2xl border-2 border-dashed border-gray-200/80 bg-gray-50/50 text-sm font-medium text-gray-500 transition-colors hover:border-gray-300 hover:bg-gray-100 hover:text-gray-700 disabled:cursor-not-allowed disabled:opacity-50 dark:border-gray-700/60 dark:bg-gray-800/20 dark:text-gray-400 dark:hover:border-gray-600 dark:hover:bg-gray-800 dark:hover:text-gray-200"
          >
            <Plus class="h-5 w-5" />
            Add new list
          </button>
        </div>
      </div>
    </ScrollArea.Viewport>
  </ScrollArea.Root>
</div>

<style>
  :global(.list-ghost) {
    opacity: 0.3;
    background: var(--color-gray-100) !important;
    border: 2px dashed var(--color-gray-300) !important;
    border-radius: 1rem !important;
  }
  :global(.dark .list-ghost) {
    background: var(--color-gray-800) !important;
    border-color: var(--color-gray-600) !important;
  }
  :global(.list-chosen) {
    cursor: grabbing !important;
  }
  :global(.list-drag) {
    box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25) !important;
    opacity: 1 !important;
    transform: rotate(1deg) scale(1.01);
    border-radius: 1rem !important;
  }
</style>