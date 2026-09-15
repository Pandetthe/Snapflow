<script lang="ts">
  import { KeyRound, RefreshCw, ShieldOff } from 'lucide-svelte';
  import { AppDialog, Button, CodeInput } from '$lib/ui/components';
  import { createForm } from '$lib/ui/utils';
  import type { Response as AppResponse } from '$lib/core/types/app';
  import type { RecoveryCodesResponse, UsersService } from '../api/users';
  import RecoveryCodes from './RecoveryCodes.svelte';

  let {
    open = $bindable(false),
    usersService,
    recoveryCodesLeft,
    onChange
  }: {
    open: boolean;
    usersService: UsersService;
    recoveryCodesLeft: number;
    onChange: () => void;
  } = $props();

  let action = $state<'disable' | 'regenerate'>('disable');
  let useRecoveryCode = $state(false);
  let recoveryCodes = $state<string[] | null>(null);
  let codeError = $state<string | undefined>();
  let changed = false;

  const statusText = $derived(
    recoveryCodesLeft === 0
      ? "It's on, but you have no recovery codes left. Generate new ones."
      : `It's on. You have ${recoveryCodesLeft} recovery ${recoveryCodesLeft === 1 ? 'code' : 'codes'} left.`
  );

  const form = createForm({
    initialValues: { code: '' },
    validate: (values) => {
      const errors: Record<string, string> = {};
      if (useRecoveryCode && values.code.length !== 10) {
        errors.code = 'Enter the 10-character recovery code.';
      } else if (!useRecoveryCode && values.code.length !== 6) {
        errors.code = 'Enter the 6-digit code from your app.';
      }
      return errors;
    },
    onSubmit: async (values): Promise<AppResponse<RecoveryCodesResponse | undefined>> => {
      const code = values.code.trim();
      if (action === 'regenerate') {
        return usersService.regenerateRecoveryCodes({ code });
      }
      const response = await usersService.disableTwoFactor(
        useRecoveryCode ? { recoveryCode: code } : { code }
      );
      return response.ok ? { ok: true, value: undefined } : response;
    },
    onSuccess: (response) => {
      changed = true;
      if (response) {
        recoveryCodes = response.recoveryCodes;
      } else {
        open = false;
      }
    },
    onError: (problem) => {
      if (problem.title === 'Users.TwoFactor.InvalidCode') {
        codeError = useRecoveryCode
          ? 'This recovery code is not valid or has already been used.'
          : (problem.detail ?? 'The code is not valid.');
        return true;
      }
      return false;
    }
  });

  async function submitCode(event?: Event) {
    event?.preventDefault();
    await form.handleSubmit();
    if (codeError) {
      form.values.code = '';
    }
  }

  function toggleRecoveryCode() {
    useRecoveryCode = !useRecoveryCode;
    form.values.code = '';
    codeError = undefined;
  }

  function close() {
    open = false;
  }

  $effect(() => {
    if (open) {
      action = 'disable';
      useRecoveryCode = false;
      recoveryCodes = null;
      codeError = undefined;
      form.reset();
    } else if (changed) {
      changed = false;
      onChange();
    }
  });
</script>

<AppDialog
  bind:open
  size="sm"
  icon={recoveryCodes ? KeyRound : undefined}
  tone="success"
  title={recoveryCodes ? 'New recovery codes' : 'Two-factor authentication'}
  description={recoveryCodes
    ? "The old codes no longer work. Save these somewhere safe, they won't be shown again."
    : statusText}
  onsubmit={recoveryCodes ? undefined : submitCode}
>
  {#if recoveryCodes}
    <RecoveryCodes codes={recoveryCodes} />
  {:else}
    <div class="space-y-3">
      {#if useRecoveryCode}
        <CodeInput
          id="twoFactorManageCode"
          name="recoveryCode"
          label="Recovery code"
          kind="alphanumeric"
          length={10}
          bind:value={form.values.code}
          error={codeError ?? form.errors.code}
          onValueChange={() => (codeError = undefined)}
        />
      {:else}
        <CodeInput
          id="twoFactorManageCode"
          name="code"
          label="Code from the app"
          bind:value={form.values.code}
          error={codeError ?? form.errors.code}
          onValueChange={() => (codeError = undefined)}
        />
      {/if}

      <button
        type="button"
        class="rounded-sm text-sm text-brand-500 underline underline-offset-2 transition-all duration-200 hover:text-brand-600 dark:text-brand-400 dark:hover:text-brand-500"
        onclick={toggleRecoveryCode}
      >
        {useRecoveryCode ? 'Use the authenticator app' : 'Lost your phone? Use a recovery code'}
      </button>
    </div>
  {/if}

  {#snippet actions()}
    {#if recoveryCodes}
      <Button onclick={close}>I have saved them</Button>
    {:else}
      {#if !useRecoveryCode}
        <Button
          type="submit"
          variant="outline"
          disabled={form.isSubmitting}
          isLoading={form.isSubmitting && action === 'regenerate'}
          loadingText="Generating"
          startIcon={RefreshCw}
          onclick={() => {
            action = 'regenerate';
          }}
        >
          New recovery codes
        </Button>
      {/if}
      <Button
        type="submit"
        variant="danger"
        disabled={form.isSubmitting}
        isLoading={form.isSubmitting && action === 'disable'}
        loadingText="Turning off"
        startIcon={ShieldOff}
        onclick={() => {
          action = 'disable';
        }}
      >
        Turn off
      </Button>
    {/if}
  {/snippet}
</AppDialog>
