<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { Button, ScrollArea } from 'bits-ui';
	import { invalidate } from '$app/navigation';

	let { data } = $props();
	let boards = $state(data.boards);
	let error = $state(data.error);
	let intervalId: NodeJS.Timeout;
	let lastRefreshTime = $state<Date | null>(
		data.initialRefreshTime ? new Date(data.initialRefreshTime) : null
	);

	function formatDate(value: string) {
		return new Date(value).toLocaleDateString();
	}

	function formatTime(date: Date) {
		return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' });
	}

	function stopPropagation(e: MouseEvent) {
		e.stopPropagation();
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

<div class="relative min-h-screen p-4 sm:p-6 lg:p-8">
	<h1 class="mb-4 text-2xl font-bold text-gray-900 sm:mb-6 sm:text-3xl dark:text-white">
		Hi {data.user.userName}, these are your boards:
	</h1>

	{#if error}
		<p class="text-sm text-red-600 sm:text-base dark:text-red-400">{error}</p>
	{:else if boards.length === 0}
		<p class="text-sm text-gray-600 sm:text-base dark:text-gray-400">
			You don't have any boards yet.
		</p>
	{:else}
		<div class="grid grid-cols-1 items-stretch gap-4 sm:gap-6 md:grid-cols-2 lg:grid-cols-3">
			{#each boards as board}
				<Button.Root
					href="/boards/{board.id}"
					class="block h-full w-full transform cursor-pointer touch-manipulation flex-col rounded-lg bg-white p-4 text-left shadow-md transition-all duration-300 hover:-translate-y-1 hover:scale-[1.02] hover:shadow-xl focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 focus:outline-none active:scale-[0.98] sm:rounded-xl sm:p-6 dark:bg-gray-800 dark:focus:ring-offset-gray-900"
				>
					<div class="mb-2 flex items-center justify-between gap-2 sm:mb-3">
						<h2
							class="min-w-0 flex-1 text-lg font-semibold wrap-break-word text-gray-900 sm:text-xl dark:text-white"
						>
							{board.title}
						</h2>
						<span class="shrink-0 text-xs text-gray-500 dark:text-gray-400">#{board.id}</span>
					</div>
					<div class="min-h-0 flex-1">
						<ScrollArea.Root
							type="auto"
							class="h-full max-h-20 sm:max-h-24"
							onmousedown={stopPropagation}
							onclick={stopPropagation}
						>
							<ScrollArea.Viewport class="max-h-20 sm:max-h-24">
								<p
									class="pr-2 text-sm wrap-break-word text-gray-600 sm:text-base dark:text-gray-300"
								>
									{board.description}
								</p>
							</ScrollArea.Viewport>
							<ScrollArea.Scrollbar
								orientation="vertical"
								class="w-2"
								onmousedown={stopPropagation}
								onclick={stopPropagation}
							>
								<ScrollArea.Thumb
									class="rounded-full bg-gray-300 dark:bg-gray-600"
									onmousedown={stopPropagation}
									onclick={stopPropagation}
								/>
							</ScrollArea.Scrollbar>
						</ScrollArea.Root>
					</div>
					<div
						class="mt-3 flex flex-col items-start justify-between gap-1 text-xs text-gray-500 sm:mt-4 sm:flex-row sm:items-center sm:gap-0 dark:text-gray-400"
					>
						<span class="max-w-full truncate">Created by {board.createdBy.userName}</span>
						<span class="whitespace-nowrap">{formatDate(board.createdAt)}</span>
					</div>
				</Button.Root>
			{/each}
		</div>
	{/if}

	{#if lastRefreshTime}
		{#key lastRefreshTime.getTime()}
			<div
				class="animate-refresh-update fixed right-2 bottom-2 rounded-lg border border-gray-200 bg-white px-2 py-1.5 text-[10px] text-gray-600 shadow-md sm:right-4 sm:bottom-4 sm:px-3 sm:py-2 sm:text-xs dark:border-gray-700 dark:bg-gray-800 dark:text-gray-400"
			>
				Last refreshed: {formatTime(lastRefreshTime)}
			</div>
		{/key}
	{/if}
</div>
