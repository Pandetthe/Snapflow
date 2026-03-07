<script lang="ts">
  import { Button } from 'bits-ui';
  import { theme } from '$lib/ui/stores/theme';

  function toggleTheme() {
    theme.toggle();
  }
</script>

<Button.Root
  onclick={toggleTheme}
  class="inline-flex h-9 w-9 items-center justify-center rounded-lg bg-white text-gray-700 shadow-sm transition-all duration-200 hover:bg-gray-50 hover:shadow-md dark:bg-gray-800 dark:text-gray-300 dark:hover:bg-gray-700"
>
  {#if $theme === 'light'}
    <svg
      class="h-4 w-4"
      fill="none"
      stroke="currentColor"
      viewBox="0 0 24 24"
      xmlns="http://www.w3.org/2000/svg"
    >
      <path
        stroke-linecap="round"
        stroke-linejoin="round"
        stroke-width="2"
        d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z"
      ></path>
    </svg>
  {:else}
    <svg
      class="h-4 w-4"
      fill="none"
      stroke="currentColor"
      viewBox="0 0 24 24"
      xmlns="http://www.w3.org/2000/svg"
    >
      <path
        stroke-linecap="round"
        stroke-linejoin="round"
        stroke-width="2"
        d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z"
      ></path>
    </svg>
  {/if}
  <span class="sr-only">Toggle theme</span>
</Button.Root>
