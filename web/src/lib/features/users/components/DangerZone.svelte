<script lang="ts">
  import { Dialog } from 'bits-ui';
  import { Button, ResponsiveDialog } from '$lib/ui/components';
  import type { UsersService } from '../api/users';
  import { errorStore } from '$lib/ui/stores/error.svelte';
  import { Trash2, TriangleAlert, X } from 'lucide-svelte';
  import { goto } from '$app/navigation';

  let {
    usersService
  }: {
    usersService: UsersService;
  } = $props();

  let isConfirmingDelete = $state(false);
  let isDeletingAccount = $state(false);

  async function handleDeleteAccount() {
    if (isDeletingAccount) return;
    isDeletingAccount = true;
    const result = await usersService.deleteAccount();
    isDeletingAccount = false;
    if (result.ok) {
      await goto('/sign-in');
    } else {
      errorStore.addError(result.problem?.title ?? null, result.problem?.detail ?? null);
    }
  }
</script>

<section class="rounded-2xl border border-error-200 bg-white p-5 shadow-sm sm:rounded-3xl sm:p-6 dark:border-error-900/60 dark:bg-gray-900/50">
  <h2 class="mb-5 flex items-center gap-2 text-lg font-bold text-error-600 dark:text-error-400">
    <TriangleAlert size={18} class="text-error-500" />
    Danger zone
  </h2>
  <div class="flex items-center justify-between gap-4">
    <div class="flex min-w-0 items-center gap-3">
      <Trash2 size={15} class="shrink-0 text-error-400 dark:text-error-500" />
      <div class="min-w-0">
        <p class="text-sm font-medium text-gray-700 dark:text-gray-300">Delete account</p>
        <p class="text-xs text-gray-400 dark:text-gray-500">
          Permanently removes all your data. This cannot be undone.
        </p>
      </div>
    </div>
    <Button
      variant="outline"
      size="xs"
      startIcon={Trash2}
      onclick={() => { isConfirmingDelete = true; }}
      class="border-error-300 text-error-600 hover:bg-error-50 dark:border-error-800 dark:text-error-400 dark:hover:bg-error-900/20"
    >
      Delete
    </Button>
  </div>

  <ResponsiveDialog bind:open={isConfirmingDelete} size="sm">
    <div class="space-y-6">
      <div class="space-y-2">
        <Dialog.Title class="text-xl font-bold tracking-tight text-error-600 dark:text-error-400">
          Delete account
        </Dialog.Title>
        <Dialog.Description class="text-sm text-gray-500 dark:text-gray-400">
          Are you absolutely sure? This action cannot be undone.
        </Dialog.Description>
      </div>

      <div class="space-y-3 rounded-xl border border-error-100 bg-error-50/50 p-4 dark:border-error-900/40 dark:bg-error-900/10">
        <div class="flex items-start gap-2.5">
          <TriangleAlert size={15} class="mt-0.5 shrink-0 text-error-500" />
          <p class="text-sm text-error-700 dark:text-error-300">
            Your account will be deactivated and you will be signed out immediately. All data will be permanently removed.
          </p>
        </div>
      </div>

      <div class="flex justify-end gap-2">
        <Button
          variant="ghost"
          size="sm"
          startIcon={X}
          onclick={() => { isConfirmingDelete = false; }}
        >Cancel</Button>
        <Button
          variant="primary"
          size="sm"
          onclick={handleDeleteAccount}
          disabled={isDeletingAccount}
          isLoading={isDeletingAccount}
          loadingText="Deleting..."
          startIcon={Trash2}
          class="bg-error-600 hover:bg-error-700 focus-visible:ring-error-500"
        >
          Delete my account
        </Button>
      </div>
    </div>
  </ResponsiveDialog>
</section>
