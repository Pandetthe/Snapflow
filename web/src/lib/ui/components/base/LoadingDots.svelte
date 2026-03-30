<script lang="ts">
  import { cn } from '$lib/ui/utils';

  interface Props {
    class?: string;
  }

  const { class: className }: Props = $props();
</script>

<span class={cn('inline-flex h-4 items-baseline', className)}>
  <span class="animate-dots-bounce block" style="animation-delay: 0ms">.</span>
  <span class="animate-dots-bounce block" style="animation-delay: 150ms">.</span>
  <span class="animate-dots-bounce block" style="animation-delay: 300ms">.</span>
</span>

<style>
  @keyframes dots-bounce {
    25% {
      transform: translateY(-10%);
      animation-timing-function: cubic-bezier(0.4, 0, 1, 1);
    }
    0%,
    50%,
    100% {
      transform: none;
      animation-timing-function: cubic-bezier(0, 0, 0.2, 1);
    }
  }

  .animate-dots-bounce {
    animation: dots-bounce 1.5s infinite;
  }
</style>
