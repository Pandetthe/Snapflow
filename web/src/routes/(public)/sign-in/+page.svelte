<script lang="ts">
  import { onMount } from 'svelte';
  import { authConfig } from '$lib/config/auth';
  import { AuthService } from '$lib/features/auth/api/auth';
  import type { ProblemDetails } from '$lib/core/types/api';
  import { apiClient } from '$lib/core/api.client';
  import {
    Button,
    Checkbox,
    CodeInput,
    GoBackButton,
    InputTextField,
    SegmentedControl,
    SplitLayout
  } from '$lib/ui/components';
  import { Fingerprint, Mail, Lock, User } from 'lucide-svelte';
  import { createForm } from '$lib/ui/utils';
  import { errorStore } from '$lib/ui/stores/error.svelte';
  import {
    getPasskey,
    isPasskeyDismissed,
    passkeyAutofillSupported,
    passkeysSupported,
    type PasskeyJson
  } from '$lib/features/auth/passkeys';
  import SignInModal from '$lib/features/auth/components/SignInModal.svelte';
  import ExternalProviderButtons from '$lib/features/auth/components/ExternalProviderButtons.svelte';

  let { data } = $props();

  let showSignInInfoModal = $state(false);
  let signInInfoCode = $state('');
  let signInMethod = $state<'ldap' | 'email'>('ldap');
  let twoFactorStep = $state(false);
  let useRecoveryCode = $state(false);
  let twoFactorCodeError = $state<string | undefined>();
  let passkeySupported = $state(true);
  let passkeyAvailable = $state(false);
  let passkeyPending = $state(false);
  let twoFactorPasskeyPending = $state(false);
  let redirecting = $state(false);
  let autofillController: AbortController | null = null;

  const authService = new AuthService(apiClient);

  const auth = $derived(data.authProviders);
  const ldapMode = $derived(!!auth.ldap && (!auth.passwordSignIn || signInMethod === 'ldap'));
  const showForm = $derived(auth.passwordSignIn || !!auth.ldap);
  const canSignUp = $derived(
    auth.passwordSignIn || (auth.externalSignUp && auth.providers.length > 0)
  );

  const handledCodes = [
    'Users.SignIn.Failed',
    'Users.SignIn.LockedOut',
    'Users.SignIn.NotAllowed',
    'Users.SignIn.TwoFactorRequired',
    'Users.AccountDeleted',
    'Users.External.Failed',
    'Users.External.ProviderNotAvailable',
    'Users.External.EmailMissing',
    'Users.External.EmailNotVerified',
    'Users.External.AccountNotConfirmed',
    'Users.External.SignUpDisabled',
    'Users.External.ConfirmationSent',
    'Users.PasswordAuthentication.Disabled',
    'Users.TwoFactor.SignInExpired',
    'Users.Passkeys.NotRecognized',
    'Users.Passkeys.Expired'
  ];

  function showSignInInfo(code: string | null | undefined): boolean {
    if (code === 'Users.SignIn.TwoFactorRequired') {
      twoFactorStep = true;
      return true;
    }
    if (code && handledCodes.includes(code)) {
      signInInfoCode = code;
      showSignInInfoModal = true;
      return true;
    }
    return false;
  }

  onMount(() => {
    passkeySupported = passkeysSupported();
    showSignInInfo(new URLSearchParams(window.location.search).get('error'));
  });

  function problemOf(response: { ok: false }): ProblemDetails | null {
    return 'title' in response ? (response as unknown as ProblemDetails) : null;
  }

  function goToBoards() {
    redirecting = true;
    setTimeout(() => {
      window.location.href = '/';
    }, 300);
  }

  async function startPasskeyAutofill() {
    autofillController?.abort();
    const controller = new AbortController();
    autofillController = controller;

    if (!(await passkeyAutofillSupported())) return;
    const options = await authService.passkeySignInOptions();
    if (!options.ok || controller.signal.aborted) return;

    try {
      const credential = await getPasskey(options.options, {
        autofill: true,
        signal: controller.signal
      });
      await completePasskeySignIn(credential, options.state);
    } catch (error) {
      if (!isPasskeyDismissed(error)) {
        showSignInInfo('Users.Passkeys.NotRecognized');
      }
    }
  }

  async function completePasskeySignIn(credential: PasskeyJson, state: string) {
    passkeyPending = true;
    const response = await authService.passkeySignIn({
      credential,
      state,
      rememberMe: form.values.rememberMe
    });

    if (response.ok) {
      goToBoards();
      return;
    }

    passkeyPending = false;
    const problem = problemOf(response);
    if (!showSignInInfo(problem?.title)) {
      errorStore.addError(
        problem?.title ?? null,
        problem?.detail ?? 'Signing in with a passkey failed.'
      );
    }
    startPasskeyAutofill();
  }

  async function signInWithPasskey() {
    if (passkeyPending) return;
    autofillController?.abort();
    passkeyPending = true;

    try {
      const options = await authService.passkeySignInOptions();
      if (!options.ok) {
        passkeyPending = false;
        const problem = problemOf(options);
        if (!showSignInInfo(problem?.title)) {
          errorStore.addError(
            problem?.title ?? null,
            problem?.detail ?? 'Signing in with a passkey failed.'
          );
        }
        return;
      }

      const credential = await getPasskey(options.options);
      await completePasskeySignIn(credential, options.state);
    } catch (error) {
      passkeyPending = false;
      if (!isPasskeyDismissed(error)) {
        showSignInInfo('Users.Passkeys.NotRecognized');
      }
      startPasskeyAutofill();
    }
  }

  function showTwoFactorPasskeyProblem(response: { ok: false }) {
    const problem = problemOf(response);
    if (
      problem?.title === 'Users.Passkeys.NotRecognized' ||
      problem?.title === 'Users.Passkeys.Expired'
    ) {
      twoFactorCodeError = 'The passkey could not be used. Try again or enter a code.';
      return;
    }
    if (problem?.title === 'Users.TwoFactor.SignInExpired') {
      leaveTwoFactorStep();
    }
    if (!showSignInInfo(problem?.title)) {
      errorStore.addError(
        problem?.title ?? null,
        problem?.detail ?? 'Signing in with a passkey failed.'
      );
    }
  }

  async function useTwoFactorPasskey() {
    if (twoFactorPasskeyPending || redirecting) return;
    twoFactorPasskeyPending = true;
    twoFactorCodeError = undefined;

    try {
      const options = await authService.twoFactorPasskeyOptions();
      if (!options.ok) {
        twoFactorPasskeyPending = false;
        showTwoFactorPasskeyProblem(options);
        return;
      }

      const credential = await getPasskey(options.options);
      const response = await authService.twoFactorSignIn({
        passkeyCredential: credential,
        passkeyState: options.state,
        rememberMe: form.values.rememberMe,
        rememberDevice: twoFactorForm.values.rememberDevice
      });

      if (response.ok) {
        goToBoards();
        return;
      }

      twoFactorPasskeyPending = false;
      showTwoFactorPasskeyProblem(response);
    } catch (error) {
      twoFactorPasskeyPending = false;
      if (!isPasskeyDismissed(error)) {
        twoFactorCodeError = 'The passkey could not be used. Try again or enter a code.';
      }
    }
  }

  $effect(() => {
    if (passkeySupported && auth.passwordSignIn && !ldapMode && !twoFactorStep) {
      startPasskeyAutofill();
      return () => autofillController?.abort();
    }
  });

  const form = createForm({
    initialValues: {
      email: '',
      password: '',
      rememberMe: false
    },
    validate: (values) => {
      const errors: Record<string, string> = {};
      const fieldName = ldapMode ? 'User name' : 'Email';
      if (!values.email) {
        errors.email = `${fieldName} is required.`;
      } else if (values.email.length > authConfig.email.maxLength) {
        errors.email = `${fieldName} must be less than ${authConfig.email.maxLength} characters.`;
      } else if (!ldapMode) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(values.email)) {
          errors.email = 'Please enter a valid email address.';
        }
      }

      if (!values.password) {
        errors.password = 'Password is required.';
      }
      return errors;
    },
    onSubmit: async (values) => {
      const response = ldapMode
        ? await authService.ldapSignIn({
            userName: values.email,
            password: values.password,
            rememberMe: values.rememberMe
          })
        : await authService.signIn(values);
      passkeyAvailable =
        !response.ok && (response as { passkeyAvailable?: boolean }).passkeyAvailable === true;
      return response;
    },
    onSuccess: goToBoards,
    onError: (problem: ProblemDetails) => showSignInInfo(problem.title)
  });

  const twoFactorForm = createForm({
    initialValues: {
      code: '',
      rememberDevice: false
    },
    validate: (values) => {
      const errors: Record<string, string> = {};
      if (useRecoveryCode && values.code.length !== 10) {
        errors.code = 'Enter the 10-character recovery code.';
      } else if (!useRecoveryCode && values.code.length !== 6) {
        errors.code = 'Enter the 6-digit code from your app.';
      }
      return errors;
    },
    onSubmit: async (values) => {
      const code = values.code.trim();
      return await authService.twoFactorSignIn({
        ...(useRecoveryCode ? { recoveryCode: code } : { code }),
        rememberMe: form.values.rememberMe,
        rememberDevice: !useRecoveryCode && values.rememberDevice
      });
    },
    onSuccess: goToBoards,
    onError: (problem: ProblemDetails) => {
      if (problem.title === 'Users.TwoFactor.InvalidCode') {
        twoFactorCodeError = useRecoveryCode
          ? 'This recovery code is not valid or has already been used.'
          : (problem.detail ?? 'The code is not valid.');
        return true;
      }
      if (problem.title === 'Users.TwoFactor.SignInExpired') {
        twoFactorStep = false;
      }
      return showSignInInfo(problem.title);
    }
  });

  function toggleRecoveryCode() {
    useRecoveryCode = !useRecoveryCode;
    twoFactorForm.values.code = '';
    twoFactorCodeError = undefined;
  }

  async function submitTwoFactor(event?: Event) {
    event?.preventDefault();
    if (redirecting) return;
    await twoFactorForm.handleSubmit();
    if (twoFactorCodeError) {
      twoFactorForm.values.code = '';
    }
  }

  function leaveTwoFactorStep() {
    twoFactorStep = false;
    useRecoveryCode = false;
    twoFactorCodeError = undefined;
    twoFactorForm.reset();
  }
