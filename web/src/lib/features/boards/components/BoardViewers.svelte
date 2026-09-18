<script lang="ts">
  import { flip } from 'svelte/animate';
  import { scale } from 'svelte/transition';
  import { cubicOut } from 'svelte/easing';
  import { UserAvatar } from '$lib/ui/components';
  import type { GetBoardByIdResponse } from '$lib/features/boards/types/boards.api';

  let {
    viewers,
    currentUserId,
    max = 4
  }: {
    viewers: GetBoardByIdResponse.UserDto[];
    currentUserId: number;
    max?: number;
  } = $props();

  const RING_COLORS = [
    'ring-rose-500',
    'ring-orange-500',
    'ring-amber-500',
    'ring-emerald-500',
    'ring-teal-500',
    'ring-sky-500',
    'ring-indigo-500',
    'ring-fuchsia-500'
  ];

  const ringColor = (id: number) =>
    RING_COLORS[((id % RING_COLORS.length) + RING_COLORS.length) % RING_COLORS.length];

  const shown = $derived(viewers.slice(0, max));
  const overflow = $derived(viewers.slice(max));
  const label = (viewer: GetBoardByIdResponse.UserDto) =>
    viewer.id === currentUserId ? `${viewer.userName} (you)` : viewer.userName;

  const summary = $derived(
    viewers.length === 1
      ? `${label(viewers[0])} is viewing this board`
      : `${viewers.length} people are viewing this board`
  );

  const pop = { duration: 200, easing: cubicOut, start: 0.6 };
</script>

{#if viewers.length > 0}
  <div class="flex shrink-0 items-center" aria-label={summary}>
    <span class="sr-only" role="status">{summary}</span>
    <div class="flex items-center -space-x-2">
      <!--
        The avatars overlap, so each one is stacked above the one to its right (the row is dealt like a hand
        of cards) and the one under the pointer comes in front of them all, ring and all.
      -->
      {#each shown as viewer, index (viewer.id)}
        <div
          animate:flip={{ duration: 200, easing: cubicOut }}
          in:scale={pop}
          out:scale={pop}
          style:--viewer-z={shown.length - index}
          class="group/viewer relative z-[var(--viewer-z)] hover:z-20"
        >
          <div class="transition-transform duration-150 group-hover/viewer:-translate-y-0.5">
            <UserAvatar
              src={viewer.avatarUrl}
              name={viewer.userName}
              size="sm"
              class="ring-2 ring-offset-2 ring-offset-white dark:ring-offset-gray-900 {ringColor(
                viewer.id
              )}"
            />
          </div>
          <span
            class="pointer-events-none absolute top-full left-1/2 z-30 mt-2 -translate-x-1/2 scale-95 rounded-md bg-gray-900 px-2 py-1 text-[11px] leading-none font-medium whitespace-nowrap text-white opacity-0 shadow-lg transition duration-150 group-hover/viewer:scale-100 group-hover/viewer:opacity-100 dark:bg-gray-700"
          >
            {label(viewer)}
          </span>
        </div>
      {/each}

      {#if overflow.length > 0}
        <div in:scale={pop} out:scale={pop} class="group/viewer relative z-0 hover:z-20">
          <span
            class="flex h-8 w-8 items-center justify-center rounded-full bg-gray-100 text-xs font-semibold text-gray-600 ring-2 ring-gray-300 ring-offset-2 ring-offset-white dark:bg-gray-800 dark:text-gray-300 dark:ring-gray-600 dark:ring-offset-gray-900"
          >
            +{overflow.length}
          </span>
          <span
            class="pointer-events-none absolute top-full right-0 z-30 mt-2 flex max-w-48 scale-95 flex-col gap-0.5 rounded-md bg-gray-900 px-2 py-1.5 text-[11px] leading-tight font-medium text-white opacity-0 shadow-lg transition duration-150 group-hover/viewer:scale-100 group-hover/viewer:opacity-100 dark:bg-gray-700"
          >
            {#each overflow as viewer (viewer.id)}
              <span class="truncate">{label(viewer)}</span>
            {/each}
          </span>
        </div>
      {/if}
    </div>
  </div>
{/if}
