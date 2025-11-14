<svelte:head>
	{#if $page.status === 404}
		<title>Snapflow | Page not found</title>
	{:else}
		<title>Snapflow | Error</title>
	{/if}
</svelte:head>

<script lang="ts">
	import { page } from '$app/stores';
	import { Button } from 'bits-ui';

	let error = $page.status;
</script>

<div class="min-h-screen bg-gray-100 dark:bg-gray-900 flex items-center justify-center px-4">
	<div class="max-w-lg w-full text-center">
		<!-- Error Graphics -->
		<div>
			{#if error === 404}
				<!-- 404 Specific Graphics -->
					<div class="relative w-full max-w-2xl mx-auto">
						<div class="relative">
							<div class="flex items-center justify-center gap-4">
								<span class="text-[10rem] font-black bg-linear-to-r from-blue-600 via-purple-600 to-blue-600 bg-clip-text text-transparent">404</span>
							</div>
						</div>
					</div>
			{:else}
				<!-- General Error Graphics -->
				<div class="flex justify-center mb-8">
					<div class="relative">
						<div class="w-32 h-32 bg-red-100 dark:bg-red-900/20 rounded-full flex items-center justify-center">
							<svg class="w-16 h-16 text-red-500 dark:text-red-400" fill="currentColor" viewBox="0 0 24 24">
								<path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z"/>
							</svg>
						</div>
						<div class="absolute -top-2 -right-2 w-8 h-8 bg-red-500 rounded-full flex items-center justify-center">
							<svg class="w-5 h-5 text-white" fill="currentColor" viewBox="0 0 20 20">
								<path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd"/>
							</svg>
						</div>
					</div>
				</div>
			{/if}
		</div>

		<!-- Error Message -->
		<div class="mb-10">
			{#if error === 404}
				<h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-4">
					Page Not Found
				</h1>
				<p class="text-lg text-gray-600 dark:text-gray-300 mb-2">
					Oops! The page you're looking for seems to have vanished.
				</p>
				<p class="text-gray-500 dark:text-gray-400">
					Don't worry, even the best kanban boards sometimes have missing cards!
				</p>
			{:else}
				<h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-4">
					Something went wrong
				</h1>
				<p class="text-lg text-gray-600 dark:text-gray-300 mb-2">
					We encountered an unexpected error.
				</p>
			{/if}
		</div>

		<!-- Action Buttons -->
		<div class="flex flex-col sm:flex-row gap-4 justify-center">
			<Button.Root href="/" class="px-6 py-3 inline-flex items-center justify-center border border-blue-300 dark:border-blue-600 bg-blue-50 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300 font-medium rounded-lg transition-colors duration-200 shadow-sm hover:shadow-md hover:bg-blue-100 dark:hover:bg-blue-900/50">
				<svg class="w-5 h-5 mr-2" fill="currentColor" viewBox="0 0 20 20">
					<path d="M10.707 2.293a1 1 0 00-1.414 0l-7 7a1 1 0 001.414 1.414L4 10.414V17a1 1 0 001 1h2a1 1 0 001-1v-2a1 1 0 011-1h2a1 1 0 011 1v2a1 1 0 001 1h2a1 1 0 001-1v-6.586l.293.293a1 1 0 001.414-1.414l-7-7z"/>
				</svg>
				Go Home
			</Button.Root>
			
			{#if error !== 404}
				<Button.Root 
					href="https://github.com/Pandetthe/Snapflow/issues" 
					target="_blank"
					class="px-6 py-3 inline-flex items-center justify-center border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 font-medium rounded-lg transition-colors duration-200 shadow-sm hover:shadow-md hover:bg-gray-50 dark:hover:bg-gray-700"
				>
					<svg class="w-5 h-5 mr-2" fill="currentColor" viewBox="0 0 24 24">
						<path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z"/>
					</svg>
					Report Issue
				</Button.Root>
			{/if}
		</div>
	</div>
</div>
