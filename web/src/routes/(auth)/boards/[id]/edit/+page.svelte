<script lang="ts">
  import { BoardsService } from '$lib/features/boards/api/boards.api';
  import { apiClient } from '$lib/core/api.client';
  import { errorStore } from '$lib/ui/stores/error.svelte';
  import {
    Button,
    FullLayout,
    InputTextField,
    GoBackButton,
    SettingsSection,
    Textarea
  } from '$lib/ui/components';
  import BoardMembersEditor from '$lib/features/boards/components/BoardMembersEditor.svelte';
  import { createForm } from '$lib/ui/utils';
  import {
    Trash2,
    Users,
    Check,
    TriangleAlert,
    LayoutDashboard,
    Tags as TagsIcon,
    RotateCcw,
    Eye
  } from 'lucide-svelte';
  import { afterNavigate, goto } from '$app/navigation';
  import { resolve } from '$app/paths';
  import type { Pathname } from '$app/types';
  import type { BoardVisibility, MemberRole } from '$lib/features/boards/types/boards.api';
  import type { OwnerMember, SelectedMember } from '$lib/features/boards/types/members';
  import { untrack } from 'svelte';
  import TransferOwnershipModal from '$lib/features/boards/components/TransferOwnershipModal.svelte';
  import DeleteBoardModal from '$lib/features/boards/components/DeleteBoardModal.svelte';
  import TagDefinitionsEditor from '$lib/features/boards/components/TagDefinitionsEditor.svelte';
  import VisibilitySelector from '$lib/features/boards/components/VisibilitySelector.svelte';

  let { data } = $props();
  const board = $derived.by(() => data.board);

  const boardsService = new BoardsService(apiClient);

  type MemberData = {
    id?: number;
    userId?: number;
    role: MemberRole | string;
    userName: string;
    avatarUrl: string | null;
  };

  let backHref = $state<Pathname>('/');

  let membersEditor = $state<ReturnType<typeof BoardMembersEditor>>();

  let tags = $state(untrack(() => data.tags));

  let visibility = $state<BoardVisibility>(untrack(() => data.board.visibility));

  function toSelectedMembers(members: MemberData[] | undefined): SelectedMember[] {
    return (
      members
        ?.filter((m) => m.role !== 'owner')
        .map((m) => ({
          id: (m.id ?? m.userId) as number,
          userName: m.userName,
          avatarUrl: m.avatarUrl,
          role: m.role as MemberRole
        })) ?? []
    );
  }

  let selectedMembers = $state<SelectedMember[]>(
    untrack(() => toSelectedMembers(data.board.members))
  );

  const ownerMember = $derived.by<OwnerMember | null>(() => {
    const owner = data.board.members?.find((m: MemberData) => m.role === 'owner');
    if (!owner) return null;
    return {
      id: (owner.id ?? (owner as MemberData).userId) as number,
      userName: owner.userName,
      avatarUrl: owner.avatarUrl,
      role: 'owner'
    } as OwnerMember;
  });

  const currentUserIsOwner = $derived(ownerMember?.id === data.user?.id);

  const selectedMembersCount = $derived(selectedMembers.length + (ownerMember ? 1 : 0));

  afterNavigate(({ from }) => {
    backHref = (from?.url.pathname.replace(/\/edit$/, '') ?? '/') as Pathname;
  });

  const form = createForm({
    initialValues: {
      title: untrack(() => data.board.title) || '',
      description: untrack(() => data.board.description) || ''
    },
    validate: (values) => {
      const errors: Record<string, string> = {};

      if (!values.title.trim()) {
        errors.title = 'Title is required';
      } else if (values.title.trim().length < 3) {
        errors.title = 'Title must be at least 3 characters';
      } else if (values.title.trim().length > 100) {
        errors.title = 'Title must be less than 100 characters';
      }

      if (values.description.length > 500) {
        errors.description = 'Description must be less than 500 characters';
      }

      return errors;
    },
    onSubmit: async (values) => {
      const members = selectedMembers.map((member) => ({
        userId: member.id,
        role: member.role
      }));

      if (ownerMember) {
        members.push({ userId: ownerMember.id, role: 'owner' });
      }

      return await boardsService.updateBoard(board.id, {
        title: values.title.trim(),
        description: values.description.trim(),
        members,
        visibility
      });
    },
    onSuccess: () => {
      goto(resolve(backHref));
    }
  });

  function memberSignature(members: SelectedMember[]) {
    return members
      .map((m) => `${m.id}:${m.role}`)
      .sort()
      .join(',');
  }

  let baseline = $state(
    untrack(() => ({
      title: data.board.title || '',
      description: data.board.description || '',
      members: memberSignature(toSelectedMembers(data.board.members)),
      visibility: data.board.visibility
    }))
  );

  const isDirty = $derived(
    form.values.title !== baseline.title ||
      form.values.description !== baseline.description ||
      memberSignature(selectedMembers) !== baseline.members ||
      visibility !== baseline.visibility
  );

  function discardChanges() {
    form.reset({ title: baseline.title, description: baseline.description });
    selectedMembers = toSelectedMembers(data.board.members);
    visibility = baseline.visibility;
    membersEditor?.clearSearch();
  }

  let currentBoardId = $state(untrack(() => data.board.id));

  $effect(() => {
    if (board && board.id !== currentBoardId) {
      currentBoardId = board.id;

      form.reset({
        title: board.title,
        description: board.description
      });

      selectedMembers = toSelectedMembers(board.members);
      visibility = board.visibility;
      baseline = {
        title: board.title || '',
        description: board.description || '',
        members: memberSignature(selectedMembers),
        visibility: board.visibility
      };

      tags = data.tags;
    }
  });

  let isTransferModalOpen = $state(false);
  let transferTargetUserId = $state<number | null>(null);
  let transferTargetUserName = $state('');
  let isTransferring = $state(false);

  function openTransferModal(userId: number, userName: string) {
    transferTargetUserId = userId;
    transferTargetUserName = userName;
    isTransferModalOpen = true;
  }

  async function confirmTransferOwnership() {
    if (!transferTargetUserId) return;

    isTransferring = true;
    try {
      const response = await boardsService.changeOwner(board.id, { userId: transferTargetUserId });
      if (response.ok) {
        goto(resolve(`/boards/${board.id}`), { invalidateAll: true });
      } else {
        errorStore.addError(
          response.problem?.title ?? 'Transfer Failed',
          response.problem?.detail ?? 'Could not transfer ownership'
        );
        isTransferModalOpen = false;
      }
    } catch {
      errorStore.addError('Error', 'An unexpected error occurred during transfer.');
      isTransferModalOpen = false;
    } finally {
      isTransferring = false;
    }
  }

  let deleteConfirmation = $state('');
  let isDeleting = $state(false);
  let isDeleteModalOpen = $state(false);

  const canDelete = $derived(deleteConfirmation.trim() === board.title.trim() && !isDeleting);

  async function handleDeleteBoard() {
    if (!canDelete || isDeleting) {
      return;
    }

    isDeleting = true;

    try {
      const response = await boardsService.deleteBoard(board.id);

      if (response.ok) {
        window.location.href = '/';
        return;
      }

      if (response.validationProblem?.errors?.length) {
        errorStore.addErrors(
          response.validationProblem.errors.map((err) => ({
            code: err.code,
            description: err.description
          }))
        );
        isDeleteModalOpen = false;
        return;
      }

      errorStore.addError(
        response.problem?.title ?? 'Web.DeleteBoardFailed',
        response.problem?.detail ?? 'Failed to delete board'
      );
      isDeleteModalOpen = false;
    } finally {
      isDeleting = false;
    }
  }
