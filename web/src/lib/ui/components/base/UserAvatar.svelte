<script lang="ts">
  import { cn } from '$lib/ui/utils';
  import Skeleton from './Skeleton.svelte';
  import { onMount } from 'svelte';

  interface Props {
    src?: string | null;
    name?: string;
    size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl' | number;
    isLoading?: boolean;
    class?: string;
  }

  let {
    src,
    name = 'User',
    size = 'md',
    isLoading = false,
    class: className
  }: Props = $props();

  let isMounted = $state(false);
  onMount(() => {
    isMounted = true;
  });

  const sizeMap: Record<string, string> = {
    xs: 'h-6 w-6 text-[10px]',
    sm: 'h-8 w-8 text-xs',
    md: 'h-10 w-10 text-sm',
    lg: 'h-12 w-12 text-base',
    xl: 'h-14 w-14 text-lg'
  };

  const initials = $derived.by(() => {
    if (!name) return '?';
    return name
      .trim()
      .split(/\s+/)
      .map((n) => n[0])
      .join('')
      .slice(0, 2)
      .toUpperCase();
  });

  const resolvedSizeClass = $derived(typeof size === 'string' ? sizeMap[size] : '');
  const customSizeStyle = $derived(typeof size === 'number' ? `width: ${size}px; height: ${size}px; font-size: ${size * 0.4}px;` : '');
</script>

<div
  class={cn(
    'relative flex shrink-0 items-center justify-center overflow-hidden rounded-full ring-1 ring-black/5 dark:ring-white/10',
    resolvedSizeClass,
    !src && 'bg-brand-50 text-brand-700 dark:bg-brand-500/10 dark:text-brand-400 font-bold',
    className
  )}
  style={customSizeStyle}
>
  {#if isLoading || !isMounted}
    <Skeleton class="h-full w-full rounded-full" />
  {:else if src}
    <img {src} alt={name} class="h-full w-full object-cover" />
  {:else}
    <span>{initials}</span>
  {/if}
</div>
