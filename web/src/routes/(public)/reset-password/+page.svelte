<script lang="ts">
	import { Button, Toggle, Dialog } from 'bits-ui';
	import { authConfig } from '$lib/config/auth';
	import { AuthService } from '$lib/services/auth';
	import type { ProblemDetails, PropertyValidationError } from '$lib/types/api';
	import { errorStore } from '$lib/stores/error';
	import { apiClient } from '$lib/services/api.client.ts';
	import logger from '$lib/logger';

	let { data } = $props();

	let password = $state('');
	let repeatPassword = $state('');
	let isLoading = $state(false);
	let passwordError = $state('');
	let showPassword = $state(false);
	let showRepeatPassword = $state(false);

	let showSuccessModal = $state(false);
	let redirectCountdown = $state(5);
	let sliderWidth = $state(100);
	let redirectTimer: NodeJS.Timeout;
	let redirectDelayTimer: NodeJS.Timeout;

	const authService = new AuthService(apiClient);

	$effect(() => {
		if (showSuccessModal && redirectCountdown > 0) {
			redirectTimer = setTimeout(() => {
				redirectCountdown--;
				sliderWidth = (100 / 5) * redirectCountdown;
				if (sliderWidth < 0) sliderWidth = 0;
			}, 1000);
		} else if (showSuccessModal && redirectCountdown === 0) {
			redirectDelayTimer = setTimeout(() => {
				window.location.href = '/sign-in';
			}, 1000);
		}

		return () => {
			if (redirectTimer) {
				clearTimeout(redirectTimer);
			}
			if (redirectDelayTimer) {
				clearTimeout(redirectDelayTimer);
			}
		};
	});

	$effect(() => {
		if (!showSuccessModal) {
			redirectCountdown = 5;
			sliderWidth = 100;
		}
	});

	let passwordRequirements = $state({
		length: false,
		lowercase: false,
		uppercase: false,
		digit: false,
		nonAlphanumeric: false
	});

	let passwordStrength = $state(0);
	let passwordStrengthText = $state('');

  const resetPasswordInfoByCode: Record<string, { title: string; message: string }> = {
		'Users.ResetPassword.InvalidCode': {
			title: 'Reset password failed',
			message: 'The reset password attempt failed. Please try again later.'
		}
	};

  let showResetPasswordInfoModal = $state(false);
  let resetPasswordInfoTitle = $state('');
  let resetPasswordInfoMessage = $state('');

	function tryHandleNonValidationResetPasswordError(problem: ProblemDetails): boolean {
		if (problem.title && resetPasswordInfoByCode[problem.title]) {
			const info = resetPasswordInfoByCode[problem.title];
			resetPasswordInfoTitle = info.title;
			resetPasswordInfoMessage = info.message;
			showResetPasswordInfoModal = true;
			return true;
		}
		return false;
	}

  function handleValidationErrors(errors: PropertyValidationError[]) {
		passwordError = '';

		const fieldErrors: { [key: string]: string[] } = {};
		const generalErrors: PropertyValidationError[] = [];

		errors.forEach((err) => {
			if (err.propertyName) {
				const fieldName = err.propertyName.toLowerCase();
				if (!fieldErrors[fieldName]) {
					fieldErrors[fieldName] = [];
				}
				fieldErrors[fieldName].push(err.description);
			} else {
				generalErrors.push(err);
			}
		});
		if (fieldErrors.password) {
			passwordError = fieldErrors.password.join('. ');
		}
    errorStore.addErrors(generalErrors);
	}

	function validatePasswordRequirements(pwd: string) {
		passwordRequirements.length = pwd.length >= authConfig.password.minLength;
		passwordRequirements.lowercase = /[a-z]/.test(pwd);
		passwordRequirements.uppercase = /[A-Z]/.test(pwd);
		passwordRequirements.digit = /\d/.test(pwd);
		passwordRequirements.nonAlphanumeric = /[^a-zA-Z0-9]/.test(pwd);

		const metRequirements = Object.values(passwordRequirements).filter(Boolean).length;
		passwordStrength = (metRequirements / 5) * 100;

		if (passwordStrength < 40) {
			passwordStrengthText = 'Weak';
		} else if (passwordStrength < 80) {
			passwordStrengthText = 'Medium';
		} else {
			passwordStrengthText = 'Strong';
		}
	}

	async function handleResetPassword(e: Event) {
		e.preventDefault();
		passwordError = '';

		if (!password) {
			passwordError = 'Password is required';
			return;
		}

		if (password.length < authConfig.password.minLength) {
			passwordError = `Password must be at least ${authConfig.password.minLength} characters`;
			return;
		}

		if (password.length > authConfig.password.maxLength) {
			passwordError = `Password must be less than ${authConfig.password.maxLength} characters`;
			return;
		}

		validatePasswordRequirements(password);

		if (authConfig.password.requireLowercase && !passwordRequirements.lowercase) {
			passwordError = 'Password must contain at least one lowercase letter';
			return;
		}

		if (authConfig.password.requireUppercase && !passwordRequirements.uppercase) {
			passwordError = 'Password must contain at least one uppercase letter';
			return;
		}

		if (authConfig.password.requireDigit && !passwordRequirements.digit) {
			passwordError = 'Password must contain at least one digit';
			return;
		}

		if (authConfig.password.requireNonAlphanumeric && !passwordRequirements.nonAlphanumeric) {
			passwordError = 'Password must contain at least one special character';
			return;
		}

		if (password !== repeatPassword) {
			passwordError = 'Passwords do not match';
			return;
		}

		isLoading = true;
    try {
		  const response = await authService.resetPassword({ email: data.email, resetCode: data.code, newPassword: password });
      logger.debug({ response }, 'Reset password response');
      if (response.ok) {
        showSuccessModal = true;
      } else {
        if ('errors' in response && response.errors && Array.isArray(response.errors)) {
          handleValidationErrors(response.errors);
        } else if ('title' in response) {
          const problem = response as ProblemDetails;
          if (!tryHandleNonValidationResetPasswordError(problem)) {
            errorStore.addError(problem.title, problem.detail);
          }
        } else {
          errorStore.addError(null, 'Problem with connection to the server');
        }
      }
    } catch (err) {
      if (err instanceof Error) {
        if (err.message === 'Failed to fetch') {
          errorStore.addError("Web.ConnectionProblem", 'Problem with connection to the server');
        }
        else {
          errorStore.addError(err.name, err.message);
        }
      } else {
        errorStore.addError(null, 'Unknown error occurred during sign up');
      }
    } finally {
      isLoading = false;
    }
	}
