<script lang="ts">
  import { Dialog } from 'bits-ui';
  import { AvatarType, UsersService } from '$lib/features/users/api/users';
  import { avatarBust, bustAvatar } from '$lib/features/users/avatarBust.svelte';
  import { apiClient } from '$lib/core/api.client';
  import {
    Button,
    FullLayout,
    InputTextField,
    GoBackButton,
    UserAvatar,
    SegmentedControl,
    Dropzone,
    ResponsiveDialog
  } from '$lib/ui/components';
  import PasswordStrength from '$lib/features/auth/components/PasswordStrength.svelte';
  import { validateEmail, validateUsername, validatePassword } from '$lib/features/auth/validation';
  import { createForm } from '$lib/ui/utils';
  import { errorStore } from '$lib/ui/stores/error';
  import {
    Check,
    Upload,
    Sparkles,
    User as UserIcon,
    ShieldCheck,
    KeyRound,
    Camera,
    Mail,
    Trash2,
    TriangleAlert,
    CircleCheck,
    Pencil,
    X
  } from 'lucide-svelte';
  import { fade, slide } from 'svelte/transition';
  import { invalidateAll, goto, afterNavigate } from '$app/navigation';

  let { data } = $props();
  const usersService = new UsersService(apiClient);

  let backHref = $state('/boards');
  afterNavigate(({ from }) => {
    backHref = from?.url.pathname ?? '/boards';
  });

  // --- Edit states (declared before forms so closures can reference them) ---
  let isEditingUsername = $state(false);
  let usernameChangeSuccess = $state(false);
  let isEditingEmail = $state(false);
  let emailChangeSent = $state(false);
  let isEditingPassword = $state(false);
  let passwordChangeSuccess = $state(false);

  function toggleEditUsername() {
    isEditingUsername = !isEditingUsername;
    if (isEditingUsername) {
      usernameChangeSuccess = false;
      usernameForm.reset({ userName: data.user?.userName ?? '' });
    }
  }

  function toggleEditEmail() {
    isEditingEmail = !isEditingEmail;
    if (!isEditingEmail) {
      emailChangeSent = false;
      emailForm.reset();
    }
  }

  function toggleEditPassword() {
    isEditingPassword = !isEditingPassword;
    if (!isEditingPassword) {
      passwordChangeSuccess = false;
      passwordForm.reset();
    }
  }

  // --- Forms ---
  const usernameForm = createForm({
    initialValues: { userName: data.user?.userName ?? '' },
    validate: (values) => {
      const errors: Record<string, string> = {};
      const usernameError = validateUsername(values.userName);
      if (usernameError) errors.userName = usernameError;
      return errors;
    },
    onSubmit: (values) => usersService.updateProfile({ userName: values.userName.trim() }),
    onSuccess: () => { 
      usernameChangeSuccess = true; 
      invalidateAll(); 
    }
  });

  const emailForm = createForm({
    initialValues: { newEmail: '' },
    validate: (values) => {
      const errors: Record<string, string> = {};
      const emailError = validateEmail(values.newEmail);
      if (emailError) errors.newEmail = emailError;
      return errors;
    },
    onSubmit: (values) => usersService.requestEmailChange({ newEmail: values.newEmail.trim() }),
    onSuccess: () => { emailChangeSent = true; }
  });

  const passwordForm = createForm({
    initialValues: { currentPassword: '', newPassword: '' },
    validate: (values) => {
      const errors: Record<string, string> = {};
      if (!values.currentPassword) errors.currentPassword = 'Current password is required.';
      const passwordError = validatePassword(values.newPassword);
      if (passwordError) errors.newPassword = passwordError;
      return errors;
    },
    onSubmit: (values) =>
      usersService.changePassword({
        currentPassword: values.currentPassword,
        newPassword: values.newPassword
      }),
    mapValidationError: (err) =>
      err.code === 'PasswordMismatch' ? { ...err, propertyName: 'currentPassword' } : err,
    onSuccess: () => { 
      passwordChangeSuccess = true; 
      passwordForm.reset(); 
    }
  });

  // --- Avatar ---
  let selectedAvatarType = $state<AvatarType>(data.user?.avatarType ?? AvatarType.Generated);
  let avatarFiles = $state<File[]>([]);
  let avatarPreview = $state<string | null>(null);
  let avatarFileError = $state('');
  let isUpdatingAvatar = $state(false);

  const allowedExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.webp'];
  const allowedFormatsLabel = 'JPG, PNG, GIF, WEBP';
  const acceptMimeTypes = 'image/jpeg,image/png,image/gif,image/webp';
  const maxFileSizeMB = 5;

  const avatarFile = $derived(avatarFiles[0] ?? null);

  $effect(() => {
    const file = avatarFiles[0] ?? null;
    if (!file) { avatarPreview = null; return; }
    const url = URL.createObjectURL(file);
    avatarPreview = url;
    return () => URL.revokeObjectURL(url);
  });

  function onAvatarFilesChange(files: File[]) {
    avatarFileError = '';
    const file = files[0];
    if (!file) return;
    const ext = '.' + file.name.split('.').pop()?.toLowerCase();
    if (!allowedExtensions.includes(ext)) {
      avatarFileError = `Unsupported format · accepted: ${allowedFormatsLabel}`;
      avatarFiles = [];
      return;
    }
    if (file.size > maxFileSizeMB * 1024 * 1024) {
      avatarFileError = `File too large · max ${maxFileSizeMB} MB`;
      avatarFiles = [];
    }
  }

  async function handleAvatarSave() {
    if (isUpdatingAvatar) return;
    if (selectedAvatarType === AvatarType.Uploaded && !avatarFile) {
      avatarFileError = 'Select a file first';
      return;
    }
    isUpdatingAvatar = true;
    const formData = new FormData();
    formData.append('avatarType', selectedAvatarType);
    if (avatarFile) formData.append('file', avatarFile);
    const result = await usersService.updateAvatar(formData);
    isUpdatingAvatar = false;
    if (result.ok) {
      avatarFiles = [];
      bustAvatar();
      await invalidateAll();
    } else {
      errorStore.addError(result.problem?.title ?? null, result.problem?.detail ?? null);
    }
  }

  // --- Delete account ---
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

