<script lang="ts">
  import type { AppNotice } from '$lib/ui/stores/notice.svelte';
  import { AppDialog, Button } from '$lib/ui/components';
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

<AppDialog
  alert
  bind:open={isOpen}
  size="md"
  {desktopMode}
  {mobileMode}
  {mobileDrawerSide}
  icon={Info}
  tone="brand"
  title={notice?.title ?? ''}
  description={notice?.message ?? ''}
>
  {#snippet actions()}
    <Button
      onclick={() => {
        isOpen = false;
      }}
      haptic="light"
    >
      Got it
    </Button>
  {/snippet}
</AppDialog>
