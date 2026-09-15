<script lang="ts">
  import { AppDialog, Button, InputTextField } from '$lib/ui/components';
  import { TriangleAlert } from 'lucide-svelte';

  let {
    open = $bindable(false),
    title = '',
    deleteConfirmation = $bindable(''),
    canDelete = false,
    desktopMode = 'modal',
    mobileMode = 'drawer',
    desktopPlacement = 'center',
    mobilePlacement = 'center',
    desktopAnimation = 'fade-zoom',
    mobileAnimation = 'slide-up',
    mobileDrawerSide = 'bottom',
    triggerElement = undefined,
    onConfirm = () => {},
    isDeleting = false
  }: {
    open: boolean;
    title: string;
    deleteConfirmation: string;
    canDelete: boolean;
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
    isDeleting?: boolean;
  } = $props();

  $effect(() => {
    if (open) {
      deleteConfirmation = '';
    }
  });
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
  tone="danger"
  title="Delete board"
  description="Permanently remove this board and all of its data."
>
  <div class="space-y-4 rounded-xl bg-gray-50 p-4 text-left dark:bg-gray-900/50">
    <p class="text-sm font-medium text-gray-800 dark:text-gray-200">
      Please type <span class="font-bold text-error-600 underline">{title}</span> to confirm:
    </p>
    <InputTextField
      id="modal-delete-confirmation"
      name="modalDeleteConfirmation"
      placeholder={title}
      bind:value={deleteConfirmation}
      class="border-error-200 focus:border-error-500 focus:ring-error-500/20 dark:border-error-900/50"
    />
  </div>

  {#snippet actions()}
    <Button
      variant="outline"
      disabled={isDeleting}
      haptic="light"
      onclick={() => {
        open = false;
      }}
    >
      Cancel
    </Button>
    <Button
      variant="danger"
      class="shadow-md shadow-error-500/20"
      onclick={onConfirm}
      disabled={!canDelete}
      isLoading={isDeleting}
      loadingText="Deleting"
      haptic="heavy"
    >
      Delete board
    </Button>
  {/snippet}
</AppDialog>
