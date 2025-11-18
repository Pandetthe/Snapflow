<script lang="ts">
	import { Button, Toggle, Dialog } from 'bits-ui';
	import { authConfig } from '$lib/config/auth';
	import { authService } from '$lib/services/auth';
	import type { PropertyValidationError, ProblemDetails } from '$lib/types/api';
	import ErrorModal from '$lib/components/ErrorModal.svelte';

	let email = $state('');
	let password = $state('');
	let isLoading = $state(false);
	let showPassword = $state(false);
	let emailError = $state('');
	let passwordError = $state('');

	// Modal states
	let showErrorModal = $state(false);
	let modalErrorMessage = $state('');

	// Special sign-in info modal (for specific sign-in codes)
	let showSignInInfoModal = $state(false);
	let signInInfoTitle = $state('');
	let signInInfoMessage = $state('');

	// Sign-in special error code mapping (title + message)
	const signInInfoByCode: Record<string, { title: string; message: string }> = {
		'Users.SignIn.Failed': {
			title: 'Sign in failed',
			message: 'The sign-in attempt failed. Please check your credentials and try again.'
		},
		'Users.SignIn.LockedOut': {
			title: 'Account locked out',
			message: 'Your account has been locked out. Please try again later or contact support.'
		},
		'Users.SignIn.NotAllowed': {
			title: 'Sign in not allowed',
			message:
				'Sign in is not allowed for this account. You need to verify your email address first.'
		},
		'Users.SignIn.TwoFactorRequired': {
			title: 'Two-factor authentication required',
			message:
				'Two-factor authentication is required. Please sign in using your two-factor authentication method.'
		}
	};

	function tryHandleNonValidationSignInError(problem: ProblemDetails): boolean {
		if (problem.title && signInInfoByCode[problem.title]) {
			const info = signInInfoByCode[problem.title];
			signInInfoTitle = info.title;
			signInInfoMessage = info.message;
			showSignInInfoModal = true;
			return true;
		}
		return false;
	}

	// Function to handle validation errors from API
	function handleValidationErrors(errors: PropertyValidationError[]) {
		// Clear previous errors
		emailError = '';
		passwordError = '';

		// Group errors by field
		const fieldErrors: { [key: string]: string[] } = {};
		const generalErrors: string[] = [];

		errors.forEach((err) => {
			if (err.propertyName) {
				const fieldName = err.propertyName.toLowerCase();
				if (!fieldErrors[fieldName]) {
					fieldErrors[fieldName] = [];
				}
				fieldErrors[fieldName].push(err.description);
			} else {
				generalErrors.push(err.description);
			}
		});

		// Set field-specific errors
		if (fieldErrors.email) {
			emailError = fieldErrors.email.join('. ');
		}
		if (fieldErrors.password) {
			passwordError = fieldErrors.password.join('. ');
		}

		// Set general errors in modal
		if (generalErrors.length > 0) {
			modalErrorMessage = generalErrors.join('. ');
			showErrorModal = true;
		}
	}

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

	function validatePasswordField() {
		passwordError = '';
	}

	async function handleSignin(e: Event) {
		e.preventDefault();
		emailError = '';
		passwordError = '';
		
		// Basic validation
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
		
		if (!password) {
			passwordError = 'Password is required';
			return;
		}
		
		isLoading = true;
		const response = await authService.signin({ email, password });		
		if (response.ok) {
			window.location.href = '/boards';
		} else {
			if ('errors' in response && response.errors && Array.isArray(response.errors)) {
				handleValidationErrors(response.errors);
			} else if ('title' in response) {
				const problem = response as ProblemDetails;
				if (!tryHandleNonValidationSignInError(problem))
				{
					showErrorModal = true;
					if (problem.title && problem.detail) 
						modalErrorMessage = problem.title + ': ' + problem.detail;
					else
						modalErrorMessage = problem.title || problem.detail || 'Server failure';
				}
			} else {
				modalErrorMessage = 'Problem with connection to the server';
				showErrorModal = true;
			}
		}
		isLoading = false;
	}
</script>
<svelte:head>
	<title>Snapflow | Sign in</title>
</svelte:head>

