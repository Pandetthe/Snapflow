<script lang="ts">
  import QRCode from 'qrcode';
  import { ShieldCheck } from '@lucide/svelte';
  import { AppDialog, Button, CodeInput, Skeleton } from '$lib/ui/components';
  import { createForm } from '$lib/ui/utils';
  import type { AuthenticatorSetup, UsersService } from '../api/users';
  import RecoveryCodes from './RecoveryCodes.svelte';

  let {
    open = $bindable(false),
    usersService,
    onChange
  }: {
    open: boolean;
    usersService: UsersService;
    onChange: () => void;
  } = $props();

  let setup = $state<AuthenticatorSetup | null>(null);
  let qrCode = $state<string | null>(null);
  let setupFailed = $state(false);
  let recoveryCodes = $state<string[] | null>(null);
  let codeError = $state<string | undefined>();
  let changed = false;

  const formattedKey = $derived(
    setup?.sharedKey
      .toLowerCase()
      .match(/.{1,4}/g)
      ?.join(' ') ?? ''
  );

  const form = createForm({
    initialValues: { code: '' },
    validate: (values) => {
      const errors: Record<string, string> = {};
      if (values.code.length !== 6) errors.code = 'Enter the 6-digit code from your app.';
      return errors;
    },
    onSubmit: (values) => usersService.enableTwoFactor({ code: values.code }),
    onSuccess: (response) => {
      recoveryCodes = response.recoveryCodes;
      changed = true;
    },
    onError: (problem) => {
      if (problem.title === 'Users.TwoFactor.InvalidCode') {
        codeError = problem.detail ?? 'The code is not valid.';
        return true;
      }
      return false;
    }
  });

  async function submitCode(event?: Event) {
    event?.preventDefault();
    if (!setup) return;

    await form.handleSubmit();
    if (codeError) {
      form.values.code = '';
    }
  }

  async function loadSetup() {
    setup = null;
    qrCode = null;
    setupFailed = false;

    const response = await usersService.setupAuthenticator();
    if (!response.ok) {
      setupFailed = true;
      return;
    }

    setup = response.value;
    qrCode = await QRCode.toDataURL(response.value.authenticatorUri, { margin: 1, width: 224 });
  }

  function close() {
    open = false;
  }

  $effect(() => {
    if (open) {
      recoveryCodes = null;
      codeError = undefined;
      form.reset();
      loadSetup();
    } else if (changed) {
      changed = false;
      onChange();
    }
  });
</script>

<AppDialog
  bind:open
  size="sm"
  icon={recoveryCodes ? ShieldCheck : undefined}
  tone="success"
  title={recoveryCodes ? 'Two-factor authentication is on' : 'Set up two-factor authentication'}
  description={recoveryCodes
    ? "Save these recovery codes somewhere safe. Each one signs you in once if you lose your phone, and they won't be shown again."
    : 'Scan the QR code with an authenticator app, then enter the 6-digit code it shows.'}
  onsubmit={recoveryCodes ? undefined : submitCode}
>
  {#if recoveryCodes}
    <RecoveryCodes codes={recoveryCodes} />
  {:else}
    <div class="space-y-5">
      <div
        class="flex items-center gap-4 rounded-xl border border-gray-200 bg-gray-50 p-4 dark:border-gray-800 dark:bg-gray-900/50"
      >
        {#if setupFailed}
          <p class="text-sm text-error-600 dark:text-error-400">
            The setup could not be started. Close this window and try again.
          </p>
        {:else if setup && qrCode}
          <img
            src={qrCode}
            alt="QR code to scan with your authenticator app"
            width="112"
            height="112"
            class="size-28 shrink-0 rounded-md bg-white p-1.5"
          />
          <div class="min-w-0 space-y-1">
            <p class="text-xs text-gray-500 dark:text-gray-400">Can't scan it? Enter this key:</p>
            <p class="font-mono text-sm break-all text-gray-900 select-all dark:text-white">
              {formattedKey}
            </p>
          </div>
        {:else}
          <Skeleton class="size-28 shrink-0" />
          <div class="w-full space-y-2">
            <Skeleton class="h-3 w-3/4" />
            <Skeleton class="h-4 w-full" />
          </div>
        {/if}
      </div>

      <CodeInput
        id="twoFactorSetupCode"
        name="code"
        label="Code from the app"
        bind:value={form.values.code}
        error={codeError ?? form.errors.code}
        onValueChange={() => (codeError = undefined)}
        onComplete={() => submitCode()}
      />
    </div>
  {/if}

  {#snippet actions()}
    {#if recoveryCodes}
      <Button haptic="medium" onclick={close}>I have saved them</Button>
    {:else}
      <Button variant="ghost" haptic="light" onclick={close}>Cancel</Button>
      <Button
        type="submit"
        disabled={!setup || form.isSubmitting}
        isLoading={form.isSubmitting}
        loadingText="Turning on"
        startIcon={ShieldCheck}
      >
        Turn on
      </Button>
    {/if}
  {/snippet}
</AppDialog>
