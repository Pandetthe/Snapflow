<script lang="ts">
  import { BoardsService } from '$lib/features/boards/api/boards.api';
  import { UsersService, type SearchUserDto } from '$lib/features/users/api/users';
  import { apiClient } from '$lib/core/api.client';
  import {
    Button,
    FullLayout,
    InputTextField,
    GoBackButton,
    RoleBadge,
    Textarea,
    UserAvatar
  } from '$lib/ui/components';
  import RoleSelector from '$lib/features/boards/components/RoleSelector.svelte';
  import { createForm } from '$lib/ui/utils';
  import { FolderPlus, Plus, Users, X, Search, UserPlus } from 'lucide-svelte';
  import type { MemberRole } from '$lib/features/boards/types/boards.api';
  import { fade, slide, fly } from 'svelte/transition';

  const boardsService = new BoardsService(apiClient);
  const usersService = new UsersService(apiClient);

  let { data } = $props();

  type SelectedMember = SearchUserDto & { role: MemberRole };
  type OwnerMember = SearchUserDto & { role: 'owner' };

  let searchQuery = $state('');
  let searchResults = $state<SearchUserDto[]>([]);
  let searchError = $state('');
  let isSearching = $state(false);
  let searchRequestId = 0;

  const ownerMember = $derived.by<OwnerMember>(() => ({
    id: data.user.id,
    userName: data.user.userName,
    avatarUrl: data.user.avatarUrl,
    role: 'owner'
  }));

  let selectedMembers = $state<SelectedMember[]>([]);

  const backHref = '/boards';
  const selectedMembersCount = $derived(selectedMembers.length + 1);

  const searchExcludedIds = $derived.by(() => [
    ownerMember.id,
    ...selectedMembers.map((member) => member.id)
  ]);

  const form = createForm({
    initialValues: {
      title: '',
      description: ''
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

      return await boardsService.createBoard({
        title: values.title.trim(),
        description: values.description.trim(),
        members: members.length > 0 ? members : undefined
      });
    },
    onSuccess: (response) => {
      window.location.href = `/boards/${response.id}`;
    }
  });

  function isSelectedMember(userId: number) {
    return userId === ownerMember.id || selectedMembers.some((member) => member.id === userId);
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
  <title>Snapflow | Create your new board!</title>
</svelte:head>

<FullLayout>
  <div class="mx-auto w-full max-w-5xl space-y-8 pb-12" in:fade={{ duration: 400 }}>
    <header class="flex flex-col gap-4">
      <div class="flex items-center gap-2">
        <GoBackButton
          href={backHref}
        />
      </div>

      <div class="flex flex-col gap-1 sm:flex-row sm:items-end sm:justify-between">
        <div class="space-y-1">
          <h1 class="text-3xl font-bold tracking-tight text-gray-900 sm:text-4xl dark:text-white">
            Create your new board!
          </h1>
          <p class="text-gray-600 dark:text-gray-400">
            Set up your project environment and invite your team.
          </p>
        </div>
      </div>
    </header>

    <div class="grid items-start gap-8 lg:grid-cols-[1fr_minmax(20rem,25rem)]">
      <div class="space-y-6">
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
              <p class="text-xs text-gray-600 dark:text-gray-400">
                <span class="text-error-500">*</span> Required fields
              </p>
              <Button
                type="submit"
                variant="primary"
                size="lg"
                disabled={!form.values.title.trim() || form.isSubmitting}
                isLoading={form.isSubmitting}
                loadingText="Creating..."
                startIcon={Plus}
                class="px-8"
              >
                Create board
              </Button>
            </div>
          </form>
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
                class="absolute top-full right-0 left-0 z-50 mt-2 max-h-60 overflow-y-auto rounded-2xl border border-gray-300 bg-white p-1.5 shadow-xl transition-all duration-200 will-change-[opacity,transform] dark:border-gray-700 dark:bg-gray-900/90 dark:backdrop-blur-xl"
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

            {#each selectedMembers as member (member.id)}
              <div
                class="flex items-center justify-between rounded-2xl border border-gray-100 bg-gray-50/50 p-3 transition-all duration-200 dark:border-gray-800 dark:bg-white/2"
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
                      onRoleChange={(role) => updateMemberRole(member.id, role)}
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
