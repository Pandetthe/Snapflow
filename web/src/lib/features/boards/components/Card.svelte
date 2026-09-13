<script lang="ts">
  import type { GetBoardByIdResponse } from '$lib/features/boards/types/boards.api';
  import type { GetRecentMove, IsInFlight } from '$lib/features/boards/composables/boardState.svelte';
  import { dragHandle } from 'svelte-dnd-action';
  import { getContext } from 'svelte';
  import { Button, UserAvatar } from '$lib/ui/components';
  import { CalendarDays, GripVertical, Pencil } from 'lucide-svelte';
  import MovedByIndicator from './MovedByIndicator.svelte';

  let { card, listId }: { card: GetBoardByIdResponse.CardDto; listId: number } = $props();

  const getBoardState = getContext<() => string>('boardState');
  const boardState = $derived(getBoardState());
  const getCanManageCards = getContext<() => boolean>('canManageCards');
  const canManageCards = $derived(getCanManageCards());
  const getRecentMove = getContext<GetRecentMove | undefined>('recentMove');
  const recentMove = $derived(getRecentMove?.('card', card.id));
  const getIsInFlight = getContext<IsInFlight | undefined>('isInFlight');
  const inFlight = $derived(getIsInFlight?.('card', card.id) ?? false);

  interface BoardUI {
    openListModal: (swimlaneId: number, list?: GetBoardByIdResponse.ListDto) => void;
    openCardModal: (listId: number, card?: GetBoardByIdResponse.CardDto) => void;
  }
  const ui = getContext<BoardUI>('ui');
</script>

<!-- --flight-landing-scale must match the card motion in animations/elementFlight.ts -->
<div
  data-id={card.id}
  data-card-id={card.id}
  data-board-item="card"
  class="group/card relative flex flex-col gap-1.5 rounded-lg border border-gray-200/90 bg-white p-2.5 shadow-sm dark:border-gray-700/60 dark:bg-gray-800"
  class:flight-hidden={inFlight}
  class:flight-settle={recentMove?.pop && !inFlight}
  style:--flight-landing-scale="1.015"
>
  <MovedByIndicator move={recentMove} />

  <div class="flex items-start gap-1.5">
    {#if canManageCards}
      <div
        use:dragHandle
        class="card-drag-handle mt-0.5 shrink-0 rounded p-0.5 text-gray-300 opacity-0 transition-[opacity,outline-color] duration-150 hover:bg-gray-100 hover:text-gray-500 focus-visible:outline-none group-hover/card:opacity-100 group-focus-within/card:opacity-100 dark:text-gray-600 dark:hover:bg-gray-700 dark:hover:text-gray-400 {boardState === 'connected' ? 'cursor-grab' : 'cursor-not-allowed'}"
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
        class="h-5 w-5 min-w-0 shrink-0 rounded p-0 text-gray-300 opacity-0 transition-[opacity,outline-color] duration-150 hover:bg-gray-100 hover:text-gray-500 group-hover/card:opacity-100 group-focus-within/card:opacity-100 dark:hover:bg-gray-700 dark:hover:text-gray-400"
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
    <span title={card.createdBy.userName}>
      <UserAvatar src={card.createdBy.avatarUrl} name={card.createdBy.userName} size={18} />
    </span>
    <div class="flex items-center gap-1 text-gray-400">
      <CalendarDays class="h-3 w-3" />
      <span class="text-[10px] tabular-nums">{new Date(card.createdAt).toLocaleDateString()}</span>
    </div>
  </div>
</div>
