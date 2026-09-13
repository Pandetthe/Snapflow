<script lang="ts">
  import { Dialog } from 'bits-ui';
  import { Button, InputTextField, ResponsiveDialog } from '$lib/ui/components';
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

<ResponsiveDialog bind:open size="sm">
  <div class="space-y-6">
    {#if changeSuccess}
      <div class="space-y-6 text-center pt-4">
        <div class="mx-auto flex h-16 w-16 items-center justify-center rounded-full shadow-inner transition-all duration-500 bg-success-100 dark:bg-success-900/40">
          <KeyRound size={32} class="transition-transform duration-500 text-success-600 dark:text-success-400" />
        </div>
        <div class="space-y-2">
          <Dialog.Title class="text-xl font-bold tracking-tight text-gray-900 dark:text-white">
            Password updated
          </Dialog.Title>
          <Dialog.Description class="text-base leading-relaxed text-gray-500 dark:text-gray-400">
            Your password has been changed successfully.
          </Dialog.Description>
        </div>
        <div class="pt-4">
          <Button class="w-full justify-center" onclick={close}>Close</Button>
        </div>
      </div>
    {:else}
      <div class="space-y-2">
        <Dialog.Title class="text-xl font-bold tracking-tight text-gray-900 dark:text-white">
          Change password
        </Dialog.Title>
        <Dialog.Description class="text-sm text-gray-500 dark:text-gray-400">
          Update your password to keep your account secure.
        </Dialog.Description>
      </div>
      <form onsubmit={form.handleSubmit} novalidate class="space-y-4">
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
        <div class="flex justify-end gap-2">
          <Button variant="ghost" size="sm" onclick={close}>Cancel</Button>
          <Button
            type="submit"
            variant="primary"
            size="sm"
            disabled={form.isSubmitting}
            isLoading={form.isSubmitting}
            loadingText="Updating..."
            startIcon={Check}
          >
            Update password
          </Button>
        </div>
      </form>
    {/if}
  </div>
</ResponsiveDialog>
