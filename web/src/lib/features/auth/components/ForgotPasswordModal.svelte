<script lang="ts">
  import { AppDialog, Button } from '$lib/ui/components';
  import { Info } from 'lucide-svelte';

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

<AppDialog
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
  icon={Info}
  tone="info"
  {title}
  description={message}
>
  {#snippet actions()}
    <Button
      onclick={() => {
        open = false;
      }}
      haptic="light"
    >
      Got it
    </Button>
  {/snippet}
</AppDialog>