</script>

<svelte:head>
  <title>Snapflow | Sign in</title>
</svelte:head>

<SplitLayout>
  {#snippet header()}
    <GoBackButton href="/" />
  {/snippet}

  <div class="mb-3 sm:mb-8">
    <h1 class="mb-2 text-2xl font-semibold text-gray-800 sm:text-3xl dark:text-white/90">
      {twoFactorStep ? 'Two-factor authentication' : 'Welcome back!'}
    </h1>
    <p class="text-sm text-gray-500 dark:text-gray-400">
      {#if twoFactorStep}
        {#if useRecoveryCode}
          Enter one of the recovery codes you saved when you turned on two-factor authentication.
        {:else if passkeyAvailable && passkeySupported}
          Enter the 6-digit code from your authenticator app or use a passkey.
        {:else}
          Enter the 6-digit code from your authenticator app.
        {/if}
      {:else if ldapMode}
        Enter your {auth.ldap?.displayName} user name and password to sign in.
      {:else if showForm}
        Enter your email and password to sign in.
      {:else}
        Choose how you want to sign in.
      {/if}
    </p>
  </div>

  {#if twoFactorStep}
    <form onsubmit={submitTwoFactor} novalidate class="space-y-5">
      {#if useRecoveryCode}
        <CodeInput
          id="twoFactorCode"
          name="recoveryCode"
          ariaLabel="Recovery code"
          kind="alphanumeric"
          length={10}
          autofocus
          bind:value={twoFactorForm.values.code}
          error={twoFactorCodeError ?? twoFactorForm.errors.code}
          onValueChange={() => (twoFactorCodeError = undefined)}
          onComplete={() => submitTwoFactor()}
        />
      {:else}
        <CodeInput
          id="twoFactorCode"
          name="code"
          ariaLabel="Authentication code"
          autofocus
          bind:value={twoFactorForm.values.code}
          error={twoFactorCodeError ?? twoFactorForm.errors.code}
          onValueChange={() => (twoFactorCodeError = undefined)}
          onComplete={() => submitTwoFactor()}
        />
      {/if}

      {#if !useRecoveryCode}
        <Checkbox
          bind:checked={twoFactorForm.values.rememberDevice}
          label="Don't ask for a code on this device"
        />
      {/if}

      <Button
        type="submit"
        variant="primary"
        size="md"
        class="w-full justify-center"
        disabled={!twoFactorForm.values.code || twoFactorForm.isSubmitting || redirecting}
        isLoading={twoFactorForm.isSubmitting || redirecting}
        loadingText="Verifying"
      >
        Verify
      </Button>

      {#if passkeyAvailable && passkeySupported && !useRecoveryCode}
        <Button
          variant="outline"
          size="md"
          class="w-full justify-center"
          startIcon={Fingerprint}
          disabled={twoFactorPasskeyPending || twoFactorForm.isSubmitting || redirecting}
          isLoading={twoFactorPasskeyPending}
          loadingText="Waiting for your passkey"
          onclick={useTwoFactorPasskey}
        >
          Use a passkey instead
        </Button>
      {/if}

      <div class="flex items-center justify-between gap-4 text-sm">
        <button
          type="button"
          class="rounded-sm text-brand-500 underline underline-offset-2 transition-all duration-200 hover:text-brand-600 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500 dark:text-brand-400 dark:hover:text-brand-500"
          onclick={toggleRecoveryCode}
        >
          {useRecoveryCode ? 'Use the authenticator app' : 'Use a recovery code'}
        </button>
        <button
          type="button"
          class="rounded-sm text-gray-500 underline underline-offset-2 transition-all duration-200 hover:text-gray-700 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500 dark:text-gray-400 dark:hover:text-gray-200"
          onclick={leaveTwoFactorStep}
        >
          Back to sign in
        </button>
      </div>
    </form>
  {:else}
    {#if auth.providers.length > 0}
      <ExternalProviderButtons
        providers={auth.providers}
        rememberMe={form.values.rememberMe}
        action="Sign in"
        showDivider={showForm}
      />
      {#if !showForm}
        <div class="mt-5">
          <Checkbox bind:checked={form.values.rememberMe} label="Keep me logged in" />
        </div>
      {/if}
    {/if}

    {#if showForm}
      {#if auth.passwordSignIn && auth.ldap}
        <SegmentedControl
          class="mb-5"
          options={[
            { value: 'ldap', label: auth.ldap.displayName },
            { value: 'email', label: 'Email' }
          ]}
          bind:value={signInMethod}
          onValueChange={() => (form.errors.email = undefined)}
        />
      {/if}
      <form onsubmit={form.handleSubmit} novalidate class="space-y-5">
        <InputTextField
          id="email"
          name={ldapMode ? 'username' : 'email'}
          type={ldapMode ? 'text' : 'email'}
          label={ldapMode ? 'User name' : 'Email'}
          placeholder={ldapMode ? 'Enter your user name' : 'info@example.com'}
          autocomplete={ldapMode ? 'username' : 'username webauthn'}
          bind:value={form.values.email}
          error={form.errors.email}
          leftIcon={ldapMode ? User : Mail}
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
          {#if !ldapMode}
            <a
              href="/forgot-password"
              class="rounded-sm text-sm whitespace-nowrap text-brand-500 underline underline-offset-2 transition-all duration-200 hover:text-brand-600 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500 dark:text-brand-400 dark:hover:text-brand-500"
            >
              Forgot password?
            </a>
          {/if}
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
    {/if}

    {#if auth.passwordSignIn && passkeySupported}
      <Button
        variant="outline"
        size="md"
        class="mt-3 w-full justify-center"
        startIcon={Fingerprint}
        disabled={passkeyPending}
        isLoading={passkeyPending}
        loadingText="Signing in"
        onclick={signInWithPasskey}
      >
        Sign in with a passkey
      </Button>
    {/if}
  {/if}

  {#snippet footer()}
    {#if canSignUp && !twoFactorStep}
      <p class="text-center text-sm font-normal text-gray-800 sm:text-start dark:text-gray-100">
        Don't have an account?
        <a
          href="/sign-up"
          class="rounded-sm text-brand-500 underline underline-offset-2 transition-all duration-200 hover:text-brand-600 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500 dark:text-brand-400 dark:hover:text-brand-500"
        >
          Sign up
        </a>
      </p>
    {/if}
  {/snippet}
</SplitLayout>

<SignInModal
  bind:open={showSignInInfoModal}
  problemCode={signInInfoCode}
  userEmail={ldapMode ? '' : form.values.email}
/>
