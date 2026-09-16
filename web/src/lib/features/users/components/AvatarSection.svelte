<script lang="ts">
  import {
    Button,
    UserAvatar,
    SegmentedControl,
    Dropzone,
    SettingsSection
  } from '$lib/ui/components';
  import { AvatarType, type UsersService } from '../api/users';
  import { avatarBust, bustAvatar } from '../avatarBust.svelte';
  import { errorStore } from '$lib/ui/stores/error.svelte';
  import { Check, Upload, Sparkles, User as UserIcon, Camera } from 'lucide-svelte';
  import { invalidateAll } from '$app/navigation';
  import { slide } from 'svelte/transition';
  import { slideReveal, triggerHaptic } from '$lib/ui/utils';
  import { untrack } from 'svelte';

  let {
    user,
    usersService
  }: {
    user:
      { userName?: string; avatarUrl?: string | null; avatarType?: AvatarType } | null | undefined;
    usersService: UsersService;
  } = $props();

  const allowedExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.webp'];
  const allowedFormatsLabel = 'JPG, PNG, GIF, WEBP';
  const acceptMimeTypes = 'image/jpeg,image/png,image/gif,image/webp';
  const maxFileSizeMB = 5;

  let selectedAvatarType = $state<AvatarType>(
    untrack(() => user?.avatarType) ?? AvatarType.Generated
  );
  let avatarFiles = $state<File[]>([]);
  let avatarPreview = $state<string | null>(null);
  let avatarFileError = $state('');
  let isUpdatingAvatar = $state(false);

  const avatarFile = $derived(avatarFiles[0] ?? null);

  $effect(() => {
    const file = avatarFiles[0] ?? null;
    if (!file) {
      avatarPreview = null;
      return;
    }
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
      triggerHaptic('error');
      avatarFileError = `Unsupported format · accepted: ${allowedFormatsLabel}`;
      avatarFiles = [];
      return;
    }
    if (file.size > maxFileSizeMB * 1024 * 1024) {
      triggerHaptic('error');
      avatarFileError = `File too large · max ${maxFileSizeMB} MB`;
      avatarFiles = [];
    }
  }

  async function handleSave() {
    if (isUpdatingAvatar) return;
    if (selectedAvatarType === AvatarType.Uploaded && !avatarFile) {
      triggerHaptic('error');
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
      triggerHaptic('success');
      avatarFiles = [];
      bustAvatar();
      await invalidateAll();
    } else {
      triggerHaptic('error');
      errorStore.addError(result.problem?.title ?? null, result.problem?.detail ?? null);
    }
  }
</script>

<SettingsSection
  icon={Camera}
  title="Profile picture"
  description="How you appear across your boards."
>
  <div class="mb-5 flex justify-center">
    <UserAvatar
      src={avatarPreview ?? (user?.avatarUrl ? `${user.avatarUrl}?v=${avatarBust.count}` : null)}
      name={user?.userName}
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
    onValueChange={() => {
      avatarFiles = [];
      avatarFileError = '';
    }}
  />

  {#if selectedAvatarType === AvatarType.Uploaded}
    <div class="mt-3" transition:slide={slideReveal}>
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
      onclick={handleSave}
      disabled={isUpdatingAvatar || (selectedAvatarType === AvatarType.Uploaded && !avatarFile)}
      isLoading={isUpdatingAvatar}
      loadingText="Saving"
      startIcon={Check}
      haptic="medium"
    >
      Save picture
    </Button>
  </div>
</SettingsSection>
