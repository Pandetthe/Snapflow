<script lang="ts">
  import { Progress } from 'bits-ui';
  import { cn } from '$lib/ui/utils';
  import { Tween } from 'svelte/motion';
  import { cubicOut } from 'svelte/easing';

  type Variant = 'primary' | 'success' | 'warning' | 'danger' | 'info';
  type Size = 'xs' | 'sm' | 'md' | 'lg' | 'xl';

  let {
    value = 0,
    max = 100,
    variant = 'primary',
    size = 'md',
    class: className = '',
    label = '',
    showValue = false,
    indeterminate = false,
    ...rest
  }: {
    value?: number | null;
    max?: number;
    variant?: Variant;
    size?: Size;
    class?: string;
    label?: string;
    showValue?: boolean;
    indeterminate?: boolean;
    [key: string]: any;
  } = $props();

  const effectiveValue = $derived(indeterminate || value === null ? null : value);

  const tweenedValue = new Tween(0, {
    duration: 400,
    easing: cubicOut
  });

  $effect(() => {
    if (effectiveValue !== null) {
      tweenedValue.set(effectiveValue);
    }
  });

  const percentage = $derived(
    effectiveValue === null
      ? undefined
      : (Math.min(Math.max(tweenedValue.current, 0), max) / max) * 100
  );

  const isFull = $derived(
    effectiveValue !== null &&
      (Math.round((effectiveValue / max) * 100) === 100 || Math.round(percentage ?? 0) === 100)
  );

  const sizeClasses: Record<Size, string> = {
    xs: 'h-1',
    sm: 'h-1.5',
    md: 'h-2',
    lg: 'h-3',
    xl: 'h-4'
  };

  const variantClasses: Record<Variant, string> = {
    primary: 'bg-brand-500',
    success: 'bg-emerald-500',
    warning: 'bg-amber-500',
    danger: 'bg-red-500',
    info: 'bg-sky-500'
  };

  const shadowClasses: Record<Variant, string> = {
    primary: 'shadow-[0_0_8px_rgba(var(--color-brand-500-rgb,99,102,241),0.4)]',
    success: 'shadow-[0_0_8px_rgba(16,185,129,0.4)]',
    warning: 'shadow-[0_0_8px_rgba(245,158,11,0.4)]',
    danger: 'shadow-[0_0_8px_rgba(239,68,68,0.4)]',
    info: 'shadow-[0_0_8px_rgba(14,165,233,0.4)]'
  };
</script>

<div class={cn('w-full space-y-2', className)}>
  {#if label || showValue}
    <div
      class="flex items-center justify-between text-sm font-medium text-gray-700 dark:text-gray-300"
    >
      {#if label}
        <span>{label}</span>
      {/if}
      {#if showValue && effectiveValue !== null}
        <span class="tabular-nums">{Math.round(percentage ?? 0)}%</span>
      {/if}
    </div>
  {/if}

  <Progress.Root
    value={effectiveValue}
    {max}
    class={cn(
      'relative w-full rounded-full bg-gray-100 dark:bg-gray-800/40',
      !isFull && 'overflow-hidden',
      sizeClasses[size]
    )}
    {...rest}
  >
    <div
      class="h-full w-full flex-1 rounded-full"
      style={effectiveValue !== null ? `transform: translateX(-${100 - (percentage ?? 0)}%)` : ''}
    >
      <div
        class={cn(
          'h-full w-full rounded-full transition-transform',
          isFull && 'animate-pulse-completion',
          effectiveValue === null
            ? 'bg-transparent'
            : [variantClasses[variant], shadowClasses[variant]]
        )}
      >
        {#if effectiveValue === null}
          <div
            class={cn(
              'animate-loading-bar absolute h-full w-1/2 rounded-full',
              variantClasses[variant],
              shadowClasses[variant]
            )}
          ></div>
        {/if}
      </div>
    </div>
  </Progress.Root>
</div>

<style>
  @keyframes loading-bar {
    0% {
      transform: translateX(-100%);
    }
    100% {
      transform: translateX(200%);
    }
  }
  .animate-loading-bar {
    animation: loading-bar 1.5s cubic-bezier(0.4, 0, 0.2, 1) infinite;
  }
  @keyframes pulse-completion {
    0%,
    100% {
      opacity: 0.9;
      transform: scale(1);
      filter: brightness(1);
    }
    50% {
      opacity: 1;
      transform: scale(1.001);
      filter: brightness(1.1);
    }
  }
  .animate-pulse-completion {
    animation: pulse-completion 1.5s ease-in-out infinite;
    z-index: 10;
  }
</style>
