<script lang="ts">
  import { FullLayout, GoBackButton } from '$lib/ui/components';
  import { SlidersHorizontal } from 'lucide-svelte';
  import { afterNavigate } from '$app/navigation';

  let backHref = $state('/boards');
  afterNavigate(({ from }) => {
    backHref = from?.url.pathname ?? '/boards';
  });
</script>

<svelte:head>
  <title>Snapflow | Settings</title>
</svelte:head>

<FullLayout>
  <div class="mx-auto w-full max-w-5xl space-y-6 pb-12 sm:space-y-8">
    <header class="flex flex-col gap-4">
      <GoBackButton href={backHref} />
      <div class="space-y-1">
        <h1 class="text-2xl font-bold tracking-tight text-gray-900 sm:text-4xl dark:text-white">
          Settings
        </h1>
        <p class="text-sm text-gray-600 sm:text-base dark:text-gray-400">
          Choose how Snapflow looks and behaves for you.
        </p>
      </div>
    </header>

    <div
      class="flex flex-col items-center justify-center rounded-2xl border-2 border-dashed border-gray-200 px-4 py-12 text-center sm:py-20 dark:border-gray-800"
    >
      <div
        class="mb-5 flex h-16 w-16 items-center justify-center rounded-full bg-gray-50 dark:bg-gray-800/30"
      >
        <SlidersHorizontal class="h-8 w-8 text-gray-400" />
      </div>
      <h2 class="mb-2 text-xl font-semibold text-gray-900 dark:text-white">Nothing here yet</h2>
      <p class="max-w-sm text-sm text-gray-500 dark:text-gray-400">
        Settings will show up here as they arrive. Your account details live in your profile.
      </p>
    </div>
  </div>
</FullLayout>
