<script lang="ts">
  import type { HTMLInputAttributes } from 'svelte/elements';
  import { Label } from 'bits-ui';
  import { cn } from '$lib/ui/utils';
  import { Eye, EyeOff, Search, Loader2, type Icon as IconType } from 'lucide-svelte';
  import { slide } from 'svelte/transition';

  interface Props extends HTMLInputAttributes {
    value?: string;
    label?: string;
    helperText?: string;
    helperTextClass?: string;
    error?: string;
    type?: string;
    showPasswordToggle?: boolean;
    leftIcon?: typeof IconType;
    rightIcon?: typeof IconType;
    leftIconDecorated?: boolean;
    id?: string;
    isLoading?: boolean;
  }

  const generatedId = $props.id();

  let {
    class: className,
    value = $bindable(''),
    label,
    helperText,
    helperTextClass,
    error,
    type = 'text',
    showPasswordToggle = false,
    leftIcon: LeftIcon,
    rightIcon: RightIcon,
    leftIconDecorated = false,
    id = generatedId,
    name,
    disabled = false,
    required = false,
    readonly = false,
    isLoading = false,
    ...rest
  }: Props = $props();

  let showPassword = $state(false);
  const inputType = $derived(type === 'password' && showPassword ? 'text' : type);
  const normalizedName = $derived.by(() => (typeof name === 'string' ? name : undefined));
  const isDisabled = $derived.by(() => Boolean(disabled));
  const isRequired = $derived.by(() => Boolean(required));
  const isReadonly = $derived.by(() => Boolean(readonly));
  const errorText = $derived.by(() => error ?? '');
  const hasError = $derived.by(() => errorText.length > 0);
  const helperTextId = $derived.by(() => (helperText ? `${id}-help` : undefined));
  const errorTextId = $derived.by(() => (hasError ? `${id}-error` : undefined));
  const describedBy = $derived.by(() => {
    if (hasError) {
      return errorTextId;
    }

    if (helperTextId) {
      return helperTextId;
    }

    return undefined;
  });

  const leadingPaddingClass = $derived.by(() => {
    if (!(LeftIcon || type === 'search')) {
      return '';
    }

    return leftIconDecorated ? 'pl-[62px]' : 'pl-11';
  });
</script>

