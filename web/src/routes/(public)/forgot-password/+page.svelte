<script lang="ts">
	import { Button, Dialog } from 'bits-ui';
	import { authConfig } from '$lib/config/auth';
	import { AuthService } from '$lib/services/auth';
	import { errorStore } from '$lib/stores/error';
	import { apiClient } from '$lib/services/api.client.ts';

	let email = $state('');
	let isLoading = $state(false);
	let emailError = $state('');
  let showForgotPasswordInfoModal = $state(false);
  const forgotPasswordInfoTitle = 'Password Reset Requested';
  const forgotPasswordInfoMessage = 'If an account exists for the provided email, you will receive an email with instructions to reset your password shortly. Please check your inbox and spam folder.';

	const authService = new AuthService(apiClient);

	function validateEmailField(value: string) {
		emailError = '';

		if (!value) {
			emailError = 'Email is required';
			return;
		}

		if (value.length > authConfig.email.maxLength) {
			emailError = `Email must be less than ${authConfig.email.maxLength} characters`;
			return;
		}

		const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
		if (!emailRegex.test(value)) {
			emailError = 'Please enter a valid email address';
		}
	}

	async function handleForgotPassword(e: Event) {
		e.preventDefault();
		emailError = '';
		if (!email) {
			emailError = 'Email is required';
			return;
		}

		if (email.length > authConfig.email.maxLength) {
			emailError = `Email must be less than ${authConfig.email.maxLength} characters`;
			return;
		}

		const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
		if (!emailRegex.test(email)) {
			emailError = 'Please enter a valid email address';
			return;
		}

		isLoading = true;

		try {
			await authService.forgotPassword({ email });
			showForgotPasswordInfoModal = true;
    } catch (err) {
      if (err instanceof Error) {
        if (err.message === 'Failed to fetch') {
          errorStore.addError("Web.ConnectionProblem", 'Problem with connection to the server');
        }
        else {
          errorStore.addError(err.name, err.message);
        }
      } else {
        errorStore.addError(null, 'Unknown error occurred during password reset');
      }
    } finally {
      isLoading = false;
    }
	}
</script>

<svelte:head>
	<title>Snapflow | Forgot password</title>
</svelte:head>

<div class="flex min-h-screen items-center justify-center px-4 py-12">
	<div class="w-full max-w-md">
		<div class="rounded-2xl bg-white p-8 shadow-xl dark:bg-gray-800">
			<div class="mb-8 text-center">
				<h1 class="mb-2 text-3xl font-bold text-gray-900 dark:text-white">Reset your password</h1>
				<p class="text-gray-600 dark:text-gray-400">
					Enter the email associated with your account and we'll send you a reset link.
				</p>
			</div>

			<form onsubmit={handleForgotPassword} novalidate class="space-y-4">
				<div class="space-y-1">
					<label for="email" class="block text-xs font-medium text-gray-700 dark:text-gray-300">
						Email address
					</label>
					<input
						id="email"
						name="email"
						type="email"
						autocomplete="email"
						bind:value={email}
						placeholder="Enter your email"
						class="w-full rounded-lg border border-gray-300 bg-white px-3 py-2.5 text-sm text-gray-900 placeholder-gray-500 transition-all duration-200 focus:border-transparent focus:ring-2 focus:ring-blue-500 dark:border-gray-600 dark:bg-gray-700 dark:text-white dark:placeholder-gray-400"
						maxlength={authConfig.email.maxLength}
						oninput={(e) => validateEmailField(e.currentTarget.value)}
					/>
					<div
						class={`overflow-hidden transition-all duration-300 ${emailError ? 'mt-2 max-h-96' : 'max-h-0'}`}
					>
						<p class="mt-1 text-xs text-red-600 dark:text-red-400">{emailError}</p>
					</div>
				</div>

				<Button.Root
					type="submit"
					disabled={isLoading || !email || !!emailError}
					class="flex w-full items-center justify-center rounded-lg bg-blue-600 px-4 py-2.5 text-sm font-semibold text-white shadow-md transition-all duration-200 hover:bg-blue-700 hover:shadow-lg disabled:cursor-not-allowed disabled:bg-blue-400"
				>
					{#if isLoading}
						<svg
							class="mr-2 h-4 w-4 animate-spin text-white"
							xmlns="http://www.w3.org/2000/svg"
							fill="none"
							viewBox="0 0 24 24"
						>
							<circle
								class="opacity-25"
								cx="12"
								cy="12"
								r="10"
								stroke="currentColor"
								stroke-width="4"
							></circle>
							<path
								class="opacity-75"
								fill="currentColor"
								d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
							></path>
						</svg>
						<span>Sending</span>
						<span class="inline-flex">
							<span class="animate-dots-bounce" style="animation-delay: 0ms">.</span>
							<span class="animate-dots-bounce" style="animation-delay: 150ms">.</span>
							<span class="animate-dots-bounce" style="animation-delay: 300ms">.</span>
						</span>
					{:else}
						<span>Send reset link</span>
					{/if}
				</Button.Root>
			</form>

			<div class="mt-8 text-center">
				<p class="text-gray-600 dark:text-gray-400">
					Remember your password?
					<a
						href="/sign-in"
						class="font-semibold text-blue-600 transition-colors duration-200 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300"
					>
						Sign in
					</a>
				</p>
			</div>
		</div>

		<div class="mt-6 text-center">
			<a
				href="/"
				class="text-sm text-gray-500 transition-colors duration-200 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300"
			>
				← Back to Home
			</a>
		</div>
	</div>
</div>


<Dialog.Root bind:open={showForgotPasswordInfoModal}>
	<Dialog.Portal>
		<Dialog.Overlay
			class="fixed inset-0 z-50 bg-black/60 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:animate-in data-[state=open]:fade-in-0"
		/>
		<Dialog.Content
			class="fixed top-1/2 left-1/2 z-50 w-full max-w-md -translate-x-1/2 -translate-y-1/2 rounded-lg bg-white p-6 shadow-lg duration-300 ease-out data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:zoom-out-95 data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=open]:zoom-in-95 dark:bg-gray-800"
		>
			<div class="space-y-4 text-center">
				<div
					class="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-blue-100 dark:bg-blue-900/30"
				>
					<svg
						class="h-6 w-6 text-blue-600 dark:text-blue-400"
						fill="none"
						viewBox="0 0 24 24"
						stroke="currentColor"
					>
						<path
							stroke-linecap="round"
							stroke-linejoin="round"
							stroke-width="2"
							d="M13 16h-1v-4h-1m1-4h.01M12 2a10 10 0 100 20 10 10 0 000-20z"
						/>
					</svg>
				</div>
				<Dialog.Title class="text-lg font-semibold text-gray-900 dark:text-white">
					{forgotPasswordInfoTitle}
				</Dialog.Title>
				<Dialog.Description class="text-sm text-gray-600 dark:text-gray-300">
					{forgotPasswordInfoMessage}
				</Dialog.Description>
				<div class="mt-4 flex justify-center">
					<Button.Root
						onclick={() => (showForgotPasswordInfoModal = false)}
						class="inline-flex h-9 items-center justify-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
					>
						OK
					</Button.Root>
				</div>
			</div>
		</Dialog.Content>
	</Dialog.Portal>
</Dialog.Root>