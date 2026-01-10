<script lang="ts">
	import type { GetBoardByIdResponse } from '$lib/types/boards.api';
	import { dragHandle } from 'svelte-dnd-action';
	import { getContext } from 'svelte';

	let { card, listId }: { card: GetBoardByIdResponse.CardDto; listId: number } = $props();

	interface BoardUI {
		openListModal: (swimlaneId: number, list?: GetBoardByIdResponse.ListDto) => void;
		openCardModal: (listId: number, card?: GetBoardByIdResponse.CardDto) => void;
	}
	const ui = getContext<BoardUI>('ui');
</script>

<div
	data-id={card.id}
	class="group flex flex-col gap-2 rounded-lg border border-gray-200 bg-white p-3 shadow-sm transition-[background-color,border-color,box-shadow,opacity] duration-200 hover:border-blue-400 hover:shadow-md dark:border-gray-600 dark:bg-gray-800 dark:hover:border-blue-500"
>
	<div class="flex items-start justify-between gap-2">
		<div class="flex min-w-0 flex-1 items-start gap-2">
			<div class="show-on-hover flex items-center gap-1">
				<div
					use:dragHandle
					class="card-drag-handle cursor-move p-1 text-gray-400 transition-colors hover:text-gray-600 dark:text-gray-500 dark:hover:text-gray-300"
				>
					<svg class="h-3.5 w-3.5" viewBox="0 0 20 20" fill="currentColor">
						<path
							d="M7 6a1 1 0 100-2 1 1 0 000 2zM7 11a1 1 0 100-2 1 1 0 000 2zM7 16a1 1 0 100-2 1 1 0 000 2zM13 6a1 1 0 100-2 1 1 0 000 2zM13 11a1 1 0 100-2 1 1 0 000 2zM13 16a1 1 0 100-2 1 1 0 000 2z"
						/>
					</svg>
				</div>
			</div>
			<h4
				class="mt-0.5 min-w-0 flex-1 text-sm font-medium wrap-break-word text-gray-900 dark:text-white"
			>
				{card.title}
			</h4>
		</div>

		<div class="show-on-hover">
			<button
				onclick={() => ui.openCardModal(listId, card)}
				class="rounded-md p-1 text-gray-400 transition-all duration-200 hover:bg-gray-100 hover:text-gray-600 dark:hover:bg-gray-700 dark:hover:text-gray-300"
				title="Edit card"
			>
				<svg class="h-3.5 w-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
					<path
						stroke-linecap="round"
						stroke-linejoin="round"
						stroke-width="2"
						d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z"
					/>
				</svg>
			</button>
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
				class="flex h-5 w-5 items-center justify-center rounded-full bg-blue-100 text-[10px] font-bold text-blue-600 dark:bg-blue-900/40 dark:text-blue-400"
			>
				{card.createdBy.userName.charAt(0).toUpperCase()}
			</div>
		</div>
		<div class="flex items-center gap-2 text-gray-400">
			<svg class="h-3.5 w-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
				<path
					stroke-linecap="round"
					stroke-linejoin="round"
					stroke-width="2"
					d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
				/>
			</svg>
			<span class="text-[10px]">{new Date(card.createdAt).toLocaleDateString()}</span>
		</div>
	</div>
</div>
