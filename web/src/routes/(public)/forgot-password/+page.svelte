<script lang="ts">
	import { Button } from 'bits-ui';
	import { authConfig } from '$lib/config/auth';
	import { authService } from '$lib/services/auth';
	import ErrorModal from '$lib/components/ErrorModal.svelte';

	let email = $state('');
	let isLoading = $state(false);
	let emailError = $state('');
	let success = $state(false);

	let showErrorModal = $state(false);
	let modalErrorMessage = $state('');

	function validateEmailField(value: string) {
		success = false;
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
		success = false;
		modalErrorMessage = '';
		showErrorModal = false;

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
			await authService.forgotPassword(email);
			success = true;
		} catch (err) {
			modalErrorMessage =
				err instanceof Error
					? err.message
					: 'Failed to send password reset email. Please try again.';
			showErrorModal = true;
		} finally {
			isLoading = false;
		}
	}
</script>
<svelte:head>
	<title>Snapflow | Forgot password</title>
</svelte:head>

<div class="min-h-screen flex items-center justify-center px-4 py-12">
	<div class="w-full max-w-md">
		<!-- Forgot Password Card -->
		<div class="bg-white dark:bg-gray-800 rounded-2xl shadow-xl p-8">
			<!-- Header -->
			<div class="text-center mb-8">
				<h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-2">
					Reset your password
				</h1>
				<p class="text-gray-600 dark:text-gray-400">
					Enter the email associated with your account and we'll send you a reset link.
				</p>
			</div>

			<!-- Success Message -->
			{#if success}
				<div class="mb-6 p-3 bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-lg">
					<p class="text-sm text-green-700 dark:text-green-400">
						If an account exists for <span class="font-semibold">{email}</span>, you'll receive password reset instructions shortly.
					</p>
				</div>
			{/if}

			<!-- Forgot Password Form -->
			<form onsubmit={handleForgotPassword} novalidate class="space-y-4">
				<!-- Email Field -->
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
						class="w-full px-3 py-2.5 text-sm border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-200"
						maxlength={authConfig.email.maxLength}
						oninput={(e) => validateEmailField(e.currentTarget.value)}
					/>
					<div class={`overflow-hidden transition-all duration-300 ${emailError ? 'max-h-96 mt-2' : 'max-h-0'}`}>
						<p class="mt-1 text-xs text-red-600 dark:text-red-400">{emailError}</p>
					</div>
				</div>

				<!-- Send Reset Link Button -->
				<Button.Root
					type="submit"
					disabled={isLoading || !email || !!emailError}
					class="w-full px-4 py-2.5 text-sm bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white font-semibold rounded-lg transition-all duration-200 shadow-md hover:shadow-lg disabled:cursor-not-allowed flex items-center justify-center"
				>
					{#if isLoading}
						<svg class="animate-spin h-4 w-4 text-white mr-2" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
							<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
							<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
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

			<!-- Sign In Link -->
			<div class="mt-8 text-center">
				<p class="text-gray-600 dark:text-gray-400">
					Remember your password?
					<a href="/sign-in" class="text-blue-600 dark:text-blue-400 hover:text-blue-700 dark:hover:text-blue-300 font-semibold transition-colors duration-200">
						Sign in
					</a>
				</p>
			</div>
		</div>

		<!-- Back to Home -->
		<div class="mt-6 text-center">
			<a href="/" class="text-sm text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 transition-colors duration-200">
				← Back to Home
			</a>
		</div>
	</div>
</div>

<ErrorModal bind:isOpen={showErrorModal} errorMessage={modalErrorMessage} />
