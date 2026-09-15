<script lang="ts">
  import { AppDialog, Button, InputTextField } from '$lib/ui/components';
  import PasswordStrength from '$lib/features/auth/components/PasswordStrength.svelte';
  import { Check, KeyRound } from 'lucide-svelte';
  import type { UsersService } from '../api/users';
  import { validatePassword } from '$lib/features/auth/validation';
  import { createForm } from '$lib/ui/utils';

  let {
    open = $bindable(false),
    usersService
  }: {
    open: boolean;
    usersService: UsersService;
  } = $props();

  let changeSuccess = $state(false);

  const form = createForm({
    initialValues: { currentPassword: '', newPassword: '' },
    validate: (values) => {
      const errors: Record<string, string> = {};
      if (!values.currentPassword) errors.currentPassword = 'Current password is required.';
      const passwordError = validatePassword(values.newPassword);
      if (passwordError) errors.newPassword = passwordError;
      return errors;
    },
    onSubmit: (values) =>
      usersService.changePassword({
        currentPassword: values.currentPassword,
        newPassword: values.newPassword
      }),
    mapValidationError: (err) =>
      err.code === 'PasswordMismatch' ? { ...err, propertyName: 'currentPassword' } : err,
    onSuccess: () => {
      changeSuccess = true;
      form.reset();
    }
  });

  function close() {
    open = false;
  }

  $effect(() => {
    if (open) {
      changeSuccess = false;
      form.reset();
    }
  });
</script>

<AppDialog
  bind:open
  size="sm"
  icon={changeSuccess ? KeyRound : undefined}
  tone="success"
  title={changeSuccess ? 'Password updated' : 'Change password'}
  description={changeSuccess
    ? 'Your password has been changed successfully.'
    : 'Update your password to keep your account secure.'}
  onsubmit={changeSuccess ? undefined : form.handleSubmit}
>
  {#if !changeSuccess}
    <div class="space-y-4">
      <InputTextField
        id="currentPassword"
        name="currentPassword"
        label="Current password"
        type="password"
        showPasswordToggle
        required
        bind:value={form.values.currentPassword}
        error={form.errors.currentPassword}
      />
      <InputTextField
        id="newPassword"
        name="newPassword"
        label="New password"
        type="password"
        showPasswordToggle
        required
        bind:value={form.values.newPassword}
        error={form.errors.newPassword}
      />
      <PasswordStrength password={form.values.newPassword} />
    </div>
  {/if}

  {#snippet actions()}
    {#if changeSuccess}
      <Button onclick={close}>Close</Button>
    {:else}
      <Button variant="ghost" onclick={close}>Cancel</Button>
      <Button
        type="submit"
        disabled={form.isSubmitting}
        isLoading={form.isSubmitting}
        loadingText="Updating"
        startIcon={Check}
      >
        Update password
      </Button>
    {/if}
  {/snippet}
</AppDialog>
