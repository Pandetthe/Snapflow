<script lang="ts">
  import { Dialog } from 'bits-ui';
  import { Button as AppButton, ResponsiveDialog } from '$lib/ui/components';
  import { Check } from 'lucide-svelte';
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

  let redirectCountdown = $state(5);
  let sliderWidth = $state(100);
  let redirectTimer: ReturnType<typeof setTimeout> | undefined;
  let redirectDelayTimer: ReturnType<typeof setTimeout> | undefined;

  const title = 'Welcome aboard!';
  const message =
    'Registration successful! Please check your email to confirm your account before signing in.';

  $effect(() => {
    if (open && redirectCountdown > 0) {
      redirectTimer = setTimeout(() => {
        redirectCountdown--;
        sliderWidth = (100 / 5) * redirectCountdown;
        if (sliderWidth < 0) sliderWidth = 0;
      }, 1000);
    } else if (open && redirectCountdown === 0) {
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
  escapeClosable={false}
  overlayClosable={false}
  contentClass="border-white/10 bg-white/95 backdrop-blur-xl dark:bg-gray-900/95"
>
      <div class="space-y-6 text-center">
        <div
          class={cn(
            'mx-auto flex h-16 w-16 items-center justify-center rounded-full shadow-inner transition-all duration-500',
            'bg-green-100 dark:bg-green-900/40'
          )}
        >
          <Check
            size={32}
            class={cn('transition-transform duration-500', 'text-green-600 dark:text-green-400')}
          />
        </div>

        <div class="space-y-2">
          <Dialog.Title class="text-xl font-bold tracking-tight text-gray-900 dark:text-white">
            {title}
          </Dialog.Title>
          <Dialog.Description class="text-base leading-relaxed text-gray-500 dark:text-gray-400">
            {message}
          </Dialog.Description>
        </div>

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

        <div class="flex flex-col-reverse justify-center gap-3 sm:flex-row">
          <AppButton href="/sign-in" variant="primary" size="md" class="w-full justify-center">
            Sign in now
          </AppButton>
        </div>
      </div>
    </ResponsiveDialog>


