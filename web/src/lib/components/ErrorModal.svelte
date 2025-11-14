<script lang="ts">
	import { Dialog } from 'bits-ui';
	import { onDestroy } from 'svelte';
	
	export let isOpen = false;
	export let errorMessage = '';
	export let autoCloseTime: number | null = 5000;
	
	let timeout: NodeJS.Timeout | undefined;
	
	// Auto-close functionality
	$: if (isOpen && autoCloseTime) {
		timeout = setTimeout(() => {
			isOpen = false;
		}, autoCloseTime);
	}
	
	// Cleanup timeout when modal is closed or component destroyed
	$: if (!isOpen && timeout !== undefined) {
		clearTimeout(timeout);
		timeout = undefined;
	}
	
	onDestroy(() => {
		if (timeout !== undefined) {
			clearTimeout(timeout);
		}
	});
</script>

<Dialog.Root bind:open={isOpen}>
	<Dialog.Portal>
		<Dialog.Overlay class="data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0 fixed inset-0 z-50 bg-black/80" />
		<Dialog.Content class="fixed left-[50%] top-[50%] translate-x-[-50%] translate-y-[-50%] w-full max-w-md rounded-lg bg-white dark:bg-gray-800 p-8 shadow-lg data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0 data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95 duration-500 ease-out z-50">
			<div class="text-center">
				<!-- Error Icon with Animation -->
				<div class="mx-auto flex items-center justify-center h-16 w-16 rounded-full bg-red-100 dark:bg-red-900/20 mb-4 animate-pulse">
					<svg class="h-8 w-8 text-red-600 dark:text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"/>
					</svg>
				</div>
				
				<Dialog.Title class="text-lg font-semibold text-gray-900 dark:text-white mb-2 animate-fade-in">
					Something went wrong
				</Dialog.Title>
				<Dialog.Description class="text-sm text-gray-600 dark:text-gray-400 mb-6 animate-fade-in" style="animation-delay: 100ms">
					We apologize for the inconvenience. Please try again in a moment.
				</Dialog.Description>
				
				{#if errorMessage}
					<div class="mb-6 p-3 bg-gray-50 dark:bg-gray-700/50 rounded-lg animate-fade-in" style="animation-delay: 200ms">
						<p class="text-xs text-gray-500 dark:text-gray-400 font-mono">
							{errorMessage}
						</p>
					</div>
				{/if}
			</div>
		</Dialog.Content>
	</Dialog.Portal>
</Dialog.Root>
