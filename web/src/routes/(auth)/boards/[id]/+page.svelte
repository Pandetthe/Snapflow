<script lang="ts">
	import { flip } from 'svelte/animate';
	import { slide } from 'svelte/transition';
	import {
		dndzone,
		dragHandleZone,
		type DndEvent,
		SHADOW_ITEM_MARKER_PROPERTY_NAME
	} from 'svelte-dnd-action';
	import Swimlane from '$lib/components/Swimlane.svelte';
	import { onDestroy, onMount, setContext } from 'svelte';
	import { BoardsHub } from '$lib/services/boards.hub';
	import { errorStore } from '$lib/stores/error';
	import type { GetBoardByIdResponse } from '$lib/types/boards.api';
	import { Button } from 'bits-ui';

	let { data } = $props();
	let board = $state((() => data.board)());
	let hub = $state<BoardsHub | null>(null);

	function sortAll() {
		board.swimlanes.sort(
			(a: GetBoardByIdResponse.SwimlaneDto, b: GetBoardByIdResponse.SwimlaneDto) =>
				a.rank.localeCompare(b.rank)
		);
		for (const s of board.swimlanes) {
			s.lists.sort((a: GetBoardByIdResponse.ListDto, b: GetBoardByIdResponse.ListDto) =>
				a.rank.localeCompare(b.rank)
			);
			for (const l of s.lists) {
				l.cards.sort((a: GetBoardByIdResponse.CardDto, b: GetBoardByIdResponse.CardDto) =>
					a.rank.localeCompare(b.rank)
				);
			}
		}
	}

	function sortSwimlanes() {
		board.swimlanes.sort(
			(a: GetBoardByIdResponse.SwimlaneDto, b: GetBoardByIdResponse.SwimlaneDto) =>
				a.rank.localeCompare(b.rank)
		);
	}

	function sortLists(swimlane: GetBoardByIdResponse.SwimlaneDto) {
		swimlane.lists.sort((a: GetBoardByIdResponse.ListDto, b: GetBoardByIdResponse.ListDto) =>
			a.rank.localeCompare(b.rank)
		);
	}

	function sortCards(list: GetBoardByIdResponse.ListDto) {
		list.cards.sort((a: GetBoardByIdResponse.CardDto, b: GetBoardByIdResponse.CardDto) =>
			a.rank.localeCompare(b.rank)
		);
	}

	$effect(() => {
		if (data.board.id !== board.id) {
			board = data.board;
			sortAll();
		}
	});

	setContext('hub', () => hub);
	setContext('board', () => board);
	onMount(async () => {
		hub = new BoardsHub(data.board.id);

		try {
			await hub.start();

			hub.on('BoardUpdated', (payload) => {
				board.title = payload.title;
				board.description = payload.description;
			});

			hub.on('SwimlaneCreated', (payload) => {
				const newSwimlane = {
					...payload,
					lists: []
				};
				board.swimlanes.push(newSwimlane);
				sortSwimlanes();
			});

			hub.on('SwimlaneUpdated', (payload) => {
				const index = board.swimlanes.findIndex((s) => s.id === payload.id);
				if (index !== -1) {
					board.swimlanes[index].title = payload.title;
					board.swimlanes[index].height = payload.height;
				}
			});

			hub.on('SwimlaneMoved', (payload) => {
				const index = board.swimlanes.findIndex((s) => s.id === payload.id);
				if (index !== -1 && payload.rank !== board.swimlanes[index].rank) {
					board.swimlanes[index].rank = payload.rank;
					sortSwimlanes();
				}
			});

			hub.on('SwimlaneDeleted', (payload) => {
				const index = board.swimlanes.findIndex((s) => s.id === payload.id);
				if (index !== -1) {
					board.swimlanes.splice(index, 1);
				}
			});

			hub.on('ListCreated', (payload) => {
				const swimlane = board.swimlanes.find((s) => s.id === payload.swimlaneId);
				if (swimlane) {
					const newList = { ...payload, cards: [] };
					swimlane.lists.push(newList);
					sortLists(swimlane);
				}
			});

			hub.on('ListUpdated', (payload) => {
				for (const s of board.swimlanes) {
					const list = s.lists.find((l) => l.id === payload.id);
					if (list) {
						list.title = payload.title;
						break;
					}
				}
			});

			hub.on('ListMoved', (payload) => {
				let movedList: GetBoardByIdResponse.ListDto | null = null;
				for (const s of board.swimlanes) {
					const index = s.lists.findIndex((l) => l.id === payload.id);
					if (index !== -1) {
						[movedList] = s.lists.splice(index, 1);
						s.lists = s.lists; // reactivity
						break;
					}
				}
				const targetSwimlane = board.swimlanes.find((s) => s.id === payload.swimlaneId);
				if (movedList && targetSwimlane) {
					movedList.rank = payload.rank;
					targetSwimlane.lists.push(movedList);
					sortLists(targetSwimlane);
				}
			});

			hub.on('ListDeleted', (payload) => {
				for (const s of board.swimlanes) {
					const index = s.lists.findIndex((l) => l.id === payload.id);
					if (index !== -1) {
						s.lists.splice(index, 1);
						s.lists = s.lists;
						break;
					}
				}
			});

			hub.on('CardCreated', (payload) => {
				for (const s of board.swimlanes) {
					const list = s.lists.find((l) => l.id === payload.listId);
					if (list) {
						list.cards.push({
							...payload,
							createdAt: new Date().toISOString(),
							createdBy: {
								id: 1,
								userName: 'John Doe'
							},
							updatedAt: null,
							updatedBy: null
						});
						sortCards(list);
						break;
					}
				}
			});

			hub.on('CardMoved', (payload) => {
				let movedCard: GetBoardByIdResponse.CardDto | null = null;
				for (const s of board.swimlanes) {
					for (const l of s.lists) {
						const index = l.cards.findIndex((c) => c.id === payload.id);
						if (index !== -1) {
							[movedCard] = l.cards.splice(index, 1);
							l.cards = l.cards;
							break;
						}
					}
					if (movedCard) break;
				}
				if (movedCard) {
					for (const s of board.swimlanes) {
						const targetList = s.lists.find((l) => l.id === payload.listId);
						if (targetList) {
							movedCard.rank = payload.rank;
							targetList.cards.push(movedCard);
							sortCards(targetList);
							break;
						}
					}
				}
			});

			hub.on('CardUpdated', (payload) => {
				for (const s of board.swimlanes) {
					for (const l of s.lists) {
						const card = l.cards.find((c) => c.id === payload.id);
						if (card) {
							card.title = payload.title;
							card.description = payload.description;
							return;
						}
					}
				}
			});

			hub.on('CardDeleted', (payload) => {
				for (const s of board.swimlanes) {
					for (const l of s.lists) {
						const index = l.cards.findIndex((c) => c.id === payload.id);
						if (index !== -1) {
							l.cards.splice(index, 1);
							l.cards = l.cards;
							return;
						}
					}
				}
			});

			hub.on('BoardDeleted', () => {
				window.location.href = '/boards/';
			});

			hub.onClose((err) => {
				if (err) {
					errorStore.addError(err.name, err.message);
				}
			});

			hub.onReconnecting((err) => {
				if (err) {
					errorStore.addError(err.name, err.message);
				}
			});
		} catch (err) {
			if (err instanceof Error) {
				errorStore.addError(err.name, err.message);
			} else {
				errorStore.addError('Web.WebSocketConnectionProblem', 'Failed to connect to board hub');
			}
		}
	});

	function handleSwimlaneConsider(e: CustomEvent<DndEvent<GetBoardByIdResponse.SwimlaneDto>>) {
		board.swimlanes = e.detail.items;
	}

	async function handleSwimlaneFinalize(
		e: CustomEvent<DndEvent<GetBoardByIdResponse.SwimlaneDto>>
	) {
		board.swimlanes = e.detail.items;
		const { info } = e.detail;
		if (info.trigger === 'droppedIntoZone') {
			const id = Number(info.id);
			const index = board.swimlanes.findIndex((s) => s.id === id);
			const nextItem = board.swimlanes[index + 1];
			const beforeId = nextItem ? nextItem.id : null;

			let res = await hub?.moveSwimlane({ id, beforeId });
			if (res?.ok) {
				const moved = board.swimlanes.find((s) => s.id === id);
				if (moved) moved.rank = res.value.rank;
				sortSwimlanes();
			} else {
				errorStore.addError('Web.MoveSwimlaneFailed', 'Failed to move swimlane');
				sortSwimlanes();
			}
		}
	}

	onDestroy(async () => {
		await hub?.stop();
		hub = null;
	});
