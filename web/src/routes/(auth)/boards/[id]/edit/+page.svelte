<script lang="ts">
  import { BoardsService } from '$lib/features/boards/api/boards.api';
  import { UsersService, type SearchUserDto } from '$lib/features/users/api/users';
  import { apiClient } from '$lib/core/api.client';
  import { errorStore } from '$lib/ui/stores/error';
  import {
    Button,
    FullLayout,
    InputTextField,
    RoleBadge,
    Textarea,
    UserAvatar
  } from '$lib/ui/components';
  import RoleSelector from '$lib/features/boards/components/RoleSelector.svelte';
  import { createForm } from '$lib/ui/utils';
  import {
    ArrowLeft,
    Shield,
    Trash2,
    Users,
    Search,
    X,
    UserPlus,
    Plus,
    Check,
    AlertTriangle
  } from 'lucide-svelte';
  import { afterNavigate, goto } from '$app/navigation';
  import type { MemberRole } from '$lib/features/boards/types/boards.api';
  import { fade, slide, fly } from 'svelte/transition';
  import { untrack } from 'svelte';
  import TransferOwnershipModal from '$lib/features/boards/components/TransferOwnershipModal.svelte';
  import DeleteBoardModal from '$lib/features/boards/components/DeleteBoardModal.svelte';

  let { data } = $props();
  const board = $derived.by(() => data.board);

  const boardsService = new BoardsService(apiClient);
  const usersService = new UsersService(apiClient);

  type SelectedMember = SearchUserDto & { role: MemberRole };
  type OwnerMember = SearchUserDto & { role: 'owner' };
  type MemberData = { id?: number; userId?: number; role: MemberRole | string; userName: string; avatarUrl: string | null };

  let backText = $state('Back');
  let backHref = $state('/boards');

  let searchQuery = $state('');
  let searchResults = $state<SearchUserDto[]>([]);
  let searchError = $state('');
  let isSearching = $state(false);
  let searchRequestId = 0;

  let selectedMembers = $state<SelectedMember[]>(
    untrack(() =>
      data.board.members
        ?.filter((m: MemberData) => m.role !== 'owner')
        .map((m: MemberData) => ({
          id: (m.id ?? m.userId) as number,
          userName: m.userName,
          avatarUrl: m.avatarUrl,
          role: m.role as MemberRole
        }))) || []
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

  const searchExcludedIds = $derived.by(() => [
    ownerMember?.id,
    ...selectedMembers.map((member) => member.id)
  ].filter(Boolean) as number[]);

  afterNavigate(({ from }) => {
    if (!from) {
      backHref = '/boards';
      return;
    }

    const path = from.url.pathname;

    if (path === '/boards') {
      backHref = '/boards';
    } else if (path.startsWith('/boards/')) {
      const parts = path.split('/');
      const id = parts[2];
      if (id && id !== 'new' && id !== 'edit') {
        backHref = `/boards/${id}`;
      } else {
        backHref = '/boards';
      }
    } else {
      backHref = '/boards';
    }
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
        members
      });
    },
    onSuccess: () => {
      goto(backHref);
    }
  });

  let currentBoardId = $state(untrack(() => data.board.id));

  $effect(() => {
    if (board && board.id !== currentBoardId) {
      currentBoardId = board.id;
      
      form.reset({
        title: board.title,
        description: board.description
      });

      if (board.members) {
        selectedMembers = board.members
          .filter((m: MemberData) => m.role !== 'owner')
          .map((m: MemberData) => ({
            id: (m.id ?? m.userId) as number,
            userName: m.userName,
            avatarUrl: m.avatarUrl,
            role: m.role as MemberRole
          }));
      } else {
        selectedMembers = [];
      }
    }
  });

  function isSelectedMember(userId: number) {
    return userId === ownerMember?.id || selectedMembers.some((member) => member.id === userId);
  }

  function addMember(member: SearchUserDto) {
    if (isSelectedMember(member.id)) {
      return;
    }

    selectedMembers = [...selectedMembers, { ...member, role: 'member' }];
    searchQuery = '';
    searchResults = [];
    searchError = '';
  }

  function removeMember(userId: number) {
    selectedMembers = selectedMembers.filter((member) => member.id !== userId);
  }

  function updateMemberRole(userId: number, role: MemberRole) {
    selectedMembers = selectedMembers.map((member) =>
      member.id === userId ? { ...member, role } : member
    );
  }

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
        goto(`/boards/${board.id}`, { invalidateAll: true });
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
        window.location.href = '/boards';
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

  $effect(() => {
    const query = searchQuery.trim();
    searchError = '';

    if (query.length < 2) {
      searchResults = [];
      isSearching = false;
      return;
    }

    isSearching = true;

    const timeoutId = setTimeout(async () => {
      const requestId = ++searchRequestId;
      const result = await usersService.searchUsers(query, searchExcludedIds);

      if (requestId !== searchRequestId) {
        return;
      }

      isSearching = false;

      if (result.ok) {
        searchResults = result.value.filter((user) => !isSelectedMember(user.id));
        return;
      }

      searchResults = [];
      searchError = result.problem?.detail ?? result.problem?.title ?? 'Failed to search users';
    }, 250);

    return () => {
      clearTimeout(timeoutId);
    };
  });
