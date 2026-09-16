<script lang="ts">
  import { SegmentedControl } from '$lib/ui/components';
  import ShareBoardModal from '$lib/features/boards/components/ShareBoardModal.svelte';
  import type { BoardVisibility } from '$lib/features/boards/types/boards.api';
  import { Globe, Link, Lock } from 'lucide-svelte';

  let {
    value = $bindable(),
    allowedVisibilities
  }: {
    value: BoardVisibility;
    allowedVisibilities: BoardVisibility[];
  } = $props();

  const options: Record<
    BoardVisibility,
    { label: string; icon: typeof Lock; description: string }
  > = {
    private: {
      label: 'Private',
      icon: Lock,
      description: 'Only members of this board can open it.'
    },
    unlisted: {
      label: 'Link only',
      icon: Link,
      description:
        'Anyone with the link can view this board, even without an account, but it is not listed on the home page. Members stay hidden and only members can make changes.'
    },
    public: {
      label: 'Public',
      icon: Globe,
      description:
        'Everyone can find this board on the home page and view it, even without an account. Members stay hidden and only members can make changes.'
    }
  };

  let selected = $state<BoardVisibility>(value);
  let confirmOpen = $state(false);
  let pending = $state<'unlisted' | 'public'>('public');

  $effect(() => {
    if (!confirmOpen) selected = value;
  });

  const available = $derived(
    (Object.keys(options) as BoardVisibility[]).filter(
      (option) => option === value || allowedVisibilities.includes(option)
    )
  );

  function select(next: BoardVisibility) {
    if (next === value) return;
    if (next !== 'private') {
      pending = next;
      confirmOpen = true;
      return;
    }
    value = next;
  }

  function confirmShare() {
    value = pending;
    confirmOpen = false;
  }
</script>

<div class="space-y-3">
  <SegmentedControl
    size="xs"
    options={available.map((option) => ({
      value: option,
      label: options[option].label,
      icon: options[option].icon
    }))}
    bind:value={selected}
    onValueChange={select}
  />
  <p class="text-xs text-gray-500 dark:text-gray-400">
    {options[value].description}
  </p>
  {#if !allowedVisibilities.includes(value)}
    <p class="text-xs text-warning-600 dark:text-warning-500">
      This server no longer allows {options[value].label.toLowerCase()} boards, so only members can open
      it.
    </p>
  {/if}
</div>

<ShareBoardModal bind:open={confirmOpen} visibility={pending} onConfirm={confirmShare} />
