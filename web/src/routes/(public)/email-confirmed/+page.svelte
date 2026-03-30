<script lang="ts">
  import { Button as AppButton, SplitLayout } from '$lib/ui/components';
  import { Check, ChevronLeft } from 'lucide-svelte';
  import { cn } from '$lib/ui/utils';
</script>

<svelte:head>
  <title>Snapflow | Email confirmed</title>
</svelte:head>

<SplitLayout>
  {#snippet header()}
    <a
      href="/"
      class="inline-flex items-center gap-1.5 text-sm text-gray-500 transition-colors hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-300"
    >
      <ChevronLeft size={18} />
      Back to home
    </a>
  {/snippet}

  <div class="flex flex-1 flex-col items-center justify-center text-center">
    <div
      class={cn(
        'animate-bounce-subtle mx-auto flex h-16 w-16 items-center justify-center rounded-full shadow-inner transition-all duration-500',
        'bg-green-100 dark:bg-green-900/40'
      )}
    >
      <Check
        size={32}
        class={cn('transition-transform duration-500', 'text-green-600 dark:text-green-400')}
      />
    </div>

    <div class="mt-6 space-y-2">
      <h1 class="text-2xl font-semibold text-gray-800 sm:text-3xl dark:text-white/90">
        Email confirmed!
      </h1>
      <p class="text-sm text-gray-500 dark:text-gray-400">
        Your email address has been successfully verified. You can now sign in to your account.
      </p>
    </div>

    <div class="mt-8 flex w-full flex-col gap-3 sm:flex-row sm:justify-center">
      <AppButton
        href="/sign-in"
        variant="primary"
        size="md"
        class="w-full justify-center sm:w-auto sm:min-w-[160px]"
      >
        Sign in
      </AppButton>
    </div>
  </div>
</SplitLayout>

<style>
  @keyframes bounce-subtle {
    0%,
    100% {
      transform: translateY(0);
    }
    50% {
      transform: translateY(-2px);
    }
  }
  .animate-bounce-subtle {
    animation: bounce-subtle 3s ease-in-out infinite;
  }
</style>
