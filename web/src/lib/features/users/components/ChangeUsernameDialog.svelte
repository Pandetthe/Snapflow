<script lang="ts">
  import { Dialog } from 'bits-ui';
  import { Button, InputTextField, ResponsiveDialog } from '$lib/ui/components';
  import { Check } from 'lucide-svelte';
  import type { UsersService } from '../api/users';
  import { createForm } from '$lib/ui/utils';
  import { validateUsername } from '$lib/features/auth/validation';
  import { invalidateAll } from '$app/navigation';
  import { untrack } from 'svelte';

  let {
    open = $bindable(false),
    currentUserName,
    usersService
  }: {
    open: boolean;
    currentUserName: string;
    usersService: UsersService;
  } = $props();

  let changeSuccess = $state(false);

  const form = createForm({
    initialValues: { userName: untrack(() => currentUserName) },
    validate: (values) => {
      const errors: Record<string, string> = {};
      const usernameError = validateUsername(values.userName);
      if (usernameError) errors.userName = usernameError;
      return errors;
    },
    onSubmit: (values) => usersService.updateProfile({ userName: values.userName.trim() }),
    onSuccess: () => {
      changeSuccess = true;
      invalidateAll();
    }
  });

  // Only closes. Clearing here would swap the success panel back for the form while the
  // dialog animates away; the effect below puts both back on the next open.
  function close() {
    open = false;
  }

  $effect(() => {
    if (open) {
      changeSuccess = false;
      form.reset({ userName: currentUserName });
    }
  });
</script>

<ResponsiveDialog bind:open size="sm">
  <div class="space-y-6">
    {#if changeSuccess}
      <div class="space-y-6 text-center pt-4">
        <div class="mx-auto flex h-16 w-16 items-center justify-center rounded-full shadow-inner transition-all duration-500 bg-success-100 dark:bg-success-900/40">
          <Check size={32} class="transition-transform duration-500 text-success-600 dark:text-success-400" />
        </div>
        <div class="space-y-2">
          <Dialog.Title class="text-xl font-bold tracking-tight text-gray-900 dark:text-white">
            Username updated
          </Dialog.Title>
          <Dialog.Description class="text-base leading-relaxed text-gray-500 dark:text-gray-400">
            Your username has been updated successfully.
          </Dialog.Description>
        </div>
        <div class="pt-4">
          <Button class="w-full justify-center" onclick={close}>Close</Button>
        </div>
      </div>
    {:else}
      <div class="space-y-2">
        <Dialog.Title class="text-xl font-bold tracking-tight text-gray-900 dark:text-white">
          Change username
        </Dialog.Title>
        <Dialog.Description class="text-sm text-gray-500 dark:text-gray-400">
          Enter your new username below.
        </Dialog.Description>
      </div>
      <form onsubmit={form.handleSubmit} novalidate class="space-y-4">
        <InputTextField
          id="userName"
          name="userName"
          label="New username"
          required
          bind:value={form.values.userName}
          error={form.errors.userName}
        />
        <div class="flex justify-end gap-2">
          <Button variant="ghost" size="sm" onclick={close}>Cancel</Button>
          <Button
            type="submit"
            variant="primary"
            size="sm"
            disabled={form.isSubmitting}
            isLoading={form.isSubmitting}
            loadingText="Saving..."
            startIcon={Check}
          >
            Save username
          </Button>
        </div>
      </form>
    {/if}
  </div>
</ResponsiveDialog>
