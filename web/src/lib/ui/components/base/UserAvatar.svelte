<script module lang="ts">
  /**
   * Avatars already shown on this page, held by the element that loaded them so the browser keeps the
   * decoded image. A later avatar with the same address is shown right away, without the skeleton:
   * Firefox reports a cached image as not complete and resolves `decode()` only a moment later, which
   * left the board's "someone changed this" label with a skeleton for most of the time it was up.
   */
  const shownAvatars = new Map<string, HTMLImageElement>();
</script>

<script lang="ts">
  import { cn, placeholderOut } from '$lib/ui/utils';
  import { fade } from 'svelte/transition';
  import type { Action } from 'svelte/action';
  import Skeleton from './Skeleton.svelte';

  interface Props {
    src?: string | null;
    name?: string;
    size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl' | number;
    isLoading?: boolean;
    class?: string;
  }

  let { src, name = 'User', size = 'md', isLoading = false, class: className }: Props = $props();

  let loadedSrc = $state<string | null>(null);
  let failedSrc = $state<string | null>(null);

  const showImage = $derived(Boolean(src) && failedSrc !== src);
  const loaded = $derived(loadedSrc === src || (src != null && shownAvatars.has(src)));

  const watchImage: Action<HTMLImageElement, string> = (node, imageSrc) => {
    let current = imageSrc;

    function markLoaded(watched: string) {
      loadedSrc = watched;
      if (!shownAvatars.has(watched)) shownAvatars.set(watched, node);
    }

    function check() {
      const watched = current;
      if (shownAvatars.has(watched) || (node.complete && node.naturalWidth > 0)) {
        markLoaded(watched);
        return;
      }
      node.decode().then(
        () => {
          if (current === watched) markLoaded(watched);
        },
        () => {
          if (current === watched && node.complete && node.naturalWidth === 0) failedSrc = watched;
        }
      );
    }

    const onLoad = () => markLoaded(current);
    const onError = () => (failedSrc = current);
    node.addEventListener('load', onLoad);
    node.addEventListener('error', onError);
    check();

    return {
      update(nextSrc) {
        current = nextSrc;
        check();
      },
      destroy() {
        node.removeEventListener('load', onLoad);
        node.removeEventListener('error', onError);
      }
    };
  };

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
  const customSizeStyle = $derived(
    typeof size === 'number'
      ? `width: ${size}px; height: ${size}px; font-size: ${size * 0.4}px;`
      : ''
  );
</script>

<div
  class={cn(
    'relative flex shrink-0 items-center justify-center overflow-hidden rounded-full ring-1 ring-black/5 dark:ring-white/10',
    resolvedSizeClass,
    !showImage && 'bg-brand-50 font-bold text-brand-700 dark:bg-brand-500/10 dark:text-brand-400',
    className
  )}
  style={customSizeStyle}
>
  {#if showImage && src}
    <img
      use:watchImage={src}
      {src}
      alt={name}
      decoding="sync"
      class="h-full w-full object-cover transition-opacity duration-200"
      class:opacity-0={!loaded}
    />
  {:else if !isLoading}
    <span>{initials}</span>
  {/if}

  {#if isLoading || (showImage && !loaded)}
    <div class="absolute inset-0" out:fade={placeholderOut}>
      <Skeleton class="h-full w-full rounded-full" />
    </div>
  {/if}
</div>
