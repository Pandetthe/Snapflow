<script lang="ts">
  import { Button } from 'bits-ui';
  import { theme } from '$lib/ui/stores/theme';
  import { Sun, Moon } from 'lucide-svelte';

  function toggleTheme() {
    theme.toggle();
  }
</script>

<Button.Root
  onclick={toggleTheme}
  class="inline-flex h-11 w-11 cursor-pointer items-center justify-center rounded-full border border-gray-200 bg-white text-gray-700 transition-all hover:bg-gray-50 active:scale-95 dark:border-gray-800 dark:bg-gray-900 dark:text-gray-400 dark:hover:bg-gray-800"
>
  <Sun size={20} class="block dark:hidden" />
  <Moon size={20} class="hidden dark:block" />
  <span class="sr-only">Toggle theme</span>
</Button.Root>
