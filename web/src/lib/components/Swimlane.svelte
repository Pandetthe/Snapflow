<script lang="ts">
	import Sortable from 'sortablejs';
	import { getContext, onMount } from 'svelte';
	import type { HubConnection } from '@microsoft/signalr';
	import type { Swimlane } from '$lib/services/boards';
	import List from './List.svelte';
	import { ScrollArea } from 'bits-ui';
	import { errorStore } from '$lib/stores/error';

	let {
		swimlane,
		boardId
	}: {
		swimlane: Swimlane;
		boardId: number;
	} = $props();

	const getConnection = getContext<() => HubConnection | null>('connection');
	const connection = $derived(getConnection());

	let listsContainer: HTMLElement | null = null;

	onMount(() => {
		if (!listsContainer) return;

		Sortable.create(listsContainer, {
			group: {
				name: 'lists',
				pull: true,
				put: (to, from) => {
					const group = from.options.group;
					if (!group) return false;
					return (typeof group === 'string' ? group : group.name) === 'lists';
				}
			},
			animation: 150,
			dataIdAttr: 'data-id',
			draggable: '[data-id]:not(.add-list-button)',
			handle: '.list-drag-handle',
			onMove: (evt) => {
				return !evt.related.classList.contains('add-list-button');
			},
			onSort: (evt) => {
				const container = evt.to;
				const button = Array.from(container.children).find((child) =>
					child.classList.contains('add-list-button')
				);
				if (button && container.lastElementChild !== button) {
					container.appendChild(button);
				}
			},
			onEnd: async (evt) => {
				const itemEl = evt.item;
				const nextEl = itemEl.nextElementSibling;
				const id = parseInt(itemEl.getAttribute('data-id') || '0');
				const targetSwimlaneId = parseInt(evt.to.getAttribute('data-id') || '0');
				const beforeId =
					nextEl && !nextEl.classList.contains('add-list-button')
						? parseInt(nextEl.getAttribute('data-id') || '0')
						: null;

				if (id !== 0 && targetSwimlaneId !== 0) {
					try {
						await connection?.invoke('MoveList', { id, swimlaneId: targetSwimlaneId, beforeId });
					} catch (err) {
						errorStore.addError('Web.MoveListFailed', 'Failed to move list');
					}
				}
			}
		});
	});
</script>

<div
	data-id={swimlane.id}
	role="group"
	class="flex min-h-96 flex-col rounded-lg bg-gray-100 p-4 transition-[background-color,border-color,box-shadow,opacity] duration-200 dark:bg-gray-800"
>
	<div class="group mb-4 flex items-start gap-2">
		<div class="flex flex-1 items-start gap-2">
			<div
				class="swimlane-drag-handle show-on-hover cursor-move touch-none pt-1 text-gray-400 hover:text-gray-600 dark:text-gray-500 dark:hover:text-gray-300"
			>
				<svg class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
					<path
						d="M7 6a1 1 0 100-2 1 1 0 000 2zM7 11a1 1 0 100-2 1 1 0 000 2zM7 16a1 1 0 100-2 1 1 0 000 2zM13 6a1 1 0 100-2 1 1 0 000 2zM13 11a1 1 0 100-2 1 1 0 000 2zM13 16a1 1 0 100-2 1 1 0 000 2z"
					/>
				</svg>
			</div>

			<div class="min-w-0 flex-1">
				<div class="flex items-center gap-2">
					<h2
						class="mb-1 min-w-0 flex-1 text-lg font-semibold wrap-break-word text-gray-900 dark:text-white"
					>
						{swimlane.title}
					</h2>
					<button
						onclick={() => {}}
						class="show-on-hover rounded-md p-1.5 text-gray-400 hover:bg-gray-200 hover:text-gray-600 dark:hover:bg-gray-700 dark:hover:text-gray-300"
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
			</div>
		</div>
	</div>

	<ScrollArea.Root class="min-h-0 flex-1" type="auto">
		<ScrollArea.Viewport class="w-full rounded-[inherit]">
			<div class="flex gap-3 pb-4">
				<div bind:this={listsContainer} data-id={swimlane.id} class="flex items-start gap-3">
					{#each swimlane.lists.sort((a, b) => a.rank.localeCompare(b.rank)) as list (list.id)}
						<List {list} {boardId} />
					{/each}

					<button
						onclick={() => {}}
						class="add-list-button order-last flex h-fit w-72 shrink items-center justify-center gap-2 self-start rounded-lg border-2 border-dashed border-gray-300 bg-gray-50 p-4 text-gray-600 transition-colors hover:bg-gray-100 hover:text-gray-900 dark:border-gray-600 dark:bg-gray-700/50 dark:text-gray-400 dark:hover:bg-gray-700 dark:hover:text-white"
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
		</ScrollArea.Viewport>
		<ScrollArea.Scrollbar
			orientation="horizontal"
			class="flex h-2.5 touch-none flex-col bg-transparent p-0.5 transition-colors duration-200 select-none hover:bg-black/5 dark:hover:bg-white/5"
		>
			<ScrollArea.Thumb
				class="relative flex-1 rounded-full bg-gray-300 transition-colors duration-200 hover:bg-gray-400 dark:bg-gray-600 dark:hover:bg-gray-500"
			/>
		</ScrollArea.Scrollbar>
		<ScrollArea.Corner />
	</ScrollArea.Root>
</div>
