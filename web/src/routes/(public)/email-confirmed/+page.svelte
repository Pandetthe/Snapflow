<script lang="ts">
	import { Button } from 'bits-ui';
</script>

<svelte:head>
	<title>Snapflow | Email confirmed</title>
</svelte:head>

<div class="flex min-h-screen items-center justify-center px-4 py-12">
	<div class="w-full max-w-md">
		<div class="rounded-2xl bg-white p-8 shadow-xl dark:bg-gray-800">
			<div class="mb-6 text-center">
				<div class="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-green-100 dark:bg-green-900/20">
					<svg
						class="h-8 w-8 text-green-600 dark:text-green-400"
						fill="currentColor"
						viewBox="0 0 20 20"
					>
						<path
							fill-rule="evenodd"
							d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
							clip-rule="evenodd"
						/>
					</svg>
				</div>

				<h1 class="mb-2 text-3xl font-bold text-gray-900 dark:text-white">Email confirmed!</h1>
				<p class="text-gray-600 dark:text-gray-400">
					Your email address has been successfully confirmed.
				</p>
			</div>

			<div class="space-y-4">
				<p class="text-sm text-gray-600 dark:text-gray-400">
					You can now sign in with your account.
				</p>

				<div class="mt-4 flex gap-3">
					<Button.Root
						href="/sign-in"
						class="inline-flex h-9 w-full items-center justify-center rounded-md bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700"
					>
						Sign in
					</Button.Root>

					<Button.Root
						href="/"
						class="inline-flex h-9 w-full items-center justify-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
					>
						Home
					</Button.Root>
				</div>
			</div>
		</div>
	</div>
</div>