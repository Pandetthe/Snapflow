<script lang="ts">
  import { AlertDialog } from 'bits-ui';
  import { Button, ResponsiveAlertDialog } from '$lib/ui/components';
  import { TriangleAlert } from 'lucide-svelte';

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
    desktopAnimation?: 'fade-zoom' | 'slide-up' | 'slide-down' | 'slide-left' | 'slide-right' | 'none';
    mobileAnimation?: 'fade-zoom' | 'slide-up' | 'slide-down' | 'slide-left' | 'slide-right' | 'none';
    mobileDrawerSide?: 'top' | 'right' | 'bottom' | 'left';
    triggerElement?: HTMLElement | null;
    onConfirm?: () => void;
    isTransferring?: boolean;
  } = $props();
</script>

<ResponsiveAlertDialog
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
  contentClass="border-white/10 bg-white/95 backdrop-blur-xl dark:bg-gray-900/95"
>
      <div class="space-y-6 text-center">
        <div
          class="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-amber-100 shadow-inner dark:bg-amber-900/30"
        >
          <TriangleAlert size={32} class="text-amber-600 dark:text-amber-400" />
        </div>

        <div class="space-y-2">
          <AlertDialog.Title class="text-xl font-bold tracking-tight text-gray-900 dark:text-white">
            Transfer Ownership
          </AlertDialog.Title>
          <AlertDialog.Description
            class="text-base leading-relaxed text-gray-500 dark:text-gray-400"
          >
            This action cannot be undone. Are you sure you want to make <span
              class="font-bold text-gray-900 dark:text-white">{userName}</span
            > the new owner of this board? You will lose all exclusive owner privileges.
          </AlertDialog.Description>
        </div>

        <div class="mt-6 flex flex-col-reverse justify-center gap-3 sm:flex-row">
          <AlertDialog.Cancel>
            {#snippet children()}
              <Button
                variant="outline"
                size="md"
                class="w-full sm:min-w-32"
                disabled={isTransferring}
                haptic="light">Cancel</Button
              >
            {/snippet}
          </AlertDialog.Cancel>
          <Button
            variant="danger"
            size="md"
            class="w-full bg-amber-600 text-white shadow-md shadow-amber-600/20 hover:bg-amber-700 sm:min-w-32"
            onclick={onConfirm}
            isLoading={isTransferring}
            loadingText="Transferring..."
            haptic="medium"
          >
            Yes, transfer
          </Button>
        </div>
      </div>
    </ResponsiveAlertDialog>
