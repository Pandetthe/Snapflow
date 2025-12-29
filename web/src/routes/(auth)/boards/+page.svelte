<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { invalidate } from '$app/navigation';
	import Board from '$lib/components/Board.svelte';

	let { data } = $props();
	let intervalId: NodeJS.Timeout;
	let refreshTime = $derived(new Date(data.refreshTime));

	function formatTime(date: Date) {
		return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' });
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
	<h1 class="mb-4 text-2xl font-bold text-gray-900 sm:mb-6 sm:text-3xl dark:text-white">
		Hi {data.user.userName}, these are your boards:
	</h1>
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
	<div
		class="animate-refresh-update fixed right-2 bottom-2 rounded-lg border border-gray-200 bg-white px-2 py-1.5 text-[10px] text-gray-600 shadow-md sm:right-4 sm:bottom-4 sm:px-3 sm:py-2 sm:text-xs dark:border-gray-700 dark:bg-gray-800 dark:text-gray-400"
	>
		Last refreshed: {formatTime(refreshTime)}
	</div>
</div>
