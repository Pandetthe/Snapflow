<script lang="ts">
  import { flip } from 'svelte/animate';
  import { dragHandle, dragHandleZone, SHADOW_ITEM_MARKER_PROPERTY_NAME, TRIGGERS } from 'svelte-dnd-action';
  import type { DndEvent } from 'svelte-dnd-action';
  import List from './List.svelte';
  import { getContext } from 'svelte';
  import { BoardsHub } from '$lib/features/boards/hub/boards.hub';
  import type { GetBoardByIdResponse } from '$lib/features/boards/types/boards.api';
  import { errorStore } from '$lib/ui/stores/error';
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

  interface BoardUI {
    openSwimlaneModal: (swimlane?: GetBoardByIdResponse.SwimlaneDto) => void;
    openListModal: (swimlaneId: number, list?: GetBoardByIdResponse.ListDto) => void;
  }
  const ui = getContext<BoardUI>('ui');

  function handleListConsider(e: CustomEvent<DndEvent<GetBoardByIdResponse.ListDto>>) {
    swimlane.lists = e.detail.items;
  }

  async function handleListFinalize(e: CustomEvent<DndEvent<GetBoardByIdResponse.ListDto>>) {
    console.log('[ListFinalize] EVENT WYWOŁANY!', e.detail.info);
    swimlane.lists = e.detail.items;
    const { info } = e.detail;
    
    if (info.trigger === TRIGGERS.DROPPED_INTO_ZONE || info.trigger === TRIGGERS.DROPPED_INTO_ANOTHER) {
      triggerHaptic('success');
      const id = Number(info.id);

      const index = swimlane.lists.findIndex((l) => l.id === id);
      console.log(`[ListFinalize] Szukam index dla id ${id} -> Wynik: ${index}`);
      if (index === -1) {
        console.log('[ListFinalize] PRZERWANO: index === -1 (nie znaleziono listy na tej pływalni)');
        return;
      }

      const nextItem = swimlane.lists[index + 1];
      const beforeId = nextItem ? nextItem.id : null;
      const oldRank = swimlane.lists[index].rank;

      console.log(`[ListFinalize] Wysyłam do API -> id: ${id}, swimlaneId: ${swimlane.id}, beforeId: ${beforeId}`);
      let res = await hub?.moveList({ id, swimlaneId: swimlane.id, beforeId });
      console.log(`[ListFinalize] Wynik API:`, res);
      
      if (res && res.ok) {
        const movedItem = swimlane.lists.find((l) => l.id === id);
        if (movedItem && res.value?.rank) {
          console.log(`[MoveListRank SUCCESS] Lista: "${movedItem.title}" | RANK PRZED: ${oldRank} | RANK PO: ${res.value.rank}`);
          movedItem.rank = res.value.rank;
        }
        swimlane.lists.sort((a, b) => a.rank.localeCompare(b.rank));
        swimlane.lists = [...swimlane.lists];
      } else {
        console.log(`[MoveListRank ERROR] Failed to move list`);
        errorStore.addError('Web.MoveListFailed', 'Failed to move list');
        swimlane.lists.sort((a, b) => a.rank.localeCompare(b.rank));
        swimlane.lists = [...swimlane.lists];
      }
    } else {
       console.log('[ListFinalize] Inny trigger:', info.trigger);
    }
  }
</script>

<div
  data-id={swimlane.id}
  role="group"
  style:height={swimlane.height ? `${swimlane.height}px` : undefined}
  class="flex flex-col transition-all duration-200 focus-within:shadow-sm {swimlane.height
    ? ''
    : 'flex-1'}"
