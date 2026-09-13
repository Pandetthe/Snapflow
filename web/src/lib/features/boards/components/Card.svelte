<script lang="ts">
  import type { GetBoardByIdResponse } from '$lib/features/boards/types/boards.api';
  import { dragHandle } from 'svelte-dnd-action';
  import { getContext } from 'svelte';
  import { Button } from '$lib/ui/components';
  import { CalendarDays, GripVertical, Pencil } from 'lucide-svelte';

  let { card, listId }: { card: GetBoardByIdResponse.CardDto; listId: number } = $props();

  const getBoardState = getContext<() => string>('boardState');
  const boardState = $derived(getBoardState());
  const getCanManageCards = getContext<() => boolean>('canManageCards');
  const canManageCards = $derived(getCanManageCards());

  interface BoardUI {
    openListModal: (swimlaneId: number, list?: GetBoardByIdResponse.ListDto) => void;
    openCardModal: (listId: number, card?: GetBoardByIdResponse.CardDto) => void;
  }
  const ui = getContext<BoardUI>('ui');
</script>

<div
  data-id={card.id}
  class="group/card flex flex-col gap-1.5 rounded-lg border border-gray-200/90 bg-white p-2.5 shadow-sm transition-[transform,border-color,box-shadow] duration-150 hover:-translate-y-px hover:border-brand-300/60 hover:shadow-md focus-within:ring-2 focus-within:ring-brand-500/50 focus-within:ring-offset-1 focus-within:ring-offset-white dark:border-gray-700/60 dark:bg-gray-800 dark:hover:border-brand-500/40 dark:focus-within:ring-offset-gray-900"
>
  <div class="flex items-start gap-1.5">
    {#if canManageCards}
      <div
        use:dragHandle
        class="card-drag-handle mt-0.5 shrink-0 rounded p-0.5 text-gray-300 opacity-0 transition-opacity duration-150 hover:bg-gray-100 hover:text-gray-500 focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:outline-none group-hover/card:opacity-100 dark:text-gray-600 dark:hover:bg-gray-700 dark:hover:text-gray-400 {boardState === 'connected' ? 'cursor-grab' : 'cursor-not-allowed'}"
      >
        <GripVertical class="h-3 w-3" />
      </div>
    {/if}

    <h4 class="min-w-0 flex-1 text-xs font-medium leading-relaxed wrap-break-word text-gray-800 dark:text-gray-100">
      {card.title}
    </h4>

    {#if canManageCards}
      <Button
        type="button"
        variant="ghost"
        size="xs"
        onclick={() => ui.openCardModal(listId, card)}
        disabled={boardState !== 'connected'}
        startIcon={Pencil}
        class="h-5 w-5 min-w-0 shrink-0 rounded p-0 text-gray-300 opacity-0 transition-opacity duration-150 hover:bg-gray-100 hover:text-gray-500 group-hover/card:opacity-100 dark:hover:bg-gray-700 dark:hover:text-gray-400"
        title="Edit card"
      >
        <span class="sr-only">Edit card</span>
      </Button>
    {/if}
  </div>

  {#if card.description}
    <p class="line-clamp-2 pl-5 text-[11px] leading-relaxed text-gray-500 dark:text-gray-400">
      {card.description}
    </p>
  {/if}

  <div class="flex items-center justify-between pl-5">
    <div
      class="flex h-4.5 w-4.5 items-center justify-center rounded-full bg-brand-100 text-[9px] font-bold text-brand-600 dark:bg-brand-500/20 dark:text-brand-400"
      title={card.createdBy.userName}
    >
      {card.createdBy.userName.charAt(0).toUpperCase()}
    </div>
    <div class="flex items-center gap-1 text-gray-400">
      <CalendarDays class="h-3 w-3" />
      <span class="text-[10px] tabular-nums">{new Date(card.createdAt).toLocaleDateString()}</span>
    </div>
  </div>
</div>
