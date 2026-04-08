<script lang="ts">
  import { Dialog } from 'bits-ui';
  import { Button, ResponsiveDialog } from '$lib/ui/components';
  import { Info } from 'lucide-svelte';
  import { cn } from '$lib/ui/utils';

  let {
    open = $bindable(false),
    desktopMode = 'modal',
    mobileMode = 'drawer',
    desktopPlacement = 'center',
    mobilePlacement = 'center',
    desktopAnimation = 'fade-zoom',
    mobileAnimation = 'slide-up',
    mobileDrawerSide = 'bottom',
    triggerElement = undefined
  } = $props<{
    open: boolean;
    desktopMode?: 'modal' | 'drawer';
    mobileMode?: 'modal' | 'drawer';
    desktopPlacement?: 'center' | 'trigger';
    mobilePlacement?: 'center' | 'trigger';
    desktopAnimation?: 'fade-zoom' | 'slide-up' | 'slide-down' | 'slide-left' | 'slide-right' | 'none';
    mobileAnimation?: 'fade-zoom' | 'slide-up' | 'slide-down' | 'slide-left' | 'slide-right' | 'none';
    mobileDrawerSide?: 'top' | 'right' | 'bottom' | 'left';
    triggerElement?: HTMLElement | null;
  }>();

  const title = 'Password Reset Requested';
  const message =
    "If an account exists for the provided email, you'll receive instructions to reset your password shortly. Please check your inbox and spam folder.";
</script>

<ResponsiveDialog
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
    </ResponsiveDialog>


