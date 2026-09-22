<script lang="ts">
  import { Button, InputTextField, UserAvatar } from '$lib/ui/components';
  import RoleSelector from '$lib/features/boards/components/RoleSelector.svelte';
  import { createUserSearch } from '$lib/features/users/composables/userSearch.svelte';
  import { itemIn, itemOut, slideReveal } from '$lib/ui/utils';
  import { UserPlus, X } from '@lucide/svelte';
  import { fade, fly, slide } from 'svelte/transition';
  import type { SearchUserDto } from '$lib/features/users/api/users';
  import type { MemberRole } from '$lib/features/boards/types/boards.api';
  import type { OwnerMember, SelectedMember } from '$lib/features/boards/types/members';

  let {
    members = $bindable(),
    owner,
    allowTransferOwnership = false,
    onTransferOwnership
  }: {
    members: SelectedMember[];
    owner: OwnerMember | null;
    allowTransferOwnership?: boolean;
    onTransferOwnership?: (userId: number, userName: string) => void;
  } = $props();

  function isSelectedMember(userId: number) {
    return userId === owner?.id || members.some((member) => member.id === userId);
  }

  const search = createUserSearch({
    excludedIds: () =>
      [owner?.id, ...members.map((member) => member.id)].filter((id) => id !== undefined),
    isExcluded: isSelectedMember
  });

  function addMember(member: SearchUserDto) {
    if (isSelectedMember(member.id)) return;

    members = [...members, { ...member, role: 'member' }];
    search.clear();
  }

  function removeMember(userId: number) {
    members = members.filter((member) => member.id !== userId);
  }

  function updateMemberRole(userId: number, role: MemberRole) {
    members = members.map((member) => (member.id === userId ? { ...member, role } : member));
  }

  export function clearSearch() {
    search.clear();
  }
</script>

<div class="relative mb-6">
  <InputTextField
    id="member-search"
    name="member-search"
    type="search"
    label="Add teammates"
    placeholder="Search by name..."
    bind:value={search.query}
    isLoading={search.isSearching}
    error={search.error}
    class="pr-10"
  />

  {#if search.results.length > 0}
    <div
      class="absolute top-full right-0 left-0 z-50 mt-2 max-h-60 overflow-y-auto rounded-2xl border border-gray-300 bg-white p-1.5 shadow-xl transition-all duration-200 will-change-[opacity,transform] dark:border-gray-700 dark:bg-gray-900/90 dark:backdrop-blur-xl"
      in:fly={itemIn}
      out:fade={itemOut}
    >
      {#each search.results as user (user.id)}
        <button
          type="button"
          class="group flex w-full cursor-pointer items-center justify-between rounded-xl px-3 py-2 transition-all duration-200 hover:bg-gray-50 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500 active:scale-[0.98] dark:hover:bg-white/5"
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
  {#if owner}
    <div
      class="flex items-center justify-between rounded-2xl border border-brand-100 bg-brand-50/30 p-3 transition-all duration-200 dark:border-brand-900/30 dark:bg-brand-500/5"
    >
      <div class="flex items-center gap-3">
        <UserAvatar
          src={owner.avatarUrl}
          name={owner.userName}
          size={36}
          class="bg-brand-100 dark:bg-brand-500/10"
        />
        <div class="min-w-0">
          <p class="truncate text-sm font-bold text-gray-900 dark:text-white">
            {owner.userName}
          </p>
          <RoleSelector role="owner" showArrow={false} disabled={true} />
        </div>
      </div>
    </div>
  {/if}

  {#each members as member (member.id)}
    <div
      class="flex items-center justify-between rounded-2xl border border-gray-100 bg-gray-50/50 p-3 transition-all duration-200 dark:border-gray-800 dark:bg-white/2"
      transition:slide={slideReveal}
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
            {allowTransferOwnership}
            onTransferOwnership={onTransferOwnership &&
              (() => onTransferOwnership(member.id, member.userName))}
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
