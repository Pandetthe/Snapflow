<script lang="ts">
  import { AlertDialog } from 'bits-ui';
  import { onMount } from 'svelte';
  import { DialogBehavior, type DialogBehaviorOptions } from './dialogBehavior.svelte.js';

  let {
    open = $bindable(false),
    onOpenChange,
    children,
    ...options
  }: DialogBehaviorOptions & {
    open: boolean;
    onOpenChange?: (open: boolean) => void;
    children: any;
  } = $props();

  const behavior = new DialogBehavior({ ...options, zIndex: 'z-[60]' });

  onMount(() => behavior.mount());

  $effect(() => {
    if (open && behavior.useTriggerPosition) {
      behavior.updateTriggerPosition();
    }
  });

  function handleOpenChange(next: boolean) {
    if (open !== next) {
      if (!next) behavior.onClose();
      open = next;
      onOpenChange?.(next);
    }
  }
</script>

<AlertDialog.Root bind:open onOpenChange={handleOpenChange}>
  <AlertDialog.Portal>
    <AlertDialog.Overlay class={behavior.overlayClasses} />
    <AlertDialog.Content
      escapeKeydownBehavior={behavior.escapeClosable ? 'close' : 'ignore'}
      interactOutsideBehavior={behavior.overlayClosable ? 'close' : 'ignore'}
      class={behavior.baseContentClasses}
      style={behavior.positionedStyle}
    >
      {#if behavior.activeMode === 'drawer' && behavior.activeDrawerSide === 'bottom'}
        <div
          class="flex w-full cursor-grab justify-center pb-6 pt-2 active:cursor-grabbing"
          ontouchstart={behavior.handleHandleTouchStart}
          ontouchmove={behavior.handleHandleTouchMove}
          ontouchend={() => behavior.handleHandleTouchEnd(() => handleOpenChange(false))}
          onmousedown={(e) => behavior.handleHandleMouseDown(e, () => handleOpenChange(false))}
          role="button"
          tabindex="0"
          aria-label="Drag down to close"
        >
          <div class="h-1.5 w-12 rounded-full bg-gray-200 dark:bg-gray-700"></div>
        </div>
      {/if}
      {@render children()}
    </AlertDialog.Content>
  </AlertDialog.Portal>
</AlertDialog.Root>