</script>

<svelte:head>
	<title>Snapflow | {board.title}</title>
</svelte:head>

<div class="fixed top-4 left-4 z-50 flex items-center space-x-3">
	<Button.Root
		href="/boards/"
		class="inline-flex h-9 w-9 items-center justify-center rounded-lg bg-white text-gray-700 shadow-sm transition-all duration-200 hover:bg-gray-50 hover:shadow-md dark:bg-gray-800 dark:text-gray-300 dark:hover:bg-gray-700"
		title="Boards"
	>
		←
	</Button.Root>
</div>

<div class="mt-16 flex min-h-screen flex-col bg-gray-50 sm:mt-0 dark:bg-gray-900">
	<div
		class="shrink-0 border-b border-gray-200 bg-white px-6 py-4 dark:border-gray-700 dark:bg-gray-800"
	>
		<div class="flex items-center justify-between">
			<div class="flex items-center gap-4">
				<div class="h-6 w-px bg-gray-300 dark:bg-gray-600"></div>
				<div>
					<h1 class="text-xl font-bold text-gray-900 dark:text-white">
						{board.title}
					</h1>
				</div>
			</div>
		</div>
	</div>

	<div class="flex min-h-0 flex-1 flex-col gap-4 py-6">
		<section
			use:dragHandleZone={{
				items: board.swimlanes,
				flipDurationMs: 150,
				type: 'swimlanes',
				dropTargetStyle: {},
				useCursorForDetection: true
			}}
			onconsider={handleSwimlaneConsider}
			onfinalize={handleSwimlaneFinalize}
			class="flex flex-col gap-4"
		>
			{#each board.swimlanes as swimlane (swimlane.id)}
				<div animate:flip={{ duration: 150 }}>
					<Swimlane {swimlane} />
				</div>
			{/each}
			<div class="mx-6">
				<button
					onclick={() => {}}
					class="flex h-full w-full items-center justify-center gap-2 rounded-lg border-2 border-dashed border-gray-300 bg-gray-50/50 p-4 text-gray-600 transition-colors hover:bg-gray-100 hover:text-gray-900 dark:border-gray-600 dark:bg-gray-800/50 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white"
				>
					<svg class="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path
							stroke-linecap="round"
							stroke-linejoin="round"
							stroke-width="2"
							d="M12 4v16m8-8H4"
						/>
					</svg>
					<span class="font-medium">Add Swimlane</span>
				</button>
			</div>
		</section>
	</div>
</div>

<style>
	:global(.swimlane-ghost) {
		opacity: 0.5;
		background: var(--color-gray-300) !important;
		border: 2px dashed var(--color-gray-500) !important;
	}

	:global(.dark .swimlane-ghost) {
		background: var(--color-gray-700) !important;
		border-color: var(--color-gray-400) !important;
	}

	:global(.swimlane-chosen) {
		cursor: grabbing !important;
	}

	:global(.swimlane-drag) {
		box-shadow:
			0 20px 25px -5px rgba(0, 0, 0, 0.2),
			0 10px 10px -5px rgba(0, 0, 0, 0.1) !important;
		opacity: 0.95 !important;
		transform: rotate(0.5deg);
		background: var(--color-white) !important;
		border: 1px solid var(--color-gray-200) !important;
	}

	:global(.dark .swimlane-drag) {
		background: var(--color-gray-800) !important;
		border-color: var(--color-gray-700) !important;
		box-shadow:
			0 20px 25px -5px rgba(0, 0, 0, 0.4),
			0 10px 10px -5px rgba(0, 0, 0, 0.2) !important;
	}
</style>
