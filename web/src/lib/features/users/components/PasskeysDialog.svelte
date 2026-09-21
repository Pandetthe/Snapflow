<script lang="ts">
  import { tick } from 'svelte';
  import { KeyRound, Pencil, Plus, Trash2 } from 'lucide-svelte';
  import { AppDialog, Button, InputTextField } from '$lib/ui/components';
  import {
    createPasskey,
    isPasskeyAlreadyOnDevice,
    isPasskeyDismissed,
    passkeysSupported
  } from '$lib/features/auth/passkeys';
  import type { Result } from '$lib/core/types/app';
  import { triggerHaptic } from '$lib/ui/utils';
  import type { Passkey, UsersService } from '../api/users';

  const MAX_PASSKEYS = 20;
  const MAX_NAME_LENGTH = 50;

  let {
    open = $bindable(false),
    usersService,
    passkeys,
    onChange
  }: {
    open: boolean;
    usersService: UsersService;
    passkeys: Passkey[];
    onChange: () => void;
  } = $props();

  const dateFormat = new Intl.DateTimeFormat(undefined, { dateStyle: 'medium' });

  let items = $state<Passkey[]>([]);
  let supported = $state(true);
  let adding = $state(false);
  let editingId = $state<string | null>(null);
  let editName = $state('');
  let nameError = $state<string | undefined>();
  let removingId = $state<string | null>(null);
  let busyId = $state<string | null>(null);
  let notice = $state<string | null>(null);
  let changed = false;

  function problemText(response: Result<unknown>, fallback: string): string {
    if (response.ok) return fallback;
    return (
      response.validationProblem?.errors[0]?.description ?? response.problem?.detail ?? fallback
    );
  }

  async function addPasskey() {
    if (adding) return;
    adding = true;
    notice = null;

    try {
      const options = await usersService.createPasskeyOptions();
      if (!options.ok) {
        triggerHaptic('error');
        notice = problemText(options, 'The passkey could not be added. Try again.');
        return;
      }

      const credential = await createPasskey(options.value.options);
      const added = await usersService.addPasskey({ credential, state: options.value.state });
      if (!added.ok) {
        triggerHaptic('error');
        notice = problemText(added, 'The passkey could not be added. Try again.');
        return;
      }

      triggerHaptic('success');
      items = [...items, added.value];
      changed = true;
      await startRename(added.value);
    } catch (error) {
      if (isPasskeyAlreadyOnDevice(error)) {
        triggerHaptic('error');
        notice = 'This device already has a passkey for your account.';
      } else if (!isPasskeyDismissed(error)) {
        triggerHaptic('error');
        notice = 'Your browser could not create the passkey. Try again.';
      }
    } finally {
      adding = false;
    }
  }

  async function startRename(passkey: Passkey) {
    removingId = null;
    editingId = passkey.id;
    editName = passkey.name;
    nameError = undefined;
    await tick();
    const input = document.getElementById('passkeyName') as HTMLInputElement | null;
    input?.focus();
    input?.select();
  }

  function cancelRename() {
    editingId = null;
    nameError = undefined;
  }

  async function saveName(event: SubmitEvent, passkey: Passkey) {
    event.preventDefault();
    const name = editName.trim();
    if (!name) {
      triggerHaptic('error');
      nameError = 'Enter a name.';
      return;
    }
    if (name.length > MAX_NAME_LENGTH) {
      triggerHaptic('error');
      nameError = `Use at most ${MAX_NAME_LENGTH} characters.`;
      return;
    }
    if (name === passkey.name) {
      cancelRename();
      return;
    }

    busyId = passkey.id;
    const response = await usersService.renamePasskey(passkey.id, { name });
    busyId = null;

    if (!response.ok) {
      triggerHaptic('error');
      nameError = problemText(response, 'The name could not be saved.');
      return;
    }

    triggerHaptic('success');
    items = items.map((item) => (item.id === passkey.id ? { ...item, name } : item));
    changed = true;
    cancelRename();
  }

  function startRemove(passkey: Passkey) {
    editingId = null;
    removingId = passkey.id;
  }

  async function remove(passkey: Passkey) {
    busyId = passkey.id;
    notice = null;
    const response = await usersService.removePasskey(passkey.id);
    busyId = null;

    if (!response.ok) {
      triggerHaptic('error');
      notice = problemText(response, 'The passkey could not be removed.');
      return;
    }

    triggerHaptic('success');
    items = items.filter((item) => item.id !== passkey.id);
    removingId = null;
    changed = true;
  }

  function close() {
    open = false;
  }

  $effect(() => {
    if (open) {
      items = [...passkeys];
      supported = passkeysSupported();
      editingId = null;
      removingId = null;
      busyId = null;
      notice = null;
    } else if (changed) {
      changed = false;
      onChange();
    }
  });