<div class="flex w-full flex-col gap-1.5">
  {#if label}
    <Label.Root for={id} class="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-400">
      {label}{#if isRequired}<span class="text-error-500" aria-hidden="true">*</span>{/if}
    </Label.Root>
  {/if}

  <div class="group relative">
    {#if LeftIcon}
      {#if leftIconDecorated}
        <span
          class="absolute top-1/2 left-0 flex h-11 w-[46px] -translate-y-1/2 items-center justify-center border-r border-gray-200 text-gray-500 dark:border-gray-800 dark:text-gray-400"
        >
          <LeftIcon size={18} />
        </span>
      {:else}
        <div
          class="absolute top-1/2 left-4 -translate-y-1/2 text-gray-400 transition-colors"
        >
          <LeftIcon size={18} />
        </div>
      {/if}
    {:else if type === 'search'}
      <div
        class="absolute top-1/2 left-4 -translate-y-1/2 text-gray-400 transition-colors"
      >
        <Search size={18} />
      </div>
    {/if}

    <input
      {id}
      name={normalizedName}
      type={inputType}
      disabled={isDisabled}
      required={isRequired}
      readonly={isReadonly}
      bind:value
      aria-invalid={hasError}
      aria-describedby={describedBy}
      aria-errormessage={hasError ? errorTextId : undefined}
      class={cn(
        'flex h-11 w-full appearance-none rounded-lg border bg-transparent px-4 py-2.5 text-sm shadow-theme-xs transition-all duration-200',
        hasError
          ? 'border-error-500 text-gray-800 placeholder:text-gray-400 hover:border-error-500 hover:bg-error-50/30 dark:border-error-500 dark:bg-transparent dark:text-white/90 dark:placeholder:text-white/30 dark:hover:border-error-500 dark:hover:bg-error-500/10'
          : 'border-gray-300 text-gray-800 placeholder:text-gray-400 hover:border-brand-500 hover:bg-black/3 dark:border-gray-700 dark:bg-transparent dark:text-white/90 dark:placeholder:text-white/30 dark:hover:border-brand-500 dark:hover:bg-white/6',
        hasError
          ? 'focus-visible:border-error-500 focus-visible:ring-2 focus-visible:ring-error-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none dark:focus-visible:border-error-500 dark:focus-visible:ring-offset-gray-950'
          : 'focus-visible:border-brand-500 focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none dark:focus-visible:border-brand-500 dark:focus-visible:ring-offset-gray-950',
        'disabled:cursor-not-allowed disabled:bg-gray-50/50 disabled:opacity-80 dark:disabled:bg-gray-900/50',
        isReadonly &&
          !isDisabled &&
          (hasError
            ? 'cursor-default hover:border-error-500 hover:bg-transparent dark:hover:border-error-500 dark:hover:bg-transparent'
            : 'cursor-default hover:border-gray-300 hover:bg-transparent dark:hover:border-gray-700 dark:hover:bg-transparent'),
        leadingPaddingClass,
        type === 'password' && showPasswordToggle && 'pr-12',
        type !== 'password' && RightIcon && 'pr-11',
        className
      )}
      {...rest}
    />

    {#if type === 'password' && showPasswordToggle}
      <button
        type="button"
        class={cn(
          'absolute top-1/2 right-4 -translate-y-1/2 transition-all duration-200 flex items-center justify-center',
          'h-11 w-11 min-w-11 min-h-11 rounded-lg outline-none',
          'focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white dark:focus-visible:ring-offset-gray-950',
          isDisabled || isReadonly
            ? 'cursor-not-allowed text-gray-400 dark:text-gray-600'
            : hasError
              ? 'cursor-pointer text-error-500 hover:text-error-600 focus-visible:ring-error-500 dark:text-error-400 dark:hover:text-error-300'
              : 'cursor-pointer text-gray-500 hover:text-brand-500 active:scale-95 group-focus-within:text-brand-500 dark:text-gray-400 dark:hover:text-brand-400 dark:group-focus-within:text-brand-500'
        )}
        onclick={() => (showPassword = !showPassword)}
        aria-label={showPassword ? 'Hide password' : 'Show password'}
        disabled={isDisabled || isReadonly}
      >
        {#if showPassword}
          <EyeOff size={18} />
        {:else}
          <Eye size={18} />
        {/if}
      </button>
    {:else if RightIcon || isLoading}
      <span
        class={cn(
          'absolute top-1/2 right-4 -translate-y-1/2 transition-colors',
          hasError
            ? 'text-error-500 dark:text-error-400'
            : 'text-gray-500 dark:text-gray-400'
        )}
      >
        {#if isLoading}
          <Loader2 size={18} class="animate-spin text-brand-500" />
        {:else if RightIcon}
          <RightIcon size={20} />
        {/if}
      </span>
    {/if}
  </div>

  {#if helperText && !hasError}
    <div transition:slide={{ axis: 'y', duration: 200 }}>
      <span id={helperTextId} class={cn("text-xs text-gray-600 dark:text-gray-400", helperTextClass)}>
        {helperText}
      </span>
    </div>
  {/if}

  {#if hasError}
    <div transition:slide={{ axis: 'y', duration: 200 }}>
      <span id={errorTextId} class="text-xs font-medium text-error-500">{errorText}</span>
    </div>
  {/if}
</div>

<style>
  input[type="search"]::-webkit-search-decoration,
  input[type="search"]::-webkit-search-cancel-button,
  input[type="search"]::-webkit-search-results-button,
  input[type="search"]::-webkit-search-results-decoration {
    display: none;
  }
</style>
