<script lang="ts">
	import { Button, Toggle, Dialog } from 'bits-ui';
	import { authConfig } from '$lib/config/auth';
	import { AuthService } from '$lib/services/auth';
	import type { PropertyValidationError, ProblemDetails } from '$lib/types/api';
  import { errorStore } from '$lib/stores/error';
	import { apiClient } from '$lib/services/api.client';

	let email = $state('');
	let password = $state('');
	let isLoading = $state(false);
	let showPassword = $state(false);
	let emailError = $state('');
	let passwordError = $state('');

	let showSignInInfoModal = $state(false);
  let showResendConfirmationButton = $state(false);
  let isResendLoading = $state(false);
	let signInInfoTitle = $state('');
	let signInInfoMessage = $state('');

	const authService = new AuthService(apiClient);

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
      isResendLoading = false;
      showResendConfirmationButton = problem.title === 'Users.SignIn.NotAllowed';
			return true;
		}
		return false;
	}

	function handleValidationErrors(errors: PropertyValidationError[]) {
		emailError = '';
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

		if (fieldErrors.email) {
			emailError = fieldErrors.email.join('. ');
		}
		if (fieldErrors.password) {
			passwordError = fieldErrors.password.join('. ');
		}
    errorStore.addErrors(generalErrors);
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

  async function resendEmailConfirmation(e: Event) {
    isResendLoading = true;
    try {
      await authService.resendEmailConfirmation({ email });
    } catch (err) {
      if (err instanceof Error) {
        if (err.message === 'Failed to fetch') {
          errorStore.addError("Web.ConnectionProblem", 'Problem with connection to the server');
        }
        else {
          errorStore.addError(err.name, err.message);
        }
      } else {
        errorStore.addError(null, 'Unknown error occurred while resending confirmation email');
      }
    } finally {
      isResendLoading = false;
      showSignInInfoModal = false;
    }
  }

	async function handleSignin(e: Event) {
		e.preventDefault();
		emailError = '';
		passwordError = '';
    
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
    try {
		  const response = await authService.signIn({ email, password });
      if (response.ok) {
        window.location.href = '/boards';
      } else {
        if ('errors' in response && response.errors && Array.isArray(response.errors)) {
          handleValidationErrors(response.errors);
        } else if ('title' in response) {
          const problem = response as ProblemDetails;
          if (!tryHandleNonValidationSignInError(problem)) {
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
        errorStore.addError(null, 'Unknown error occurred during sign in');
      }
    } finally {
      isLoading = false;
    }
	}
</script>

<svelte:head>
	<title>Snapflow | Sign in</title>
</svelte:head>

<div class="flex min-h-screen items-center justify-center px-4 py-12">
	<div class="w-full max-w-md">
		<div class="rounded-2xl bg-white p-8 shadow-xl dark:bg-gray-800">
			<div class="mb-8 text-center">
				<h1 class="mb-2 text-3xl font-bold text-gray-900 dark:text-white">Welcome back!</h1>
				<p class="text-gray-600 dark:text-gray-400">Sign in to your Snapflow account</p>
			</div>

			<form onsubmit={handleSignin} novalidate class="space-y-4">
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
							class="w-full rounded-lg border border-gray-300 bg-white px-3 py-2.5 pr-10 text-sm text-gray-900 placeholder-gray-500 transition-all duration-200 focus:border-transparent focus:ring-2 focus:ring-blue-500 dark:border-gray-600 dark:bg-gray-700 dark:text-white dark:placeholder-gray-400"
							oninput={validatePasswordField}
						/>
						<div class="absolute inset-y-0 right-0 flex items-center pr-3">
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

						<div
							class={`overflow-hidden transition-all duration-300 ${passwordError ? 'mt-2 max-h-96' : 'max-h-0'}`}
						>
							<p class="mt-1 text-xs text-red-600 dark:text-red-400">{passwordError}</p>
						</div>
					</div>
				</div>

				<div class="text-right">
					<a
						href="/forgot-password"
						class="text-sm text-blue-600 transition-colors duration-200 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300"
					>
						Forgot your password?
					</a>
				</div>

				<Button.Root
					type="submit"
					disabled={isLoading || !email || !!emailError || !password || !!passwordError}
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
								d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373	 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
							></path>
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

			<div class="mt-8 text-center">
				<p class="text-gray-600 dark:text-gray-400">
					Don't have an account?
					<a
						href="/sign-up"
						class="font-semibold text-blue-600 transition-colors duration-200 hover:text-blue-700 dark:text-blue-400 dark:hover:text-blue-300"
					>
						Sign up
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

<Dialog.Root bind:open={showSignInInfoModal}>
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
					{signInInfoTitle}
				</Dialog.Title>
				<Dialog.Description class="text-sm text-gray-600 dark:text-gray-300">
					{signInInfoMessage}
				</Dialog.Description>
				<div class="mt-4 flex justify-center">
          {#if showResendConfirmationButton}
            <Button.Root
              onclick={resendEmailConfirmation}
              class="inline-flex h-9 items-center justify-center rounded-md bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700 mr-3"
            >
              {#if isResendLoading}
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
                    d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373	 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                  ></path>
                </svg>
                <span>Resending email confirmation</span>
                <span class="inline-flex">
                  <span class="animate-dots-bounce" style="animation-delay: 0ms">.</span>
                  <span class="animate-dots-bounce" style="animation-delay: 150ms">.</span>
                  <span class="animate-dots-bounce" style="animation-delay: 300ms">.</span>
                </span>
              {:else}
                <span>Resend email confirmation</span>
              {/if}
            </Button.Root>
          {/if}
					<Button.Root
						onclick={() => (showSignInInfoModal = false)}
						class="inline-flex h-9 items-center justify-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700"
					>
						OK
					</Button.Root>
				</div>
			</div>
		</Dialog.Content>
	</Dialog.Portal>
</Dialog.Root>
