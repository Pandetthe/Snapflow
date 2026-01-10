<script lang="ts">
	import { Dialog, Button } from 'bits-ui';
	import { slide } from 'svelte/transition';
	import type { GetBoardByIdResponse } from '$lib/types/boards.api';

	let {
		open = $bindable(false),
		swimlane = $bindable(undefined),
		onConfirm,
		onDelete
	}: {
		open: boolean;
		swimlane?: GetBoardByIdResponse.SwimlaneDto;
		onConfirm: (title: string, height: number | null) => void;
		onDelete?: (id: number) => void;
	} = $props();

	let title = $state('');
	let height: number | null = $state(null);

	$effect(() => {
		if (open) {
			if (swimlane) {
				title = swimlane.title;
				height = swimlane.height;
			} else {
				title = '';
				height = null;
			}
		}
	});

	function handleSubmit(e: SubmitEvent) {
		e.preventDefault();
		onConfirm(title, height);
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
				{swimlane ? 'Edit Swimlane' : 'Create Swimlane'}
			</Dialog.Title>
			<form onsubmit={handleSubmit} class="mt-4 space-y-4">
				<div class="space-y-2">
					<label
						for="title"
						class="text-sm leading-none font-medium text-gray-900 peer-disabled:cursor-not-allowed peer-disabled:opacity-70 dark:text-gray-100"
					>
						Title
					</label>
					<input
						id="swimlane-title"
						type="text"
						bind:value={title}
						class="border-input bg-background ring-offset-background placeholder:text-muted-foreground focus-visible:ring-ring flex h-10 w-full rounded-md border px-3 py-2 text-sm file:border-0 file:bg-transparent file:text-sm file:font-medium focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:outline-none disabled:cursor-not-allowed disabled:opacity-50 dark:border-gray-600 dark:bg-gray-700 dark:text-white"
						placeholder="Swimlane title"
						minlength="3"
						maxlength="100"
						required
					/>
				</div>
				<div
					class="space-y-4 rounded-xl border border-gray-100 bg-gray-50/50 p-4 dark:border-gray-700 dark:bg-gray-900/30"
				>
					<div class="flex items-center justify-between">
						<label for="height" class="text-sm font-semibold text-gray-700 dark:text-gray-300">
							Height Mode
						</label>
						<div class="flex gap-1 rounded-lg bg-gray-100 p-1 dark:bg-gray-800">
							<button
								type="button"
								onclick={() => (height = null)}
								class="rounded-md px-3 py-1 text-xs font-medium transition-all {height === null
									? 'bg-white text-gray-900 shadow-sm dark:bg-gray-700 dark:text-white'
									: 'text-gray-500 hover:text-gray-700 dark:hover:text-gray-400'}"
							>
								Auto
							</button>
							<button
								type="button"
								onclick={() => (height = height || 400)}
								class="rounded-md px-3 py-1 text-xs font-medium transition-all {height !== null
									? 'bg-white text-gray-900 shadow-sm dark:bg-gray-700 dark:text-white'
									: 'text-gray-500 hover:text-gray-700 dark:hover:text-gray-400'}"
							>
								Custom
							</button>
						</div>
					</div>

					{#if height !== null}
						<div transition:slide={{ duration: 200 }} class="space-y-4">
							<div class="flex items-center gap-4">
								<input
									id="height"
									type="range"
									bind:value={height}
									min="100"
									max="1200"
									step="20"
									class="h-1.5 flex-1 cursor-pointer appearance-none rounded-lg bg-gray-200 accent-gray-900 dark:bg-gray-700 dark:accent-gray-50"
								/>
								<div
									class="flex items-center gap-1 rounded-md border border-gray-200 bg-white px-2 py-1 dark:border-gray-600 dark:bg-gray-700"
								>
									<input
										type="number"
										bind:value={height}
										min="100"
										class="w-12 border-none bg-transparent p-0 text-right text-sm font-medium focus:ring-0 dark:text-white"
									/>
									<span class="text-xs text-gray-400">px</span>
								</div>
							</div>
							<p class="text-[0.8rem] text-gray-500 dark:text-gray-400">
								Adjust the vertical space for this swimlane.
							</p>
						</div>
					{/if}
				</div>
				<div class="flex justify-end gap-2">
					{#if swimlane && onDelete}
						<Button.Root
							type="button"
							onclick={() => {
								onDelete(swimlane.id);
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
						{swimlane ? 'Save Changes' : 'Create'}
					</Button.Root>
				</div>
			</form>
		</Dialog.Content>
	</Dialog.Portal>
</Dialog.Root>
