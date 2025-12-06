<script lang="ts">
	import { Button } from 'bits-ui';
	import type { BoardDetailed } from '$lib/services/boards';

	let { data } = $props<{ data: { board: BoardDetailed } }>();

	// Local state for drag-and-drop
	let board = $state<BoardDetailed>({
		...structuredClone(data.board),
		swimlanes: data.board.swimlanes || []
	});
	let draggedSwimlane = $state<number | null>(null);
	let draggedList = $state<{ swimlaneId: number; listId: number } | null>(null);
	let draggedCard = $state<{ swimlaneId: number; listId: number; cardId: number } | null>(null);
	let dragOverSwimlane = $state<number | null>(null);
	let dragOverList = $state<{ swimlaneId: number; listId: number } | null>(null);
	let dragOverCard = $state<{ swimlaneId: number; listId: number; cardId: number } | null>(null);

	// Edit state
	let editingSwimlane = $state<number | null>(null);
	let editingList = $state<{ swimlaneId: number; listId: number } | null>(null);
	let editSwimlaneTitle = $state('');
	let editListTitle = $state('');

	// Counter for generating temporary IDs
	let nextTempId = $state(-1);

	function formatDateTime(value: string) {
		return new Date(value).toLocaleString();
	}

	// Swimlane drag handlers
	function handleSwimlaneDragStart(e: DragEvent, swimlaneId: number) {
		draggedSwimlane = swimlaneId;
		if (e.dataTransfer) {
			e.dataTransfer.effectAllowed = 'move';
		}
	}

	function handleSwimlaneDragOver(e: DragEvent, swimlaneId: number) {
		e.preventDefault();
		if (draggedSwimlane !== null && draggedSwimlane !== swimlaneId) {
			dragOverSwimlane = swimlaneId;
		}
	}

	function handleSwimlaneDrop(e: DragEvent, targetSwimlaneId: number) {
		e.preventDefault();
		if (draggedSwimlane !== null && draggedSwimlane !== targetSwimlaneId) {
			const fromIndex = board.swimlanes.findIndex((s) => s.id === draggedSwimlane);
			const toIndex = board.swimlanes.findIndex((s) => s.id === targetSwimlaneId);

			if (fromIndex !== -1 && toIndex !== -1) {
				const newSwimlanes = [...board.swimlanes];
				const [movedSwimlane] = newSwimlanes.splice(fromIndex, 1);
				newSwimlanes.splice(toIndex, 0, movedSwimlane);
				board.swimlanes = newSwimlanes;
			}
		}
		draggedSwimlane = null;
		dragOverSwimlane = null;
	}

	function handleSwimlaneDragEnd() {
		draggedSwimlane = null;
		dragOverSwimlane = null;
	}

	// List drag handlers
	function handleListDragStart(e: DragEvent, swimlaneId: number, listId: number) {
		e.stopPropagation();
		draggedList = { swimlaneId, listId };
		if (e.dataTransfer) {
			e.dataTransfer.effectAllowed = 'move';
		}
	}

	function handleListDragOver(e: DragEvent, swimlaneId: number, listId: number) {
		e.preventDefault();
		e.stopPropagation();
		if (draggedList && !(draggedList.swimlaneId === swimlaneId && draggedList.listId === listId)) {
			dragOverList = { swimlaneId, listId };
		}
	}

	function handleListDrop(e: DragEvent, targetSwimlaneId: number, targetListId: number) {
		e.preventDefault();
		e.stopPropagation();

		if (draggedList) {
			const sourceSwimlane = board.swimlanes.find((s) => s.id === draggedList?.swimlaneId);
			const targetSwimlane = board.swimlanes.find((s) => s.id === targetSwimlaneId);

			if (sourceSwimlane && targetSwimlane) {
				const sourceListIndex = sourceSwimlane.lists.findIndex((l) => l.id === draggedList?.listId);
				const targetListIndex = targetSwimlane.lists.findIndex((l) => l.id === targetListId);

				if (sourceListIndex !== -1 && targetListIndex !== -1) {
					const [movedList] = sourceSwimlane.lists.splice(sourceListIndex, 1);
					targetSwimlane.lists.splice(targetListIndex, 0, movedList);
					board.swimlanes = [...board.swimlanes];
				}
			}
		}
		draggedList = null;
		dragOverList = null;
	}

	function handleListDragEnd() {
		draggedList = null;
		dragOverList = null;
	}

	// Card drag handlers
	function handleCardDragStart(e: DragEvent, swimlaneId: number, listId: number, cardId: number) {
		e.stopPropagation();
		draggedCard = { swimlaneId, listId, cardId };
		if (e.dataTransfer) {
			e.dataTransfer.effectAllowed = 'move';
		}
	}

	function handleCardDragOver(e: DragEvent, swimlaneId: number, listId: number, cardId: number) {
		e.preventDefault();
		e.stopPropagation();
		if (
			draggedCard &&
			!(
				draggedCard.swimlaneId === swimlaneId &&
				draggedCard.listId === listId &&
				draggedCard.cardId === cardId
			)
		) {
			dragOverCard = { swimlaneId, listId, cardId };
		}
	}

	function handleCardDrop(
		e: DragEvent,
		targetSwimlaneId: number,
		targetListId: number,
		targetCardId: number
	) {
		e.preventDefault();
		e.stopPropagation();

		if (draggedCard) {
			const sourceSwimlane = board.swimlanes.find((s) => s.id === draggedCard?.swimlaneId);
			const targetSwimlane = board.swimlanes.find((s) => s.id === targetSwimlaneId);

			if (sourceSwimlane && targetSwimlane) {
				const sourceList = sourceSwimlane.lists.find((l) => l.id === draggedCard?.listId);
				const targetList = targetSwimlane.lists.find((l) => l.id === targetListId);

				if (sourceList && targetList) {
					const sourceCardIndex = sourceList.cards.findIndex((c) => c.id === draggedCard?.cardId);
					const targetCardIndex = targetList.cards.findIndex((c) => c.id === targetCardId);

					if (sourceCardIndex !== -1 && targetCardIndex !== -1) {
						const [movedCard] = sourceList.cards.splice(sourceCardIndex, 1);
						targetList.cards.splice(targetCardIndex, 0, movedCard);
						board.swimlanes = [...board.swimlanes];
					}
				}
			}
		}
		draggedCard = null;
		dragOverCard = null;
	}

	function handleCardDragEnd() {
		draggedCard = null;
		dragOverCard = null;
	}

	// Handle drop on empty list
	function handleEmptyListDrop(e: DragEvent, swimlaneId: number, listId: number) {
		e.preventDefault();
		e.stopPropagation();

		if (draggedCard) {
			const sourceSwimlane = board.swimlanes.find((s) => s.id === draggedCard?.swimlaneId);
			const targetSwimlane = board.swimlanes.find((s) => s.id === swimlaneId);

			if (sourceSwimlane && targetSwimlane) {
				const sourceList = sourceSwimlane.lists.find((l) => l.id === draggedCard?.listId);
				const targetList = targetSwimlane.lists.find((l) => l.id === listId);

				if (sourceList && targetList) {
					const sourceCardIndex = sourceList.cards.findIndex((c) => c.id === draggedCard?.cardId);

					if (sourceCardIndex !== -1) {
						const [movedCard] = sourceList.cards.splice(sourceCardIndex, 1);
						targetList.cards.push(movedCard);
						board.swimlanes = [...board.swimlanes];
					}
				}
			}
		}
		draggedCard = null;
		dragOverCard = null;
	}

	function handleEmptyListDragOver(e: DragEvent) {
		e.preventDefault();
		e.stopPropagation();
	}
