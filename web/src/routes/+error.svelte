<script lang="ts">
  import { page } from '$app/state';
  import { Button } from '$lib/ui/components';
  import { dev } from '$app/environment';
</script>

<svelte:head>
  {#if page.status === 404}
    <title>Snapflow | Page not found</title>
  {:else}
    <title>Snapflow | Error</title>
  {/if}
</svelte:head>

<div class="flex min-h-screen items-center justify-center bg-white px-4 dark:bg-gray-900">
  <div class="w-full max-w-lg text-center">
    <div>
      {#if page.status === 404}
        <div class="relative mx-auto w-full max-w-2xl">
          <div class="relative">
            <div class="flex items-center justify-center gap-4">
              <span
                class="bg-linear-to-r from-brand-600 via-brand-500 to-brand-600 bg-clip-text text-[10rem] font-black text-transparent"
                >404</span
              >
            </div>
          </div>
        </div>
      {:else}
        <div class="mb-8 flex justify-center">
          <div class="relative">
            <div
              class="flex h-32 w-32 items-center justify-center rounded-full bg-red-100 dark:bg-red-900/20"
            >
              <svg
                class="h-16 w-16 text-red-500 dark:text-red-400"
                fill="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z"
                />
              </svg>
            </div>
            <div
              class="absolute -top-2 -right-2 flex h-8 w-8 items-center justify-center rounded-full bg-red-500"
            >
              <svg class="h-5 w-5 text-white" fill="currentColor" viewBox="0 0 20 20">
                <path
                  fill-rule="evenodd"
                  d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z"
                  clip-rule="evenodd"
                />
              </svg>
            </div>
          </div>
        </div>
      {/if}
    </div>

    <div class="mb-10">
      {#if page.status === 404}
        <h1 class="mb-4 text-3xl font-bold text-gray-900 dark:text-white">Page Not Found</h1>
        <p class="mb-2 text-lg text-gray-600 dark:text-gray-300">
          Oops! The page you're looking for seems to have vanished.
        </p>
        <p class="text-gray-500 dark:text-gray-400">
          Don't worry, even the best kanban boards sometimes have missing cards!
        </p>
      {:else}
        <h1 class="mb-4 text-3xl font-bold text-gray-900 dark:text-white">Something went wrong</h1>
        <p class="mb-2 text-lg text-gray-600 dark:text-gray-300">
          We encountered an unexpected error.
        </p>
        {#if dev}
          <div
            class="mx-auto mt-6 max-w-full overflow-hidden rounded-lg border border-red-200 bg-red-50 p-4 text-left dark:border-red-900/30 dark:bg-red-900/10"
          >
            <p
              class="font-mono text-sm font-bold wrap-break-word whitespace-pre-wrap text-red-600 dark:text-red-400"
            >
              Error: {page.error?.message}
            </p>
          </div>
        {/if}
      {/if}
    </div>

    <div class="flex flex-col justify-center gap-4 sm:flex-row">
      <Button
        href="/"
        variant="primary"
        size="lg"
        class="inline-flex min-w-40 items-center justify-center"
      >
        <svg class="mr-2 h-5 w-5" fill="currentColor" viewBox="0 0 20 20">
          <path
            d="M10.707 2.293a1 1 0 00-1.414 0l-7 7a1 1 0 001.414 1.414L4 10.414V17a1 1 0 001 1h2a1 1 0 001-1v-2a1 1 0 011-1h2a1 1 0 011 1v2a1 1 0 001 1h2a1 1 0 001-1v-6.586l.293.293a1 1 0 001.414-1.414l-7-7z"
          />
        </svg>
        Go Home
      </Button>

      {#if page.status !== 404}
        <Button
          href="https://github.com/Pandetthe/Snapflow/issues"
          variant="outline"
          size="lg"
          class="inline-flex min-w-40 items-center justify-center"
        >
          <svg class="mr-2 h-5 w-5" fill="currentColor" viewBox="0 0 24 24">
            <path
              d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z"
            />
          </svg>
          Report Issue
        </Button>
      {/if}
    </div>
  </div>
</div>
