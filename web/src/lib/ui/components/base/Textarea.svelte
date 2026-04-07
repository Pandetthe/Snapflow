<script lang="ts">
  import type { HTMLTextareaAttributes } from 'svelte/elements';
  import { Label } from 'bits-ui';
  import { cn } from '$lib/ui/utils';
  import type { Icon as IconType } from 'lucide-svelte';

  interface Props extends HTMLTextareaAttributes {
    value?: string;
    label?: string;
    helperText?: string;
    error?: string;
    leftIcon?: typeof IconType;
    rightIcon?: typeof IconType;
    leftIconDecorated?: boolean;
    id?: string;
  }

  const generatedId = $props.id();

  let {
    class: className,
    value = $bindable(''),
    label,
    helperText,
    error,
    leftIcon: LeftIcon,
    rightIcon: RightIcon,
    leftIconDecorated = false,
    id = generatedId,
    name,
    disabled = false,
    required = false,
    readonly = false,
    rows = 4,
    ...rest
  }: Props = $props();

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
    if (!LeftIcon) {
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
          class="absolute top-6 left-0 flex h-11 w-[46px] -translate-y-1/2 items-center justify-center border-r border-gray-200 text-gray-500 dark:border-gray-800 dark:text-gray-400"
        >
          <LeftIcon size={18} />
        </span>
      {:else}
        <div
          class="absolute top-6 left-4 -translate-y-1/2 text-gray-400 transition-colors group-focus-within:text-brand-500"
        >
          <LeftIcon size={18} />
        </div>
      {/if}
    {/if}

    <textarea
      {id}
      name={normalizedName}
      disabled={isDisabled}
      required={isRequired}
      readonly={isReadonly}
      bind:value
      {rows}
      aria-invalid={hasError}
      aria-describedby={describedBy}
      aria-errormessage={hasError ? errorTextId : undefined}
      class={cn(
        'flex w-full appearance-none rounded-lg border bg-transparent px-4 py-2.5 text-sm shadow-theme-xs transition-all duration-200',
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
        RightIcon && 'pr-11',
        className
      )}
      {...rest}
    ></textarea>

    {#if RightIcon}
      <span
        class={cn(
          'pointer-events-none absolute top-6 right-4 -translate-y-1/2 transition-colors',
          hasError
            ? 'text-error-500 group-focus-within:text-error-500 dark:text-error-400'
            : 'text-gray-500 group-focus-within:text-brand-500 dark:text-gray-400'
        )}
      >
        <RightIcon size={20} />
      </span>
    {/if}
  </div>

  {#if helperText && !hasError}
    <span id={helperTextId} class="text-xs text-gray-600 dark:text-gray-400">{helperText}</span>
  {/if}

  {#if hasError}
    <span id={errorTextId} class="text-xs font-medium text-error-500">{errorText}</span>
  {/if}
</div>
