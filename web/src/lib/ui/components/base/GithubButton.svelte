<script lang="ts">
  import { Button } from '$lib/ui/components';
  import githubBlack from '$lib/assets/github-black.svg';
  import githubWhite from '$lib/assets/github-white.svg';
  import { ExternalLink } from 'lucide-svelte';
  import { cn } from '$lib/ui/utils';

  interface Props {
    href?: string;
    class?: string;
    showLabel?: boolean;
    onclick?: () => void;
  }

  let {
    href = 'https://github.com/pandetthe/Snapflow',
    class: className = '',
    showLabel = false,
    onclick
  }: Props = $props();
</script>

<Button
  {href}
  variant="outline"
  target="_blank"
  rel="noopener noreferrer"
  {onclick}
  class={cn(
    showLabel 
      ? 'w-full justify-between'
      : 'h-11 w-11 rounded-full p-0 flex items-center justify-center text-gray-700 dark:text-gray-400',
    className
  )}
  aria-label="GitHub Repository"
>
  {#if showLabel}
    <div class="flex items-center gap-2">
      <img src={githubBlack} alt="GitHub" class="h-4 w-4 dark:hidden" />
      <img src={githubWhite} alt="GitHub" class="hidden h-4 w-4 dark:block" />
      <span>GitHub</span>
    </div>
    <ExternalLink size={18} class="text-gray-500 dark:text-gray-400" />
  {:else}
    <img src={githubBlack} alt="GitHub" class="h-6 w-6 dark:hidden" />
    <img src={githubWhite} alt="GitHub" class="hidden h-6 w-6 dark:block" />
  {/if}
</Button>
