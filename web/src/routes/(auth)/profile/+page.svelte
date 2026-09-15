<script lang="ts">
  import { UsersService } from '$lib/features/users/api/users';
  import { apiClient } from '$lib/core/api.client';
  import { FullLayout, GoBackButton, Button } from '$lib/ui/components';
  import AvatarSection from '$lib/features/users/components/AvatarSection.svelte';
  import ChangeUsernameDialog from '$lib/features/users/components/ChangeUsernameDialog.svelte';
  import ChangeEmailDialog from '$lib/features/users/components/ChangeEmailDialog.svelte';
  import ChangePasswordDialog from '$lib/features/users/components/ChangePasswordDialog.svelte';
  import DangerZone from '$lib/features/users/components/DangerZone.svelte';
  import { ShieldCheck, User as UserIcon, Mail, KeyRound, Pencil } from 'lucide-svelte';
  import { afterNavigate } from '$app/navigation';

  let { data } = $props();
  const usersService = new UsersService(apiClient);

  let backHref = $state('/boards');
  afterNavigate(({ from }) => {
    backHref = from?.url.pathname ?? '/boards';
  });

  let isEditingUsername = $state(false);
  let isEditingEmail = $state(false);
  let isEditingPassword = $state(false);
</script>

<svelte:head>
  <title>Snapflow | Profile</title>
</svelte:head>

<FullLayout>
  <div class="mx-auto w-full max-w-5xl space-y-6 pb-12 sm:space-y-8">
    <header class="flex flex-col gap-4">
      <div class="flex items-center gap-2">
        <GoBackButton href={backHref} />
      </div>
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
        <!-- Details section -->
        <section
          class="rounded-2xl border border-gray-200 bg-white p-5 shadow-sm sm:rounded-3xl sm:p-6 dark:border-gray-800 dark:bg-gray-900/50"
        >
          <h2 class="mb-5 flex items-center gap-2 text-lg font-bold text-gray-900 dark:text-white">
            <UserIcon size={18} class="text-gray-400" />
            Details
          </h2>

          <div>
            <div class="flex items-center justify-between gap-4">
              <div class="flex min-w-0 flex-1 items-center gap-3">
                <UserIcon size={15} class="shrink-0 text-gray-400" />
                <div class="min-w-0 flex-1">
                  <p class="text-xs text-gray-400 dark:text-gray-500">Username</p>
                  <p class="truncate text-sm font-medium text-gray-900 dark:text-white">
                    {data.user?.userName}
                  </p>
                </div>
              </div>
              <Button
                variant="outline"
                size="xs"
                onclick={() => {
                  isEditingUsername = true;
                }}
                startIcon={Pencil}
              >
                Change
              </Button>
            </div>
            <ChangeUsernameDialog
              bind:open={isEditingUsername}
              currentUserName={data.user?.userName ?? ''}
              {usersService}
            />
          </div>

          <div class="my-5 h-px bg-gray-100 dark:bg-gray-800"></div>

          <div>
            <div class="flex items-center justify-between gap-4">
              <div class="flex min-w-0 flex-1 items-center gap-3">
                <Mail size={15} class="shrink-0 text-gray-400" />
                <div class="min-w-0 flex-1">
                  <p class="text-xs text-gray-400 dark:text-gray-500">Email</p>
                  <div
                    class="flex min-w-0 flex-col items-start gap-1 sm:flex-row sm:items-center sm:gap-2"
                  >
                    <p class="w-full truncate text-sm font-medium text-gray-900 dark:text-white">
                      {data.user?.email}
                    </p>
                    <span
                      class="shrink-0 rounded-full bg-success-50 px-2 py-0.5 text-[10px] font-medium text-success-600 sm:text-xs dark:bg-success-500/10 dark:text-success-400"
                    >
                      Verified
                    </span>
                  </div>
                </div>
              </div>
              <Button
                variant="outline"
                size="xs"
                onclick={() => {
                  isEditingEmail = true;
                }}
                startIcon={Pencil}
              >
                Change
              </Button>
            </div>
            <ChangeEmailDialog
              bind:open={isEditingEmail}
              currentEmail={data.user?.email ?? ''}
              {usersService}
            />
          </div>
        </section>

        <!-- Security section -->
        {#if data.authProviders.passwordSignIn}
          <section
            class="rounded-2xl border border-gray-200 bg-white p-5 shadow-sm sm:rounded-3xl sm:p-6 dark:border-gray-800 dark:bg-gray-900/50"
          >
            <h2
              class="mb-5 flex items-center gap-2 text-lg font-bold text-gray-900 dark:text-white"
            >
              <ShieldCheck size={18} class="text-gray-400" />
              Security
            </h2>

            <div>
              <div class="flex items-center justify-between gap-4">
                <div class="flex min-w-0 items-center gap-3">
                  <KeyRound size={15} class="shrink-0 text-gray-400" />
                  <div class="min-w-0">
                    <p class="text-xs text-gray-400 dark:text-gray-500">Password</p>
                    <p class="text-sm font-medium tracking-widest text-gray-400 dark:text-gray-500">
                      ••••••••
                    </p>
                  </div>
                </div>
                <Button
                  variant="outline"
                  size="xs"
                  onclick={() => {
                    isEditingPassword = true;
                  }}
                  startIcon={Pencil}
                >
                  Change
                </Button>
              </div>
              <ChangePasswordDialog bind:open={isEditingPassword} {usersService} />
            </div>
          </section>
        {/if}

        <DangerZone {usersService} />
      </div>
    </div>
  </div>
</FullLayout>
