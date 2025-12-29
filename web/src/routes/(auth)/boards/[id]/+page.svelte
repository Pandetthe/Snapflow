<script lang="ts">
	import { Button } from 'bits-ui';
	import Sortable from 'sortablejs';
	import Swimlane from '$lib/components/Swimlane.svelte';
	import { onDestroy, onMount, setContext } from 'svelte';
	import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
	import { PUBLIC_API_BASE_URL } from '$env/static/public';
	import { errorStore } from '$lib/stores/error';
	import Error from '../../../+error.svelte';

	let { data } = $props();

	let board = $derived(data.board);
	let swimlaneContainer: HTMLElement | null = null;

	$effect(() => {
		board = data.board;
	});

	let connection = $state<HubConnection | null>(null);

	setContext('connection', () => connection);

	onMount(async () => {
		connection = new HubConnectionBuilder()
			.withUrl(`${PUBLIC_API_BASE_URL.replace(/\/+$/, '')}/boards/${data.board.id}/hub`, {
				withCredentials: true
			})
			.withAutomaticReconnect()
			.build();

		try {
			await connection.start();
			connection.on('BoardUpdated', () => {
				console.log('Board updated');
			});

			connection.on('SwimlaneCreated', (id: number, title: string) => {
				//board.swimlanes.push({ id, title });
			});

			connection.on('SwimlaneUpdated', () => {});

			connection.on('SwimlaneMoved', () => {});

			connection.on('SwimlaneDeleted', (id: number) => {
				board.swimlanes = board.swimlanes.filter((swimlane) => swimlane.id !== id);
			});

			connection.on('ListCreated', () => {});

			connection.on('ListUpdated', () => {});

			connection.on('ListMoved', () => {});

			connection.on('ListDeleted', () => {});

			connection.on('CardCreated', () => {});

			connection.on('CardUpdated', () => {});

			connection.on('CardMoved', () => {});

			connection.on('CardLocked', () => {});

			connection.on('CardUnlocked', () => {});

			connection.on('CardDeleted', () => {});

			connection.on('BoardDeleted', () => {
				window.location.href = '/boards/';
			});
			connection.onclose((err) => {
				if (err) {
					errorStore.addError(err.name, err.message);
				}
			});
			connection.onreconnecting((err) => {
				if (err) {
					errorStore.addError(err.name, err.message);
				}
			});
		} catch (err) {
			if (err instanceof Error) {
				errorStore.addError(err.name, err.message);
			} else {
				errorStore.addError('Web.WebSocketConnectionProblem', 'Failed to connect to SignalR hub');
			}
		}
		if (!swimlaneContainer) return;
		Sortable.create(swimlaneContainer, {
			handle: '.swimlane-drag-handle',
			animation: 150,
			group: {
				name: 'swimlanes',
				pull: false,
				put: false
			},
			dataIdAttr: 'data-id',
			draggable: '[data-id]:not(.add-swimlane-button)',
			onMove: (evt) => {
				return !evt.related.classList.contains('add-swimlane-button');
			},
			onSort: (evt) => {
				const container = evt.to;
				const button = Array.from(container.children).find((child) =>
					child.classList.contains('add-swimlane-button')
				);
				if (button && container.lastElementChild !== button) {
					container.appendChild(button);
				}
			},
			onEnd: async (evt) => {
				const itemEl = evt.item;
				const nextEl = itemEl.nextElementSibling;
				const id = parseInt(itemEl.getAttribute('data-id') || '0');
				const beforeId =
					nextEl && !nextEl.classList.contains('add-swimlane-button')
						? parseInt(nextEl.getAttribute('data-id') || '0')
						: null;

				if (id !== 0) {
					try {
						let result = await connection?.invoke('MoveSwimlane', { id, beforeId });
						console.log(result);
					} catch (err) {
						errorStore.addError('Web.MoveSwimlaneFailed', 'Failed to move swimlane');
					}
				}
			}
		});
	});

	onDestroy(async () => await connection?.stop());
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

	<div class="min-h-0 flex-1 py-6">
		<div bind:this={swimlaneContainer} class="flex flex-1 flex-col gap-4">
			{#each board.swimlanes.sort((a, b) => a.rank.localeCompare(b.rank)) as swimlane (swimlane.id)}
				<Swimlane {swimlane} boardId={board.id} />
			{/each}

			<div class="add-swimlane-button order-last mx-6 flex-1">
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
		</div>
	</div>
</div>
