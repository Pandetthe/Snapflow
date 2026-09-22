<script lang="ts">
  import { AppDialog, Button, SettingsSection } from '$lib/ui/components';
  import type { UsersService } from '../api/users';
  import { triggerHaptic } from '$lib/ui/utils';
  import { Trash2, TriangleAlert } from '@lucide/svelte';
  import { goto } from '$app/navigation';
  import { resolve } from '$app/paths';

  let {
    usersService
  }: {
    usersService: UsersService;
  } = $props();

  let isConfirmingDelete = $state(false);
  let isDeletingAccount = $state(false);
  let notice = $state<string | null>(null);

  async function handleDeleteAccount() {
    if (isDeletingAccount) return;
    isDeletingAccount = true;
    notice = null;
    const result = await usersService.deleteAccount();
    isDeletingAccount = false;
    if (result.ok) {
      triggerHaptic('success');
      await goto(resolve('/sign-in'));
    } else {
      triggerHaptic('error');
      notice = result.problem?.detail ?? 'Your account could not be deleted. Try again.';
    }
  }
</script>

<SettingsSection
  danger
  icon={TriangleAlert}
  title="Danger zone"
  description="Permanently remove your account and all of its data."
>
  <Button
    variant="danger"
    size="lg"
    class="w-full justify-center shadow-lg shadow-error-500/10"
    startIcon={Trash2}
    haptic="medium"
    onclick={() => {
      notice = null;
      isConfirmingDelete = true;
    }}
  >
    Delete account
  </Button>

  <AppDialog
    alert
    bind:open={isConfirmingDelete}
    size="sm"
    icon={TriangleAlert}
    tone="danger"
    title="Delete account"
    description="Are you absolutely sure? This action cannot be undone."
  >
    <div
      class="rounded-xl border border-error-100 bg-error-50/50 p-4 text-left dark:border-error-900/40 dark:bg-error-900/10"
    >
      <p class="text-sm text-error-700 dark:text-error-300">
        Your account will be deactivated and you will be signed out immediately. All data will be
        permanently removed.
      </p>
    </div>

    {#if notice}
      <p class="mt-4 text-sm text-error-600 dark:text-error-400" role="alert">{notice}</p>
    {/if}

    {#snippet actions()}
      <Button
        variant="outline"
        disabled={isDeletingAccount}
        haptic="light"
        onclick={() => {
          isConfirmingDelete = false;
        }}
      >
        Cancel
      </Button>
      <Button
        variant="danger"
        class="shadow-md shadow-error-500/20"
        onclick={handleDeleteAccount}
        disabled={isDeletingAccount}
        isLoading={isDeletingAccount}
        loadingText="Deleting"
        startIcon={Trash2}
        haptic="heavy"
      >
        Delete my account
      </Button>
    {/snippet}
  </AppDialog>
</SettingsSection>
