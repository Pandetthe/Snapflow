<script lang="ts">
  import type { GetBoardByIdResponse } from '$lib/features/boards/types/boards.api';
  import { dragHandle } from 'svelte-dnd-action';
  import { dragHandles } from '$lib/features/boards/stores/dragHandles';
  import { getBoardContext, getBoardUI } from '$lib/features/boards/context/board.context';
  import { renderDescriptionHtml } from '$lib/features/boards/markdown/render';
  import { summarizeDescription } from '$lib/features/boards/markdown/summary';
  import '$lib/features/boards/styles/markdown.css';
  import { UserAvatar } from '$lib/ui/components';
  import { CalendarDays, GripVertical, ListChecks } from 'lucide-svelte';
  import MovedByIndicator from './MovedByIndicator.svelte';
  import TagChip from './TagChip.svelte';

  let { card, listId }: { card: GetBoardByIdResponse.CardDto; listId: number } = $props();

  const boardCtx = getBoardContext();
  const boardState = $derived(boardCtx.connectionState);
  const canManageCards = $derived(boardCtx.canManageCards);
  const recentMove = $derived(boardCtx.getRecentMove('card', card.id));
  const inFlight = $derived(boardCtx.isInFlight('card', card.id));

  // The card holds ids only; the board's definitions give each one its title and colour.
  const tags = $derived(
    card.tagIds
      .map((id) => boardCtx.board.tags.find((t) => t.id === id))
      .filter((t) => t !== undefined)
  );

  // The description shows formatted but small (markdown.css), with the checklist as a count.
  const descriptionHtml = $derived(renderDescriptionHtml(card.description));
  const summary = $derived(summarizeDescription(card.description));

  const ui = getBoardUI();

  const surfaceDrag = $derived(canManageCards && $dragHandles === 'hidden');
</script>

<!-- --flight-landing-scale must match the card motion in animations/elementFlight.ts -->
<div
  data-id={card.id}
  data-card-id={card.id}
  data-board-item="card"
  class="group/card relative flex flex-col gap-1.5 rounded-lg border border-gray-200/90 bg-white p-2 shadow-sm dark:border-gray-700/60 dark:bg-gray-800"
  class:flight-hidden={inFlight}
  class:flight-settle={recentMove?.pop && !inFlight}
  style:--flight-landing-scale="1.015"
>
  <MovedByIndicator move={recentMove} />

  <div
    class="absolute inset-0 rounded-lg"
    aria-hidden="true"
    onclick={() => ui.openCardModal(listId, card)}
  ></div>

  <div class="board-item-bar flex items-start gap-1.5">
    {#if canManageCards && !surfaceDrag}
      <!-- relative z-10 keeps the handle above the click area, which covers the card -->
      <div
        use:dragHandle
        class="card-drag-handle board-drag-handle board-control relative z-10 touch-none focus-visible:outline-none {boardState ===
        'connected'
          ? 'cursor-grab'
          : 'cursor-not-allowed opacity-40'}"
        aria-label="Drag card"
      >
        <GripVertical class="h-3.5 w-3.5" />
      </div>
    {/if}

    <!-- py-0.5 centres the first line on the 24px drag handle -->
    <h4
      class="min-w-0 flex-1 py-0.5 text-xs leading-relaxed font-medium wrap-break-word text-gray-800 dark:text-gray-100"
    >
      <button
        type="button"
        onclick={() => ui.openCardModal(listId, card)}
        class="cursor-pointer text-left focus-visible:outline-none"
      >
        {card.title}
      </button>
    </h4>
  </div>

  {#if descriptionHtml}
    <div class="card-description markdown-content markdown-view">
      <!-- eslint-disable-next-line svelte/no-at-html-tags -- Tiptap's static renderer escapes text and emits only schema nodes (markdown/render.ts) -->
      {@html descriptionHtml}
    </div>
  {/if}

  {#if tags.length > 0}
    <div class="flex flex-wrap gap-1">
      {#each tags as tag (tag.id)}
        <TagChip {tag} size="xs" />
      {/each}
    </div>
  {/if}

  <div class="flex items-center justify-between">
    {#if card.createdBy}
      <span title={card.createdBy.userName}>
        <UserAvatar src={card.createdBy.avatarUrl} name={card.createdBy.userName} size={18} />
      </span>
    {:else}
      <span></span>
    {/if}
    <div class="flex items-center gap-2 text-gray-400">
      {#if summary.tasksTotal > 0}
        <span
          class="flex items-center gap-1 text-[10px] tabular-nums {summary.tasksDone ===
          summary.tasksTotal
            ? 'text-success-600 dark:text-success-500'
            : ''}"
          title="Checklist items done"
        >
          <ListChecks class="h-3 w-3" />
          {summary.tasksDone}/{summary.tasksTotal}
        </span>
      {/if}
      <span class="flex items-center gap-1 text-[10px] tabular-nums">
        <CalendarDays class="h-3 w-3" />
        {new Date(card.createdAt).toLocaleDateString()}
      </span>
    </div>
  </div>
</div>
