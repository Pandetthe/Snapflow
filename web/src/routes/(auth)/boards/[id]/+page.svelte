<script lang="ts">
	import { Button } from 'bits-ui';
	import type { Board } from '$lib/services/boards';

	let { data } = $props<{ data: { board: Board } }>();

	function formatDate(value: string) {
		return new Date(value).toLocaleDateString();
	}

	function formatDateTime(value: string) {
		return new Date(value).toLocaleString();
	}
</script>

<div class="p-8">
	<div class="max-w-4xl mx-auto">
		<!-- Back button -->
		<Button.Root 
			href="/boards"
			class="mb-6 inline-flex items-center text-sm text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white transition-colors"
		>
			<svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
				<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
			</svg>
			Back to boards
		</Button.Root>

		<!-- Board header -->
		<div class="bg-white dark:bg-gray-800 rounded-xl p-6 shadow-md mb-6">
			<div class="flex items-start justify-between mb-4">
				<div class="flex-1">
					<h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-2">
						{data.board.title}
					</h1>
					<span class="text-sm text-gray-500 dark:text-gray-400">Board #{data.board.id}</span>
				</div>
			</div>

			{#if data.board.description}
				<p class="text-gray-600 dark:text-gray-300 mb-6">
					{data.board.description}
				</p>
			{/if}

			<!-- Board metadata -->
			<div class="grid grid-cols-1 md:grid-cols-2 gap-4 pt-4 border-t border-gray-200 dark:border-gray-700">
				<div>
					<p class="text-xs text-gray-500 dark:text-gray-400 mb-1">Created by</p>
					<p class="text-sm font-medium text-gray-900 dark:text-white">
						{data.board.createdBy.userName}
					</p>
					<p class="text-xs text-gray-500 dark:text-gray-400 mt-1">
						{formatDateTime(data.board.createdAt)}
					</p>
				</div>
				{#if data.board.updatedAt && data.board.updatedBy}
					<div>
						<p class="text-xs text-gray-500 dark:text-gray-400 mb-1">Last updated by</p>
						<p class="text-sm font-medium text-gray-900 dark:text-white">
							{data.board.updatedBy.userName}
						</p>
						<p class="text-xs text-gray-500 dark:text-gray-400 mt-1">
							{formatDateTime(data.board.updatedAt)}
						</p>
					</div>
				{/if}
			</div>
		</div>

		<!-- Board content placeholder -->
		<div class="bg-white dark:bg-gray-800 rounded-xl p-6 shadow-md">
			<p class="text-gray-600 dark:text-gray-400 text-center py-8">
				Board content will be displayed here
			</p>
		</div>
	</div>
</div>

