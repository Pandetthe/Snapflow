<script lang="ts">
  import { Dialog } from 'bits-ui';
  import { authConfig } from '$lib/config/auth';
  import { AuthService } from '$lib/features/auth/api/auth';
  import type { PropertyValidationError, ProblemDetails } from '$lib/core/types/api';
  import { errorStore } from '$lib/ui/stores/error';
  import { apiClient } from '$lib/core/api.client';
  import Button from '$lib/ui/components/Button.svelte';
  import Checkbox from '$lib/ui/components/Checkbox.svelte';
  import InputTextField from '$lib/ui/components/input/InputTextField.svelte';
  import LoadingDots from '$lib/ui/components/LoadingDots.svelte';
  import { triggerHaptic } from '$lib/ui/utils/haptics';
  import { Mail, Lock, ChevronLeft, Info, LoaderCircle } from 'lucide-svelte';

  let email = $state('');
  let password = $state('');
  let isLoading = $state(false);
  let keepLoggedIn = $state<boolean | null>(null);
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
      triggerHaptic('error');
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
    triggerHaptic('error');
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
          errorStore.addError('Web.ConnectionProblem', 'Problem with connection to the server');
        } else {
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
        triggerHaptic('success');
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
          errorStore.addError('Web.ConnectionProblem', 'Problem with connection to the server');
        } else {
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

<div class="relative z-1 bg-white dark:bg-gray-900">
  <div class="relative flex h-screen w-full flex-col overflow-hidden lg:flex-row dark:bg-gray-900">
    <div class="flex w-full flex-1 flex-col lg:w-1/2">
      <div class="mx-auto w-full max-w-md px-6 pt-5 sm:px-0 lg:pt-10">
        <a
          href="/"
          class="inline-flex items-center gap-1.5 text-sm text-gray-500 transition-colors hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300"
        >
          <ChevronLeft size={18} />
          Back to home
        </a>
      </div>

      <div
        class="mx-auto flex w-full max-w-md flex-1 flex-col justify-center px-6 py-4 sm:px-0 lg:py-12"
      >
        <div class="mb-3 sm:mb-8">
          <h1 class="mb-2 text-2xl font-semibold text-gray-800 sm:text-3xl dark:text-white/90">
            Welcome back!
          </h1>
          <p class="text-sm text-gray-500 dark:text-gray-400">
            Enter your email and password to sign in!
          </p>
        </div>

        <form onsubmit={handleSignin} novalidate class="space-y-5">
          <InputTextField
            id="email"
            name="email"
            type="email"
            label="Email"
            placeholder="info@example.com"
            autocomplete="email"
            bind:value={email}
            error={emailError}
            leftIcon={Mail}
            maxlength={authConfig.email.maxLength}
            oninput={(e) => validateEmailField((e.currentTarget as HTMLInputElement).value)}
          />

          <InputTextField
            id="password"
            name="password"
            type="password"
            label="Password"
            placeholder="Enter your password"
            autocomplete="current-password"
            bind:value={password}
            error={passwordError}
            leftIcon={Lock}
            showPasswordToggle={true}
            oninput={validatePasswordField}
          />
          <div class="flex items-center justify-between">
            <Checkbox bind:checked={keepLoggedIn} label="Keep me logged in" />
            <a
              href="/forgot-password"
              class="text-sm whitespace-nowrap text-brand-500 hover:text-brand-600 dark:text-brand-400"
            >
              Forgot password?
            </a>
          </div>
          <Button
            type="submit"
            variant="primary"
            size="md"
            class="w-full justify-center"
            disabled={isLoading || !email || !!emailError || !password || !!passwordError}
          >
            {#if isLoading}
              <LoaderCircle size={16} class="animate-spin" />
              <span>Signing in<LoadingDots /></span>
            {:else}
              <span>Sign in</span>
            {/if}
          </Button>
        </form>

        <div class="mt-5">
          <p class="text-center text-sm font-normal text-gray-700 sm:text-start dark:text-gray-400">
            Don't have an account?
            <a href="/sign-up" class="text-brand-500 hover:text-brand-600 dark:text-brand-400">
              Sign Up
            </a>
          </p>
        </div>
      </div>
    </div>

    <div
      class="relative hidden min-h-screen w-full items-center justify-center overflow-hidden bg-brand-950 lg:flex lg:w-1/2 dark:bg-white/5"
    >
      <div class="absolute inset-0 opacity-10">
        <svg class="h-full w-full" xmlns="http://www.w3.org/2000/svg">
          <defs>
            <pattern id="grid" width="40" height="40" patternUnits="userSpaceOnUse">
              <path d="M 40 0 L 0 0 0 40" fill="none" stroke="white" stroke-width="0.5" />
            </pattern>
          </defs>
          <rect width="100%" height="100%" fill="url(#grid)" />
        </svg>
      </div>
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
          class="mx-auto flex h-12 w-12 items-center justify-center rounded-full bg-brand-100 dark:bg-brand-900/30"
        >
          <Info size={24} class="text-brand-600 dark:text-brand-400" />
        </div>
        <Dialog.Title class="text-lg font-semibold text-gray-900 dark:text-white">
          {signInInfoTitle}
        </Dialog.Title>
        <Dialog.Description class="text-sm text-gray-600 dark:text-gray-300">
          {signInInfoMessage}
        </Dialog.Description>
        <div class="mt-4 flex justify-center gap-3">
          {#if showResendConfirmationButton}
            <Button
              variant="outline"
              size="sm"
              onclick={resendEmailConfirmation}
              disabled={isResendLoading}
            >
              {#if isResendLoading}
                <LoaderCircle size={14} class="animate-spin" />
                <span>Resending<LoadingDots /></span>
              {:else}
                <span>Resend confirmation</span>
              {/if}
            </Button>
          {/if}
          <Button variant="primary" size="sm" onclick={() => (showSignInInfoModal = false)}>
            OK
          </Button>
        </div>
      </div>
    </Dialog.Content>
  </Dialog.Portal>
</Dialog.Root>
