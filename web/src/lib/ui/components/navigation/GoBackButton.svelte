<script lang="ts">
  import Button from '../base/Button.svelte';
  import { ChevronLeft } from 'lucide-svelte';

  interface Props {
    href: string;
    hideTextOnMobile?: boolean;
  }

  let { href, hideTextOnMobile = false }: Props = $props();
</script>

<Button
  {href}
  variant="ghost"
  size="sm"
  aria-label="Go back"
  class="text-gray-600 hover:bg-gray-100 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-white text-sm"
>
  <span class="flex w-full items-center justify-center gap-2">
    <ChevronLeft class="h-6 w-6" />
    <span class={hideTextOnMobile ? 'hidden sm:inline' : 'inline'}>Go back</span>
  </span>
</Button>