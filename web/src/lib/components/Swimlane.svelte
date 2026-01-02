<script lang="ts">
	import { flip } from 'svelte/animate';
	import { slide } from 'svelte/transition';
	import { dragHandle, dragHandleZone, SHADOW_ITEM_MARKER_PROPERTY_NAME } from 'svelte-dnd-action';
	import type { DndEvent } from 'svelte-dnd-action';
	import List from './List.svelte';
	import { getContext } from 'svelte';
	import { BoardsHub } from '$lib/services/boards.hub';
	import type { GetBoardByIdResponse } from '$lib/types/boards.api';
	import { errorStore } from '$lib/stores/error';
	import { ScrollArea } from 'bits-ui';

	let { swimlane }: { swimlane: GetBoardByIdResponse.SwimlaneDto } = $props();

	const getHub = getContext<() => BoardsHub | null>('hub');
	const hub = $derived(getHub());

	function handleListConsider(e: CustomEvent<DndEvent<GetBoardByIdResponse.ListDto>>) {
		swimlane.lists = e.detail.items;
	}

	async function handleListFinalize(e: CustomEvent<DndEvent<GetBoardByIdResponse.ListDto>>) {
		swimlane.lists = e.detail.items;
		const { info } = e.detail;
		if (info.trigger === 'droppedIntoZone') {
			const id = Number(info.id);
			const index = swimlane.lists.findIndex((l) => l.id === id);
			const nextItem = swimlane.lists[index + 1];
			const beforeId = nextItem ? nextItem.id : null;

			let res = await hub?.moveList({ id, swimlaneId: swimlane.id, beforeId });
			if (!res?.ok) {
				errorStore.addError('Web.MoveListFailed', 'Failed to move list');
			}
		}
	}
</script>

<div
	data-id={swimlane.id}
	role="group"
	style:height={swimlane.height ? `${swimlane.height}px` : undefined}
	class="flex flex-col rounded-lg bg-gray-100 p-4 transition-[background-color,border-color,box-shadow,opacity] duration-200 dark:bg-gray-800 {swimlane.height
		? ''
		: 'flex-1'}"
>
	<div class="group mb-4 flex items-start gap-2">
		<div class="flex flex-1 items-start gap-2">
			<div
				use:dragHandle
				class="show-on-hover cursor-move touch-none pt-1 text-gray-400 hover:text-gray-600 dark:text-gray-500 dark:hover:text-gray-300"
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

	<ScrollArea.Root class="swimlane-scroll-area relative mb-2 flex-1 overflow-hidden" type="auto">
		<ScrollArea.Viewport class="h-full w-full rounded-[inherit]">
			<div class="flex h-full gap-3 pb-4">
				<section
					use:dragHandleZone={{
						items: swimlane.lists,
						flipDurationMs: 150,
						type: 'lists',
						dropTargetStyle: {},
						useCursorForDetection: true
					}}
					onconsider={handleListConsider}
					onfinalize={handleListFinalize}
					class="flex h-full gap-3"
				>
					{#each swimlane.lists as list (list.id)}
						<div
							class="h-full min-h-0"
							animate:flip={{ duration: 150 }}
							in:slide={{ duration: 150 }}
							out:slide={{ duration: 150 }}
						>
							<List {list} />
						</div>
					{/each}
					<div class="mt-0">
						<button
							onclick={() => {}}
							class="add-list-button order-last flex h-fit w-72 shrink items-center justify-center gap-2 self-start rounded-lg border-2 border-dashed border-gray-300 bg-gray-50 px-4 py-6 text-gray-600 transition-colors hover:bg-gray-100 hover:text-gray-900 dark:border-gray-600 dark:bg-gray-700/50 dark:text-gray-400 dark:hover:bg-gray-700 dark:hover:text-white"
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
				</section>
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

<style>
	:global(.swimlane-scroll-area [data-scroll-area-viewport] > [data-scroll-area-content]) {
		height: 100%;
	}

	:global(.list-ghost) {
		opacity: 0.5;
		background: var(--color-gray-200) !important;
		border: 2px dashed var(--color-gray-400) !important;
	}

	:global(.dark .list-ghost) {
		background: var(--color-gray-700) !important;
		border-color: var(--color-gray-500) !important;
	}

	:global(.list-chosen) {
		cursor: grabbing !important;
	}

	:global(.list-drag) {
		box-shadow:
			0 15px 20px -5px rgba(0, 0, 0, 0.15),
			0 5px 5px -5px rgba(0, 0, 0, 0.1) !important;
		opacity: 0.95 !important;
		transform: rotate(0.5deg);
		background: var(--color-white) !important;
		border: 1px solid var(--color-gray-200) !important;
	}

	:global(.dark .list-drag) {
		background: var(--color-gray-800) !important;
		border-color: var(--color-gray-700) !important;
		box-shadow:
			0 15px 20px -5px rgba(0, 0, 0, 0.3),
			0 5px 5px -5px rgba(0, 0, 0, 0.2) !important;
	}
</style>
