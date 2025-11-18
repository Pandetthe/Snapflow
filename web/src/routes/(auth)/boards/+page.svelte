<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { boardsService } from '$lib/services/boards';
	import { Button, ScrollArea } from 'bits-ui';

	let { data } = $props();
	let boards = $state(data.boards);
	let error = $state(data.error);
	let intervalId: NodeJS.Timeout;
	let lastRefreshTime = $state<Date | null>(data.initialRefreshTime ? new Date(data.initialRefreshTime) : null);

	function formatDate(value: string) {
		return new Date(value).toLocaleDateString();
	}

	function formatTime(date: Date) {
		return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' });
	}

	function stopPropagation(e: MouseEvent) {
		e.stopPropagation();
	}

	async function refreshBoards() {
		const result = await boardsService.getBoards();
		if (!result.ok) {
			if ('title' in result && result.title) {
				error = result.title as string;
			} else {
				error = 'Failed to load boards.';
			}
			return;
		}
		boards = result.boards;
		error = null;
		lastRefreshTime = new Date();
	}

	onMount(() => {
		// Set up interval refresh (initial refresh time is already set from SSR)
		intervalId = setInterval(refreshBoards, 10000);
	});

	onDestroy(() => {
		clearInterval(intervalId);
	});
</script>

<svelte:head>
	<title>Snapflow | Boards</title>
</svelte:head>

<div class="p-4 sm:p-6 lg:p-8 relative min-h-screen">
	<h1 class="text-2xl sm:text-3xl font-bold text-gray-900 dark:text-white mb-4 sm:mb-6">Hi {data.user.userName}, these are your boards:</h1>

	{#if error}
		<p class="text-red-600 dark:text-red-400 text-sm sm:text-base">{error}</p>
	{:else if boards.length === 0}
		<p class="text-gray-600 dark:text-gray-400 text-sm sm:text-base">You don't have any boards yet.</p>
	{:else}
		<div class="grid gap-4 sm:gap-6 grid-cols-1 md:grid-cols-2 lg:grid-cols-3 items-stretch">
			{#each boards as board}
				<Button.Root 
					href="/boards/{board.id}"
					class="block w-full h-full text-left bg-white dark:bg-gray-800 rounded-lg sm:rounded-xl p-4 sm:p-6 shadow-md hover:shadow-xl transition-all duration-300 transform hover:-translate-y-1 hover:scale-[1.02] active:scale-[0.98] cursor-pointer focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 dark:focus:ring-offset-gray-900 flex flex-col touch-manipulation"
				>
					<div class="flex items-center justify-between mb-2 sm:mb-3 gap-2">
						<h2 class="text-lg sm:text-xl font-semibold text-gray-900 dark:text-white break-words flex-1 min-w-0">{board.title}</h2>
						<span class="text-xs text-gray-500 dark:text-gray-400 flex-shrink-0">#{board.id}</span>
					</div>
					<div class="flex-1 min-h-0">
						<ScrollArea.Root type="auto" class="h-full max-h-20 sm:max-h-24" onmousedown={stopPropagation} onclick={stopPropagation}>
							<ScrollArea.Viewport class="max-h-20 sm:max-h-24">
								<p class="text-sm sm:text-base text-gray-600 dark:text-gray-300 break-words pr-2">
									{board.description}
								</p>
							</ScrollArea.Viewport>
							<ScrollArea.Scrollbar orientation="vertical" class="w-2" onmousedown={stopPropagation} onclick={stopPropagation}>
								<ScrollArea.Thumb class="bg-gray-300 dark:bg-gray-600 rounded-full" onmousedown={stopPropagation} onclick={stopPropagation} />
							</ScrollArea.Scrollbar>
						</ScrollArea.Root>
					</div>
					<div class="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-1 sm:gap-0 text-xs text-gray-500 dark:text-gray-400 mt-3 sm:mt-4">
						<span class="truncate max-w-full">Created by {board.createdBy.userName}</span>
						<span class="whitespace-nowrap">{formatDate(board.createdAt)}</span>
					</div>
				</Button.Root>
			{/each}
		</div>
	{/if}

	{#if lastRefreshTime}
		{#key lastRefreshTime.getTime()}
			<div class="fixed bottom-2 right-2 sm:bottom-4 sm:right-4 bg-white dark:bg-gray-800 text-gray-600 dark:text-gray-400 text-[10px] sm:text-xs px-2 py-1.5 sm:px-3 sm:py-2 rounded-lg shadow-md border border-gray-200 dark:border-gray-700 animate-refresh-update">
				Last refreshed: {formatTime(lastRefreshTime)}
			</div>
		{/key}
	{/if}
</div>
