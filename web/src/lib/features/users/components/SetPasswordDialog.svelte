<script lang="ts">
  import { AppDialog, Button, InputTextField } from '$lib/ui/components';
  import PasswordStrength from '$lib/features/auth/components/PasswordStrength.svelte';
  import { Check, KeyRound } from '@lucide/svelte';
  import type { UsersService } from '../api/users';
  import { validatePassword } from '$lib/features/auth/validation';
  import { createForm } from '$lib/ui/utils';

  let {
    open = $bindable(false),
    email,
    usersService,
    onChange
  }: {
    open: boolean;
    email: string;
    usersService: UsersService;
    onChange: () => void;
  } = $props();

  let setSuccess = $state(false);

  const form = createForm({
    initialValues: { newPassword: '', confirmPassword: '' },
    validate: (values) => {
      const errors: Record<string, string> = {};
      const passwordError = validatePassword(values.newPassword);
      if (passwordError) errors.newPassword = passwordError;
      if (values.confirmPassword !== values.newPassword)
        errors.confirmPassword = 'Passwords do not match.';
      return errors;
    },
    onSubmit: (values) => usersService.setPassword({ newPassword: values.newPassword }),
    onSuccess: () => {
      setSuccess = true;
      form.reset();
    }
  });

  function close() {
    open = false;
  }

  $effect(() => {
    if (open) {
      setSuccess = false;
      form.reset();
    } else if (setSuccess) {
      setSuccess = false;
      onChange();
    }
  });
</script>

<AppDialog
  bind:open
  size="sm"
  icon={setSuccess ? KeyRound : undefined}
  tone="success"
  title={setSuccess ? 'Password set' : 'Set a password'}
  description={setSuccess
    ? 'You can now sign in with your email and password.'
    : 'Add a password so you can sign in with your email, not only with connected accounts.'}
  onsubmit={setSuccess ? undefined : form.handleSubmit}
>
  {#if !setSuccess}
    <div class="space-y-4">
      <input type="text" name="username" autocomplete="username" value={email} readonly hidden />
      <InputTextField
        id="setNewPassword"
        name="newPassword"
        label="New password"
        type="password"
        autocomplete="new-password"
        showPasswordToggle
        required
        bind:value={form.values.newPassword}
        error={form.errors.newPassword}
      />
      <PasswordStrength password={form.values.newPassword} />
      <InputTextField
        id="setConfirmPassword"
        name="confirmPassword"
        label="Confirm password"
        type="password"
        autocomplete="new-password"
        showPasswordToggle
        required
        bind:value={form.values.confirmPassword}
        error={form.errors.confirmPassword}
      />
    </div>
  {/if}

  {#snippet actions()}
    {#if setSuccess}
      <Button haptic="medium" onclick={close}>Close</Button>
    {:else}
      <Button variant="ghost" haptic="light" onclick={close}>Cancel</Button>
      <Button
        type="submit"
        disabled={form.isSubmitting}
        isLoading={form.isSubmitting}
        loadingText="Saving"
        startIcon={Check}
      >
        Set password
      </Button>
    {/if}
  {/snippet}
</AppDialog>
