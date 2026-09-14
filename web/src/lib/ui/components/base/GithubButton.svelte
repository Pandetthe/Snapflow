<script lang="ts">
  import { Button } from '$lib/ui/components';
  import githubBlack from '$lib/assets/github-black.svg';
  import githubWhite from '$lib/assets/github-white.svg';
  import { cn } from '$lib/ui/utils';

  // Header icon button; the mobile menu has its own GitHub row (AppHeader).
  interface Props {
    href?: string;
    class?: string;
  }

  let {
    href = 'https://github.com/pandetthe/Snapflow',
    class: className = ''
  }: Props = $props();
</script>

<Button
  {href}
  variant="outline"
  target="_blank"
  rel="noopener noreferrer"
  class={cn(
    'h-11 w-11 rounded-full p-0 flex items-center justify-center text-gray-700 dark:text-gray-400',
    className
  )}
  aria-label="GitHub Repository"
>
  <img src={githubBlack} alt="GitHub" class="h-6 w-6 dark:hidden" />
  <img src={githubWhite} alt="GitHub" class="hidden h-6 w-6 dark:block" />
</Button>
