<script lang="ts">
  import { authConfig } from '$lib/config/auth';
  import { AuthService } from '$lib/features/auth/api/auth';
  import { apiClient } from '$lib/core/api.client';
  import { Button as AppButton, GoBackButton, InputTextField, SplitLayout } from '$lib/ui/components';
  import { Lock } from 'lucide-svelte';
  import { createForm } from '$lib/ui/_utils/form.svelte';
  import type { ProblemDetails } from '$lib/core/types/api';
  import ResetPasswordModal from '$lib/features/auth/components/ResetPasswordModal.svelte';
  import PasswordStrength from '$lib/features/auth/components/PasswordStrength.svelte';
  import { validatePassword, validatePasswordConfirm } from '$lib/features/auth/validation';

  let { data } = $props();

  const authService = new AuthService(apiClient);

  let showModal = $state(false);
  let modalVariant = $state<'success' | 'error' | 'accountDeleted'>('success');

  const resetPasswordInfoByCode: Record<string, { title: string; message: string }> = {
    'Users.ResetPassword.InvalidCode': {
      title: 'Reset password failed',
      message: 'The reset password attempt failed. Please try again later.'
    }
  };

  const form = createForm({
    initialValues: {
      password: '',
      repeatPassword: ''
    },
    validate: (values) => {
      const errors: Partial<Record<keyof typeof values, string>> = {};
      const passwordError = validatePassword(values.password);
      if (passwordError) errors.password = passwordError;
      const confirmError = validatePasswordConfirm(values.password, values.repeatPassword);
      if (confirmError) errors.repeatPassword = confirmError;
      return errors;
    },
    onSubmit: async (values) => {
      return await authService.resetPassword({
        email: data.email,
        resetCode: data.code,
        newPassword: values.password
      });
    },
    onSuccess: () => {
      modalVariant = 'success';
      showModal = true;
    },
    onError: (problem: ProblemDetails) => {
      if (problem.title === 'Users.AccountDeleted') {
        modalVariant = 'accountDeleted';
        showModal = true;
        return true;
      }
      if (problem.title && resetPasswordInfoByCode[problem.title]) {
        modalVariant = 'error';
        showModal = true;
        return true;
      }
      return false;
    }
  });

  const passwordsMatch = $derived(
    form.values.repeatPassword && form.values.password === form.values.repeatPassword
  );
</script>

<svelte:head>
  <title>Snapflow | Reset password</title>
</svelte:head>

<SplitLayout>
  {#snippet header()}
    <GoBackButton
      href="/"
    />
  {/snippet}

  <div class="mb-3 sm:mb-8">
    <h1 class="mb-2 text-2xl font-semibold text-gray-800 sm:text-3xl dark:text-white/90">
      Reset your password!
    </h1>
    <p class="text-sm text-gray-500 dark:text-gray-400">Enter your new password below.</p>
  </div>

  <form onsubmit={form.handleSubmit} novalidate class="space-y-5">
    <div class="space-y-2">
      <InputTextField
        id="password"
        name="password"
        type="password"
        label="New password"
        placeholder="Create a new password"
        autocomplete="new-password"
        bind:value={form.values.password}
        error={form.errors.password}
        leftIcon={Lock}
        showPasswordToggle={true}
        minlength={authConfig.password.minLength}
        maxlength={authConfig.password.maxLength}
      />

      <PasswordStrength password={form.values.password} />
    </div>

    <InputTextField
      id="repeatPassword"
      name="repeatPassword"
      type="password"
      label="Confirm password"
      placeholder="Confirm your new password"
      autocomplete="new-password"
      bind:value={form.values.repeatPassword}
      leftIcon={Lock}
      showPasswordToggle={true}
      error={form.errors.repeatPassword}
      helperText={passwordsMatch && !form.errors.repeatPassword ? 'Passwords match' : undefined}
      helperTextClass="text-green-600 dark:text-green-400"
    />

    <AppButton
      type="submit"
      disabled={form.isSubmitting ||
        !form.values.password ||
        !form.values.repeatPassword ||
        form.values.password !== form.values.repeatPassword}
      class="w-full justify-center"
      isLoading={form.isSubmitting}
      loadingText="Resetting password"
    >
      Reset password
    </AppButton>
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

<ResetPasswordModal bind:open={showModal} variant={modalVariant} />
