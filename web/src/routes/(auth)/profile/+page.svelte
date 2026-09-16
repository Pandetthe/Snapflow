<script lang="ts">
  import { UsersService } from '$lib/features/users/api/users';
  import { apiClient } from '$lib/core/api.client';
  import { FullLayout, GoBackButton, Button, SettingsSection } from '$lib/ui/components';
  import AvatarSection from '$lib/features/users/components/AvatarSection.svelte';
  import SettingRow from '$lib/features/users/components/SettingRow.svelte';
  import ChangeUsernameDialog from '$lib/features/users/components/ChangeUsernameDialog.svelte';
  import ChangeEmailDialog from '$lib/features/users/components/ChangeEmailDialog.svelte';
  import ChangePasswordDialog from '$lib/features/users/components/ChangePasswordDialog.svelte';
  import SetPasswordDialog from '$lib/features/users/components/SetPasswordDialog.svelte';
  import DangerZone from '$lib/features/users/components/DangerZone.svelte';
  import TwoFactorSetupDialog from '$lib/features/users/components/TwoFactorSetupDialog.svelte';
  import TwoFactorManageDialog from '$lib/features/users/components/TwoFactorManageDialog.svelte';
  import PasskeysDialog from '$lib/features/users/components/PasskeysDialog.svelte';
  import LinkedAccountsSection from '$lib/features/users/components/LinkedAccountsSection.svelte';
  import {
    Fingerprint,
    Plus,
    ShieldCheck,
    User as UserIcon,
    Mail,
    KeyRound,
    Pencil,
    Smartphone,
    Settings2
  } from 'lucide-svelte';
  import { afterNavigate, invalidateAll } from '$app/navigation';

  let { data } = $props();
  const usersService = new UsersService(apiClient);

  let backHref = $state('/boards');
  afterNavigate(({ from }) => {
    backHref = from?.url.pathname ?? '/boards';
  });

  let isEditingUsername = $state(false);
  let isEditingEmail = $state(false);
  let isEditingPassword = $state(false);
  let isEditingTwoFactor = $state(false);
  let isEditingPasskeys = $state(false);

  const passkeyCount = $derived(data.passkeys?.length ?? 0);
</script>

<svelte:head>
  <title>Snapflow | Profile</title>
</svelte:head>

