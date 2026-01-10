<script lang="ts">
	import type { GetBoardsResponse } from '$lib/types/boards.api';
	import { Button, ScrollArea } from 'bits-ui';
	import { getContext } from 'svelte';

	let { board }: { board: GetBoardsResponse.BoardDto } = $props();

	interface BoardsUI {
		openBoardModal: (board?: GetBoardsResponse.BoardDto) => void;
	}
	const ui = getContext<BoardsUI>('boards-ui');

	function formatDate(value: string) {
		return new Date(value).toLocaleDateString();
	}
</script>

<Button.Root
	href="/boards/{board.id}"
	class="flex h-full w-full transform cursor-pointer flex-col rounded-lg bg-white p-4 text-left shadow-md transition-all duration-300 hover:-translate-y-1 hover:scale-[1.02] hover:shadow-xl focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 focus:outline-none active:scale-[0.98] sm:rounded-xl sm:p-6 dark:bg-gray-800 dark:focus:ring-offset-gray-900"
>
	<div class="mb-2 flex items-center justify-between gap-2 sm:mb-3">
		<h2
			class="min-w-0 flex-1 text-lg font-semibold wrap-break-word text-gray-900 sm:text-xl dark:text-white"
		>
			{board.title}
		</h2>
		<div class="flex items-center gap-2">
			<button
				onclick={(e) => {
					e.preventDefault();
					e.stopPropagation();
					ui.openBoardModal(board);
				}}
				class="rounded-md p-1.5 text-gray-400 hover:bg-gray-100 hover:text-gray-600 dark:hover:bg-gray-700 dark:hover:text-gray-300"
				title="Edit board"
			>
				<svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
					<path
						stroke-linecap="round"
						stroke-linejoin="round"
						stroke-width="2"
						d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z"
					/>
				</svg>
			</button>
			<span class="shrink-0 text-xs text-gray-500 dark:text-gray-400">
				#{board.id}
			</span>
		</div>
	</div>
	<div class="min-h-0 flex-1">
		<ScrollArea.Root type="auto" class="h-full max-h-20 sm:max-h-24">
			<ScrollArea.Viewport class="h-full max-h-20 sm:max-h-24">
				<p class="pr-2 text-sm wrap-break-word text-gray-600 sm:text-base dark:text-gray-300">
					{board.description}
				</p>
			</ScrollArea.Viewport>

			<div
				role="none"
				onclick={(e) => e.preventDefault()}
				onpointerdown={(e) => e.preventDefault()}
				onmousedown={(e) => e.preventDefault()}
				class="touch-pan-y select-none"
			>
				<ScrollArea.Scrollbar orientation="vertical" class="w-2">
					<ScrollArea.Thumb class="rounded-full bg-gray-300 dark:bg-gray-600" />
				</ScrollArea.Scrollbar>
			</div>
		</ScrollArea.Root>
	</div>
	<div
		class="mt-auto flex items-center justify-between gap-1 text-xs text-gray-500 sm:mt-4 sm:flex-row sm:items-center sm:gap-0 dark:text-gray-400"
	>
		<span class="max-w-full truncate">
			Created by {board.createdBy.userName}
		</span>
		<span class="whitespace-nowrap">
			{formatDate(board.createdAt)}
		</span>
	</div>
</Button.Root>
