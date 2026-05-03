<script lang="ts">
  import { Dialog } from 'bits-ui';
  import { onMount } from 'svelte';
  import { cn } from '$lib/ui/utils';

  type DialogMode = 'modal' | 'drawer';
  type DialogPlacement = 'center' | 'trigger';
  type DialogSize = 'sm' | 'md' | 'lg' | 'xl' | 'full';
  type DrawerSide = 'top' | 'right' | 'bottom' | 'left';
  type DialogAnimation = 'fade-zoom' | 'slide-up' | 'slide-down' | 'slide-left' | 'slide-right' | 'none';

  const SIZE_CLASS: Record<DialogSize, string> = {
    sm: 'max-w-sm',
    md: 'max-w-md',
    lg: 'max-w-lg',
    xl: 'max-w-xl',
    full: 'max-w-none'
  };

  let {
    open = $bindable(false),
    onOpenChange,
    desktopMode = 'modal',
    mobileMode = 'drawer',
    desktopPlacement = 'center',
    mobilePlacement = 'center',
    desktopAnimation = 'fade-zoom',
    mobileAnimation = 'slide-up',
    desktopDrawerSide = 'right',
    mobileDrawerSide = 'bottom',
    size = 'md',
    sizeClass = '',
    overlayClass = '',
    contentClass = '',
    escapeClosable = true,
    overlayClosable = true,
    breakpoint = 768,
    triggerElement = undefined,
    triggerOffset = 8,
    maxHeight = '88vh',
    children
  }: {
    open: boolean;
    onOpenChange?: (open: boolean) => void;
    desktopMode?: DialogMode;
    mobileMode?: DialogMode;
    desktopPlacement?: DialogPlacement;
    mobilePlacement?: DialogPlacement;
    desktopAnimation?: DialogAnimation;
    mobileAnimation?: DialogAnimation;
    desktopDrawerSide?: DrawerSide;
    mobileDrawerSide?: DrawerSide;
    size?: DialogSize;
    sizeClass?: string;
    overlayClass?: string;
    contentClass?: string;
    escapeClosable?: boolean;
    overlayClosable?: boolean;
    breakpoint?: number;
    triggerElement?: HTMLElement | null;
    triggerOffset?: number;
    maxHeight?: string;
    children: any;
  } = $props();

  let isMobile = $state(false);
  let triggerTop = $state(0);
  let triggerLeft = $state(0);

  let isDragging = $state(false);
  let dragStartY = $state(0);
  let dragY = $state(0);

  const activeMode = $derived(isMobile ? mobileMode : desktopMode);
  const activePlacement = $derived(isMobile ? mobilePlacement : desktopPlacement);
  const activeAnimation = $derived(isMobile ? mobileAnimation : desktopAnimation);
  const activeDrawerSide = $derived(isMobile ? mobileDrawerSide : desktopDrawerSide);
  const useTriggerPosition = $derived(activeMode === 'modal' && activePlacement === 'trigger');

  const overlayClasses = $derived(
    cn(
      'fixed inset-0 z-50 bg-black/40 backdrop-blur-md',
      'data-[state=closed]:animate-out data-[state=closed]:fade-out-0',
      'data-[state=open]:animate-in data-[state=open]:fade-in-0',
      overlayClass
    )
  );

  const animationClasses = $derived(getAnimationClasses(activeAnimation));
  const drawerSideClasses = $derived(getDrawerSideClasses(activeDrawerSide));

  const baseContentClasses = $derived(
    cn(
      'fixed z-50 w-[calc(100%-1.25rem)] overflow-y-auto border border-gray-200 bg-white shadow-2xl dark:border-gray-700 dark:bg-gray-900',
      SIZE_CLASS[size],
      sizeClass,
      animationClasses,
      activeMode === 'modal'
        ? useTriggerPosition
          ? 'translate-x-0 translate-y-0 rounded-2xl p-5 sm:p-6'
          : 'top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 rounded-2xl p-5 sm:p-6'
        : cn('w-full max-w-none p-6 pb-12 sm:p-8 sm:pb-14', drawerSideClasses),
      contentClass
    )
  );

  const positionedStyle = $derived.by(() => {
    let style = '';
    
    if (!useTriggerPosition) {
      style += `max-height: ${maxHeight};`;
    } else {
      style += `
        top: ${triggerTop}px;
        left: ${triggerLeft}px;
        max-height: ${maxHeight};
      `;
    }

    if (dragY > 0) {
      style += `transform: translateY(${dragY}px) !important; transition: ${isDragging ? 'none' : 'transform 0.2s ease-out'} !important;`;
    }

    return style;
  });

  function getAnimationClasses(animation: DialogAnimation) {
    switch (animation) {
      case 'slide-up':
        return 'duration-200 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:slide-out-to-bottom-4 data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=open]:slide-in-from-bottom-4';
      case 'slide-down':
        return 'duration-200 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:slide-out-to-top-4 data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=open]:slide-in-from-top-4';
      case 'slide-left':
        return 'duration-200 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:slide-out-to-right-4 data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=open]:slide-in-from-right-4';
      case 'slide-right':
        return 'duration-200 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:slide-out-to-left-4 data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=open]:slide-in-from-left-4';
      case 'none':
        return '';
      case 'fade-zoom':
      default:
        return 'duration-200 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:zoom-out-95 data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=open]:zoom-in-95';
    }
  }

  function getDrawerSideClasses(side: DrawerSide) {
    switch (side) {
      case 'top':
        return 'top-0 left-0 right-0 rounded-b-2xl border-x-0 border-t-0';
      case 'right':
        return 'top-0 right-0 h-full max-h-none w-[min(100%,26rem)] rounded-l-2xl border-y-0 border-r-0';
      case 'left':
        return 'top-0 left-0 h-full max-h-none w-[min(100%,26rem)] rounded-r-2xl border-y-0 border-l-0';
      case 'bottom':
      default:
        return 'right-0 bottom-0 left-0 rounded-t-2xl border-x-0 border-b-0';
    }
  }

  function updateViewport() {
    isMobile = window.innerWidth < breakpoint;
  }

  function updateTriggerPosition() {
    if (!triggerElement) return;

    const rect = triggerElement.getBoundingClientRect();
    triggerTop = rect.bottom + triggerOffset;
    triggerLeft = rect.left;
  }

  function handleOpenChange(next: boolean) {
    if (open !== next) {
      if (!next && dragY === 0) {
        // Normal close, do nothing
      } else if (!next) {
        // Close with reset delay to allow our override animation to play
        setTimeout(() => { dragY = 0; }, 300);
      }
      open = next;
      onOpenChange?.(next);
    }
  }

  function handleHandleTouchStart(e: TouchEvent) {
    isDragging = true;
    dragStartY = e.touches[0].clientY;
  }
  function handleHandleTouchMove(e: TouchEvent) {
    if (!isDragging) return;
    const currentY = e.touches[0].clientY;
    const deltaY = currentY - dragStartY;
    if (deltaY > 0) {
      dragY = deltaY;
    }
  }
  function handleHandleTouchEnd() {
    if (!isDragging) return;
    isDragging = false;
    if (dragY > 100) {
      dragY = window.innerHeight; // Animate off screen naturally
      handleOpenChange(false);
    } else {
      dragY = 0;
    }
  }
  
  function handleHandleMouseDown(e: MouseEvent) {
    isDragging = true;
    dragStartY = e.clientY;
    window.addEventListener('mousemove', handleWindowMouseMove);
    window.addEventListener('mouseup', handleWindowMouseUp);
  }
  function handleWindowMouseMove(e: MouseEvent) {
    if (!isDragging) return;
    const deltaY = e.clientY - dragStartY;
    if (deltaY > 0) {
      dragY = deltaY;
    }
  }
  function handleWindowMouseUp() {
    if (!isDragging) return;
    isDragging = false;
    window.removeEventListener('mousemove', handleWindowMouseMove);
    window.removeEventListener('mouseup', handleWindowMouseUp);
    
    if (dragY > 100) {
      dragY = window.innerHeight; // Animate off screen naturally
      handleOpenChange(false);
    } else {
      dragY = 0;
    }
  }

  onMount(() => {
    updateViewport();
    updateTriggerPosition();

    const handleResize = () => {
      updateViewport();
      updateTriggerPosition();
    };

    const handleScroll = () => {
      updateTriggerPosition();
    };

    window.addEventListener('resize', handleResize);
    window.addEventListener('scroll', handleScroll, true);

    return () => {
      window.removeEventListener('resize', handleResize);
      window.removeEventListener('scroll', handleScroll, true);
    };
  });

  $effect(() => {
    if (open && useTriggerPosition) {
      updateTriggerPosition();
    }
  });
</script>

<Dialog.Root bind:open onOpenChange={handleOpenChange}>
  <Dialog.Portal>
    <Dialog.Overlay class={overlayClasses} />
    <Dialog.Content
      escapeKeydownBehavior={escapeClosable ? 'close' : 'ignore'}
      interactOutsideBehavior={overlayClosable ? 'close' : 'ignore'}
      class={baseContentClasses}
      style={positionedStyle}
    >
      {#if activeMode === 'drawer' && activeDrawerSide === 'bottom'}
        <div
          class="flex w-full cursor-grab justify-center pb-6 pt-2 active:cursor-grabbing"
          ontouchstart={handleHandleTouchStart}
          ontouchmove={handleHandleTouchMove}
          ontouchend={handleHandleTouchEnd}
          onmousedown={handleHandleMouseDown}
          role="button"
          tabindex="0"
          aria-label="Drag down to close"
        >
          <div class="h-1.5 w-12 rounded-full bg-gray-200 dark:bg-gray-700"></div>
        </div>
      {/if}
      {@render children()}
    </Dialog.Content>
  </Dialog.Portal>
</Dialog.Root>
