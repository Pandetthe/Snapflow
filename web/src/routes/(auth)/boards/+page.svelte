<script lang="ts">
  import { onMount, onDestroy, setContext } from 'svelte';
  import { invalidate } from '$app/navigation';
  import Board from '$lib/features/boards/components/Board.svelte';
  import BoardModal from '$lib/features/boards/components/BoardModal.svelte';
  import { BoardsService } from '$lib/features/boards/api/boards.api';
  import { apiClient } from '$lib/core/api.client';
  import { errorStore } from '$lib/ui/stores/error';
  import { recentBoards } from '$lib/features/boards/stores/recent';
  import logger from '$lib/logger';
  import { Button } from 'bits-ui';
  import type { GetBoardsResponse } from '$lib/features/boards/types/boards.api';

  let { data } = $props();
  let intervalId: NodeJS.Timeout;
  let refreshTime = $derived(new Date(data.refreshTime));

  let boardModalOpen = $state(false);
  let editingBoard = $state<GetBoardsResponse.BoardDto | undefined>(undefined);
  let boardsService: BoardsService;

  setContext('boards-ui', {
    openBoardModal: (board?: GetBoardsResponse.BoardDto) => {
      editingBoard = board;
      boardModalOpen = true;
    }
  });

  function formatTime(date: Date) {
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' });
  }

  async function handleConfirmBoard(title: string, description: string) {
    if (editingBoard) {
      const res = await boardsService.updateBoard({
        id: editingBoard.id,
        title,
        description
      });
      logger.debug({ res }, 'Board update result');
      if (res.ok) {
        invalidate('/api/boards');
      } else {
        errorStore.addError('Web.UpdateBoardFailed', 'Failed to update board');
      }
    } else {
      const res = await boardsService.createBoard({ title, description });
      if (res.ok) {
        invalidate('/api/boards');
      } else {
        errorStore.addError('Web.CreateBoardFailed', 'Failed to create board');
      }
    }
  }

  async function handleDeleteBoard(id: number) {
    const res = await boardsService.deleteBoard(id);
    if (res.ok) {
      invalidate('/api/boards');
    } else {
      errorStore.addError('Web.DeleteBoardFailed', 'Failed to delete board');
    }
  }

  onMount(() => {
    boardsService = new BoardsService(apiClient);
    intervalId = setInterval(() => invalidate('/api/boards'), 10000);
  });

  onDestroy(() => {
    clearInterval(intervalId);
  });
</script>

<svelte:head>
  <title>Snapflow | Boards</title>
</svelte:head>

<div class="relative mt-10 min-h-screen p-4 sm:p-6 md:mt-0 lg:p-8">
  <div class="mb-4 flex items-center justify-between sm:mb-6">
    <h1 class="text-2xl font-bold text-gray-900 sm:text-3xl dark:text-white">
      Hi {data.user.userName}, these are your boards:
    </h1>
    <Button.Root
      onclick={() => {
        editingBoard = undefined;
        boardModalOpen = true;
      }}
      class="inline-flex h-10 items-center justify-center rounded-md bg-gray-900 px-4 py-2 text-sm font-medium text-gray-50 shadow transition-colors hover:bg-gray-900/90 focus-visible:ring-1 focus-visible:ring-gray-950 focus-visible:outline-none disabled:pointer-events-none disabled:opacity-50 dark:bg-gray-50 dark:text-gray-900 dark:hover:bg-gray-50/90"
    >
      <svg
        xmlns="http://www.w3.org/2000/svg"
        width="20"
        height="20"
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        stroke-width="2"
        stroke-linecap="round"
        stroke-linejoin="round"
        class="mr-2"
      >
        <line x1="12" y1="5" x2="12" y2="19"></line>
        <line x1="5" y1="12" x2="19" y2="12"></line>
      </svg>
      New Board
    </Button.Root>
  </div>

  {#if $recentBoards.length > 0}
    <div class="mb-8">
      <h2 class="mb-4 text-lg font-semibold text-gray-700 dark:text-gray-300">Recently Visited</h2>
      <div class="flex flex-wrap gap-3">
        {#each $recentBoards as board}
          <a
            href="/boards/{board.id}"
            class="group relative flex min-w-[120px] items-center gap-3 rounded-xl border border-gray-200 bg-white p-3 shadow-sm transition-all duration-300 hover:-translate-y-1 hover:border-blue-500/50 hover:shadow-md hover:shadow-blue-500/10 dark:border-gray-700 dark:bg-gray-800 dark:hover:border-blue-400/50 dark:hover:shadow-blue-400/10"
          >
            <div
              class="flex h-8 w-8 items-center justify-center rounded-lg bg-blue-50 text-blue-600 transition-colors group-hover:bg-blue-600 group-hover:text-white dark:bg-blue-900/30 dark:text-blue-400"
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                class="h-4 w-4"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
                />
              </svg>
            </div>
            <span
              class="text-sm font-medium text-gray-700 transition-colors group-hover:text-blue-600 dark:text-gray-300 dark:group-hover:text-blue-400"
            >
              {board.title}
            </span>
            <div
              class="absolute inset-0 rounded-xl bg-linear-to-r from-blue-500/0 to-blue-500/0 transition-all duration-300 group-hover:from-blue-500/5 group-hover:to-transparent"
            ></div>
          </a>
        {/each}
      </div>
    </div>
  {/if}

  {#if data.boards.length === 0}
    <p class="text-sm text-gray-600 sm:text-base dark:text-gray-400">
      You don't have any boards yet.
    </p>
  {:else}
    <div class="grid grid-cols-1 items-stretch gap-4 sm:gap-6 md:grid-cols-2 lg:grid-cols-3">
      {#each data.boards as board}
        <Board {board} />
      {/each}
    </div>
  {/if}

  <BoardModal
    bind:open={boardModalOpen}
    board={editingBoard}
    onConfirm={handleConfirmBoard}
    onDelete={handleDeleteBoard}
  />

  <div
    class="animate-refresh-update fixed right-2 bottom-2 rounded-lg border border-gray-200 bg-white px-2 py-1.5 text-[10px] text-gray-600 shadow-sm sm:right-4 sm:bottom-4 sm:px-3 sm:py-2 sm:text-xs dark:border-gray-700 dark:bg-gray-800 dark:text-gray-400"
  >
    Last refreshed: {formatTime(refreshTime)}
  </div>
</div>