<div class="min-h-screen flex items-center justify-center px-4 py-12">
	<div class="w-full max-w-md">
		<!-- Sign In Card -->
		<div class="bg-white dark:bg-gray-800 rounded-2xl shadow-xl p-8">
			<!-- Header -->
			<div class="text-center mb-8">
				<h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-2">
					Welcome back!
				</h1>
				<p class="text-gray-600 dark:text-gray-400">
					Sign in to your Snapflow account
				</p>
			</div>

			<!-- Sign In Form -->
			<form onsubmit={handleSignin} novalidate class="space-y-4">
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

				<!-- Password Field -->
				<div class="space-y-1">
					<label for="password" class="block text-xs font-medium text-gray-700 dark:text-gray-300">
						Password
					</label>
					<div class="relative">
						<input
							id="password"
							name="password"
							type={showPassword ? 'text' : 'password'}
							autocomplete="current-password"
							bind:value={password}
							placeholder="Enter your password"
							class="w-full px-3 py-2.5 pr-10 text-sm border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-500 dark:placeholder-gray-400 focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all duration-200"
							oninput={validatePasswordField}
						/>
						<div class="absolute inset-y-0 right-0 flex items-center pr-3">
							<Toggle.Root bind:pressed={showPassword} aria-label="Show password" class="text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 transition-colors duration-200 p-1">
								{#if showPassword}
									<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21"/>
									</svg>
								{:else}
									<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
										<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/>
									</svg>
								{/if}
							</Toggle.Root>
						</div>
					
					<!-- Password Error Display -->
					<div class={`overflow-hidden transition-all duration-300 ${passwordError ? 'max-h-96 mt-2' : 'max-h-0'}`}>
						<p class="mt-1 text-xs text-red-600 dark:text-red-400">{passwordError}</p>
					</div>
					</div>
				</div>

				<!-- Forgot Password Link -->
				<div class="text-right">
					<a href="/forgot-password" class="text-sm text-blue-600 dark:text-blue-400 hover:text-blue-700 dark:hover:text-blue-300 transition-colors duration-200">
						Forgot your password?
					</a>
				</div>

				<!-- Sign In Button -->
				<Button.Root
					type="submit"
					disabled={isLoading || !email || !!emailError || !password || !!passwordError}
					class="w-full px-4 py-2.5 text-sm bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white font-semibold rounded-lg transition-all duration-200 shadow-md hover:shadow-lg disabled:cursor-not-allowed flex items-center justify-center"
				>
					{#if isLoading}
						<svg class="animate-spin h-4 w-4 text-white mr-2" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
							<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
							<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373	 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
						</svg>
						<span>Signing in</span>
                        <span class="inline-flex">
							<span class="animate-dots-bounce" style="animation-delay: 0ms">.</span>
							<span class="animate-dots-bounce" style="animation-delay: 150ms">.</span>
							<span class="animate-dots-bounce" style="animation-delay: 300ms">.</span>
						</span>
					{:else}
						<span>Sign in</span>
					{/if}
				</Button.Root>
			</form>

			<!-- Sign Up Link -->
			<div class="mt-8 text-center">
				<p class="text-gray-600 dark:text-gray-400">
					Don't have an account?
					<a href="/sign-up" class="text-blue-600 dark:text-blue-400 hover:text-blue-700 dark:hover:text-blue-300 font-semibold transition-colors duration-200">
						Sign up
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

<!-- Error Modal -->
<ErrorModal bind:isOpen={showErrorModal} errorMessage={modalErrorMessage} />

<!-- Sign-in info modal for special sign-in codes -->
<Dialog.Root bind:open={showSignInInfoModal}>
	<Dialog.Portal>
		<Dialog.Overlay class="data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0 fixed inset-0 z-50 bg-black/60" />
		<Dialog.Content class="fixed left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 w-full max-w-md rounded-lg bg-white dark:bg-gray-800 p-6 shadow-lg data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0 data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95 duration-300 ease-out z-50">
			<div class="text-center space-y-4">
				<div class="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-blue-100 dark:bg-blue-900/30">
					<svg class="h-6 w-6 text-blue-600 dark:text-blue-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
						<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M12 2a10 10 0 100 20 10 10 0 000-20z" />
					</svg>
				</div>
				<Dialog.Title class="text-lg font-semibold text-gray-900 dark:text-white">
					{signInInfoTitle}
				</Dialog.Title>
				<Dialog.Description class="text-sm text-gray-600 dark:text-gray-300">
					{signInInfoMessage}
				</Dialog.Description>
				<div class="mt-4 flex justify-center">
					<Button.Root onclick={() => (showSignInInfoModal = false)} class="inline-flex items-center justify-center rounded-md text-sm font-medium bg-blue-600 text-white hover:bg-blue-700 h-9 px-4 py-2">
						OK
					</Button.Root>
				</div>
			</div>
		</Dialog.Content>
	</Dialog.Portal>
</Dialog.Root>
