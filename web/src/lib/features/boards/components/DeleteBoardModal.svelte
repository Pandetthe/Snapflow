<script lang="ts">
  import { AlertDialog } from 'bits-ui';
  import { Button, InputTextField, ResponsiveAlertDialog } from '$lib/ui/components';
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
    desktopAnimation?: 'fade-zoom' | 'slide-up' | 'slide-down' | 'slide-left' | 'slide-right' | 'none';
    mobileAnimation?: 'fade-zoom' | 'slide-up' | 'slide-down' | 'slide-left' | 'slide-right' | 'none';
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
          class="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-rose-50 shadow-inner dark:bg-rose-500/10"
        >
          <TriangleAlert size={32} class="text-rose-600 dark:text-rose-400" />
        </div>

        <div class="space-y-2">
          <AlertDialog.Title class="text-xl font-bold tracking-tight text-gray-900 dark:text-white">
            Delete board
          </AlertDialog.Title>
          <AlertDialog.Description
            class="text-base leading-relaxed text-gray-500 dark:text-gray-400"
          >
            Permanently remove this board and all of its data.
          </AlertDialog.Description>
        </div>

        <div class="space-y-4 rounded-xl bg-gray-50 p-4 text-left dark:bg-gray-900/50">
          <p class="text-sm font-medium text-gray-800 dark:text-gray-200">
            Please type <span class="font-bold text-error-600 underline">{title}</span> to confirm:
          </p>
          <InputTextField
            id="modal-delete-confirmation"
            name="modalDeleteConfirmation"
            placeholder={title}
            bind:value={deleteConfirmation}
            class="border-rose-200 focus:border-rose-500 focus:ring-rose-500/20 dark:border-rose-900/50"
          />
        </div>

        <div class="mt-6 flex flex-col-reverse justify-center gap-3 sm:flex-row">
          <AlertDialog.Cancel>
            {#snippet children()}
              <Button
                variant="outline"
                size="md"
                class="w-full sm:min-w-32"
                disabled={isDeleting}
                haptic="light">Cancel</Button
              >
            {/snippet}
          </AlertDialog.Cancel>
          <Button
            variant="danger"
            size="md"
            class="w-full justify-center shadow-md shadow-rose-500/20 sm:min-w-32"
            onclick={onConfirm}
            disabled={!canDelete}
            isLoading={isDeleting}
            loadingText="Deleting"
            haptic="heavy"
          >
            Delete board
          </Button>
        </div>
      </div>
    </ResponsiveAlertDialog>
