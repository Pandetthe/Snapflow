<script lang="ts">
  import { Dialog } from 'bits-ui';
  import QRCode from 'qrcode';
  import { LoaderCircle, ShieldCheck } from 'lucide-svelte';
  import { Button, InputTextField, ResponsiveDialog } from '$lib/ui/components';
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
      if (!values.code.trim()) errors.code = 'Enter the code from your app.';
      return errors;
    },
    onSubmit: (values) => usersService.enableTwoFactor({ code: values.code.trim() }),
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
    qrCode = await QRCode.toDataURL(response.value.authenticatorUri, { margin: 1, width: 192 });
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

<ResponsiveDialog bind:open size="sm">
  <div class="space-y-6">
    {#if recoveryCodes}
      <div class="space-y-2">
        <Dialog.Title class="text-xl font-bold tracking-tight text-gray-900 dark:text-white">
          Save your recovery codes
        </Dialog.Title>
        <Dialog.Description class="text-sm text-gray-500 dark:text-gray-400">
          Two-factor authentication is on. If you lose your phone, each of these codes signs you in
          once. Keep them somewhere safe, they won't be shown again.
        </Dialog.Description>
      </div>
      <RecoveryCodes codes={recoveryCodes} />
      <Button class="w-full justify-center" onclick={close}>I have saved them</Button>
    {:else}
      <div class="space-y-2">
        <Dialog.Title class="text-xl font-bold tracking-tight text-gray-900 dark:text-white">
          Set up two-factor authentication
        </Dialog.Title>
        <Dialog.Description class="text-sm text-gray-500 dark:text-gray-400">
          Scan the QR code with an authenticator app such as Google Authenticator, Microsoft
          Authenticator or 1Password, then enter the 6-digit code it shows.
        </Dialog.Description>
      </div>

      {#if setupFailed}
        <p class="text-sm text-error-600 dark:text-error-400">
          The setup could not be started. Close this window and try again.
        </p>
      {:else if !setup || !qrCode}
        <div class="flex justify-center py-12 text-gray-400">
          <LoaderCircle size={24} class="animate-spin" />
        </div>
      {:else}
        <div class="flex flex-col items-center gap-3">
          <img
            src={qrCode}
            alt="QR code to scan with your authenticator app"
            width="192"
            height="192"
            class="rounded-lg bg-white p-2"
          />
          <div class="w-full text-center">
            <p class="text-xs text-gray-400 dark:text-gray-500">
              Can't scan it? Enter this key in the app instead:
            </p>
            <p
              class="mt-1 font-mono text-sm tracking-wide text-gray-900 select-all dark:text-white"
            >
              {formattedKey}
            </p>
          </div>
        </div>

        <form onsubmit={form.handleSubmit} novalidate class="space-y-4">
          <InputTextField
            id="twoFactorSetupCode"
            name="code"
            label="Code from the app"
            placeholder="123456"
            inputmode="numeric"
            autocomplete="one-time-code"
            maxlength={7}
            bind:value={form.values.code}
            error={codeError ?? form.errors.code}
            oninput={() => (codeError = undefined)}
          />
          <div class="flex justify-end gap-2">
            <Button variant="ghost" size="sm" onclick={close}>Cancel</Button>
            <Button
              type="submit"
              variant="primary"
              size="sm"
              disabled={form.isSubmitting}
              isLoading={form.isSubmitting}
              loadingText="Turning on..."
              startIcon={ShieldCheck}
            >
              Turn on
            </Button>
          </div>
        </form>
      {/if}
    {/if}
  </div>
</ResponsiveDialog>
