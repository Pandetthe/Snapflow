<script lang="ts">
	import { onMount, onDestroy, setContext } from 'svelte';
	import { invalidate } from '$app/navigation';
	import Board from '$lib/components/Board.svelte';
	import BoardModal from '$lib/components/BoardModal.svelte';
	import { BoardsService } from '$lib/services/boards.api';
	import { apiClient } from '$lib/services/api.client';
	import { errorStore } from '$lib/stores/error';
	import { Button } from 'bits-ui';
	import type { GetBoardsResponse } from '$lib/types/boards.api';

	let { data } = $props();
	let intervalId: NodeJS.Timeout;
	let refreshTime = $derived(new Date(data.refreshTime));

	let boardModalOpen = $state(false);
	let editingBoard = $state<GetBoardsResponse.BoardDto | undefined>(undefined);
	const boardsService = new BoardsService(apiClient);

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
			console.log(res);
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
