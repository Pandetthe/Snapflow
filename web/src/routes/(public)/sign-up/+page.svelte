<script lang="ts">
  import { authConfig } from '$lib/config/auth';
  import { AuthService } from '$lib/features/auth/api/auth';
  import { apiClient } from '$lib/core/api.client';
  import { Button as AppButton, GoBackButton, InputTextField, SplitLayout } from '$lib/ui/components';
  import { Mail, Lock, User } from 'lucide-svelte';
  import { createForm } from '$lib/ui/_utils/form.svelte';
  import SignUpModal from '$lib/features/auth/components/SignUpModal.svelte';
  import PasswordStrength from '$lib/features/auth/components/PasswordStrength.svelte';
  import { validateEmail, validateUsername, validatePassword, validatePasswordConfirm } from '$lib/features/auth/validation';

  const authService = new AuthService(apiClient);

  let showSuccessModal = $state(false);

  const form = createForm({
    initialValues: {
      email: '',
      userName: '',
      password: '',
      repeatPassword: ''
    },
    validate: (values) => {
      const errors: Partial<Record<keyof typeof values, string>> = {};
      const emailError = validateEmail(values.email);
      if (emailError) errors.email = emailError;
      const usernameError = validateUsername(values.userName);
      if (usernameError) errors.userName = usernameError;
      const passwordError = validatePassword(values.password);
      if (passwordError) errors.password = passwordError;
      const confirmError = validatePasswordConfirm(values.password, values.repeatPassword);
      if (confirmError) errors.repeatPassword = confirmError;
      return errors;
    },
    onSubmit: async (values) => {
      return await authService.signUp({
        email: values.email,
        userName: values.userName,
        password: values.password
      });
    },
    onSuccess: () => {
      showSuccessModal = true;
    }
  });

  const passwordsMatch = $derived(
    form.values.repeatPassword && form.values.password === form.values.repeatPassword
  );
</script>

<svelte:head>
  <title>Snapflow | Sign up</title>
</svelte:head>

<SplitLayout>
  {#snippet header()}
    <GoBackButton
      href="/"
    />
  {/snippet}

  <div class="mb-3 sm:mb-8">
    <h1 class="mb-2 text-2xl font-semibold text-gray-800 sm:text-3xl dark:text-white/90">
      Create your new account!
    </h1>
    <p class="text-sm text-gray-500 dark:text-gray-400">Join Snapflow to start collaborating.</p>
  </div>

  <form onsubmit={form.handleSubmit} novalidate class="space-y-5">
    <InputTextField
      id="email"
      name="email"
      type="email"
      label="Email address"
      placeholder="Enter your email"
      autocomplete="email"
      bind:value={form.values.email}
      error={form.errors.email}
      leftIcon={Mail}
      maxlength={authConfig.email.maxLength}
    />

    <InputTextField
      id="username"
      name="username"
      type="text"
      label="User name"
      placeholder="Choose a username"
      autocomplete="username"
      bind:value={form.values.userName}
      error={form.errors.userName}
      leftIcon={User}
      minlength={authConfig.userName.minLength}
      maxlength={authConfig.userName.maxLength}
    />

    <div class="space-y-2">
      <InputTextField
        id="password"
        name="password"
        type="password"
        label="Password"
        placeholder="Create a password"
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
      placeholder="Confirm your password"
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
        !form.values.email ||
        !form.values.userName ||
        !form.values.password ||
        !form.values.repeatPassword ||
        form.values.password !== form.values.repeatPassword}
      class="w-full justify-center"
      isLoading={form.isSubmitting}
      loadingText="Creating account"
    >
      Create account
    </AppButton>
  </form>

  {#snippet footer()}
    <p class="text-center text-sm font-normal text-gray-700 sm:text-start dark:text-gray-400">
      Already have an account?
      <a
        href="/sign-in"
        class="rounded-sm text-brand-500 underline underline-offset-2 transition-all duration-200 hover:text-brand-600 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 dark:text-brand-400 dark:hover:text-brand-500 dark:focus-visible:ring-offset-gray-950"
      >
        Sign in
      </a>
    </p>
  {/snippet}
</SplitLayout>

<SignUpModal bind:open={showSuccessModal} />
