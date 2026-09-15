<script lang="ts">
  import type { AppNotice } from '$lib/ui/stores/notice.svelte';
  import { AlertDialog } from 'bits-ui';
  import { Button, ResponsiveAlertDialog } from '$lib/ui/components';
  import { Info } from 'lucide-svelte';

  let {
    isOpen = $bindable(false),
    notice = $bindable(undefined),
    desktopMode = 'modal',
    mobileMode = 'drawer',
    mobileDrawerSide = 'bottom'
  }: {
    isOpen?: boolean;
    notice?: AppNotice;
    desktopMode?: 'modal' | 'drawer';
    mobileMode?: 'modal' | 'drawer';
    mobileDrawerSide?: 'top' | 'right' | 'bottom' | 'left';
  } = $props();
</script>

<ResponsiveAlertDialog
  bind:open={isOpen}
  size="md"
  {desktopMode}
  {mobileMode}
  {mobileDrawerSide}
  contentClass="border-white/10 bg-white/95 backdrop-blur-xl dark:bg-gray-900/95"
>
  <div class="text-center">
    <div
      class="mx-auto mb-6 flex h-16 w-16 animate-fade-in items-center justify-center rounded-full bg-brand-50 shadow-inner motion-reduce:animate-none dark:bg-brand-900/20"
    >
      <Info class="h-8 w-8 text-brand-600 dark:text-brand-400" aria-hidden="true" />
    </div>

    <AlertDialog.Title
      class="mb-2 animate-fade-in text-xl font-bold tracking-tight text-gray-900 motion-reduce:animate-none dark:text-white"
      style="animation-delay: 100ms"
    >
      {notice?.title ?? ''}
    </AlertDialog.Title>
    <AlertDialog.Description
      class="animate-fade-in text-base leading-relaxed text-gray-500 motion-reduce:animate-none dark:text-gray-400"
      style="animation-delay: 200ms"
    >
      {notice?.message ?? ''}
    </AlertDialog.Description>

    <AlertDialog.Cancel>
      {#snippet children()}
        <Button
          variant="primary"
          size="md"
          class="mt-6 w-full justify-center sm:min-w-35"
          haptic="light"
        >
          Got it
        </Button>
      {/snippet}
    </AlertDialog.Cancel>
  </div>
</ResponsiveAlertDialog>
