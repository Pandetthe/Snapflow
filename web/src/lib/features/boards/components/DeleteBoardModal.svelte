<script lang="ts">
  import { AlertDialog } from 'bits-ui';
  import { Button, InputTextField } from '$lib/ui/components';
  import { TriangleAlert } from 'lucide-svelte';

  let {
    open = $bindable(false),
    title = '',
    deleteConfirmation = $bindable(''),
    canDelete = false,
    onConfirm = () => {},
    isDeleting = false
  }: {
    open: boolean;
    title: string;
    deleteConfirmation: string;
    canDelete: boolean;
    onConfirm?: () => void;
    isDeleting?: boolean;
  } = $props();

  $effect(() => {
    if (open) {
      deleteConfirmation = '';
    }
  });
</script>

<AlertDialog.Root bind:open>
  <AlertDialog.Portal>
    <AlertDialog.Overlay
      class="fixed inset-0 z-50 bg-black/40 backdrop-blur-md transition-all duration-500 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:animate-in data-[state=open]:fade-in-0"
    />
    <AlertDialog.Content
      interactOutsideBehavior="close"
      escapeKeydownBehavior="close"
      class="fixed top-1/2 left-1/2 z-50 w-[calc(100%-2rem)] max-w-md -translate-x-1/2 -translate-y-1/2 rounded-2xl border border-white/10 bg-white/95 p-8 shadow-2xl backdrop-blur-xl duration-500 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:zoom-out-95 data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=open]:zoom-in-95 dark:bg-gray-800/95"
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
                variant="ghost"
                size="md"
                class="w-full sm:min-w-[140px]"
                disabled={isDeleting}
                haptic="light">Cancel</Button
              >
            {/snippet}
          </AlertDialog.Cancel>
          <Button
            variant="danger"
            size="md"
            class="w-full justify-center shadow-md shadow-rose-500/20 sm:min-w-[140px]"
            onclick={onConfirm}
            disabled={!canDelete}
            isLoading={isDeleting}
            loadingText="Deleting..."
            haptic="heavy"
          >
            Delete board
          </Button>
        </div>
      </div>
    </AlertDialog.Content>
  </AlertDialog.Portal>
</AlertDialog.Root>