</script>

<svelte:head>
  <title>Snapflow | Edit board</title>
</svelte:head>

<FullLayout>
  <div class="mx-auto w-full max-w-3xl space-y-6 pb-12 sm:space-y-8">
    <header class="flex flex-col gap-4">
      <GoBackButton href={backHref} />

      <div class="space-y-1">
        <h1 class="text-2xl font-bold tracking-tight text-gray-900 sm:text-4xl dark:text-white">
          Edit board
        </h1>
        <p class="text-sm text-gray-600 sm:text-base dark:text-gray-400">
          Update your board settings and manage your team collaborators.
        </p>
      </div>
    </header>

    <form onsubmit={form.handleSubmit} novalidate class="space-y-6">
      <SettingsSection
        icon={LayoutDashboard}
        title="Board details"
        description="The name and summary everyone sees on this board."
      >
        <div class="space-y-5">
          <InputTextField
            id="title"
            name="title"
            label="Board title"
            placeholder="Development Roadmap"
            required={true}
            bind:value={form.values.title}
            error={form.errors.title}
            class="h-12 text-lg"
          />

          <Textarea
            id="description"
            name="description"
            label="Description"
            placeholder="Describe the goals and scope of this board..."
            maxlength={500}
            rows={4}
            bind:value={form.values.description}
            error={form.errors.description}
            helperText={`${form.values.description.length}/500`}
            class="resize-none"
          />

          <p class="text-xs text-gray-600 dark:text-gray-400">
            <span class="text-error-500">*</span> Required fields
          </p>
        </div>
      </SettingsSection>

      <SettingsSection icon={Eye} title="Visibility" description="Who can open this board.">
        <VisibilitySelector
          bind:value={visibility}
          allowedVisibilities={data.visibilityOptions.allowedVisibilities}
        />
      </SettingsSection>

      <SettingsSection
        icon={Users}
        title="Team"
        description={`${selectedMembersCount} member${selectedMembersCount === 1 ? '' : 's'} assigned — a role decides what each of them may change.`}
      >
        <BoardMembersEditor
          bind:this={membersEditor}
          bind:members={selectedMembers}
          owner={ownerMember}
          allowTransferOwnership={currentUserIsOwner}
          onTransferOwnership={openTransferModal}
        />
      </SettingsSection>

      <div
        class="sticky bottom-4 z-30 flex flex-col-reverse gap-3 rounded-2xl border border-gray-200 bg-white/95 p-3 shadow-lg backdrop-blur-sm sm:flex-row sm:items-center sm:justify-between sm:px-4 dark:border-gray-800 dark:bg-gray-900/95"
      >
        <p class="text-xs text-gray-600 sm:text-sm dark:text-gray-400">
          {#if isDirty}
            Board settings have unsaved changes.
          {:else}
            Board settings are up to date.
          {/if}
        </p>

        <div class="flex gap-2">
          <Button
            type="button"
            variant="ghost"
            size="md"
            startIcon={RotateCcw}
            disabled={!isDirty || form.isSubmitting}
            onclick={discardChanges}
          >
            Discard
          </Button>
          <Button
            type="submit"
            variant="primary"
            size="md"
            disabled={!form.values.title.trim() || !isDirty || form.isSubmitting}
            isLoading={form.isSubmitting}
            loadingText="Saving"
            startIcon={Check}
            class="flex-1 px-6 sm:flex-none"
          >
            Save changes
          </Button>
        </div>
      </div>
    </form>

    <SettingsSection
      icon={TagsIcon}
      title="Tags"
      description={`${tags.length} tag${tags.length === 1 ? '' : 's'} on this board — every change here is saved right away.`}
    >
      <TagDefinitionsEditor boardId={board.id} bind:tags />
    </SettingsSection>

    <SettingsSection
      danger
      icon={TriangleAlert}
      title="Danger zone"
      description="Permanently remove this board and all of its data."
    >
      <Button
        type="button"
        variant="danger"
        size="lg"
        class="w-full justify-center shadow-lg shadow-error-500/10"
        startIcon={Trash2}
        haptic="medium"
        onclick={() => {
          deleteConfirmation = '';
          isDeleteModalOpen = true;
        }}
      >
        Delete board
      </Button>
    </SettingsSection>
  </div>
</FullLayout>

<TransferOwnershipModal
  bind:open={isTransferModalOpen}
  userName={transferTargetUserName}
  {isTransferring}
  onConfirm={confirmTransferOwnership}
/>

<DeleteBoardModal
  bind:open={isDeleteModalOpen}
  title={board.title}
  bind:deleteConfirmation
  {canDelete}
  {isDeleting}
  onConfirm={handleDeleteBoard}
/>
