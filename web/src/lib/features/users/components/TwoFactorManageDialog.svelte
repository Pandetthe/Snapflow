<script lang="ts">
  import { Dialog } from 'bits-ui';
  import { RefreshCw, ShieldOff } from 'lucide-svelte';
  import { Button, InputTextField, ResponsiveDialog } from '$lib/ui/components';
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

  const form = createForm({
    initialValues: { code: '' },
    validate: (values) => {
      const errors: Record<string, string> = {};
      if (!values.code.trim()) {
        errors.code = useRecoveryCode ? 'Enter a recovery code.' : 'Enter the code from your app.';
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
        codeError = problem.detail ?? 'The code is not valid.';
        return true;
      }
      return false;
    }
  });

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

<ResponsiveDialog bind:open size="sm">
  <div class="space-y-6">
    {#if recoveryCodes}
      <div class="space-y-2">
        <Dialog.Title class="text-xl font-bold tracking-tight text-gray-900 dark:text-white">
          Your new recovery codes
        </Dialog.Title>
        <Dialog.Description class="text-sm text-gray-500 dark:text-gray-400">
          The old codes no longer work. Keep these somewhere safe, they won't be shown again.
        </Dialog.Description>
      </div>
      <RecoveryCodes codes={recoveryCodes} />
      <Button class="w-full justify-center" onclick={close}>I have saved them</Button>
    {:else}
      <div class="space-y-2">
        <Dialog.Title class="text-xl font-bold tracking-tight text-gray-900 dark:text-white">
          Two-factor authentication
        </Dialog.Title>
        <Dialog.Description class="text-sm text-gray-500 dark:text-gray-400">
          It's on.
          {#if recoveryCodesLeft === 0}
            You have no recovery codes left, so generate new ones.
          {:else}
            You have {recoveryCodesLeft} recovery {recoveryCodesLeft === 1 ? 'code' : 'codes'} left.
          {/if}
          Confirm either change with a code.
        </Dialog.Description>
      </div>

      <form onsubmit={form.handleSubmit} novalidate class="space-y-4">
        <InputTextField
          id="twoFactorManageCode"
          name={useRecoveryCode ? 'recoveryCode' : 'code'}
          label={useRecoveryCode ? 'Recovery code' : 'Code from the app'}
          placeholder={useRecoveryCode ? 'XXXXX-XXXXX' : '123456'}
          inputmode={useRecoveryCode ? 'text' : 'numeric'}
          autocomplete={useRecoveryCode ? 'off' : 'one-time-code'}
          maxlength={useRecoveryCode ? 16 : 7}
          bind:value={form.values.code}
          error={codeError ?? form.errors.code}
          oninput={() => (codeError = undefined)}
        />

        <button
          type="button"
          class="rounded-sm text-sm text-brand-500 underline underline-offset-2 transition-all duration-200 hover:text-brand-600 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500 dark:text-brand-400 dark:hover:text-brand-500"
          onclick={toggleRecoveryCode}
        >
          {useRecoveryCode ? 'Use the authenticator app' : 'Lost your phone? Use a recovery code'}
        </button>

        <div class="flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">
          {#if !useRecoveryCode}
            <Button
              type="submit"
              variant="outline"
              size="sm"
              class="justify-center"
              disabled={form.isSubmitting}
              isLoading={form.isSubmitting && action === 'regenerate'}
              loadingText="Generating..."
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
            size="sm"
            class="justify-center"
            disabled={form.isSubmitting}
            isLoading={form.isSubmitting && action === 'disable'}
            loadingText="Turning off..."
            startIcon={ShieldOff}
            onclick={() => {
              action = 'disable';
            }}
          >
            Turn off
          </Button>
        </div>
      </form>
    {/if}
  </div>
</ResponsiveDialog>
