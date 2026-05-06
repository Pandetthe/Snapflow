<script lang="ts">
  import type { GetBoardByIdResponse } from '$lib/features/boards/types/boards.api';
  import { dragHandle } from 'svelte-dnd-action';
  import { getContext } from 'svelte';
  import { CalendarDays, GripVertical, MessageSquare } from 'lucide-svelte';

  let { card, listId }: { card: GetBoardByIdResponse.CardDto; listId: number } = $props();

  const getBoardState = getContext<() => string>('boardState');
  const boardState = $derived(getBoardState());

  interface BoardUI {
    openListModal: (swimlaneId: number, list?: GetBoardByIdResponse.ListDto) => void;
    openCardModal: (listId: number, card?: GetBoardByIdResponse.CardDto) => void;
  }
  const ui = getContext<BoardUI>('ui');

  function handleOpenCard() {
    if (boardState === 'connected') {
      ui.openCardModal(listId, card);
    }
  }
</script>

<div
  data-id={card.id}
  role="button"
  tabindex="0"
  onclick={handleOpenCard}
  onkeydown={(e) => e.key === 'Enter' && handleOpenCard()}
  class="group relative flex flex-col gap-3 rounded-xl border border-gray-200/80 bg-white p-3.5 shadow-sm transition-all duration-200 hover:border-brand-300 hover:shadow-md focus:outline-none focus:ring-2 focus:ring-brand-500/60 focus:ring-offset-2 dark:border-gray-700/80 dark:bg-gray-800 dark:hover:border-brand-600/50 cursor-pointer dark:focus:ring-offset-gray-900"
>
  <!-- Górna sekcja: Tytuł, opis i uchwyt -->
  <div class="flex items-start justify-between gap-3">
    <div class="flex min-w-0 flex-1 flex-col">
      <h4 class="min-w-0 text-sm font-semibold leading-snug text-gray-800 wrap-break-word dark:text-gray-100">
        {card.title}
      </h4>

      {#if card.description}
        <p class="mt-1.5 text-xs leading-relaxed text-gray-500 line-clamp-2 dark:text-gray-400">
          {card.description}
        </p>
      {/if}
    </div>

    <button type="button" class="ml-auto shrink-0 opacity-0 transition-opacity duration-200 group-hover:opacity-100" onclick={(e) => e.stopPropagation()}>
      <div
        use:dragHandle
        class="flex items-center justify-center rounded-md p-1 text-gray-400 transition-colors hover:bg-gray-100 hover:text-gray-600 dark:text-gray-500 dark:hover:bg-gray-700 dark:hover:text-gray-300 {boardState === 'connected' ? 'cursor-grab active:cursor-grabbing' : 'cursor-not-allowed opacity-50'}"
        aria-label="Drag card"
      >
        <GripVertical class="h-4 w-4" />
      </div>
    </button>
  </div>

  <!-- Stopka: Metadane karty -->
  <div class="mt-auto flex items-center justify-between pt-1">
    <!-- Lewa strona: Awatar + Komentarze -->
    <div class="flex items-center gap-3">
      <div
        title={card.createdBy.userName}
        class="flex h-6 w-6 items-center justify-center rounded-full bg-gradient-to-br from-brand-100 to-brand-200 ring-2 ring-white text-[10px] font-bold text-brand-700 shadow-sm dark:from-brand-500/30 dark:to-brand-500/10 dark:ring-gray-800 dark:text-brand-300"
      >
        {card.createdBy.userName.charAt(0).toUpperCase()}
      </div>
      
      <!-- Licznik komentarzy -->
      {#if card.comments && card.comments.length > 0}
        <div class="flex items-center gap-1 text-gray-400 transition-colors group-hover:text-brand-500 dark:text-gray-500 dark:group-hover:text-brand-400" title="{card.comments.length} comments">
          <MessageSquare class="h-3.5 w-3.5" />
          <span class="text-[10px] font-semibold">{card.comments.length}</span>
        </div>
      {/if}
    </div>

    <!-- Prawa strona: Data -->
    <div class="flex items-center gap-1.5 rounded-md bg-gray-50 px-2 py-1 text-gray-400 dark:bg-gray-800/50 dark:text-gray-500" title="Created on {new Date(card.createdAt).toLocaleDateString()}">
      <CalendarDays class="h-3 w-3" />
      <span class="text-[10px] font-medium tracking-wide">
        {new Date(card.createdAt).toLocaleDateString(undefined, { month: 'short', day: 'numeric' })}
      </span>
    </div>
  </div>
</div>