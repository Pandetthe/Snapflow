<script lang="ts">
  import { AppDialog, Button } from '$lib/ui/components';
  import type { DialogTone } from '$lib/ui/components/dialogs/AppDialog.svelte';
  import { AuthService } from '../api/auth';
  import { apiClient } from '$lib/core/api.client';
  import { errorStore } from '$lib/ui/stores/error.svelte';
  import type { Icon as IconType } from 'lucide-svelte';
  import {
    ShieldAlert,
    KeyRound,
    CircleX,
    MailCheck,
    MailQuestionMark,
    UserX
  } from 'lucide-svelte';

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
    desktopAnimation?:
      'fade-zoom' | 'slide-up' | 'slide-down' | 'slide-left' | 'slide-right' | 'none';
    mobileAnimation?:
      'fade-zoom' | 'slide-up' | 'slide-down' | 'slide-left' | 'slide-right' | 'none';
    mobileDrawerSide?: 'top' | 'right' | 'bottom' | 'left';
    triggerElement?: HTMLElement | null;
  }>();

  let isResending = $state(false);
  const authService = new AuthService(apiClient);

  const signInInfoByCode: Record<
    string,
    { title: string; message: string; icon: typeof IconType; tone: DialogTone }
  > = {
    'Users.SignIn.Failed': {
      title: 'Sign in failed',
      message: 'The sign-in attempt failed. Please check your credentials and try again.',
      icon: CircleX,
      tone: 'danger'
    },
    'Users.SignIn.LockedOut': {
      title: 'Account locked out',
      message: 'Your account has been locked out. Please try again later or contact support.',
      icon: ShieldAlert,
      tone: 'warning'
    },
    'Users.SignIn.NotAllowed': {
      title: 'Verify your email',
      message:
        'Sign in is not allowed for this account. You need to verify your email address first.',
      icon: MailQuestionMark,
      tone: 'info'
    },
    'Users.SignIn.TwoFactorRequired': {
      title: '2FA Required',
      message:
        'Two-factor authentication is required. Please sign in using your two-factor authentication method.',
      icon: KeyRound,
      tone: 'brand'
    },
    'Users.AccountDeleted': {
      title: 'Account deleted',
      message: 'This account has been deleted and can no longer be used to sign in.',
      icon: UserX,
      tone: 'danger'
    },
    'Users.External.Failed': {
      title: 'Sign in failed',
      message: 'Signing in with the provider did not work. Please try again.',
      icon: CircleX,
      tone: 'danger'
    },
    'Users.External.ProviderNotAvailable': {
      title: 'Sign-in option unavailable',
      message: 'This sign-in option is not available. Please choose another one.',
      icon: CircleX,
      tone: 'danger'
    },
    'Users.External.EmailMissing': {
      title: 'Email not shared',
      message:
        'The provider did not share your email address, which is needed to sign you in. Allow access to your email and try again.',
      icon: MailQuestionMark,
      tone: 'info'
    },
    'Users.External.EmailNotVerified': {
      title: 'Account already exists',
      message:
        'An account with this email already exists, but the provider has not verified the email, so it cannot be used for that account. Sign in with your password instead.',
      icon: ShieldAlert,
      tone: 'warning'
    },
    'Users.External.AccountNotConfirmed': {
      title: 'Confirm your email first',
      message:
        'An account with this email exists, but its email is not confirmed yet. Confirm it or sign in with its password before using this provider.',
      icon: MailQuestionMark,
      tone: 'info'
    },
    'Users.External.SignUpDisabled': {
      title: 'No account found',
      message: 'There is no account for this sign-in, and new accounts cannot be created with it.',
      icon: UserX,
      tone: 'danger'
    },
    'Users.External.ConfirmationSent': {
      title: 'Confirm your email',
      message:
        'Your account was created. Open the link we sent to your email address, then sign in again.',
      icon: MailCheck,
      tone: 'brand'
    },
    'Users.PasswordAuthentication.Disabled': {
      title: 'Password sign-in is off',
      message: "Signing in with a password is disabled. Use your organization's sign-in instead.",
      icon: ShieldAlert,
      tone: 'warning'
    },
    'Users.TwoFactor.SignInExpired': {
      title: 'Sign in again',
      message:
        'The sign-in expired before the code was entered. Sign in again to get a new chance.',
      icon: KeyRound,
      tone: 'brand'
    },
    'Users.Passkeys.NotRecognized': {
      title: 'Passkey not recognized',
      message:
        'This passkey is not linked to an account here. Try another passkey or sign in with your password.',
      icon: KeyRound,
      tone: 'warning'
    },
    'Users.Passkeys.Expired': {
      title: 'Try again',
      message:
        'The passkey request expired before it was finished. Sign in with your passkey again.',
      icon: KeyRound,
      tone: 'brand'
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
  icon={info.icon}
  tone={info.tone}
  title={info.title}
  description={info.message}
>
  {#snippet actions()}
    {#if showResendButton}
      <Button
        variant="outline"
        onclick={resendEmailConfirmation}
        isLoading={isResending}
        loadingText="Sending"
        haptic="light"
      >
        Resend email
      </Button>
    {/if}
    <Button
      onclick={() => {
        open = false;
      }}
      haptic="medium"
    >
      Understand
    </Button>
  {/snippet}
</AppDialog>
