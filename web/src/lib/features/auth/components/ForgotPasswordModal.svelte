<script lang="ts">
  import { Dialog } from 'bits-ui';
  import { Button } from '$lib/ui/components';
  import { Info } from 'lucide-svelte';
  import { cn } from '$lib/ui/utils';

  let { open = $bindable(false) } = $props<{ open: boolean }>();

  const title = 'Password Reset Requested';
  const message =
    "If an account exists for the provided email, you'll receive instructions to reset your password shortly. Please check your inbox and spam folder.";
</script>

<Dialog.Root bind:open>
  <Dialog.Portal>
    <Dialog.Overlay
      class="fixed inset-0 z-50 bg-black/40 backdrop-blur-md transition-all duration-500 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:animate-in data-[state=open]:fade-in-0"
    />
    <Dialog.Content
      class="fixed top-1/2 left-1/2 z-50 w-[calc(100%-2rem)] max-w-md -translate-x-1/2 -translate-y-1/2 rounded-2xl border border-white/10 bg-white/95 p-8 shadow-2xl backdrop-blur-xl duration-500 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:zoom-out-95 data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=open]:zoom-in-95 dark:bg-gray-800/95"
    >
      <div class="space-y-6 text-center">
        <div
          class={cn(
            'mx-auto flex h-16 w-16 items-center justify-center rounded-full shadow-inner transition-all duration-500',
            'bg-blue-100 dark:bg-blue-900/30'
          )}
        >
          <Info size={32} class={cn('transition-transform duration-500', 'text-blue-600 dark:text-blue-400')} />
        </div>
        
        <div class="space-y-2">
          <Dialog.Title class="text-xl font-bold tracking-tight text-gray-900 dark:text-white">
            {title}
          </Dialog.Title>
          <Dialog.Description class="text-base leading-relaxed text-gray-500 dark:text-gray-400">
            {message}
          </Dialog.Description>
        </div>
        
        <div class="mt-4 flex flex-col-reverse justify-center gap-3 sm:flex-row">
          <Button
            onclick={() => {
              open = false;
            }}
            variant="primary"
            size="md"
            class="w-full justify-center"
            haptic="light"
          >
            Got it
          </Button>
        </div>
      </div>
    </Dialog.Content>
  </Dialog.Portal>
</Dialog.Root>


