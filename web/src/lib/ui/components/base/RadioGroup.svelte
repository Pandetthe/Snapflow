<script lang="ts">
  import { RadioGroup } from 'bits-ui';
  import { cn } from '$lib/ui/utils';
  import type { Snippet } from 'svelte';

  interface Props {
    value?: string;
    onValueChange?: (value: string) => void;
    disabled?: boolean;
    required?: boolean;
    name?: string;
    label?: string;
    class?: string;
    children: Snippet;
  }

  let {
    value = $bindable(''),
    onValueChange,
    disabled = false,
    required = false,
    name,
    label,
    class: className,
    children
  }: Props = $props();
</script>

<RadioGroup.Root
  bind:value
  {onValueChange}
  {disabled}
  {required}
  {name}
  class={cn('flex flex-col gap-3', className)}
>
  {#if label}
    <span class="mb-1 block text-sm font-medium text-gray-700 dark:text-gray-400">
      {label}
    </span>
  {/if}
  {@render children()}
</RadioGroup.Root>
