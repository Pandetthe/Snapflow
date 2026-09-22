<script lang="ts">
  import { AppDialog, Button, InputTextField } from '$lib/ui/components';
  import { Check } from '@lucide/svelte';
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

<AppDialog
  bind:open
  size="sm"
  icon={changeSuccess ? Check : undefined}
  tone="success"
  title={changeSuccess ? 'Username updated' : 'Change username'}
  description={changeSuccess
    ? 'Your username has been updated successfully.'
    : 'Enter your new username below.'}
  onsubmit={changeSuccess ? undefined : form.handleSubmit}
>
  {#if !changeSuccess}
    <InputTextField
      id="userName"
      name="userName"
      label="New username"
      autocomplete="nickname"
      required
      bind:value={form.values.userName}
      error={form.errors.userName}
    />
  {/if}

  {#snippet actions()}
    {#if changeSuccess}
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
        Save username
      </Button>
    {/if}
  {/snippet}
</AppDialog>