<FullLayout>
  <div class="mx-auto w-full max-w-5xl space-y-6 pb-12 sm:space-y-8">
    <header class="flex flex-col gap-4">
      <GoBackButton href={backHref} />
      <div class="space-y-1">
        <h1 class="text-2xl font-bold tracking-tight text-gray-900 sm:text-4xl dark:text-white">
          Profile
        </h1>
        <p class="text-sm text-gray-600 sm:text-base dark:text-gray-400">
          Manage your account settings.
        </p>
      </div>
    </header>

    <div class="grid items-start gap-6 lg:grid-cols-[minmax(0,22rem)_1fr] lg:gap-8">
      <aside class="min-w-0 space-y-6">
        <AvatarSection user={data.user} {usersService} />
      </aside>

      <div class="min-w-0 space-y-6">
        <SettingsSection
          icon={UserIcon}
          title="Details"
          description="How your account identifies you."
        >
          <div class="space-y-5">
            <div>
              <SettingRow icon={UserIcon} label="Username" value={data.user?.userName}>
                {#snippet action()}
                  <Button
                    variant="outline"
                    size="xs"
                    haptic="light"
                    onclick={() => {
                      isEditingUsername = true;
                    }}
                    startIcon={Pencil}
                  >
                    Change
                  </Button>
                {/snippet}
              </SettingRow>
              <ChangeUsernameDialog
                bind:open={isEditingUsername}
                currentUserName={data.user?.userName ?? ''}
                {usersService}
              />
            </div>

            <div class="h-px bg-gray-100 dark:bg-gray-800"></div>

            <div>
              <SettingRow
                icon={Mail}
                label="Email"
                value={data.user?.email}
                badge={data.user?.emailConfirmed ? 'Verified' : 'Not verified'}
                badgeTone={data.user?.emailConfirmed ? 'success' : 'warning'}
              >
                {#snippet action()}
                  <Button
                    variant="outline"
                    size="xs"
                    haptic="light"
                    onclick={() => {
                      isEditingEmail = true;
                    }}
                    startIcon={Pencil}
                  >
                    Change
                  </Button>
                {/snippet}
              </SettingRow>
              <ChangeEmailDialog
                bind:open={isEditingEmail}
                currentEmail={data.user?.email ?? ''}
                {usersService}
              />
            </div>
          </div>
        </SettingsSection>

        {#if data.authProviders.passwordSignIn || data.twoFactor}
          <SettingsSection
            icon={ShieldCheck}
            title="Security"
            description="How you sign in and protect your account."
          >
            <div class="space-y-5">
              {#if data.authProviders.passwordSignIn}
                <div>
                  <SettingRow
                    icon={KeyRound}
                    label="Password"
                    badge={data.hasPassword ? 'Set' : 'Not set'}
                    badgeTone={data.hasPassword ? 'success' : 'neutral'}
                  >
                    {#snippet action()}
                      <Button
                        variant="outline"
                        size="xs"
                        haptic="light"
                        onclick={() => {
                          isEditingPassword = true;
                        }}
                        startIcon={data.hasPassword ? Pencil : Plus}
                      >
                        {data.hasPassword ? 'Change' : 'Set'}
                      </Button>
                    {/snippet}
                  </SettingRow>
                  {#if data.hasPassword}
                    <ChangePasswordDialog
                      bind:open={isEditingPassword}
                      email={data.user?.email ?? ''}
                      {usersService}
                    />
                  {:else}
                    <SetPasswordDialog
                      bind:open={isEditingPassword}
                      email={data.user?.email ?? ''}
                      {usersService}
                      onChange={invalidateAll}
                    />
                  {/if}
                </div>
              {/if}

              {#if data.passkeys}
                <div class="h-px bg-gray-100 dark:bg-gray-800"></div>
                <div>
                  <SettingRow
                    icon={Fingerprint}
                    label="Passkeys"
                    badge={passkeyCount === 0
                      ? 'None'
                      : `${passkeyCount} ${passkeyCount === 1 ? 'passkey' : 'passkeys'}`}
                    badgeTone={passkeyCount === 0 ? 'neutral' : 'success'}
                  >
                    {#snippet action()}
                      <Button
                        variant="outline"
                        size="xs"
                        haptic="light"
                        onclick={() => {
                          isEditingPasskeys = true;
                        }}
                        startIcon={Settings2}
                      >
                        Manage
                      </Button>
                    {/snippet}
                  </SettingRow>
                  <PasskeysDialog
                    bind:open={isEditingPasskeys}
                    {usersService}
                    passkeys={data.passkeys}
                    onChange={invalidateAll}
                  />
                </div>
              {/if}

              {#if data.authProviders.passwordSignIn && data.twoFactor}
                <div class="h-px bg-gray-100 dark:bg-gray-800"></div>
              {/if}

              {#if data.twoFactor}
                {@const twoFactor = data.twoFactor}
                <div>
                  <SettingRow
                    icon={Smartphone}
                    label="Two-factor authentication"
                    badge={twoFactor.isEnabled ? 'On' : 'Off'}
                    badgeTone={twoFactor.isEnabled ? 'success' : 'neutral'}
                  >
                    {#snippet action()}
                      <Button
                        variant="outline"
                        size="xs"
                        haptic="light"
                        onclick={() => {
                          isEditingTwoFactor = true;
                        }}
                        startIcon={twoFactor.isEnabled ? Settings2 : ShieldCheck}
                      >
                        {twoFactor.isEnabled ? 'Manage' : 'Set up'}
                      </Button>
                    {/snippet}
                  </SettingRow>
                  {#if twoFactor.isEnabled}
                    <TwoFactorManageDialog
                      bind:open={isEditingTwoFactor}
                      {usersService}
                      recoveryCodesLeft={twoFactor.recoveryCodesLeft}
                      onChange={invalidateAll}
                    />
                  {:else}
                    <TwoFactorSetupDialog
                      bind:open={isEditingTwoFactor}
                      {usersService}
                      onChange={invalidateAll}
                    />
                  {/if}
                </div>
              {/if}
            </div>
          </SettingsSection>
        {/if}

        {#if data.logins && (data.authProviders.providers.length > 0 || data.logins.length > 0)}
          <LinkedAccountsSection
            providers={data.authProviders.providers}
            logins={data.logins}
            ldapDisplayName={data.authProviders.ldap?.displayName ?? null}
            {usersService}
            onChange={invalidateAll}
          />
        {/if}

        <DangerZone {usersService} />
      </div>
    </div>
  </div>
</FullLayout>
