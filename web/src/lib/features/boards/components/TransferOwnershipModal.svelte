<script lang="ts">
  import { AlertDialog } from 'bits-ui';
  import { Button } from '$lib/ui/components';
  import { TriangleAlert } from 'lucide-svelte';

  let {
    open = $bindable(false),
    userName = '',
    onConfirm = () => {},
    isTransferring = false
  }: {
    open: boolean;
    userName: string;
    onConfirm?: () => void;
    isTransferring?: boolean;
  } = $props();
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
                variant="ghost"
                size="md"
                class="w-full sm:min-w-[140px]"
                disabled={isTransferring}
                haptic="light">Cancel</Button
              >
            {/snippet}
          </AlertDialog.Cancel>
          <Button
            variant="danger"
            size="md"
            class="w-full !bg-amber-600 !text-white shadow-md shadow-amber-600/20 hover:!bg-amber-700 sm:min-w-[140px]"
            onclick={onConfirm}
            isLoading={isTransferring}
            loadingText="Transferring..."
            haptic="medium"
          >
            Yes, transfer
          </Button>
        </div>
      </div>
    </AlertDialog.Content>
  </AlertDialog.Portal>
</AlertDialog.Root>
