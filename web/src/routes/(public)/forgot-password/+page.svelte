<script lang="ts">
	import { Button } from 'bits-ui';
	import { authConfig } from '$lib/config/auth';
	import { authService } from '$lib/services/auth';
	
	let email = '';
	let isLoading = false;
	let error = '';
	let success = false;

	async function handleForgotPassword() {
		error = '';
		success = false;
		
		// Email validation
		if (!email) {
			error = 'Email is required';
			return;
		}
		
		if (email.length > authConfig.email.maxLength) {
			error = `Email must be less than ${authConfig.email.maxLength} characters`;
			return;
		}
		
		const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
		if (!emailRegex.test(email)) {
			error = 'Please enter a valid email address';
			return;
		}
		
		isLoading = true;
		
		try {
			await authService.forgotPassword(email);
			console.log('Password reset email sent to:', email);
			success = true;
		} catch (err) {
			error = err instanceof Error ? err.message : 'Failed to send password reset email. Please try again.';
		} finally {
			isLoading = false;
		}
	}
</script>
<svelte:head>
	<title>Snapflow | Forgot password</title>
</svelte:head>

<div class="min-h-screen flex items-center justify-center px-4 py-12 bg-gray-500/1">
	<div class="w-full max-w-md">
		<!-- Forgot Password Card -->
		<div class="bg-white dark:bg-gray-800 rounded-2xl shadow-xl p-8">
			<!-- Header -->
			<div class="text-center mb-8">
				<div class="w-16 h-16 bg-blue-100 dark:bg-blue-900/30 rounded-full flex items-center justify-center mx-auto mb-4">
					<svg class="w-8 h-8 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
						<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"/>
					</svg>
				</div>
				<h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-2">
					Forgot your password?
				</h1>
				<p class="text-gray-600 dark:text-gray-400">
					No worries, we'll send you reset instructions.
				</p>
			</div>

			<!-- Error Message -->
			{#if error}
				<div class="mb-6 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg">
					<p class="text-red-600 dark:text-red-400 text-sm">{error}</p>
				</div>
			{/if}

			<!-- Success Message -->
			{#if success}
				<div class="mb-6 p-4 bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-lg">
					<p class="text-green-600 dark:text-green-400 text-sm">
						Password reset instructions have been sent to your email address.
					</p>
				</div>
			{/if}

			<!-- Forgot Password Form -->
			{#if !success}
				<form on:submit|preventDefault={handleForgotPassword} class="space-y-6">
					<!-- Email Field -->
					<div>
						<label for="email" class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
							Email Address
						</label>
						<input
							id="email"
							type="email"
							bind:value={email}
							placeholder="Enter your email address"
							class="w-full px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-200"
							maxlength={authConfig.email.maxLength}
							required
						/>
						<p class="mt-1 text-xs text-gray-500 dark:text-gray-400">
							We'll send you a link to reset your password.
						</p>
					</div>

					<!-- Send Reset Link Button -->
					<Button.Root
						type="submit"
						disabled={isLoading || !email}
						class="w-full px-6 py-3 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white font-semibold rounded-lg transition-all duration-200 shadow-md hover:shadow-lg disabled:cursor-not-allowed flex items-center justify-center"
					>
						{#if isLoading}
							<svg class="animate-spin h-5 w-5 text-white mr-2" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
								<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
								<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
							</svg>
							<span>Sending</span>
							<span class="inline-flex">
								<span class="animate-bounce-custom" style="animation-delay: 0ms">.</span>
								<span class="animate-bounce-custom" style="animation-delay: 500ms">.</span>
								<span class="animate-bounce-custom" style="animation-delay: 1000ms">.</span>
							</span>
						{:else}
							<span>Send Reset Link</span>
						{/if}
					</Button.Root>
				</form>
			{:else}
				<!-- Back to Sign In Button (shown after success) -->
				<Button.Root
					href="/signin"
					class="w-full px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-lg transition-all duration-200 shadow-md hover:shadow-lg"
				>
					Back to Sign In
				</Button.Root>
			{/if}

			<!-- Sign In Link -->
			<div class="mt-8 text-center">
				<p class="text-gray-600 dark:text-gray-400">
					Remember your password?
					<a href="/signin" class="text-blue-600 dark:text-blue-400 hover:text-blue-700 dark:hover:text-blue-300 font-semibold transition-colors duration-200">
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
