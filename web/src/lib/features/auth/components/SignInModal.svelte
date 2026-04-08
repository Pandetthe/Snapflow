<script lang="ts">
  import { Dialog } from 'bits-ui';
  import { Button, ResponsiveDialog } from '$lib/ui/components';
  import { cn } from '$lib/ui/utils';
  import { AuthService } from '../api/auth';
  import { apiClient } from '$lib/core/api.client';
  import { errorStore } from '$lib/ui/stores/error';
  import { ShieldAlert, KeyRound, CircleX, MailQuestionMark } from 'lucide-svelte';
  import { fade, scale } from 'svelte/transition';
  import { quintOut } from 'svelte/easing';

  let {
    open = $bindable(false),
    problemCode = '',
    userEmail = '',
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
    problemCode: string;
    userEmail: string;
    desktopMode?: 'modal' | 'drawer';
    mobileMode?: 'modal' | 'drawer';
    desktopPlacement?: 'center' | 'trigger';
    mobilePlacement?: 'center' | 'trigger';
    desktopAnimation?: 'fade-zoom' | 'slide-up' | 'slide-down' | 'slide-left' | 'slide-right' | 'none';
    mobileAnimation?: 'fade-zoom' | 'slide-up' | 'slide-down' | 'slide-left' | 'slide-right' | 'none';
    mobileDrawerSide?: 'top' | 'right' | 'bottom' | 'left';
    triggerElement?: HTMLElement | null;
  }>();

  let isResending = $state(false);
  const authService = new AuthService(apiClient);

  const signInInfoByCode: Record<
    string,
    { title: string; message: string; icon: any; color: string; bgColor: string }
  > = {
    'Users.SignIn.Failed': {
      title: 'Sign in failed',
      message: 'The sign-in attempt failed. Please check your credentials and try again.',
      icon: CircleX,
      color: 'text-rose-600 dark:text-rose-400',
      bgColor: 'bg-rose-50 dark:bg-rose-500/10'
    },
    'Users.SignIn.LockedOut': {
      title: 'Account locked out',
      message: 'Your account has been locked out. Please try again later or contact support.',
      icon: ShieldAlert,
      color: 'text-amber-600 dark:text-amber-400',
      bgColor: 'bg-amber-100 dark:bg-amber-900/30'
    },
    'Users.SignIn.NotAllowed': {
      title: 'Verify your email',
      message:
        'Sign in is not allowed for this account. You need to verify your email address first.',
      icon: MailQuestionMark,
      color: 'text-sky-600 dark:text-sky-400',
      bgColor: 'bg-sky-100 dark:bg-sky-900/30'
    },
    'Users.SignIn.TwoFactorRequired': {
      title: '2FA Required',
      message:
        'Two-factor authentication is required. Please sign in using your two-factor authentication method.',
      icon: KeyRound,
      color: 'text-brand-600 dark:text-brand-400',
      bgColor: 'bg-brand-100 dark:bg-brand-900/30'
    }
  };

  const currentInfo = $derived(signInInfoByCode[problemCode] || null);
  let lastInfo = $state(signInInfoByCode['Users.SignIn.Failed']);

  $effect(() => {
    if (currentInfo) {
      lastInfo = currentInfo;
    }
  });

  const info = $derived(currentInfo || lastInfo);
  const showResendButton = $derived(problemCode === 'Users.SignIn.NotAllowed');

  async function resendEmailConfirmation() {
    if (isResending) return;
    isResending = true;
    try {
      await authService.resendEmailConfirmation({ email: userEmail });
      open = false;
    } catch (err) {
      if (err instanceof Error) {
        if (err.message === 'Failed to fetch') {
          errorStore.addError('Web.ConnectionProblem', 'Problem with connection to the server');
        } else {
          errorStore.addError(err.name, err.message);
        }
      } else {
        errorStore.addError(null, 'Unknown error occurred while resending confirmation email');
      }
    } finally {
      isResending = false;
    }
  }
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
        <div class="mt-4 flex flex-col-reverse justify-center gap-3 sm:flex-row">
          {#if showResendButton}
            <Button
              variant="outline"
              size="md"
              class="min-w-[140px]"
              onclick={resendEmailConfirmation}
              isLoading={isResending}
              loadingText="Sending..."
              haptic="light"
            >
              Resend email
            </Button>
          {/if}
          <Button
            variant="primary"
            size="md"
            class="min-w-[140px]"
            onclick={() => {
              open = false;
            }}
            haptic="medium"
          >
            Understand
          </Button>
        </div>
      </div>
    </ResponsiveDialog>


