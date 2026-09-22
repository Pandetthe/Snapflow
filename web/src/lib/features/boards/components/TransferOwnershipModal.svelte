<script lang="ts">
  import { AppDialog, Button } from '$lib/ui/components';
  import { TriangleAlert } from '@lucide/svelte';

  let {
    open = $bindable(false),
    userName = '',
    desktopMode = 'modal',
    mobileMode = 'drawer',
    desktopPlacement = 'center',
    mobilePlacement = 'center',
    desktopAnimation = 'fade-zoom',
    mobileAnimation = 'slide-up',
    mobileDrawerSide = 'bottom',
    triggerElement = undefined,
    onConfirm = () => {},
    isTransferring = false
  }: {
    open: boolean;
    userName: string;
    desktopMode?: 'modal' | 'drawer';
    mobileMode?: 'modal' | 'drawer';
    desktopPlacement?: 'center' | 'trigger';
    mobilePlacement?: 'center' | 'trigger';
    desktopAnimation?:
      'fade-zoom' | 'slide-up' | 'slide-down' | 'slide-left' | 'slide-right' | 'none';
    mobileAnimation?:
      'fade-zoom' | 'slide-up' | 'slide-down' | 'slide-left' | 'slide-right' | 'none';
    mobileDrawerSide?: 'top' | 'right' | 'bottom' | 'left';
    triggerElement?: HTMLElement | null;
    onConfirm?: () => void;
    isTransferring?: boolean;
  } = $props();
</script>

<AppDialog
  alert
  bind:open
  size="md"
  {desktopMode}
  {mobileMode}
  {mobileDrawerSide}
  {desktopPlacement}
  {mobilePlacement}
  {desktopAnimation}
  {mobileAnimation}
  {triggerElement}
  icon={TriangleAlert}
  tone="warning"
  title="Transfer ownership"
>
  {#snippet description()}
    This action cannot be undone. Are you sure you want to make <span
      class="font-bold text-gray-900 dark:text-white">{userName}</span
    > the new owner of this board? You will lose all exclusive owner privileges.
  {/snippet}

  {#snippet actions()}
    <Button
      variant="outline"
      disabled={isTransferring}
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
      isLoading={isTransferring}
      loadingText="Transferring"
      haptic="medium"
    >
      Yes, transfer
    </Button>
  {/snippet}
</AppDialog>
