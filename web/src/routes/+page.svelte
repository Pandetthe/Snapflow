<script lang="ts">
	import { Button } from 'bits-ui';

	let { data } = $props();
</script>
<svelte:head>
	<title>Snapflow | Simple kanban for teams</title>
</svelte:head>

<div class="min-h-screen flex items-center justify-center px-4 py-12">
	<div class="max-w-4xl w-full text-center">
		<!-- App Title with Animation -->
		<div class="mb-16 animate-in fade-in-0 slide-in-from-bottom-4 duration-700">
			<h1 class="text-7xl font-bold bg-linear-to-r from-blue-600 via-purple-600 to-blue-600 bg-clip-text text-transparent mb-6 leading-tight">
				Snapflow
			</h1>
			<p class="text-2xl text-gray-600 dark:text-gray-300 font-light max-w-2xl mx-auto leading-relaxed">
				Simple, open-source kanban application that helps teams collaborate effectively
			</p>
		</div>

		<!-- Reconstruction Warning with Enhanced Design -->
		<div class="bg-linear-to-r from-yellow-50 to-orange-50 dark:from-yellow-900/20 dark:to-orange-900/20 border border-yellow-200 dark:border-yellow-800 rounded-2xl p-8 mb-12 shadow-lg hover:shadow-xl transition-all duration-300 transform hover:-translate-y-1">
			<div class="flex items-center justify-center mb-6">
				<div class="relative">
					<div class="w-12 h-12 bg-yellow-100 dark:bg-yellow-900/30 rounded-full flex items-center justify-center animate-pulse">
						<svg class="w-7 h-7 text-yellow-600 dark:text-yellow-400" fill="currentColor" viewBox="0 0 20 20">
							<path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd"/>
						</svg>
					</div>
				</div>
				<h2 class="text-2xl font-bold text-yellow-800 dark:text-yellow-200 ml-4">
					Under Reconstruction
				</h2>
			</div>
			<p class="text-lg text-yellow-700 dark:text-yellow-300 mb-8 leading-relaxed max-w-2xl mx-auto">
				Our application is currently being rebuilt with exciting new features and improvements. 
				We're working hard to bring you a better experience with enhanced functionality and beautiful design.
			</p>
			
			<!-- Action Buttons with Enhanced Styling -->
			<div class="flex flex-col sm:flex-row gap-6 justify-center">
				{#if data?.isAuthenticated}
					<Button.Root href="/boards" class="px-8 py-4 inline-flex items-center justify-center border-2 border-green-300 dark:border-green-600 bg-linear-to-r from-green-50 to-green-100 dark:from-green-900/30 dark:to-green-800/30 text-green-700 dark:text-green-300 font-semibold rounded-xl transition-all duration-300 shadow-md hover:shadow-xl hover:scale-105 transform">
						<svg class="w-6 h-6 mr-3" fill="currentColor" viewBox="0 0 20 20">
							<path d="M3 3h14a1 1 0 011 1v12a1 1 0 01-1 1H3a1 1 0 01-1-1V4a1 1 0 011-1zm2 3v8h4V6H5zm6 0v8h4V6h-4z" />
						</svg>
						Check my boards
					</Button.Root>
				{:else}
					<Button.Root href="/sign-up" class="px-8 py-4 inline-flex items-center justify-center border-2 border-blue-300 dark:border-blue-600 bg-linear-to-r from-blue-50 to-blue-100 dark:from-blue-900/30 dark:to-blue-800/30 text-blue-700 dark:text-blue-300 font-semibold rounded-xl transition-all duration-300 shadow-md hover:shadow-xl hover:scale-105 transform">
						<svg class="w-6 h-6 mr-3" fill="currentColor" viewBox="0 0 20 20">
							<path d="M8 9a3 3 0 100-6 3 3 0 000 6zM8 11a6 6 0 016 6H2a6 6 0 016-6zM16 7a1 1 0 10-2 0v1h-1a1 1 0 100 2h1v1a1 1 0 102 0v-1h1a1 1 0 100-2h-1V7z"/>
						</svg>
						Create test account
					</Button.Root>
					<Button.Root href="/sign-in" class="px-8 py-4 inline-flex items-center justify-center border-2 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 font-semibold rounded-xl transition-all duration-300 shadow-md hover:shadow-xl hover:scale-105 transform">
						<svg class="w-6 h-6 mr-3" fill="currentColor" viewBox="0 0 20 20">
							<path fill-rule="evenodd" d="M3 3a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm3 4a1 1 0 011-1h6a1 1 0 110 2H7a1 1 0 01-1-1zm0 4a1 1 0 011-1h6a1 1 0 110 2H7a1 1 0 01-1-1zm0 4a1 1 0 011-1h6a1 1 0 110 2H7a1 1 0 01-1-1z" clip-rule="evenodd"/>
						</svg>
						Sign in
					</Button.Root>
				{/if}
			</div>
		</div>

		<!-- Additional Info with Cards -->
		<div class="grid md:grid-cols-2 gap-6 max-w-3xl mx-auto">
			<div class="bg-white dark:bg-gray-800 rounded-xl p-6 shadow-md hover:shadow-lg transition-all duration-300">
				<div class="flex items-center mb-4">
					<div class="w-10 h-10 bg-blue-100 dark:bg-blue-900/30 rounded-lg flex items-center justify-center">
						<svg class="w-6 h-6 text-blue-600 dark:text-blue-400" fill="currentColor" viewBox="0 0 20 20">
							<path d="M9 4.804A7.968 7.968 0 005.5 4c-1.255 0-2.443.29-3.5.804v10A7.969 7.969 0 015.5 14c1.669 0 3.218.51 4.5 1.385A7.962 7.962 0 0114.5 14c1.255 0 2.443.29 3.5.804v-10A7.968 7.968 0 0014.5 4c-1.255 0-2.443.29-3.5.804V12a1 1 0 11-2 0V4.804z"/>
						</svg>
					</div>
					<h3 class="text-lg font-semibold text-gray-900 dark:text-white ml-3">
						Documentation
					</h3>
				</div>
				<p class="text-gray-600 dark:text-gray-300 leading-relaxed">
					Help us document our application to help other users understand how to use it.
				</p>
			</div>

			<div class="bg-white dark:bg-gray-800 rounded-xl p-6 shadow-md hover:shadow-lg transition-all duration-300">
				<div class="flex items-center mb-4">
					<div class="w-10 h-10 bg-green-100 dark:bg-green-900/30 rounded-lg flex items-center justify-center">
						<svg class="w-6 h-6 text-green-600 dark:text-green-400" fill="currentColor" viewBox="0 0 24 24">
							<path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z"/>
						</svg>
					</div>
					<h3 class="text-lg font-semibold text-gray-900 dark:text-white ml-3">
						Contribute
					</h3>
				</div>
				<p class="text-gray-600 dark:text-gray-300 leading-relaxed">
					Help us improve Snapflow by reporting issues or contributing to our codebase.
				</p>
			</div>
		</div>
	</div>
</div>
