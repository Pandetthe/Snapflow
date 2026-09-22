<script lang="ts">
  import { onMount } from 'svelte';
  import { replaceState } from '$app/navigation';
  import { resolve } from '$app/paths';
  import { Link2, Unlink } from '@lucide/svelte';
  import { AppDialog, Button, SettingsSection } from '$lib/ui/components';
  import SettingRow from './SettingRow.svelte';
  import { errorStore } from '$lib/ui/stores/error.svelte';
  import { triggerHaptic } from '$lib/ui/utils';
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
      triggerHaptic('error');
      notice =
        response.validationProblem?.errors[0]?.description ??
        response.problem?.detail ??
        'The account could not be disconnected.';
      return;
    }

    triggerHaptic('success');
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
      triggerHaptic('success');
      noticeStore.add('Account connected', `You can now sign in with ${name}.`);
    } else if (error) {
      triggerHaptic('error');
      errorStore.addError(
        error,
        linkErrors[error] ?? 'The account could not be connected. Try again.'
      );
    }

    replaceState(resolve('/profile'), {});
  });
</script>

<SettingsSection
  icon={Link2}
  title="Connected accounts"
  description="Sign in with accounts from other services."
>
  <ul class="space-y-5">
    {#each accounts as account (account.provider)}
      <li>
        <SettingRow
          label={account.displayName}
          badge={account.connected ? 'Connected' : 'Not connected'}
          badgeTone={account.connected ? 'success' : 'neutral'}
        >
          {#snippet iconContent()}
            <ProviderLogo type={account.type} />
          {/snippet}
          {#snippet action()}
            {#if account.connected}
              <Button
                variant="outline"
                size="xs"
                haptic="light"
                startIcon={Unlink}
                onclick={() => startDisconnect(account)}
              >
                Disconnect
              </Button>
            {:else if account.connectable}
              <Button
                variant="outline"
                size="xs"
                haptic="light"
                startIcon={Link2}
                href={externalLinkUrl(account.provider)}
                data-sveltekit-reload
              >
                Connect
              </Button>
            {/if}
          {/snippet}
        </SettingRow>
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
        disabled={busy}
        haptic="light"
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
        haptic="heavy"
        onclick={disconnect}
      >
        Disconnect
      </Button>
    {/snippet}
  </AppDialog>
</SettingsSection>