</script>

<div class="flex h-screen flex-col bg-gray-50 dark:bg-gray-900">
	<!-- Header -->
	<div
		class="shrink-0 border-b border-gray-200 bg-white px-6 py-4 dark:border-gray-700 dark:bg-gray-800"
	>
		<div class="flex items-center justify-between">
			<div class="flex items-center gap-4">
				<Button.Root
					href="/boards"
					class="inline-flex items-center text-sm text-gray-600 transition-colors hover:text-gray-900 dark:text-gray-400 dark:hover:text-white"
				>
					<svg class="mr-2 h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path
							stroke-linecap="round"
							stroke-linejoin="round"
							stroke-width="2"
							d="M15 19l-7-7 7-7"
						/>
					</svg>
					Back to boards
				</Button.Root>
				<div class="h-6 w-px bg-gray-300 dark:bg-gray-600"></div>
				<div>
					<h1 class="text-xl font-bold text-gray-900 dark:text-white">
						{board.title}
					</h1>
					{#if board.description}
						<p class="text-sm text-gray-600 dark:text-gray-400">{board.description}</p>
					{/if}
				</div>
			</div>
		</div>
	</div>

	<!-- Board content - swimlanes expand vertically -->
	<div class="flex-1 overflow-y-auto p-6">
		<div class="flex flex-col gap-6">
			{#each board.swimlanes as swimlane (swimlane.id)}
				<div
					role="group"
					draggable="true"
					ondragstart={(e) => handleSwimlaneDragStart(e, swimlane.id)}
					ondragover={(e) => handleSwimlaneDragOver(e, swimlane.id)}
					ondrop={(e) => handleSwimlaneDrop(e, swimlane.id)}
					ondragend={handleSwimlaneDragEnd}
					class="rounded-lg bg-gray-100 p-4 transition-all duration-200 dark:bg-gray-800"
					class:opacity-50={draggedSwimlane === swimlane.id}
					class:ring-2={dragOverSwimlane === swimlane.id}
					class:ring-blue-500={dragOverSwimlane === swimlane.id}
				>
					<!-- Swimlane header -->
					<div class="mb-4 flex items-start gap-2">
						<!-- Mobile drag handle -->
						<div class="cursor-move touch-none pt-1" draggable="true">
							<svg
								class="h-5 w-5 text-gray-400"
								fill="none"
								stroke="currentColor"
								viewBox="0 0 24 24"
							>
								<path
									stroke-linecap="round"
									stroke-linejoin="round"
									stroke-width="2"
									d="M4 8h16M4 16h16"
								/>
							</svg>
						</div>

						<div class="flex-1">
							{#if editingSwimlane === swimlane.id}
								<div class="flex items-center gap-2">
									<input
										type="text"
										bind:value={editSwimlaneTitle}
										onkeydown={(e) => {
											if (e.key === 'Enter') saveSwimlaneEdit();
											if (e.key === 'Escape') cancelSwimlaneEdit();
										}}
										class="flex-1 rounded border border-blue-500 bg-white px-2 py-1 text-lg font-semibold focus:ring-2 focus:ring-blue-500 focus:outline-none dark:bg-gray-700"
										autofocus
									/>
									<button
										onclick={saveSwimlaneEdit}
										class="rounded p-1 text-green-600 hover:bg-green-50 dark:hover:bg-green-900/20"
										title="Save"
									>
										<svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
											<path
												stroke-linecap="round"
												stroke-linejoin="round"
												stroke-width="2"
												d="M5 13l4 4L19 7"
											/>
										</svg>
									</button>
									<button
										onclick={cancelSwimlaneEdit}
										class="rounded p-1 text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20"
										title="Cancel"
									>
										<svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
											<path
												stroke-linecap="round"
												stroke-linejoin="round"
												stroke-width="2"
												d="M6 18L18 6M6 6l12 12"
											/>
										</svg>
									</button>
								</div>
							{:else}
								<div class="flex items-center gap-2">
									<h2 class="mb-1 flex-1 text-lg font-semibold text-gray-900 dark:text-white">
										{swimlane.title}
									</h2>
									<button
										onclick={() => startEditSwimlane(swimlane.id, swimlane.title)}
										class="rounded p-1 text-gray-400 hover:bg-gray-200 hover:text-gray-600 dark:hover:bg-gray-700 dark:hover:text-gray-300"
										title="Edit swimlane"
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
								</div>
								<p class="text-xs text-gray-500 dark:text-gray-400">
									{swimlane.lists.length} list{swimlane.lists.length !== 1 ? 's' : ''}
								</p>
							{/if}
						</div>
					</div>

					<!-- Lists - horizontal scroll -->
					<div class="overflow-x-auto">
						<div class="flex gap-3 pb-2">
							{#each swimlane.lists as list (list.id)}
								<div
									role="list"
									draggable="true"
									ondragstart={(e) => handleListDragStart(e, swimlane.id, list.id)}
									ondragover={(e) => handleListDragOver(e, swimlane.id, list.id)}
									ondrop={(e) => handleListDrop(e, swimlane.id, list.id)}
									ondragend={handleListDragEnd}
									class="flex max-h-96 w-72 shrink-0 flex-col rounded-lg bg-white p-3 shadow-sm transition-all duration-200 dark:bg-gray-700"
								></div>
							{/each}

							<!-- Add new list button -->
							<button
								onclick={() => addNewList(swimlane.id)}
								class="flex w-72 shrink-0 items-center justify-center gap-2 rounded-lg border-2 border-dashed border-gray-300 bg-gray-50 p-4 text-gray-600 transition-colors hover:bg-gray-100 hover:text-gray-900 dark:border-gray-600 dark:bg-gray-700/50 dark:text-gray-400 dark:hover:bg-gray-700 dark:hover:text-white"
							>
								<svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
									<path
										stroke-linecap="round"
										stroke-linejoin="round"
										stroke-width="2"
										d="M12 4v16m8-8H4"
									/>
								</svg>
								<span class="font-medium">Add List</span>
							</button>
						</div>
					</div>
				</div>
			{/each}

			{#if board.swimlanes.length === 0}
				<div class="flex h-full w-full items-center justify-center">
					<div class="text-center">
						<svg
							class="mx-auto h-12 w-12 text-gray-400"
							fill="none"
							stroke="currentColor"
							viewBox="0 0 24 24"
						>
							<path
								stroke-linecap="round"
								stroke-linejoin="round"
								stroke-width="2"
								d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
							/>
						</svg>
						<h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">No swimlanes</h3>
						<p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
							This board doesn't have any swimlanes yet.
						</p>
					</div>
				</div>
			{/if}
		</div>
	</div>
</div>

<style>
	.line-clamp-2 {
		display: -webkit-box;
		line-clamp: 2;
		-webkit-line-clamp: 2;
		-webkit-box-orient: vertical;
		overflow: hidden;
	}
</style>
