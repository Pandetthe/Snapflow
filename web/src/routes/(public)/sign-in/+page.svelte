<script lang="ts">
  import { authConfig } from '$lib/config/auth';
  import { AuthService } from '$lib/features/auth/api/auth';
  import type { ProblemDetails } from '$lib/core/types/api';
  import { apiClient } from '$lib/core/api.client';
  import { Button, Checkbox, InputTextField, SplitLayout } from '$lib/ui/components';
  import { Mail, Lock, ChevronLeft } from 'lucide-svelte';
  import { createForm } from '$lib/ui/utils';
  import SignInModal from '$lib/features/auth/components/SignInModal.svelte';

  let showSignInInfoModal = $state(false);
  let signInInfoCode = $state('');

  const authService = new AuthService(apiClient);

  const form = createForm({
    initialValues: {
      email: '',
      password: '',
      rememberMe: false
    },
    validate: (values) => {
      const errors: Record<string, string> = {};
      if (!values.email) {
        errors.email = 'Email is required';
      } else if (values.email.length > authConfig.email.maxLength) {
        errors.email = `Email must be less than ${authConfig.email.maxLength} characters`;
      } else {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(values.email)) {
          errors.email = 'Please enter a valid email address';
        }
      }

      if (!values.password) {
        errors.password = 'Password is required';
      }
      return errors;
    },
    onSubmit: async (values) => {
      return await authService.signIn(values);
    },
    onSuccess: () => {
      window.location.href = '/boards';
    },
    onError: (problem: ProblemDetails) => {
      const handledCodes = [
        'Users.SignIn.Failed',
        'Users.SignIn.LockedOut',
        'Users.SignIn.NotAllowed',
        'Users.SignIn.TwoFactorRequired'
      ];

      if (problem.title && handledCodes.includes(problem.title)) {
        signInInfoCode = problem.title;
        showSignInInfoModal = true;
        return true;
      }
      return false;
    }
  });
</script>

<svelte:head>
  <title>Snapflow | Sign in</title>
</svelte:head>

<SplitLayout>
  {#snippet header()}
    <a
      href="/"
      class="inline-flex items-center gap-1.5 text-sm text-gray-500 transition-colors hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300"
    >
      <ChevronLeft size={18} />
      Back to home
    </a>
  {/snippet}

  <div class="mb-3 sm:mb-8">
    <h1 class="mb-2 text-2xl font-semibold text-gray-800 sm:text-3xl dark:text-white/90">
      Welcome back!
    </h1>
    <p class="text-sm text-gray-500 dark:text-gray-400">
      Enter your email and password to sign in!
    </p>
  </div>

  <form onsubmit={form.handleSubmit} novalidate class="space-y-5">
    <InputTextField
      id="email"
      name="email"
      type="email"
      label="Email"
      placeholder="info@example.com"
      autocomplete="email"
      bind:value={form.values.email}
      error={form.errors.email}
      leftIcon={Mail}
      maxlength={authConfig.email.maxLength}
    />

    <InputTextField
      id="password"
      name="password"
      type="password"
      label="Password"
      placeholder="Enter your password"
      autocomplete="current-password"
      bind:value={form.values.password}
      error={form.errors.password}
      leftIcon={Lock}
      showPasswordToggle={true}
    />
    <div class="flex items-center justify-between">
      <Checkbox
        bind:checked={form.values.rememberMe}
        error={form.errors.rememberMe}
        label="Keep me logged in"
      />
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
      disabled={!form.values.email || !form.values.password || form.isSubmitting}
      isLoading={form.isSubmitting}
      loadingText="Signing in"
    >
      Sign in
    </Button>
  </form>

  {#snippet footer()}
    <p class="text-center text-sm font-normal text-gray-700 sm:text-start dark:text-gray-400">
      Don't have an account?
      <a href="/sign-up" class="text-brand-500 hover:text-brand-600 dark:text-brand-400">
        Sign Up
      </a>
    </p>
  {/snippet}
</SplitLayout>

<SignInModal
  bind:open={showSignInInfoModal}
  problemCode={signInInfoCode}
  userEmail={form.values.email}
/>
