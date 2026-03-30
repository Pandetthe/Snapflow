<script lang="ts">
  import { Checkbox } from 'bits-ui';
  import { cn } from '$lib/ui/utils';
  import { Check, X } from 'lucide-svelte';
  import { slide } from 'svelte/transition';

  interface Props {
    checked?: boolean | null;
    disabled?: boolean;
    readonly?: boolean;
    required?: boolean;
    tristate?: boolean;
    name?: string;
    value?: string;
    label?: string;
    helperText?: string;
    error?: string | boolean;
    id?: string;
    class?: string;
    onCheckedChange?: (checked: boolean | null) => void;
  }

  const generatedId = $props.id();

  let {
    checked = $bindable(null),
    onCheckedChange,
    disabled = false,
    readonly = false,
    required = false,
    tristate = false,
    name,
    value,
    label,
    helperText,
    error,
    id = generatedId,
    class: className
  }: Props = $props();

  const errorText = $derived.by(() => (typeof error === 'string' ? error : ''));
  const hasError = $derived.by(() =>
    typeof error === 'string' ? error.length > 0 : Boolean(error)
  );
  const helperTextId = $derived.by(() => (helperText ? `${id}-help` : undefined));
  const errorTextId = $derived.by(() => (hasError && errorText ? `${id}-error` : undefined));
  const describedBy = $derived.by(() => {
    if (hasError && errorTextId) {
      return errorTextId;
    }

    if (helperTextId) {
      return helperTextId;
    }

    return undefined;
  });

  function preventReadonlyInteraction(event: Event) {
    if (!readonly) {
      return;
    }

    event.preventDefault();
    event.stopPropagation();
  }

  function handleCheckedChange() {
    if (disabled || readonly) {
      return;
    }

    if (tristate) {
      if (checked === null) {
        checked = true;
      } else if (checked === true) {
        checked = false;
      } else {
        checked = null;
      }
    } else {
      checked = checked === true ? null : true;
    }

    onCheckedChange?.(checked);
  }

  function handleRootClick(e: MouseEvent | KeyboardEvent) {
    if (disabled || readonly) {
      return;
    }
    e.preventDefault();
    handleCheckedChange();
  }
</script>

<div class="flex w-full flex-col gap-1.5">
  <label
    for={id}
    class={cn(
      'inline-flex items-center text-sm font-medium select-none',
      disabled
        ? 'cursor-not-allowed text-gray-400 dark:text-gray-600'
        : readonly
          ? 'cursor-default text-gray-700 dark:text-gray-400'
          : 'cursor-pointer text-gray-700 dark:text-gray-400',
      hasError && 'text-error-600 dark:text-error-400',
      className
    )}
  >
    <Checkbox.Root
      {id}
      checked={checked === true}
      {name}
      {value}
      indeterminate={tristate && checked === false}
      {disabled}
      aria-invalid={hasError}
      aria-describedby={describedBy}
      aria-errormessage={hasError && errorText ? errorTextId : undefined}
      {readonly}
      onpointerdown={preventReadonlyInteraction}
      onclick={handleRootClick}
      {required}
      onkeydown={(e) => {
        if (e.key === ' ' || e.key === 'Enter') {
          handleRootClick(e);
        }
      }}
      class={cn(
        'flex h-5 w-5 shrink-0 items-center justify-center rounded-md border-[1.25px] transition-all duration-200',
        hasError
          ? 'border-error-500 bg-transparent hover:border-error-500 hover:bg-error-50/30 dark:border-error-500 dark:hover:border-error-500 dark:hover:bg-error-500/10'
          : 'border-gray-300 bg-transparent hover:border-brand-500 hover:bg-black/3 dark:border-gray-700 dark:hover:border-brand-500 dark:hover:bg-white/6',
        hasError
          ? 'focus-visible:ring-2 focus-visible:ring-error-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none dark:focus-visible:ring-offset-gray-950'
          : 'focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none dark:focus-visible:ring-offset-gray-950',
        hasError
          ? 'data-[state=checked]:border-error-500 data-[state=checked]:bg-error-500 data-[state=checked]:hover:bg-error-500 data-[state=indeterminate]:border-error-500 data-[state=indeterminate]:bg-error-500 dark:data-[state=checked]:border-error-500 dark:data-[state=checked]:bg-error-500 dark:data-[state=checked]:hover:bg-error-500'
          : 'data-[state=checked]:border-brand-500 data-[state=checked]:bg-brand-500 data-[state=checked]:hover:bg-brand-500 data-[state=indeterminate]:border-brand-500 data-[state=indeterminate]:bg-brand-500 dark:data-[state=checked]:hover:bg-brand-500',
        label && 'mr-3',
        readonly && !disabled && 'cursor-default',
        !disabled && !readonly && 'cursor-pointer',
        'peer disabled:cursor-not-allowed',
        disabled && 'border-gray-300 bg-gray-100 opacity-100 dark:border-gray-700 dark:bg-gray-900',
        disabled &&
          checked !== null &&
          'border-gray-400 bg-gray-200 dark:border-gray-600 dark:bg-gray-800'
      )}
    >
      {#snippet children({ checked: isChecked, indeterminate: isIndeterminate })}
        {#if isIndeterminate}
          <X
            size={14}
            class={cn(disabled ? 'text-gray-500 dark:text-gray-400' : 'text-white')}
            strokeWidth={3}
          />
        {:else if isChecked}
          <Check
            size={14}
            class={cn(disabled ? 'text-gray-500 dark:text-gray-400' : 'text-white')}
            strokeWidth={3}
          />
        {/if}
      {/snippet}
    </Checkbox.Root>

    {#if label}
      {label}
    {/if}
  </label>

  {#if helperText && !hasError}
    <div transition:slide={{ axis: 'y', duration: 200 }}>
      <span id={helperTextId} class="text-xs text-gray-500 dark:text-gray-400">{helperText}</span>
    </div>
  {/if}

  {#if hasError && errorText}
    <div transition:slide={{ axis: 'y', duration: 200 }}>
      <span id={errorTextId} class="text-xs font-medium text-error-500">{errorText}</span>
    </div>
  {/if}
</div>