</script>

<svelte:head>
  <title>Snapflow | Edit board</title>
</svelte:head>

<FullLayout>
  <div class="mx-auto w-full max-w-5xl space-y-8 pb-12" in:fade={{ duration: 400 }}>
    <header class="flex flex-col gap-4">
      <div class="flex items-center gap-2">
        <Button
          href={backHref}
          variant="ghost"
          size="sm"
          class="-ml-2.5 text-gray-600 hover:bg-gray-100 dark:hover:bg-white/5"
          startIcon={ArrowLeft}
        >
          {backText}
        </Button>
      </div>

      <div class="flex flex-col gap-1 sm:flex-row sm:items-end sm:justify-between">
        <div class="space-y-1">
          <h1 class="text-3xl font-bold tracking-tight text-gray-900 sm:text-4xl dark:text-white">
            Edit board
          </h1>
          <p class="text-gray-600 dark:text-gray-400">
            Update your board settings and manage your team collaborators.
          </p>
        </div>
      </div>
    </header>

    <div class="grid items-start gap-8 lg:grid-cols-[1fr_minmax(320px,400px)]">
      <div class="space-y-8">
        <section
          class="group rounded-3xl border border-gray-200 bg-white p-6 shadow-sm transition-all duration-200 hover:border-brand-500/30 hover:shadow-md dark:border-gray-800 dark:bg-gray-900/50 dark:hover:border-brand-500/20"
        >
          <form onsubmit={form.handleSubmit} novalidate class="space-y-6">
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

            <div class="flex items-center justify-between pt-2">
              <p class="text-[11px] text-gray-600 dark:text-gray-400">
                <span class="text-error-500">*</span> Required fields
              </p>
              <div class="flex gap-3">
                <Button
                  type="submit"
                  variant="primary"
                  size="lg"
                  disabled={!form.values.title.trim() || form.isSubmitting}
                  isLoading={form.isSubmitting}
                  loadingText="Saving..."
                  startIcon={Check}
                  class="px-8"
                >
                  Save changes
                </Button>
              </div>
            </div>
          </form>
        </section>

        <section
          class="rounded-3xl border border-rose-100 bg-rose-50/20 p-6 transition-all duration-200 dark:border-rose-900/30 dark:bg-rose-950/10 sm:p-8"
        >
          <div class="mb-6 space-y-1">
            <h2 class="text-sm font-bold tracking-wider text-rose-600 uppercase dark:text-rose-400">
              Delete board
            </h2>
            <p class="text-xs text-rose-700/60 dark:text-rose-400/60">
              Permanently remove this board and all of its data.
            </p>
          </div>

          <div class="space-y-6">
            <Button
              type="button"
              variant="danger"
              size="lg"
              class="w-full justify-center shadow-lg shadow-rose-500/10"
              onclick={() => {
                deleteConfirmation = '';
                isDeleteModalOpen = true;
              }}
              startIcon={Trash2}
            >
              Delete board
            </Button>
          </div>
        </section>
      </div>

      <aside class="space-y-6">
        <section
          class="rounded-3xl border border-gray-200 bg-white p-6 shadow-sm transition-all duration-200 dark:border-gray-800 dark:bg-gray-900/50"
        >
          <div class="mb-6 flex items-start justify-between">
            <div>
              <h2 class="text-lg font-bold text-gray-900 dark:text-white">Project team</h2>
              <p class="text-sm text-gray-600 dark:text-gray-400">
                {selectedMembersCount} member{selectedMembersCount === 1 ? '' : 's'} assigned
              </p>
            </div>
            <Users class="h-5 w-5 text-gray-500" />
          </div>

          <div class="relative mb-6">
            <InputTextField
              id="member-search"
              name="member-search"
              type="search"
              label="Add teammates"
              placeholder="Search by name..."
              bind:value={searchQuery}
              isLoading={isSearching}
              class="pr-10"
            />

            {#if searchResults.length > 0}
              <div
                class="absolute left-0 right-0 top-full z-50 mt-2 max-h-60 overflow-y-auto rounded-2xl border border-gray-300 bg-white p-1.5 shadow-xl transition-all duration-200 will-change-[opacity,transform] dark:border-gray-700 dark:bg-gray-900/90 dark:backdrop-blur-xl"
                in:fly={{ y: 8, duration: 250 }}
                out:fade={{ duration: 150 }}
              >
                {#each searchResults as user (user.id)}
                  <button
                    type="button"
                    class="group flex w-full cursor-pointer items-center justify-between rounded-xl px-3 py-2 transition-all duration-200 hover:bg-gray-50 focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:outline-none active:scale-[0.98] dark:hover:bg-white/5 dark:focus-visible:ring-offset-gray-900"
                    onclick={() => addMember(user)}
                  >
                    <div class="flex items-center gap-3">
                      <UserAvatar src={user.avatarUrl} name={user.userName} size="sm" />
                      <span
                        class="text-sm font-medium text-gray-700 transition-colors group-hover:text-gray-900 dark:text-gray-200 dark:group-hover:text-white"
                        >{user.userName}</span
                      >
                    </div>
                    <UserPlus
                      size={16}
                      class="text-brand-500 opacity-60 transition-opacity group-hover:opacity-100"
                    />
                  </button>
                {/each}
              </div>
            {/if}
          </div>

          <div class="space-y-3">
            {#if ownerMember}
              <div
                class="flex items-center justify-between rounded-2xl border border-brand-100 bg-brand-50/30 p-3 transition-all duration-200 dark:border-brand-900/30 dark:bg-brand-500/5"
              >
                <div class="flex items-center gap-3">
                  <UserAvatar
                    src={ownerMember.avatarUrl}
                    name={ownerMember.userName}
                    size={36}
                    class="bg-brand-100 dark:bg-brand-500/10"
                  />
                  <div class="min-w-0">
                    <p class="truncate text-sm font-bold text-gray-900 dark:text-white">
                      {ownerMember.userName}
                    </p>
                    <RoleSelector role="owner" showArrow={false} disabled={true} />
                  </div>
                </div>
              </div>
            {/if}

            {#each selectedMembers as member (member.id)}
              <div
                class="flex items-center justify-between rounded-2xl border border-gray-100 bg-gray-50/50 p-3 transition-all duration-200 dark:border-gray-800 dark:bg-white/[0.02]"
                transition:slide={{ duration: 300 }}
              >
                <div class="flex items-center gap-3">
                  <UserAvatar src={member.avatarUrl} name={member.userName} size={36} />
                  <div class="min-w-0">
                    <p class="truncate text-sm font-semibold text-gray-900 dark:text-white">
                      {member.userName}
                    </p>
                    <RoleSelector
                      role={member.role}
                      onRoleChange={(role) => updateMemberRole(member.id as number, role)}
                      allowTransferOwnership={currentUserIsOwner}
                      onTransferOwnership={() => openTransferModal(member.id as number, member.userName)}
                    />
                  </div>
                </div>

                <Button
                  variant="ghost"
                  size="xs"
                  class="h-8 w-8 rounded-full p-0 text-gray-400 hover:text-error-600 dark:hover:text-error-400"
                  onclick={() => removeMember(member.id)}
                  aria-label="Remove member"
                  haptic="light"
                >
                  <X size={16} />
                </Button>
              </div>
            {/each}
          </div>
        </section>


      </aside>
    </div>
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