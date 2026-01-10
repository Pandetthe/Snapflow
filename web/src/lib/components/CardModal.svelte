<script lang="ts">
	import { Dialog, Button } from 'bits-ui';
	import type { GetBoardByIdResponse } from '$lib/types/boards.api';

	let {
		open = $bindable(false),
		card = $bindable(undefined),
		onConfirm,
		onDelete
	}: {
		open: boolean;
		card?: GetBoardByIdResponse.CardDto;
		onConfirm: (title: string, description: string) => void;
		onDelete?: (id: number) => void;
	} = $props();

	let title = $state('');
	let description = $state('');

	$effect(() => {
		if (open) {
			if (card) {
				title = card.title;
				description = card.description || '';
			} else {
				title = '';
				description = '';
			}
		}
	});

	function handleSubmit(e: SubmitEvent) {
		e.preventDefault();
		onConfirm(title, description);
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
				{card ? 'Edit Card' : 'Create Card'}
			</Dialog.Title>
			<form onsubmit={handleSubmit} class="mt-4 space-y-4">
				<div class="space-y-2">
					<label
						for="card-title"
						class="text-sm leading-none font-medium text-gray-900 peer-disabled:cursor-not-allowed peer-disabled:opacity-70 dark:text-gray-100"
					>
						Title
					</label>
					<input
						id="card-title"
						type="text"
						bind:value={title}
						class="border-input bg-background ring-offset-background placeholder:text-muted-foreground focus-visible:ring-ring flex h-10 w-full rounded-md border px-3 py-2 text-sm file:border-0 file:bg-transparent file:text-sm file:font-medium focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:outline-none disabled:cursor-not-allowed disabled:opacity-50 dark:border-gray-600 dark:bg-gray-700 dark:text-white"
						placeholder="Card title"
						minlength="3"
						maxlength="50"
						required
					/>
				</div>
				<div class="space-y-2">
					<label
						for="card-description"
						class="text-sm leading-none font-medium text-gray-900 peer-disabled:cursor-not-allowed peer-disabled:opacity-70 dark:text-gray-100"
					>
						Description
					</label>
					<textarea
						id="card-description"
						bind:value={description}
						class="border-input bg-background ring-offset-background placeholder:text-muted-foreground focus-visible:ring-ring flex min-h-[80px] w-full rounded-md border px-3 py-2 text-sm focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:outline-none disabled:cursor-not-allowed disabled:opacity-50 dark:border-gray-600 dark:bg-gray-700 dark:text-white"
						placeholder="Card description"
						maxlength="1000"
					></textarea>
				</div>
				<div class="flex justify-end gap-2">
					{#if card && onDelete}
						<Button.Root
							type="button"
							onclick={() => {
								onDelete(card.id);
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
						{card ? 'Save Changes' : 'Create'}
					</Button.Root>
				</div>
			</form>
		</Dialog.Content>
	</Dialog.Portal>
</Dialog.Root>
