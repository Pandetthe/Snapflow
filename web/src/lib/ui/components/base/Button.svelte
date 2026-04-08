<script lang="ts">
  import { Button as BitsButton } from 'bits-ui';
  import type { Icon as IconType } from 'lucide-svelte';
  import type { Snippet } from 'svelte';
  import { LoaderCircle } from 'lucide-svelte';
  import { cn, haptics, type HapticPreset } from '$lib/ui/utils';
  import { LoadingDots } from '$lib/ui/components';
  import type { Variant, Size } from '$lib/ui/types';

  interface Props {
    [key: string]: any;
    variant?: Variant;
    size?: Size;
    startIcon?: typeof IconType;
    endIcon?: typeof IconType;
    children?: Snippet;
    class?: string;
    id?: string;
    href?: string;
    type?: "button" | "submit" | "reset";
    disabled?: boolean;
    haptic?: HapticPreset | number | number[];
    isLoading?: boolean;
    loadingText?: string;
    onclick?: (
      e: MouseEvent & { currentTarget: HTMLButtonElement | HTMLAnchorElement }
    ) => void | HapticPreset | Promise<void | HapticPreset>;
  }

  let {
    class: className,
    variant = 'primary',
    size = 'md',
    startIcon: StartIcon,
    endIcon: EndIcon,
    id,
    disabled = false,
    isLoading = false,
    loadingText,
    haptic,
    children,
    type = 'button',
    href,
    onclick,
    ...rest
  }: Props = $props();

  type ButtonClickEvent = MouseEvent & {
    currentTarget: HTMLButtonElement | HTMLAnchorElement;
  };

  const variantClasses: Record<Variant, string> = {
    primary:
      'bg-brand-500 text-white shadow-sm hover:bg-brand-600 active:bg-brand-700 disabled:bg-brand-300',
    outline:
      'bg-white text-gray-700 border border-gray-300 hover:bg-gray-50 active:bg-gray-100 focus-visible:border-brand-500 disabled:bg-gray-50/50 dark:bg-gray-800/60 dark:text-gray-200 dark:border-gray-700 dark:focus-visible:border-brand-500 dark:hover:bg-gray-700/70 dark:active:bg-gray-700 dark:hover:text-white dark:disabled:bg-gray-800/30',
    ghost:
      'bg-transparent text-gray-500 hover:bg-gray-100 hover:text-gray-900 dark:text-gray-400 dark:hover:bg-white/10 dark:hover:text-white',
    danger:
      'bg-error-500 text-white hover:bg-error-600 active:bg-error-700 disabled:bg-error-300',
    success:
      'bg-success-500 text-white hover:bg-success-600 active:bg-success-700 disabled:bg-success-300'
  };

  const sizeClasses: Record<Size, string> = {
    xs: 'px-2 py-1 text-xs gap-1 h-7 min-w-7',
    sm: 'px-3 py-2 text-sm gap-2 h-9 min-w-9',
    md: 'px-4 py-2.5 text-md gap-2 h-11 min-w-11',
    lg: 'px-5 py-3 text-lg gap-2.5 h-12 min-w-12',
    xl: 'px-6 py-3.5 text-xl gap-3 h-14 min-w-14'
  };

  const iconSizes: Record<Size, number> = {
    xs: 14,
    sm: 16,
    md: 20,
    lg: 24,
    xl: 28
  };

  let isExecuting = $state(false);
  const isDisabled = $derived(Boolean(disabled || isLoading || isExecuting));
  const isActualLoading = $derived(isLoading || isExecuting);
  const resolvedVariantClass = $derived(variantClasses[variant]);

  async function handleClick(event: ButtonClickEvent) {
    if (isDisabled) {
      event.preventDefault();
      event.stopPropagation();
      return;
    }

    if (haptic) {
      haptics?.trigger(haptic);
    }

    if (!onclick) return;

    const result = onclick(event);

    if (result instanceof Promise) {
      isExecuting = true;
      try {
        const hapticResult = await result;
        if (hapticResult) {
          haptics?.trigger(hapticResult);
        }
      } catch (error) {
        haptics?.trigger('error');
        console.error('Button click handler failed:', error);
      } finally {
        isExecuting = false;
      }
    } else if (result) {
      haptics?.trigger(result);
    }
  }
</script>

{#if href}
  <a
    {id}
    href={isDisabled ? undefined : href}
    class={cn(
      'inline-flex cursor-pointer items-center justify-center rounded-lg font-medium whitespace-nowrap transition focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none dark:focus-visible:ring-offset-gray-950',
      sizeClasses[size],
      resolvedVariantClass,
      !isDisabled && 'active:scale-95',
      isDisabled && 'pointer-events-none cursor-not-allowed opacity-50',
      className
    )}
    aria-disabled={isDisabled}
    tabindex={isDisabled ? -1 : undefined}
    onclick={handleClick}
    {...rest}
  >
    {#if isActualLoading}
      <LoaderCircle size={iconSizes[size]} class="animate-spin" />
      {#if loadingText}
        <span>{loadingText}<LoadingDots /></span>
      {:else if children}
        {@render children()}
      {/if}
    {:else}
      {#if StartIcon}
        <StartIcon size={iconSizes[size]} />
      {/if}

      {#if children}
        {@render children()}
      {/if}

      {#if EndIcon}
        <EndIcon size={iconSizes[size]} />
      {/if}
    {/if}
  </a>
{:else}
  <BitsButton.Root
    {id}
    {type}
    disabled={isDisabled}
    class={cn(
      'inline-flex cursor-pointer items-center justify-center rounded-lg font-medium whitespace-nowrap transition focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none disabled:cursor-not-allowed disabled:opacity-50 dark:focus-visible:ring-offset-gray-950',
      sizeClasses[size],
      resolvedVariantClass,
      !isDisabled && 'active:scale-95',
      className
    )}
    onclick={handleClick}
    {...rest}
  >
    {#if isActualLoading}
      <LoaderCircle size={iconSizes[size]} class="animate-spin" />
      {#if loadingText}
        <span>{loadingText}<LoadingDots /></span>
      {:else if children}
        {@render children()}
      {/if}
    {:else}
      {#if StartIcon}
        <StartIcon size={iconSizes[size]} />
      {/if}

      {#if children}
        {@render children()}
      {/if}

      {#if EndIcon}
        <EndIcon size={iconSizes[size]} />
      {/if}
    {/if}
  </BitsButton.Root>
{/if}