<svelte:head>
  <title>Snapflow | Profile</title>
</svelte:head>

<FullLayout>
  <div class="mx-auto w-full max-w-5xl px-4 sm:px-6 lg:px-8 space-y-8 pb-12" in:fade={{ duration: 400 }}>
    <header class="flex flex-col gap-4">
      <div class="flex items-center gap-2">
        <GoBackButton href={backHref} />
      </div>
      <div class="space-y-1">
        <h1 class="text-3xl font-bold tracking-tight text-gray-900 sm:text-4xl dark:text-white">
          Profile
        </h1>
        <p class="text-gray-600 dark:text-gray-400">Manage your account settings.</p>
      </div>
    </header>

    <div class="grid items-start gap-8 lg:grid-cols-[minmax(0,22rem)_1fr]">
      <!-- Left: profile picture card -->
      <aside class="min-w-0 space-y-6">
        <section
          class="rounded-3xl border border-gray-200 bg-white p-6 shadow-sm dark:border-gray-800 dark:bg-gray-900/50"
        >
          <h2 class="mb-5 flex items-center gap-2 text-lg font-bold text-gray-900 dark:text-white">
            <Camera size={18} class="text-gray-400" />
            Profile picture
          </h2>

          <div class="mb-5 flex justify-center">
            <UserAvatar
              src={avatarPreview ?? (data.user?.avatarUrl ? `${data.user.avatarUrl}?v=${avatarBust.count}` : null)}
              name={data.user?.userName}
              size={80}
            />
          </div>

          <SegmentedControl
            size="xs"
            options={[
              { value: AvatarType.Generated, label: 'Generated', icon: Sparkles },
              { value: AvatarType.Gravatar, label: 'Gravatar', icon: UserIcon },
              { value: AvatarType.Uploaded, label: 'Custom', icon: Upload }
            ]}
            bind:value={selectedAvatarType}
            onValueChange={() => { avatarFiles = []; avatarFileError = ''; }}
          />

          {#if selectedAvatarType === AvatarType.Uploaded}
            <div class="mt-3" transition:slide={{ duration: 150 }}>
              <Dropzone
                bind:files={avatarFiles}
                onFilesChange={onAvatarFilesChange}
                multiple={false}
                maxFiles={1}
                accept={acceptMimeTypes}
                title="Drop your photo here"
                description="{allowedFormatsLabel} · max {maxFileSizeMB} MB"
                error={avatarFileError}
              />
            </div>
          {/if}

          <div class="mt-4 flex justify-end">
            <Button
              variant="primary"
              size="sm"
              onclick={handleAvatarSave}
              disabled={isUpdatingAvatar || (selectedAvatarType === AvatarType.Uploaded && !avatarFile)}
              isLoading={isUpdatingAvatar}
              loadingText="Saving..."
              startIcon={Check}
            >
              Save picture
            </Button>
          </div>
        </section>
      </aside>

      <!-- Right: settings -->
      <div class="min-w-0 space-y-6">
        <!-- Details section -->
        <section
          class="rounded-3xl border border-gray-200 bg-white p-6 shadow-sm dark:border-gray-800 dark:bg-gray-900/50"
        >
          <h2 class="mb-5 flex items-center gap-2 text-lg font-bold text-gray-900 dark:text-white">
            <UserIcon size={18} class="text-gray-400" />
            Details
          </h2>

          <!-- Username row -->
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
                onclick={toggleEditUsername}
                startIcon={Pencil}
              >
                Change
              </Button>
            </div>

            <ResponsiveDialog bind:open={isEditingUsername} size="sm">
              <div class="space-y-6">
                {#if usernameChangeSuccess}
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
                      <Button class="w-full justify-center" onclick={toggleEditUsername}>Close</Button>
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
                  <form onsubmit={usernameForm.handleSubmit} novalidate class="space-y-4">
                    <InputTextField
                      id="userName"
                      name="userName"
                      label="New username"
                      required
                      bind:value={usernameForm.values.userName}
                      error={usernameForm.errors.userName}
                    />
                    <div class="flex justify-end gap-2">
                      <Button variant="ghost" size="sm" onclick={toggleEditUsername}>Cancel</Button>
                      <Button
                        type="submit"
                        variant="primary"
                        size="sm"
                        disabled={usernameForm.isSubmitting}
                        isLoading={usernameForm.isSubmitting}
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
          </div>

          <div class="my-5 h-px bg-gray-100 dark:bg-gray-800"></div>

          <!-- Email row -->
          <div>
            <div class="flex items-center justify-between gap-4">
              <div class="flex min-w-0 flex-1 items-center gap-3">
                <Mail size={15} class="shrink-0 text-gray-400" />
                <div class="min-w-0 flex-1">
                  <p class="text-xs text-gray-400 dark:text-gray-500">Email</p>
                  <div class="flex min-w-0 flex-col items-start gap-1 sm:flex-row sm:items-center sm:gap-2">
                    <p class="w-full truncate text-sm font-medium text-gray-900 dark:text-white">
                      {data.user?.email}
                    </p>
                    <span
                      class="shrink-0 rounded-full bg-success-50 px-2 py-0.5 text-[10px] sm:text-xs font-medium text-success-600 dark:bg-success-500/10 dark:text-success-400"
                      >Verified</span
                    >
                  </div>
                </div>
              </div>
              <Button
                variant="outline"
                size="xs"
                onclick={toggleEditEmail}
                startIcon={Pencil}
              >
                Change
              </Button>
            </div>

            <ResponsiveDialog bind:open={isEditingEmail} size="sm">
              <div class="space-y-6">
                {#if emailChangeSent}
                  <div class="space-y-6 text-center pt-4">
                    <div class="mx-auto flex h-16 w-16 items-center justify-center rounded-full shadow-inner transition-all duration-500 bg-success-100 dark:bg-success-900/40">
                      <Mail size={32} class="transition-transform duration-500 text-success-600 dark:text-success-400" />
                    </div>
                    <div class="space-y-2">
                      <Dialog.Title class="text-xl font-bold tracking-tight text-gray-900 dark:text-white">
                        Check your inbox
                      </Dialog.Title>
                      <Dialog.Description class="text-base leading-relaxed text-gray-500 dark:text-gray-400">
                        We sent a confirmation link to your new email address. Please click it to confirm the change.
                      </Dialog.Description>
                    </div>
                    <div class="pt-4">
                      <Button class="w-full justify-center" onclick={toggleEditEmail}>Close</Button>
                    </div>
                  </div>
                {:else}
                  <div class="space-y-2">
                    <Dialog.Title class="text-xl font-bold tracking-tight text-gray-900 dark:text-white">
                      Change email
                    </Dialog.Title>
                    <Dialog.Description class="text-sm text-gray-500 dark:text-gray-400">
                      Enter your new email address below.
                    </Dialog.Description>
                  </div>
                  <form onsubmit={emailForm.handleSubmit} novalidate class="space-y-4">
                    <InputTextField
                      id="newEmail"
                      name="newEmail"
                      type="email"
                      label="New email address"
                      placeholder={data.user?.email}
                      required
                      bind:value={emailForm.values.newEmail}
                      error={emailForm.errors.newEmail}
                    />
                    <div class="flex justify-end gap-2">
                      <Button variant="ghost" size="sm" onclick={toggleEditEmail}>Cancel</Button>
                      <Button
                        type="submit"
                        variant="primary"
                        size="sm"
                        disabled={emailForm.isSubmitting}
                        isLoading={emailForm.isSubmitting}
                        loadingText="Sending..."
                        startIcon={Check}
                      >
                        Send confirmation
                      </Button>
                    </div>
                  </form>
                {/if}
              </div>
            </ResponsiveDialog>
          </div>
        </section>

        <!-- Security section -->
        <section
          class="rounded-3xl border border-gray-200 bg-white p-6 shadow-sm dark:border-gray-800 dark:bg-gray-900/50"
        >
          <h2 class="mb-5 flex items-center gap-2 text-lg font-bold text-gray-900 dark:text-white">
            <ShieldCheck size={18} class="text-gray-400" />
            Security
          </h2>

          <!-- Password row -->
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
                onclick={toggleEditPassword}
                startIcon={Pencil}
              >
                Change
              </Button>
            </div>

            <ResponsiveDialog bind:open={isEditingPassword} size="sm">
              <div class="space-y-6">
                {#if passwordChangeSuccess}
                  <div class="space-y-6 text-center pt-4">
                    <div class="mx-auto flex h-16 w-16 items-center justify-center rounded-full shadow-inner transition-all duration-500 bg-success-100 dark:bg-success-900/40">
                      <KeyRound size={32} class="transition-transform duration-500 text-success-600 dark:text-success-400" />
                    </div>
                    <div class="space-y-2">
                      <Dialog.Title class="text-xl font-bold tracking-tight text-gray-900 dark:text-white">
                        Password updated
                      </Dialog.Title>
                      <Dialog.Description class="text-base leading-relaxed text-gray-500 dark:text-gray-400">
                        Your password has been changed successfully.
                      </Dialog.Description>
                    </div>
                    <div class="pt-4">
                      <Button class="w-full justify-center" onclick={toggleEditPassword}>Close</Button>
                    </div>
                  </div>
                {:else}
                  <div class="space-y-2">
                    <Dialog.Title class="text-xl font-bold tracking-tight text-gray-900 dark:text-white">
                      Change password
                    </Dialog.Title>
                    <Dialog.Description class="text-sm text-gray-500 dark:text-gray-400">
                      Update your password to keep your account secure.
                    </Dialog.Description>
                  </div>
                  <form onsubmit={passwordForm.handleSubmit} novalidate class="space-y-4">
                    <InputTextField
                      id="currentPassword"
                      name="currentPassword"
                      label="Current password"
                      type="password"
                      showPasswordToggle
                      required
                      bind:value={passwordForm.values.currentPassword}
                      error={passwordForm.errors.currentPassword}
                    />
                    <InputTextField
                      id="newPassword"
                      name="newPassword"
                      label="New password"
                      type="password"
                      showPasswordToggle
                      required
                      bind:value={passwordForm.values.newPassword}
                      error={passwordForm.errors.newPassword}
                    />
                    <PasswordStrength password={passwordForm.values.newPassword} />
                    <div class="flex justify-end gap-2">
                      <Button variant="ghost" size="sm" onclick={toggleEditPassword}>Cancel</Button>
                      <Button
                        type="submit"
                        variant="primary"
                        size="sm"
                        disabled={passwordForm.isSubmitting}
                        isLoading={passwordForm.isSubmitting}
                        loadingText="Updating..."
                        startIcon={Check}
                      >
                        Update password
                      </Button>
                    </div>
                  </form>
                {/if}
              </div>
            </ResponsiveDialog>
          </div>
        </section>

        <!-- Danger zone -->
        <section
          class="rounded-3xl border border-error-200 bg-white p-6 shadow-sm dark:border-error-900/60 dark:bg-gray-900/50"
        >
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
      </div>
    </div>
  </div>
</FullLayout>
