<script lang="ts">
  import type { GetBoardByIdResponse } from '$lib/features/boards/types/boards.api';
  import { dragHandle } from 'svelte-dnd-action';
  import { getContext } from 'svelte';
  import { Button } from '$lib/ui/components';
  import { CalendarDays, GripVertical, Pencil } from 'lucide-svelte';

  let { card, listId }: { card: GetBoardByIdResponse.CardDto; listId: number } = $props();

  interface BoardUI {
    openListModal: (swimlaneId: number, list?: GetBoardByIdResponse.ListDto) => void;
    openCardModal: (listId: number, card?: GetBoardByIdResponse.CardDto) => void;
  }
  const ui = getContext<BoardUI>('ui');
</script>

<div
  data-id={card.id}
  class="group flex flex-col gap-2 rounded-lg border border-gray-200 bg-white p-3 shadow-sm transition-[transform,background-color,border-color,box-shadow,opacity] duration-200 hover:-translate-y-0.5 hover:border-brand-500/40 hover:shadow-md focus-within:ring-2 focus-within:ring-brand-500/60 focus-within:ring-offset-2 focus-within:ring-offset-white dark:border-gray-700 dark:bg-gray-800/95 dark:hover:border-brand-500/40 dark:focus-within:ring-offset-gray-900"
>
  <div class="flex items-start justify-between gap-2">
    <div class="flex min-w-0 flex-1 items-start gap-2">
      <div class="show-on-hover flex items-center gap-1">
        <div
          use:dragHandle
          class="card-drag-handle cursor-move rounded-md p-1 text-gray-400 transition-all duration-200 hover:bg-gray-100 hover:text-gray-600 focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none dark:text-gray-500 dark:hover:bg-gray-700 dark:hover:text-gray-300 dark:focus-visible:ring-offset-gray-800"
        >
          <GripVertical class="h-3.5 w-3.5" />
        </div>
      </div>
      <h4
        class="mt-0.5 min-w-0 flex-1 text-sm font-medium wrap-break-word text-gray-900 dark:text-white"
      >
        {card.title}
      </h4>
    </div>

    <div class="show-on-hover">
      <Button
        type="button"
        variant="ghost"
        size="xs"
        onclick={() => ui.openCardModal(listId, card)}
        startIcon={Pencil}
        class="h-6 w-6 min-w-0 rounded-md p-0 text-gray-400 hover:bg-gray-100 hover:text-gray-600 dark:hover:bg-gray-700 dark:hover:text-gray-300"
        title="Edit card"
      >
        <span class="sr-only">Edit card</span>
      </Button>
    </div>
  </div>

  {#if card.description}
    <p class="line-clamp-2 text-xs text-gray-500 dark:text-gray-400">
      {card.description}
    </p>
  {/if}

  <div class="mt-1 flex items-center justify-between">
    <div class="flex items-center gap-1.5">
      <div
        class="flex h-5 w-5 items-center justify-center rounded-full bg-brand-100 text-[10px] font-bold text-brand-700 dark:bg-brand-500/25 dark:text-brand-300"
      >
        {card.createdBy.userName.charAt(0).toUpperCase()}
      </div>
    </div>
    <div class="flex items-center gap-2 text-gray-400">
      <CalendarDays class="h-3.5 w-3.5" />
      <span class="text-[10px]">{new Date(card.createdAt).toLocaleDateString()}</span>
    </div>
  </div>
</div>
