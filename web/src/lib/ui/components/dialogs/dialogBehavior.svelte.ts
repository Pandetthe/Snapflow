import { cn } from '$lib/ui/utils';

export type DialogMode = 'modal' | 'drawer';
export type DialogPlacement = 'center' | 'trigger';
export type DialogSize = 'sm' | 'md' | 'lg' | 'xl' | 'full';
export type DrawerSide = 'top' | 'right' | 'bottom' | 'left';
export type DialogAnimation = 'fade-zoom' | 'slide-up' | 'slide-down' | 'slide-left' | 'slide-right' | 'none';

export const DIALOG_SIZE_CLASS: Record<DialogSize, string> = {
  sm: 'max-w-sm',
  md: 'max-w-md',
  lg: 'max-w-lg',
  xl: 'max-w-xl',
  full: 'max-w-none'
};

export interface DialogBehaviorOptions {
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
  zIndex?: string;
}

function getAnimationClasses(animation: DialogAnimation): string {
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

function getDrawerSideClasses(side: DrawerSide): string {
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

export class DialogBehavior {
  isMobile = $state(false);
  triggerTop = $state(0);
  triggerLeft = $state(0);
  isDragging = $state(false);
  dragStartY = $state(0);
  dragY = $state(0);

  #isDragDismissing = false;

  readonly desktopMode: DialogMode;
  readonly mobileMode: DialogMode;
  readonly desktopPlacement: DialogPlacement;
  readonly mobilePlacement: DialogPlacement;
  readonly desktopAnimation: DialogAnimation;
  readonly mobileAnimation: DialogAnimation;
  readonly desktopDrawerSide: DrawerSide;
  readonly mobileDrawerSide: DrawerSide;
  readonly size: DialogSize;
  readonly sizeClass: string;
  readonly overlayClass: string;
  readonly contentClass: string;
  readonly escapeClosable: boolean;
  readonly overlayClosable: boolean;
  readonly breakpoint: number;
  readonly triggerElement: HTMLElement | null | undefined;
  readonly triggerOffset: number;
  readonly maxHeight: string;
  readonly zIndex: string;

  constructor(options: DialogBehaviorOptions = {}) {
    this.desktopMode = options.desktopMode ?? 'modal';
    this.mobileMode = options.mobileMode ?? 'drawer';
    this.desktopPlacement = options.desktopPlacement ?? 'center';
    this.mobilePlacement = options.mobilePlacement ?? 'center';
    this.desktopAnimation = options.desktopAnimation ?? 'fade-zoom';
    this.mobileAnimation = options.mobileAnimation ?? 'slide-up';
    this.desktopDrawerSide = options.desktopDrawerSide ?? 'right';
    this.mobileDrawerSide = options.mobileDrawerSide ?? 'bottom';
    this.size = options.size ?? 'md';
    this.sizeClass = options.sizeClass ?? '';
    this.overlayClass = options.overlayClass ?? '';
    this.contentClass = options.contentClass ?? '';
    this.escapeClosable = options.escapeClosable ?? true;
    this.overlayClosable = options.overlayClosable ?? true;
    this.breakpoint = options.breakpoint ?? 768;
    this.triggerElement = options.triggerElement;
    this.triggerOffset = options.triggerOffset ?? 8;
    this.maxHeight = options.maxHeight ?? '88vh';
    this.zIndex = options.zIndex ?? 'z-50';
  }

  get activeMode(): DialogMode {
    return this.isMobile ? this.mobileMode : this.desktopMode;
  }

  get activePlacement(): DialogPlacement {
    return this.isMobile ? this.mobilePlacement : this.desktopPlacement;
  }

  get activeAnimation(): DialogAnimation {
    return this.isMobile ? this.mobileAnimation : this.desktopAnimation;
  }

  get activeDrawerSide(): DrawerSide {
    return this.isMobile ? this.mobileDrawerSide : this.desktopDrawerSide;
  }

  get useTriggerPosition(): boolean {
    return this.activeMode === 'modal' && this.activePlacement === 'trigger';
  }

  get overlayClasses(): string {
    return cn(
      `fixed inset-0 ${this.zIndex} bg-black/40 backdrop-blur-md`,
      'data-[state=closed]:animate-out data-[state=closed]:fade-out-0',
      'data-[state=open]:animate-in data-[state=open]:fade-in-0',
      this.overlayClass
    );
  }

  get baseContentClasses(): string {
    return cn(
      `fixed ${this.zIndex} w-[calc(100%-1.25rem)] overflow-y-auto border border-gray-200 bg-white shadow-2xl dark:border-gray-700 dark:bg-gray-900`,
      DIALOG_SIZE_CLASS[this.size],
      this.sizeClass,
      getAnimationClasses(this.activeAnimation),
      this.activeMode === 'modal'
        ? this.useTriggerPosition
          ? 'translate-x-0 translate-y-0 rounded-2xl p-5 sm:p-6'
          : 'top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 rounded-2xl p-5 sm:p-6'
        : cn('w-full max-w-none p-6 pb-12 sm:p-8 sm:pb-14', getDrawerSideClasses(this.activeDrawerSide)),
      this.contentClass
    );
  }

  get positionedStyle(): string {
    let style = '';
    if (!this.useTriggerPosition) {
      style += `max-height: ${this.maxHeight};`;
    } else {
      style += `top: ${this.triggerTop}px; left: ${this.triggerLeft}px; max-height: ${this.maxHeight};`;
    }
    if (this.dragY > 0) {
      style += ` transform: translateY(${this.dragY}px) !important; transition: ${this.isDragging ? 'none' : 'transform 0.2s ease-out'} !important;`;
    }
    return style;
  }

  onClose() {
    if (!this.#isDragDismissing) {
      this.isDragging = false;
      this.dragY = 0;
    }
  }

  updateTriggerPosition() {
    if (!this.triggerElement) return;
    const rect = this.triggerElement.getBoundingClientRect();
    this.triggerTop = rect.bottom + this.triggerOffset;
    this.triggerLeft = rect.left;
  }

  handleHandleTouchStart = (e: TouchEvent) => {
    this.isDragging = true;
    this.dragStartY = e.touches[0].clientY;
  };

  handleHandleTouchMove = (e: TouchEvent) => {
    if (!this.isDragging) return;
    const deltaY = e.touches[0].clientY - this.dragStartY;
    if (deltaY > 0) this.dragY = deltaY;
  };

  handleHandleTouchEnd = (close: () => void) => {
    if (!this.isDragging) return;
    this.isDragging = false;
    if (this.dragY > 100) {
      this.#dismissWithDrag(close);
    } else {
      this.dragY = 0;
    }
  };

  handleHandleMouseDown = (e: MouseEvent, close: () => void) => {
    this.isDragging = true;
    this.dragStartY = e.clientY;

    const onMouseMove = (ev: MouseEvent) => {
      if (!this.isDragging) return;
      const deltaY = ev.clientY - this.dragStartY;
      if (deltaY > 0) this.dragY = deltaY;
    };

    const onMouseUp = () => {
      if (!this.isDragging) return;
      this.isDragging = false;
      window.removeEventListener('mousemove', onMouseMove);
      window.removeEventListener('mouseup', onMouseUp);
      if (this.dragY > 100) {
        this.#dismissWithDrag(close);
      } else {
        this.dragY = 0;
      }
    };

    window.addEventListener('mousemove', onMouseMove);
    window.addEventListener('mouseup', onMouseUp);
  };

  #dismissWithDrag(close: () => void) {
    this.#isDragDismissing = true;
    this.dragY = window.innerHeight;
    setTimeout(() => {
      close();
      setTimeout(() => {
        this.dragY = 0;
        this.#isDragDismissing = false;
      }, 250);
    }, 220);
  }

  mount(): () => void {
    this.isMobile = window.innerWidth < this.breakpoint;
    this.updateTriggerPosition();

    const onResize = () => {
      this.isMobile = window.innerWidth < this.breakpoint;
      this.updateTriggerPosition();
    };
    const onScroll = () => this.updateTriggerPosition();

    window.addEventListener('resize', onResize);
    window.addEventListener('scroll', onScroll, true);

    return () => {
      window.removeEventListener('resize', onResize);
      window.removeEventListener('scroll', onScroll, true);
    };
  }
}
