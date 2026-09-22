<script lang="ts">
  import { AppDialog, Button } from '$lib/ui/components';
  import { Globe, Link } from '@lucide/svelte';

  let {
    open = $bindable(false),
    visibility = 'public',
    onConfirm = () => {}
  }: {
    open: boolean;
    visibility?: 'unlisted' | 'public';
    onConfirm?: () => void;
  } = $props();

  const isPublic = $derived(visibility === 'public');
</script>

<AppDialog
  alert
  bind:open
  size="md"
  desktopMode="modal"
  mobileMode="drawer"
  mobileDrawerSide="bottom"
  icon={isPublic ? Globe : Link}
  tone="warning"
  title={isPublic ? 'Make board public' : 'Share board by link'}
>
  {#snippet description()}
    {#if isPublic}
      Everyone will be able to find this board on the home page and view it, including people
      without an account.
    {:else}
      Anyone with the link will be able to view this board, including people without an account. It
      will not be listed on the home page.
    {/if}
    They will see its swimlanes, lists, cards and tags, but not its members. Only members can change the
    board.
  {/snippet}

  {#snippet actions()}
    <Button
      variant="outline"
      haptic="light"
      onclick={() => {
        open = false;
      }}
    >
      Cancel
    </Button>
    <Button
      variant="danger"
      class="bg-amber-600 text-white shadow-md shadow-amber-600/20 hover:bg-amber-700"
      onclick={onConfirm}
      haptic="medium"
    >
      {isPublic ? 'Make public' : 'Share by link'}
    </Button>
  {/snippet}
</AppDialog>
