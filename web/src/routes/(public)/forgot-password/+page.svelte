<script lang="ts">
  import { Button, InputTextField, SplitLayout } from '$lib/ui/components';
  import { authConfig } from '$lib/config/auth';
  import { AuthService } from '$lib/features/auth/api/auth';
  import { apiClient } from '$lib/core/api.client';
  import { createForm } from '$lib/ui/utils';
  import { Mail, ChevronLeft } from 'lucide-svelte';
  import ForgotPasswordModal from '$lib/features/auth/components/ForgotPasswordModal.svelte';

  let showForgotPasswordModal = $state(false);

  const authService = new AuthService(apiClient);

  const form = createForm({
    initialValues: {
      email: ''
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
      return errors;
    },
    onSubmit: async (values) => {
      return await authService.forgotPassword(values);
    },
    onSuccess: () => {
      showForgotPasswordModal = true;
    }
  });
</script>

<svelte:head>
  <title>Snapflow | Forgot password</title>
</svelte:head>

<SplitLayout>
  {#snippet header()}
    <a
      href="/sign-in"
      class="inline-flex items-center gap-1.5 rounded-sm text-sm text-gray-500 transition-all duration-200 hover:text-gray-700 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 dark:text-gray-400 dark:hover:text-gray-300 dark:focus-visible:ring-offset-gray-950"
    >
      <ChevronLeft size={18} />
      Back to sign in
    </a>
  {/snippet}

  <div class="mb-3 sm:mb-8">
    <h1 class="mb-2 text-2xl font-semibold text-gray-800 sm:text-3xl dark:text-white/90">
      Forgot your password?
    </h1>
    <p class="text-sm text-gray-500 dark:text-gray-400">
      Enter your email associated with your account, and we’ll send you a link to reset your
      password.
    </p>
  </div>

  <form onsubmit={form.handleSubmit} novalidate class="space-y-5">
    <InputTextField
      id="email"
      name="email"
      type="email"
      label="Email address"
      placeholder="info@example.com"
      autocomplete="email"
      bind:value={form.values.email}
      error={form.errors.email}
      leftIcon={Mail}
      maxlength={authConfig.email.maxLength}
    />

    <Button
      type="submit"
      variant="primary"
      size="md"
      class="w-full justify-center"
      disabled={!form.values.email || form.isSubmitting}
      isLoading={form.isSubmitting}
      loadingText="Sending link"
    >
      Send reset link
    </Button>
  </form>

  {#snippet footer()}
    <p class="text-center text-sm font-normal text-gray-700 sm:text-start dark:text-gray-400">
      Remember your password?
      <a
        href="/sign-in"
        class="rounded-sm text-brand-500 underline underline-offset-2 transition-all duration-200 hover:text-brand-600 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 dark:text-brand-400 dark:hover:text-brand-500 dark:focus-visible:ring-offset-gray-950"
      >
        Sign in
      </a>
    </p>
  {/snippet}
</SplitLayout>

<ForgotPasswordModal bind:open={showForgotPasswordModal} />
