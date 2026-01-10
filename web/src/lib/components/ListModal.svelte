<script lang="ts">
	import { Dialog, Button } from 'bits-ui';
	import { slide } from 'svelte/transition';
	import type { GetBoardByIdResponse } from '$lib/types/boards.api';

	let {
		open = $bindable(false),
		list = $bindable(undefined),
		onConfirm,
		onDelete
	}: {
		open: boolean;
		list?: GetBoardByIdResponse.ListDto;
		onConfirm: (title: string, width: number | null) => void;
		onDelete?: (id: number) => void;
	} = $props();

	let title = $state('');
	let width: number | null = $state(null);

	$effect(() => {
		if (open) {
			if (list) {
				title = list.title;
				width = list.width;
			} else {
				title = '';
				width = null;
			}
		}
	});

	function handleSubmit(e: SubmitEvent) {
		e.preventDefault();
		onConfirm(title, width);
		open = false;
	}
</script>

<Dialog.Root bind:open>
	<Dialog.Portal>
		<Dialog.Overlay
			class="fixed inset-0 z-50 bg-black/80 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:animate-in data-[state=open]:fade-in-0"
		/>
		<Dialog.Content
			class="bg-background fixed top-[50%] left-[50%] z-50 w-full max-w-lg translate-x-[-50%] translate-y-[-50%] gap-4 border bg-white p-6 shadow-lg duration-200 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:zoom-out-95 data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=open]:zoom-in-95 sm:rounded-lg md:w-full dark:border-gray-700 dark:bg-gray-800"
		>
			<Dialog.Title
				class="text-lg leading-none font-semibold tracking-tight text-gray-900 dark:text-gray-100"
			>
				{list ? 'Edit List' : 'Create List'}
			</Dialog.Title>
			<form onsubmit={handleSubmit} class="mt-4 space-y-6">
				<div class="space-y-2">
					<label
						for="list-title"
						class="text-sm leading-none font-medium text-gray-900 peer-disabled:cursor-not-allowed peer-disabled:opacity-70 dark:text-gray-100"
					>
						Title
					</label>
					<input
						id="list-title"
						type="text"
						bind:value={title}
						class="border-input bg-background ring-offset-background placeholder:text-muted-foreground focus-visible:ring-ring flex h-10 w-full rounded-md border px-3 py-2 text-sm file:border-0 file:bg-transparent file:text-sm file:font-medium focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:outline-none disabled:cursor-not-allowed disabled:opacity-50 dark:border-gray-600 dark:bg-gray-700 dark:text-white"
						placeholder="List title"
						minlength="3"
						maxlength="100"
						required
					/>
				</div>

				<div
					class="space-y-4 rounded-xl border border-gray-100 bg-gray-50/50 p-4 dark:border-gray-700 dark:bg-gray-900/30"
				>
					<div class="flex items-center justify-between">
						<label for="width" class="text-sm font-semibold text-gray-700 dark:text-gray-300">
							Width Mode
						</label>
						<div class="flex gap-1 rounded-lg bg-gray-100 p-1 dark:bg-gray-800">
							<button
								type="button"
								onclick={() => (width = null)}
								class="rounded-md px-3 py-1 text-xs font-medium transition-all {width === null
									? 'bg-white text-gray-900 shadow-sm dark:bg-gray-700 dark:text-white'
									: 'text-gray-500 hover:text-gray-700 dark:hover:text-gray-400'}"
							>
								Auto
							</button>
							<button
								type="button"
								onclick={() => (width = width || 300)}
								class="rounded-md px-3 py-1 text-xs font-medium transition-all {width !== null
									? 'bg-white text-gray-900 shadow-sm dark:bg-gray-700 dark:text-white'
									: 'text-gray-500 hover:text-gray-700 dark:hover:text-gray-400'}"
							>
								Custom
							</button>
						</div>
					</div>

					{#if width !== null}
						<div transition:slide={{ duration: 200 }} class="space-y-4">
							<div class="flex items-center gap-4">
								<input
									id="width"
									type="range"
									bind:value={width}
									min="100"
									max="600"
									step="10"
									class="h-1.5 flex-1 cursor-pointer appearance-none rounded-lg bg-gray-200 accent-gray-900 dark:bg-gray-700 dark:accent-gray-50"
								/>
								<div
									class="flex items-center gap-1 rounded-md border border-gray-200 bg-white px-2 py-1 dark:border-gray-600 dark:bg-gray-700"
								>
									<input
										type="number"
										bind:value={width}
										min="100"
										class="w-12 border-none bg-transparent p-0 text-right text-sm font-medium focus:ring-0 dark:text-white"
									/>
									<span class="text-xs text-gray-400">px</span>
								</div>
							</div>
							<p class="text-[0.8rem] text-gray-500 dark:text-gray-400">
								Adjust the width of this list.
							</p>
						</div>
					{/if}
				</div>

				<div class="flex justify-end gap-2">
					{#if list && onDelete}
						<Button.Root
							type="button"
							onclick={() => {
								onDelete(list.id);
								open = false;
							}}
							class="mr-auto inline-flex h-10 items-center justify-center rounded-md border border-red-200 bg-white px-4 py-2 text-sm font-medium text-red-600 shadow-sm transition-colors hover:bg-red-50 hover:text-red-700 focus-visible:ring-1 focus-visible:ring-red-950 focus-visible:outline-none disabled:pointer-events-none disabled:opacity-50 dark:border-red-900 dark:bg-gray-900 dark:text-red-400 dark:hover:bg-red-900/20"
						>
							Delete
						</Button.Root>
					{/if}
					<Button.Root
						type="button"
						onclick={() => (open = false)}
						class="inline-flex h-10 items-center justify-center rounded-md border border-gray-200 bg-white px-4 py-2 text-sm font-medium text-gray-900 shadow-sm transition-colors hover:bg-gray-100 hover:text-gray-900 focus-visible:ring-1 focus-visible:ring-gray-950 focus-visible:outline-none disabled:pointer-events-none disabled:opacity-50 dark:border-gray-800 dark:bg-gray-950 dark:text-gray-50 dark:hover:bg-gray-800 dark:hover:text-gray-50"
					>
						Cancel
					</Button.Root>
					<Button.Root
						type="submit"
						class="inline-flex h-10 items-center justify-center rounded-md bg-gray-900 px-4 py-2 text-sm font-medium text-gray-50 shadow transition-colors hover:bg-gray-900/90 focus-visible:ring-1 focus-visible:ring-gray-950 focus-visible:outline-none disabled:pointer-events-none disabled:opacity-50 dark:bg-gray-50 dark:text-gray-900 dark:hover:bg-gray-50/90"
					>
						{list ? 'Save Changes' : 'Create'}
					</Button.Root>
				</div>
			</form>
		</Dialog.Content>
	</Dialog.Portal>
</Dialog.Root>
