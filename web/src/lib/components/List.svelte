<script lang="ts">
	import Sortable from 'sortablejs';
	import { getContext, onMount } from 'svelte';
	import type { HubConnection } from '@microsoft/signalr';
	import type { List } from '$lib/services/boards';
	import Card from './Card.svelte';
	import { ScrollArea } from 'bits-ui';

	let { list, boardId }: { list: List; boardId: number } = $props();

	import { apiClient } from '$lib/services/api.client';
	import { errorStore } from '$lib/stores/error';

	const getConnection = getContext<() => HubConnection | null>('connection');
	const connection = $derived(getConnection());

	let cardsContainer: HTMLElement | null = null;

	onMount(() => {
		if (!cardsContainer) return;

		Sortable.create(cardsContainer, {
			group: {
				name: 'cards',
				pull: true,
				put: (to, from) => {
					const group = from.options.group;
					if (!group) return false;
					return (typeof group === 'string' ? group : group.name) === 'cards';
				}
			},
			animation: 150,
			dataIdAttr: 'data-id',
			draggable: '[data-id]:not(.add-card-button)',
			handle: '.card-drag-handle',
			onMove: (evt) => {
				return !evt.related.classList.contains('add-card-button');
			},
			onSort: (evt) => {
				const container = evt.to;
				const button = Array.from(container.children).find((child) =>
					child.classList.contains('add-card-button')
				);
				if (button && container.lastElementChild !== button) {
					container.appendChild(button);
				}
			},
			onEnd: async (evt) => {
				const itemEl = evt.item;
				const nextEl = itemEl.nextElementSibling;
				const id = parseInt(itemEl.getAttribute('data-id') || '0');
				const targetListId = parseInt(evt.to.getAttribute('data-id') || '0');
				const beforeId =
					nextEl && !nextEl.classList.contains('add-card-button')
						? parseInt(nextEl.getAttribute('data-id') || '0')
						: null;

				if (id !== 0 && targetListId !== 0) {
					try {
						await connection?.invoke('MoveCard', { id, listId: targetListId, beforeId });
					} catch (err) {
						errorStore.addError('Web.MoveCardFailed', 'Failed to move card');
					}
				}
			}
		});
	});
</script>

<div
	data-id={list.id}
	class="flex h-full max-h-[500px] w-72 shrink-0 flex-col overflow-hidden rounded-lg bg-white shadow-sm transition-[background-color,border-color,box-shadow,opacity] duration-200 dark:bg-gray-700"
>
	<!-- List Header -->
	<div class="group mb-3 flex items-start justify-between gap-2 px-3 pt-3">
		<div class="min-w-0 flex-1">
			<div class="flex items-center gap-2">
				<div
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

	<!-- Cards Scroll Area -->
	<ScrollArea.Root class="min-h-0 flex-1" type="auto">
		<ScrollArea.Viewport class="h-full w-full">
			<div class="flex h-full flex-col px-3 pb-3">
				<div bind:this={cardsContainer} data-id={list.id} class="flex flex-1 flex-col gap-2">
					{#each list.cards.sort((a, b) => a.rank.localeCompare(b.rank)) as card (card.id)}
						<Card {card} />
					{/each}

					<button
						onclick={() => {}}
						class="add-card-button order-last mt-1 flex w-full items-center justify-center gap-2 rounded-lg bg-transparent p-2 text-sm text-gray-500 transition-colors hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-600"
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
