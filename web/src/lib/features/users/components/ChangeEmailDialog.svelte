<script lang="ts">
  import { Dialog } from 'bits-ui';
  import { Button, InputTextField, ResponsiveDialog } from '$lib/ui/components';
  import { Check, Mail } from 'lucide-svelte';
  import type { UsersService } from '../api/users';
  import { validateEmail } from '$lib/features/auth/validation';
  import { createForm } from '$lib/ui/utils';

  let {
    open = $bindable(false),
    currentEmail,
    usersService
  }: {
    open: boolean;
    currentEmail: string;
    usersService: UsersService;
  } = $props();

  let changeSent = $state(false);

  const form = createForm({
    initialValues: { newEmail: '' },
    validate: (values) => {
      const errors: Record<string, string> = {};
      const emailError = validateEmail(values.newEmail);
      if (emailError) errors.newEmail = emailError;
      return errors;
    },
    onSubmit: (values) => usersService.requestEmailChange({ newEmail: values.newEmail.trim() }),
    onSuccess: () => { changeSent = true; }
  });

  function close() {
    open = false;
  }

  $effect(() => {
    if (open) {
      changeSent = false;
      form.reset();
    }
  });
</script>

<ResponsiveDialog bind:open size="sm">
  <div class="space-y-6">
    {#if changeSent}
      <div class="space-y-6 text-center pt-4">
        <div class="mx-auto flex h-16 w-16 items-center justify-center rounded-full shadow-inner transition-all duration-500 bg-success-100 dark:bg-success-900/40">
          <Mail size={32} class="transition-transform duration-500 text-success-600 dark:text-success-400" />
        </div>
        <div class="space-y-2">
          <Dialog.Title class="text-xl font-bold tracking-tight text-gray-900 dark:text-white">
            Check your inbox
          </Dialog.Title>
          <Dialog.Description class="text-base leading-relaxed text-gray-500 dark:text-gray-400">
            We sent a confirmation link to your new email address. Please click it to confirm the change.
          </Dialog.Description>
        </div>
        <div class="pt-4">
          <Button class="w-full justify-center" onclick={close}>Close</Button>
        </div>
      </div>
    {:else}
      <div class="space-y-2">
        <Dialog.Title class="text-xl font-bold tracking-tight text-gray-900 dark:text-white">
          Change email
        </Dialog.Title>
        <Dialog.Description class="text-sm text-gray-500 dark:text-gray-400">
          Enter your new email address below.
        </Dialog.Description>
      </div>
      <form onsubmit={form.handleSubmit} novalidate class="space-y-4">
        <InputTextField
          id="newEmail"
          name="newEmail"
          type="email"
          label="New email address"
          placeholder={currentEmail}
          required
          bind:value={form.values.newEmail}
          error={form.errors.newEmail}
        />
        <div class="flex justify-end gap-2">
          <Button variant="ghost" size="sm" onclick={close}>Cancel</Button>
          <Button
            type="submit"
            variant="primary"
            size="sm"
            disabled={form.isSubmitting}
            isLoading={form.isSubmitting}
            loadingText="Sending..."
            startIcon={Check}
          >
            Send confirmation
          </Button>
        </div>
      </form>
    {/if}
  </div>
</ResponsiveDialog>