>
  <div class="group flex items-start gap-2 rounded-none bg-gray-50/95 px-3 py-2 dark:bg-gray-700/45">
    <div class="flex flex-1 items-start gap-2">
      <div
        use:dragHandle
        class="show-on-hover touch-none rounded-md p-1 text-gray-400 transition-all duration-200 hover:bg-white/70 hover:text-gray-600 focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none dark:hover:bg-gray-700 dark:text-gray-500 dark:hover:text-gray-300 dark:focus-visible:ring-offset-gray-800 {boardState === 'connected' ? 'cursor-move' : 'cursor-not-allowed opacity-50'}"
      >
        <GripVertical class="h-5 w-5" />
      </div>

      <div class="min-w-0 flex-1">
        <div class="flex items-center gap-2">
          <h2
            class="mb-1 min-w-0 flex-1 text-lg font-semibold wrap-break-word text-gray-900 dark:text-white"
          >
            {swimlane.title}
          </h2>
          <Button
            type="button"
            variant="ghost"
            size="xs"
            disabled={boardState !== 'connected'}
            onclick={() => ui.openSwimlaneModal(swimlane)}
            startIcon={Pencil}
            class="show-on-hover h-7 w-7 min-w-0 rounded-md p-0 text-gray-400 hover:bg-white hover:text-gray-600 dark:hover:bg-gray-700 dark:hover:text-gray-300"
            title="Edit swimlane"
          >
            <span class="sr-only">Edit swimlane</span>
          </Button>
        </div>
        <p class="text-xs text-gray-500 dark:text-gray-400">
          {swimlane.lists.length} list{swimlane.lists.length !== 1 ? 's' : ''}
        </p>
      </div>
    </div>
  </div>

  <ScrollArea.Root class="swimlane-scroll-area relative mt-3 flex-1 overflow-hidden" type="auto">
    <ScrollArea.Viewport class="h-full w-full rounded-[inherit]">
      <div class="flex h-full px-3 pb-0 sm:px-4">
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
          class="flex h-full items-stretch gap-4"
        >
          {#each swimlane.lists as list, index (list.id)}
            <div
              class="relative z-20 flex min-h-0 self-stretch rounded-lg transition-shadow duration-200 focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none dark:focus-visible:ring-offset-gray-900"
              animate:flip={{ duration: 150 }}
            >
              <List bind:list={swimlane.lists[index]} swimlaneId={swimlane.id} />
              {#if index < swimlane.lists.length - 1}
                <span
                  aria-hidden="true"
                  class="pointer-events-none absolute -inset-y-0.5 -right-2 w-px bg-gray-300/90 dark:bg-gray-600/90"
                ></span>
              {/if}
            </div>
          {/each}
        </section>

        <div class="relative z-10 mt-0 ml-4 self-start">
          <Button
            type="button"
            variant="outline"
            startIcon={Plus}
            disabled={boardState !== 'connected'}
            onclick={() => ui.openListModal(swimlane.id)}
            aria-label="Add list"
              class="add-list-button h-10 w-12 justify-center border-dashed border-gray-300 bg-white/70 px-0 text-gray-600 hover:border-gray-400 hover:bg-white dark:border-gray-600 dark:bg-gray-800/40 dark:text-gray-300 dark:hover:bg-gray-700"
          >
            <span class="sr-only">Add list</span>
          </Button>
        </div>
      </div>
    </ScrollArea.Viewport>
    <ScrollArea.Scrollbar
      orientation="horizontal"
      class="flex h-2.5 touch-none flex-col bg-transparent p-0.5 transition-colors duration-200 select-none hover:bg-black/5 dark:hover:bg-white/5"
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

  :global(.list-ghost) {
    opacity: 0.5;
    background: var(--color-gray-200) !important;
    border: 2px dashed var(--color-gray-400) !important;
  }

  :global(.dark .list-ghost) {
    background: var(--color-gray-700) !important;
    border-color: var(--color-gray-500) !important;
  }

  :global(.list-chosen) {
    cursor: grabbing !important;
  }

  :global(.list-drag) {
    box-shadow:
      0 15px 20px -5px rgba(0, 0, 0, 0.15),
      0 5px 5px -5px rgba(0, 0, 0, 0.1) !important;
    opacity: 0.95 !important;
    transform: rotate(0.5deg);
    background: var(--color-white) !important;
    border: 1px solid var(--color-gray-200) !important;
  }

  :global(.dark .list-drag) {
    background: var(--color-gray-800) !important;
    border-color: var(--color-gray-700) !important;
    box-shadow:
      0 15px 20px -5px rgba(0, 0, 0, 0.3),
      0 5px 5px -5px rgba(0, 0, 0, 0.2) !important;
  }
</style>