</script>

<AppDialog
  bind:open
  size="md"
  title="Passkeys"
  description="Sign in with your fingerprint, face or screen lock instead of your password. When two-factor authentication is on, a passkey also works instead of a code."
>
  <div class="space-y-3">
    {#if items.length === 0}
      <div
        class="rounded-xl border border-dashed border-gray-300 px-4 py-6 text-center text-sm text-gray-500 dark:border-gray-700 dark:text-gray-400"
      >
        You haven't added any passkeys yet.
      </div>
    {:else}
      <ul
        class="divide-y divide-gray-100 rounded-xl border border-gray-200 dark:divide-gray-800 dark:border-gray-800"
      >
        {#each items as passkey (passkey.id)}
          <li class="px-4 py-3">
            {#if editingId === passkey.id}
              <form
                class="flex flex-col gap-3 sm:flex-row sm:items-start"
                onsubmit={(event) => saveName(event, passkey)}
              >
                <div class="min-w-0 flex-1">
                  <InputTextField
                    id="passkeyName"
                    name="passkeyName"
                    label="Passkey name"
                    autocomplete="off"
                    maxlength={MAX_NAME_LENGTH}
                    bind:value={editName}
                    error={nameError}
                  />
                </div>
                <div class="flex justify-end gap-2 sm:pt-7">
                  <Button variant="ghost" size="xs" haptic="light" onclick={cancelRename}>
                    Cancel
                  </Button>
                  <Button
                    type="submit"
                    size="xs"
                    isLoading={busyId === passkey.id}
                    loadingText="Saving"
                  >
                    Save
                  </Button>
                </div>
              </form>
            {:else if removingId === passkey.id}
              <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                <p class="text-sm text-gray-700 dark:text-gray-300">
                  Remove <span class="font-medium text-gray-900 dark:text-white"
                    >{passkey.name}</span
                  >? You won't be able to sign in with it anymore.
                </p>
                <div class="flex shrink-0 justify-end gap-2">
                  <Button
                    variant="ghost"
                    size="xs"
                    haptic="light"
                    onclick={() => {
                      removingId = null;
                    }}
                  >
                    Cancel
                  </Button>
                  <Button
                    variant="danger"
                    size="xs"
                    isLoading={busyId === passkey.id}
                    loadingText="Removing"
                    haptic="heavy"
                    onclick={() => remove(passkey)}
                  >
                    Remove
                  </Button>
                </div>
              </div>
            {:else}
              <div class="flex items-center gap-3">
                <div
                  class="flex size-9 shrink-0 items-center justify-center rounded-lg border border-gray-200 text-gray-400 dark:border-gray-800 dark:text-gray-500"
                >
                  <KeyRound size={16} />
                </div>
                <div class="min-w-0 flex-1">
                  <p class="truncate text-sm font-medium text-gray-900 dark:text-white">
                    {passkey.name}
                  </p>
                  <p class="text-xs text-gray-500 dark:text-gray-400">
                    Added {dateFormat.format(new Date(passkey.createdAt))}{passkey.isBackedUp
                      ? ' · Synced'
                      : ''}
                  </p>
                </div>
                <Button
                  variant="ghost"
                  size="xs"
                  startIcon={Pencil}
                  haptic="light"
                  aria-label={`Rename ${passkey.name}`}
                  onclick={() => startRename(passkey)}
                />
                <Button
                  variant="ghost"
                  size="xs"
                  startIcon={Trash2}
                  haptic="light"
                  aria-label={`Remove ${passkey.name}`}
                  onclick={() => startRemove(passkey)}
                />
              </div>
            {/if}
          </li>
        {/each}
      </ul>
    {/if}

    {#if !supported}
      <p class="text-sm text-gray-500 dark:text-gray-400">This browser can't create passkeys.</p>
    {/if}

    {#if notice}
      <p class="text-sm text-error-600 dark:text-error-400" role="alert">{notice}</p>
    {/if}
  </div>

  {#snippet actions()}
    <Button variant="ghost" haptic="light" onclick={close}>Close</Button>
    <Button
      startIcon={Plus}
      haptic="medium"
      disabled={!supported || adding || items.length >= MAX_PASSKEYS}
      isLoading={adding}
      loadingText="Adding"
      onclick={addPasskey}
    >
      Add a passkey
    </Button>
  {/snippet}
</AppDialog>
