<script lang="ts">
  import { AppDialog, Button, InputTextField } from '$lib/ui/components';
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

<AppDialog
  bind:open
  size="sm"
  icon={changeSent ? Mail : undefined}
  tone="success"
  title={changeSent ? 'Check your inbox' : 'Change email'}
  description={changeSent
    ? 'We sent a confirmation link to your new email address. Please click it to confirm the change.'
    : 'Enter your new email address below.'}
  onsubmit={changeSent ? undefined : form.handleSubmit}
>
  {#if !changeSent}
    <InputTextField
      id="newEmail"
      name="newEmail"
      type="email"
      label="New email address"
      autocomplete="email"
      placeholder={currentEmail}
      required
      bind:value={form.values.newEmail}
      error={form.errors.newEmail}
    />
  {/if}

  {#snippet actions()}
    {#if changeSent}
      <Button onclick={close}>Close</Button>
    {:else}
      <Button variant="ghost" onclick={close}>Cancel</Button>
      <Button
        type="submit"
        disabled={form.isSubmitting}
        isLoading={form.isSubmitting}
        loadingText="Sending"
        startIcon={Check}
      >
        Send confirmation
      </Button>
    {/if}
  {/snippet}
</AppDialog>
