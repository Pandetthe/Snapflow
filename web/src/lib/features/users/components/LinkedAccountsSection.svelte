<script lang="ts">
  import { onMount } from 'svelte';
  import { replaceState } from '$app/navigation';
  import { resolve } from '$app/paths';
  import { Link2, Unlink } from 'lucide-svelte';
  import { AppDialog, Button } from '$lib/ui/components';
  import { errorStore } from '$lib/ui/stores/error.svelte';
  import { noticeStore } from '$lib/ui/stores/notice.svelte';
  import ProviderLogo from '$lib/features/auth/components/ProviderLogo.svelte';
  import {
    externalLinkUrl,
    type ExternalProvider,
    type ExternalProviderType
  } from '$lib/features/auth/api/auth';
  import type { ExternalLogin, UsersService } from '../api/users';

  let {
    providers,
    logins,
    ldapDisplayName,
    usersService,
    onChange
  }: {
    providers: ExternalProvider[];
    logins: ExternalLogin[];
    ldapDisplayName: string | null;
    usersService: UsersService;
    onChange: () => void;
  } = $props();

  interface Account {
    provider: string;
    displayName: string;
    type: ExternalProviderType | 'ldap';
    connected: boolean;
    connectable: boolean;
  }

  const linkErrors: Record<string, string> = {
    'Users.Logins.LinkedToAnotherAccount':
      'This account is already connected to a different user. Sign in with it and disconnect it there first.',
    'Users.Logins.ProviderAlreadyLinked':
      'An account from this provider is already connected. Disconnect it before connecting another one.',
    'Users.External.ProviderNotAvailable': 'This provider is not available anymore.'
  };

  const accounts = $derived.by<Account[]>(() => {
    const connected = new Set(logins.map((login) => login.provider));
    const available = providers.map((provider) => ({
      provider: provider.scheme,
      displayName: provider.displayName,
      type: provider.type,
      connected: connected.has(provider.scheme),
      connectable: true
    }));
    const unavailable = logins
      .filter((login) => !providers.some((provider) => provider.scheme === login.provider))
      .map((login) => ({
        provider: login.provider,
        displayName:
          login.provider === 'Ldap' ? (ldapDisplayName ?? login.displayName) : login.displayName,
        type: 'ldap' as const,
        connected: true,
        connectable: false
      }));
    return [...available, ...unavailable];
  });

  let disconnecting = $state<Account | null>(null);
  let confirmOpen = $state(false);
  let busy = $state(false);
  let notice = $state<string | null>(null);

  function startDisconnect(account: Account) {
    disconnecting = account;
    notice = null;
    confirmOpen = true;
  }

  async function disconnect() {
    if (!disconnecting || busy) return;
    busy = true;
    notice = null;
    const response = await usersService.removeLogin(disconnecting.provider);
    busy = false;

    if (!response.ok) {
      notice =
        response.validationProblem?.errors[0]?.description ??
        response.problem?.detail ??
        'The account could not be disconnected.';
      return;
    }

    confirmOpen = false;
    onChange();
  }

  onMount(() => {
    const params = new URLSearchParams(window.location.search);
    const linked = params.get('linked');
    const error = params.get('error');
    if (!linked && !error) return;

    if (linked) {
      const name = providers.find((provider) => provider.scheme === linked)?.displayName ?? linked;
      noticeStore.add('Account connected', `You can now sign in with ${name}.`);
    } else if (error) {
      errorStore.addError(
        error,
        linkErrors[error] ?? 'The account could not be connected. Try again.'
      );
    }

    replaceState(resolve('/profile'), {});
  });
</script>

<section
  class="rounded-2xl border border-gray-200 bg-white p-5 shadow-sm sm:rounded-3xl sm:p-6 dark:border-gray-800 dark:bg-gray-900/50"
>
  <h2 class="mb-1 flex items-center gap-2 text-lg font-bold text-gray-900 dark:text-white">
    <Link2 size={18} class="text-gray-400" />
    Connected accounts
  </h2>
  <p class="mb-5 text-sm text-gray-500 dark:text-gray-400">
    Sign in with accounts from other services.
  </p>

  <ul class="space-y-4">
    {#each accounts as account (account.provider)}
      <li class="flex items-center justify-between gap-4">
        <div class="flex min-w-0 items-center gap-3">
          <div
            class="flex size-9 shrink-0 items-center justify-center rounded-lg border border-gray-200 dark:border-gray-800"
          >
            <ProviderLogo type={account.type} />
          </div>
          <div class="min-w-0">
            <p class="truncate text-sm font-medium text-gray-900 dark:text-white">
              {account.displayName}
            </p>
            {#if account.connected}
              <span
                class="mt-0.5 inline-flex rounded-full bg-success-50 px-2 py-0.5 text-[10px] font-medium text-success-600 sm:text-xs dark:bg-success-500/10 dark:text-success-400"
              >
                Connected
              </span>
            {:else}
              <p class="text-xs text-gray-400 dark:text-gray-500">Not connected</p>
            {/if}
          </div>
        </div>
        {#if account.connected}
          <Button
            variant="outline"
            size="xs"
            startIcon={Unlink}
            onclick={() => startDisconnect(account)}
          >
            Disconnect
          </Button>
        {:else if account.connectable}
          <Button
            variant="outline"
            size="xs"
            startIcon={Link2}
            href={externalLinkUrl(account.provider)}
            data-sveltekit-reload
          >
            Connect
          </Button>
        {/if}
      </li>
    {/each}
  </ul>

  <AppDialog
    alert
    bind:open={confirmOpen}
    size="sm"
    icon={Unlink}
    tone="warning"
    title={`Disconnect ${disconnecting?.displayName ?? 'account'}`}
    description={`You won't be able to sign in with ${disconnecting?.displayName ?? 'this account'} anymore. You can connect it again later.`}
  >
    {#if notice}
      <p class="text-sm text-error-600 dark:text-error-400" role="alert">{notice}</p>
    {/if}

    {#snippet actions()}
      <Button
        variant="outline"
        onclick={() => {
          confirmOpen = false;
        }}
      >
        Cancel
      </Button>
      <Button
        variant="danger"
        startIcon={Unlink}
        disabled={busy}
        isLoading={busy}
        loadingText="Disconnecting"
        onclick={disconnect}
      >
        Disconnect
      </Button>
    {/snippet}
  </AppDialog>
</section>
