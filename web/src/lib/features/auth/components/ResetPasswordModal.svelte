<script lang="ts">
  import { Dialog } from 'bits-ui';
  import { Button, ResponsiveDialog } from '$lib/ui/components';
  import { Check, CircleX } from 'lucide-svelte';
  import { cn } from '$lib/ui/utils';

  let {
    open = $bindable(false),
    variant = 'success',
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
    variant: 'success' | 'error';
    desktopMode?: 'modal' | 'drawer';
    mobileMode?: 'modal' | 'drawer';
    desktopPlacement?: 'center' | 'trigger';
    mobilePlacement?: 'center' | 'trigger';
    desktopAnimation?: 'fade-zoom' | 'slide-up' | 'slide-down' | 'slide-left' | 'slide-right' | 'none';
    mobileAnimation?: 'fade-zoom' | 'slide-up' | 'slide-down' | 'slide-left' | 'slide-right' | 'none';
    mobileDrawerSide?: 'top' | 'right' | 'bottom' | 'left';
    triggerElement?: HTMLElement | null;
  }>();

  let redirectCountdown = $state(5);
  let sliderWidth = $state(100);
  let redirectTimer: ReturnType<typeof setTimeout> | undefined;
  let redirectDelayTimer: ReturnType<typeof setTimeout> | undefined;

  const config = {
    success: {
      title: 'Password reset successful!',
      message: 'Your password has been successfully updated. You can now sign in with your new password.',
      icon: Check,
      color: 'text-green-600 dark:text-green-400',
      bgColor: 'bg-green-100 dark:bg-green-900/40'
    },
    error: {
      title: 'Reset password failed',
      message: 'The reset password attempt failed. Please try again later.',
      icon: CircleX,
      color: 'text-rose-600 dark:text-rose-400',
      bgColor: 'bg-rose-50 dark:bg-rose-500/10'
    }
  };

  const info = $derived(config[variant as keyof typeof config]);

  $effect(() => {
    if (open && variant === 'success' && redirectCountdown > 0) {
      redirectTimer = setTimeout(() => {
        redirectCountdown--;
        sliderWidth = (100 / 5) * redirectCountdown;
        if (sliderWidth < 0) sliderWidth = 0;
      }, 1000);
    } else if (open && variant === 'success' && redirectCountdown === 0) {
      redirectDelayTimer = setTimeout(() => {
        window.location.href = '/sign-in';
      }, 1000);
    }

    return () => {
      if (redirectTimer) clearTimeout(redirectTimer);
      if (redirectDelayTimer) clearTimeout(redirectDelayTimer);
    };
  });

  $effect(() => {
    if (!open) {
      redirectCountdown = 5;
      sliderWidth = 100;
    }
  });
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
  escapeClosable={variant !== 'success'}
  overlayClosable={variant !== 'success'}
  contentClass="border-white/10 bg-white/95 backdrop-blur-xl dark:bg-gray-900/95"
>
      <div class="space-y-6 text-center">
        <div
          class={cn(
            'mx-auto flex h-16 w-16 items-center justify-center rounded-full shadow-inner transition-all duration-500',
            info.bgColor
          )}
        >
          <info.icon size={32} class={cn('transition-transform duration-500', info.color)} />
        </div>

        <div class="space-y-2">
          <Dialog.Title class="text-xl font-bold tracking-tight text-gray-900 dark:text-white">
            {info.title}
          </Dialog.Title>
          <Dialog.Description class="text-base leading-relaxed text-gray-500 dark:text-gray-400">
            {info.message}
          </Dialog.Description>
        </div>

        {#if variant === 'success'}
          <div
            class="mb-8 rounded-2xl border border-brand-100 bg-brand-50 p-5 dark:border-brand-500/10 dark:bg-brand-500/5"
          >
            <div
              class="mb-3 flex items-center justify-between text-xs font-semibold text-brand-700 dark:text-brand-400"
            >
              <span>Redirecting to sign in</span>
              <span class="rounded-full bg-white px-2 py-0.5 font-mono dark:bg-black/20"
                >{redirectCountdown}s</span
              >
            </div>
            <div class="h-2 w-full overflow-hidden rounded-full bg-brand-200/50 dark:bg-brand-900/30">
              <div
                class="h-full rounded-full bg-brand-500 shadow-[0_0_8px_rgba(var(--color-brand-500),0.4)] transition-all duration-1000 ease-linear"
                style="width: {sliderWidth}%"
              ></div>
            </div>
          </div>
        {/if}

        <div class="flex flex-col-reverse justify-center gap-3 sm:flex-row">
          {#if variant === 'success'}
            <Button href="/sign-in" variant="primary" size="md" class="w-full justify-center" haptic="medium">
              Sign in now
            </Button>
          {:else}
            <Button
              onclick={() => {
                open = false;
              }}
              variant="primary"
              size="md"
              class="min-w-[140px]"
              haptic="light"
            >
              Understand
            </Button>
          {/if}
        </div>
      </div>
    </ResponsiveDialog>