</script>

<svelte:head>
	<title>Snapflow | Reset Password</title>
</svelte:head>

<div class="flex min-h-screen items-center justify-center px-4 py-12">
	<div class="w-full max-w-md">
		<div class="rounded-2xl bg-white p-8 shadow-xl dark:bg-gray-800">
			<div class="mb-8 text-center">
				<h1 class="mb-2 text-3xl font-bold text-gray-900 dark:text-white">Reset Password</h1>
				<p class="text-gray-600 dark:text-gray-400">Enter your new password below</p>
			</div>

			<form onsubmit={handleResetPassword} novalidate class="space-y-4">
				<input type="hidden" name="email" value={data.email} />
				<input type="hidden" name="resetCode" value={data.code} />

				<div class="space-y-1">
					<label for="password" class="block text-xs font-medium text-gray-700 dark:text-gray-300">
						New Password
					</label>
					<div class="relative">
						<input
							id="password"
							name="password"
							type={showPassword ? 'text' : 'password'}
							autocomplete="new-password"
							bind:value={password}
							placeholder="Create a new password"
							class="w-full rounded-lg border border-gray-300 bg-white px-3 py-2.5 pr-10 text-sm text-gray-900 placeholder-gray-500 transition-all duration-200 focus:border-transparent focus:ring-2 focus:ring-blue-500 dark:border-gray-600 dark:bg-gray-700 dark:text-white dark:placeholder-gray-400"
							minlength={authConfig.password.minLength}
							maxlength={authConfig.password.maxLength}
							oninput={() => {
								validatePasswordRequirements(password);
								passwordError = '';
							}}
						/>
						<div class="absolute inset-y-0 right-0 flex items-center pr-2">
							<Toggle.Root
								bind:pressed={showPassword}
								aria-label="Show password"
								class="p-1 text-gray-500 transition-colors duration-200 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300"
							>
								{#if showPassword}
									<svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path
											stroke-linecap="round"
											stroke-linejoin="round"
											stroke-width="2"
											d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21"
										/>
									</svg>
								{:else}
									<svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path
											stroke-linecap="round"
											stroke-linejoin="round"
											stroke-width="2"
											d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
										/>
										<path
											stroke-linecap="round"
											stroke-linejoin="round"
											stroke-width="2"
											d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
										/>
									</svg>
								{/if}
							</Toggle.Root>
						</div>
					</div>

					<div
						class={`overflow-hidden transition-all duration-300 ${passwordError ? 'mt-2 max-h-96' : 'max-h-0'}`}
					>
						<p class="mt-1 text-xs text-red-600 dark:text-red-400">{passwordError}</p>
					</div>

					<div
						class={`overflow-hidden transition-all duration-300 ${password ? 'mt-2 max-h-96' : 'max-h-0'}`}
					>
						<div class="mb-1.5">
							<div class="mb-0.5 flex items-center justify-between">
								<span class="text-xs text-gray-600 dark:text-gray-400">Password Strength</span>
								<span
									class="text-xs font-medium {passwordStrength < 40
										? 'text-red-600'
										: passwordStrength < 80
											? 'text-yellow-600'
											: 'text-green-600'} dark:text-{passwordStrength < 40
										? 'red-400'
										: passwordStrength < 80
											? 'yellow-400'
											: 'green-400'}"
								>
									{passwordStrengthText}
								</span>
							</div>
							<div class="h-1 w-full rounded-full bg-gray-200 dark:bg-gray-700">
								<div
									class="h-1 rounded-full transition-all duration-300 {passwordStrength < 40
										? 'bg-red-500'
										: passwordStrength < 80
											? 'bg-yellow-500'
											: 'bg-green-500'}"
									style="width: {passwordStrength}%"
								></div>
							</div>
						</div>

						<div class="rounded-lg bg-gray-50 p-2 dark:bg-gray-700/50">
							<p class="mb-1 text-xs font-medium text-gray-700 dark:text-gray-300">
								Password Requirements:
							</p>
							<ul class="grid grid-cols-1 gap-x-2 gap-y-0.5 sm:grid-cols-2">
								<li class="flex items-center text-xs">
									<svg
										class="mr-1 h-2.5 w-2.5 {passwordRequirements.length
											? 'text-green-500'
											: 'text-gray-400'}"
										fill="currentColor"
										viewBox="0 0 20 20"
									>
										<path
											fill-rule="evenodd"
											d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
											clip-rule="evenodd"
										/>
									</svg>
									<span
										class={passwordRequirements.length
											? 'text-green-600 dark:text-green-400'
											: 'text-gray-600 dark:text-gray-400'}
									>
										At least {authConfig.password.minLength} characters
									</span>
								</li>
								{#if authConfig.password.requireLowercase}
									<li class="flex items-center text-xs">
										<svg
											class="mr-1 h-2.5 w-2.5 {passwordRequirements.lowercase
												? 'text-green-500'
												: 'text-gray-400'}"
											fill="currentColor"
											viewBox="0 0 20 20"
										>
											<path
												fill-rule="evenodd"
												d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
												clip-rule="evenodd"
											/>
										</svg>
										<span
											class={passwordRequirements.lowercase
												? 'text-green-600 dark:text-green-400'
												: 'text-gray-600 dark:text-gray-400'}
										>
											One lowercase letter
										</span>
									</li>
								{/if}
								{#if authConfig.password.requireUppercase}
									<li class="flex items-center text-xs">
										<svg
											class="mr-1 h-2.5 w-2.5 {passwordRequirements.uppercase
												? 'text-green-500'
												: 'text-gray-400'}"
											fill="currentColor"
											viewBox="0 0 20 20"
										>
											<path
												fill-rule="evenodd"
												d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
												clip-rule="evenodd"
											/>
										</svg>
										<span
											class={passwordRequirements.uppercase
												? 'text-green-600 dark:text-green-400'
												: 'text-gray-600 dark:text-gray-400'}
										>
											One uppercase letter
										</span>
									</li>
								{/if}
								{#if authConfig.password.requireDigit}
									<li class="flex items-center text-xs">
										<svg
											class="mr-1 h-2.5 w-2.5 {passwordRequirements.digit
												? 'text-green-500'
												: 'text-gray-400'}"
											fill="currentColor"
											viewBox="0 0 20 20"
										>
											<path
												fill-rule="evenodd"
												d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
												clip-rule="evenodd"
											/>
										</svg>
										<span
											class={passwordRequirements.digit
												? 'text-green-600 dark:text-green-400'
												: 'text-gray-600 dark:text-gray-400'}
										>
											One digit
										</span>
									</li>
								{/if}
								{#if authConfig.password.requireNonAlphanumeric}
									<li class="flex items-center text-xs">
										<svg
											class="mr-1 h-2.5 w-2.5 {passwordRequirements.nonAlphanumeric
												? 'text-green-500'
												: 'text-gray-400'}"
											fill="currentColor"
											viewBox="0 0 20 20"
										>
											<path
												fill-rule="evenodd"
												d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z"
												clip-rule="evenodd"
											/>
										</svg>
										<span
											class={passwordRequirements.nonAlphanumeric
												? 'text-green-600 dark:text-green-400'
												: 'text-gray-600 dark:text-gray-400'}
										>
											One special character
										</span>
									</li>
								{/if}
							</ul>
						</div>
					</div>
				</div>

				<div class="space-y-1">
					<label
						for="repeatPassword"
						class="block text-xs font-medium text-gray-700 dark:text-gray-300"
					>
						Confirm password
					</label>
					<div class="relative">
						<input
							id="repeatPassword"
							name="repeatPassword"
							type={showRepeatPassword ? 'text' : 'password'}
							autocomplete="new-password"
							bind:value={repeatPassword}
							placeholder="Confirm your new password"
							class="w-full rounded-lg border border-gray-300 bg-white px-3 py-2.5 pr-10 text-sm text-gray-900 placeholder-gray-500 transition-all duration-200 focus:border-transparent focus:ring-2 focus:ring-blue-500 dark:border-gray-600 dark:bg-gray-700 dark:text-white dark:placeholder-gray-400"
						/>
						<div class="absolute inset-y-0 right-0 flex items-center pr-2">
							<Toggle.Root
								bind:pressed={showRepeatPassword}
								aria-label="Show password"
								class="p-1 text-gray-500 transition-colors duration-200 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300"
							>
								{#if showRepeatPassword}
									<svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path
											stroke-linecap="round"
											stroke-linejoin="round"
											stroke-width="2"
											d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21"
										/>
									</svg>
								{:else}
									<svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
										<path
											stroke-linecap="round"
											stroke-linejoin="round"
											stroke-width="2"
											d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
										/>
										<path
											stroke-linecap="round"
											stroke-linejoin="round"
											stroke-width="2"
											d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
										/>
									</svg>
								{/if}
							</Toggle.Root>
						</div>
					</div>
					<div
						class={`overflow-hidden transition-all duration-300 ${repeatPassword && password !== repeatPassword ? 'mt-2 max-h-96' : repeatPassword && password === repeatPassword ? 'mt-2 max-h-96' : 'max-h-0'}`}
					>
						{#if repeatPassword && password !== repeatPassword}
							<p class="mt-1 text-xs text-red-600 dark:text-red-400">Passwords do not match</p>
						{:else if repeatPassword && password === repeatPassword}
							<p class="mt-1 text-xs text-green-600 dark:text-green-400">Passwords match</p>
						{/if}
					</div>
				</div>

				<Button.Root
					type="submit"
					disabled={isLoading || !password || !repeatPassword || password !== repeatPassword}
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
						<span>Resetting password</span>
						<span class="inline-flex">
							<span class="animate-dots-bounce" style="animation-delay: 0ms">.</span>
							<span class="animate-dots-bounce" style="animation-delay: 150ms">.</span>
							<span class="animate-dots-bounce" style="animation-delay: 300ms">.</span>
						</span>
					{:else}
						<span>Reset password</span>
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

<Dialog.Root bind:open={showSuccessModal}>
	<Dialog.Portal>
		<Dialog.Overlay
			class="fixed inset-0 z-50 bg-black/80 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:animate-in data-[state=open]:fade-in-0"
		/>
		<Dialog.Content
			escapeKeydownBehavior="ignore"
			interactOutsideBehavior="ignore"
			class="fixed top-[50%] left-[50%] z-50 w-full max-w-md translate-x-[-50%] translate-y-[-50%] rounded-lg bg-white p-8 shadow-lg duration-500 ease-out data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:zoom-out-95 data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=open]:zoom-in-95 dark:bg-gray-800"
		>
			<div class="text-center">
				<div
					class="mx-auto mb-4 flex h-16 w-16 animate-pulse items-center justify-center rounded-full bg-green-100 dark:bg-green-900/20"
				>
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

				<Dialog.Title
					class="animate-fade-in mb-2 text-lg font-semibold text-gray-900 dark:text-white"
				>
					Password reset successful!
				</Dialog.Title>
				<Dialog.Description
					class="animate-fade-in mb-4 text-sm text-gray-600 dark:text-gray-400"
					style="animation-delay: 100ms"
				>
					Your password has been successfully updated. You can now sign in with your new password.
				</Dialog.Description>

				<div
					class="animate-fade-in mb-6 rounded-lg bg-blue-50 p-3 dark:bg-blue-900/20"
					style="animation-delay: 200ms"
				>
					<p class="text-sm text-blue-600 dark:text-blue-400">
						Redirecting to sign in page in <span class="font-mono font-semibold"
							>{redirectCountdown}</span
						>
						{redirectCountdown === 1 ? 'second' : 'seconds'}...
					</p>
					<div class="mt-2 h-1.5 w-full rounded-full bg-blue-200 dark:bg-blue-800">
						<div
							class="h-1.5 rounded-full bg-blue-600 transition-all duration-1000 ease-linear dark:bg-blue-400"
							style="width: {sliderWidth}%"
						></div>
					</div>
				</div>

				<div class="animate-fade-in flex gap-3" style="animation-delay: 300ms">
					<Button.Root
						href="/sign-in"
						class="ring-offset-background focus-visible:ring-ring inline-flex h-10 w-full items-center justify-center rounded-md bg-green-600 px-4 py-2 text-center text-sm font-medium text-white transition-colors hover:bg-green-700 focus-visible:ring-2 focus-visible:ring-offset-2 focus-visible:outline-none disabled:pointer-events-none disabled:opacity-50"
					>
						Sign in now
					</Button.Root>
				</div>
			</div>
		</Dialog.Content>
	</Dialog.Portal>
</Dialog.Root>

<Dialog.Root bind:open={showResetPasswordInfoModal}>
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
					{resetPasswordInfoTitle}
				</Dialog.Title>
				<Dialog.Description class="text-sm text-gray-600 dark:text-gray-300">
					{resetPasswordInfoMessage}
				</Dialog.Description>
				<div class="mt-4 flex justify-center">
					<Button.Root
						onclick={() => (showResetPasswordInfoModal = false)}
						class="inline-flex h-9 items-center justify-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
					>
						OK
					</Button.Root>
				</div>
			</div>
		</Dialog.Content>
	</Dialog.Portal>
</Dialog.Root>
