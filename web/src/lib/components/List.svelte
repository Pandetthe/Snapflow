<script lang="ts">
	import { flip } from 'svelte/animate';
	import { slide } from 'svelte/transition';
	import { dragHandleZone, dragHandle, SHADOW_ITEM_MARKER_PROPERTY_NAME } from 'svelte-dnd-action';
	import type { DndEvent } from 'svelte-dnd-action';
	import { getContext } from 'svelte';
	import type { BoardsHub } from '$lib/services/boards.hub';
	import Card from './Card.svelte';
	import { ScrollArea } from 'bits-ui';
	import { errorStore } from '$lib/stores/error';
	import type { GetBoardByIdResponse } from '$lib/types/boards.api';

	let { list }: { list: GetBoardByIdResponse.ListDto } = $props();

	const getHub = getContext<() => BoardsHub | null>('hub');
	const hub = $derived(getHub());

	function handleCardConsider(e: CustomEvent<DndEvent<GetBoardByIdResponse.CardDto>>) {
		list.cards = e.detail.items;
	}

	async function handleCardFinalize(e: CustomEvent<DndEvent<GetBoardByIdResponse.CardDto>>) {
		list.cards = e.detail.items;
		const { info } = e.detail;
		if (info.trigger === 'droppedIntoZone') {
			const id = Number(info.id);
			const index = list.cards.findIndex((c) => c.id === id);
			const nextItem = list.cards[index + 1];
			const beforeId = nextItem ? nextItem.id : null;

			let res = await hub?.moveCard({ id, listId: list.id, beforeId });
			if (!res?.ok) {
				errorStore.addError('Web.MoveCardFailed', 'Failed to move card');
			}
		}
	}
</script>

<div
	class="flex w-72 shrink-0 flex-col overflow-hidden rounded-lg bg-white shadow-sm transition-[background-color,border-color,box-shadow,opacity] duration-200 dark:bg-gray-700"
>
	<!-- List Header -->
	<div class="group mb-3 flex items-start justify-between gap-2 px-3 pt-3">
		<div class="min-w-0 flex-1">
			<div class="flex items-center gap-2">
				<div
					use:dragHandle
					class="list-drag-handle show-on-hover cursor-move touch-none text-gray-400 hover:text-gray-600 dark:text-gray-500 dark:hover:text-gray-300"
				>
					<svg class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
						<path
							d="M7 6a1 1 0 100-2 1 1 0 000 2zM7 11a1 1 0 100-2 1 1 0 000 2zM7 16a1 1 0 100-2 1 1 0 000 2zM13 6a1 1 0 100-2 1 1 0 000 2zM13 11a1 1 0 100-2 1 1 0 000 2zM13 16a1 1 0 100-2 1 1 0 000 2z"
						/>
					</svg>
				</div>
				<h3 class="min-w-0 flex-1 text-sm font-bold wrap-break-word text-gray-900 dark:text-white">
					{list.title}
				</h3>
			</div>
			<p class="mt-0.5 text-xs text-gray-500 dark:text-gray-400">
				{list.cards.length} card{list.cards.length !== 1 ? 's' : ''}
			</p>
		</div>

		<button
			onclick={() => {}}
			aria-label="Edit list"
			class="show-on-hover rounded-md p-1.5 text-gray-400 hover:bg-gray-100 hover:text-gray-600 dark:hover:bg-gray-600 dark:hover:text-gray-300"
			title="Edit list"
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

	<ScrollArea.Root class="min-h-0 flex-1" type="auto">
		<ScrollArea.Viewport class="h-full w-full">
			<div class="flex min-h-full flex-col px-3 pb-3">
				<section
					use:dragHandleZone={{
						items: list.cards,
						flipDurationMs: 150,
						type: 'cards',
						dropTargetStyle: {},
						useCursorForDetection: true
					}}
					onconsider={handleCardConsider}
					onfinalize={handleCardFinalize}
					class="flex flex-1 flex-col gap-2"
				>
					{#each list.cards as card (card.id)}
						<div
							animate:flip={{ duration: 150 }}
							in:slide={{ duration: 150 }}
							out:slide={{ duration: 150 }}
						>
							<Card {card} />
						</div>
					{/each}
					<div class="mt-1">
						<button
							onclick={() => {}}
							class="add-card-button order-last flex w-full items-center justify-center gap-2 rounded-lg bg-transparent px-4 py-6 text-sm text-gray-500 transition-colors hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-600"
						>
							<svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
								<path
									stroke-linecap="round"
									stroke-linejoin="round"
									stroke-width="2"
									d="M12 4v16m8-8H4"
								/>
							</svg>
							Add Card
						</button>
					</div>
				</section>
			</div>
		</ScrollArea.Viewport>
		<ScrollArea.Scrollbar
			orientation="vertical"
			class="flex w-2 touch-none bg-transparent p-px transition-colors duration-200 select-none hover:bg-black/5 dark:hover:bg-white/5"
		>
			<ScrollArea.Thumb
				class="relative flex-1 rounded-full bg-gray-300 transition-colors duration-200 hover:bg-gray-400 dark:bg-gray-600 dark:hover:bg-gray-500"
			/>
		</ScrollArea.Scrollbar>
		<ScrollArea.Corner />
	</ScrollArea.Root>
</div>

<style>
	:global(.card-ghost) {
		opacity: 0.5;
		background: var(--color-blue-50) !important;
		border: 1px dashed var(--color-blue-400) !important;
	}

	:global(.dark .card-ghost) {
		background: var(--color-blue-900/20) !important;
		border-color: var(--color-blue-500) !important;
	}

	:global(.card-chosen) {
		cursor: grabbing !important;
	}

	:global(.card-drag) {
		box-shadow:
			0 10px 15px -3px rgba(0, 0, 0, 0.1),
			0 4px 6px -2px rgba(0, 0, 0, 0.05) !important;
		opacity: 0.95 !important;
		transform: rotate(0.5deg);
		background: var(--color-white) !important;
		border: 1px solid var(--color-gray-200) !important;
	}

	:global(.dark .card-drag) {
		background: var(--color-gray-800) !important;
		border-color: var(--color-gray-700) !important;
		box-shadow:
			0 10px 15px -3px rgba(0, 0, 0, 0.3),
			0 4px 6px -2px rgba(0, 0, 0, 0.2) !important;
	}
</style>
