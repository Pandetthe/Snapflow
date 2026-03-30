<script lang="ts">
  import { Switch } from 'bits-ui';
  import { cn } from '$lib/ui/utils';
  import { slide } from 'svelte/transition';

  interface Props {
    checked?: boolean;
    onCheckedChange?: (checked: boolean) => void;
    disabled?: boolean;
    readonly?: boolean | null;
    required?: boolean | null;
    name?: string | null;
    value?: string;
    label?: string;
    helperText?: string;
    error?: string | boolean;
    id?: string;
    class?: string;
  }

  const generatedId = $props.id();

  let {
    checked = $bindable(false),
    onCheckedChange,
    disabled = false,
    readonly = false,
    required = false,
    name,
    value = 'on',
    label,
    helperText,
    error,
    id = generatedId,
    class: className
  }: Props = $props();

  const normalizedName = $derived.by(() => (typeof name === 'string' ? name : undefined));
  const isDisabled = $derived.by(() => Boolean(disabled));
  const isReadonly = $derived.by(() => Boolean(readonly));
  const isRequired = $derived.by(() => Boolean(required));
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
    if (!isReadonly) {
      return;
    }

    event.preventDefault();
    event.stopPropagation();
  }

  function handleCheckedChange(nextChecked: boolean) {
    if (isDisabled || isReadonly) {
      return;
    }

    checked = nextChecked;
    onCheckedChange?.(nextChecked);
  }
</script>

<div class="flex w-full flex-col gap-1.5">
  {#if normalizedName}
    <input
      type="checkbox"
      class="sr-only"
      tabindex="-1"
      aria-hidden="true"
      name={normalizedName}
      {value}
      {checked}
      required={isRequired}
      disabled={isDisabled}
    />
  {/if}

  <label
    for={id}
    class={cn(
      'inline-flex items-center gap-3 text-sm font-medium select-none',
      isDisabled
        ? 'cursor-not-allowed text-gray-400 dark:text-gray-600'
        : isReadonly
          ? 'cursor-default text-gray-700 dark:text-gray-400'
          : 'cursor-pointer text-gray-700 dark:text-gray-400',
      hasError && 'text-error-600 dark:text-error-400',
      className
    )}
  >
    <Switch.Root
      {id}
      {checked}
      disabled={isDisabled}
      aria-invalid={hasError}
      aria-describedby={describedBy}
      aria-errormessage={hasError && errorText ? errorTextId : undefined}
      aria-readonly={isReadonly}
      onpointerdown={preventReadonlyInteraction}
      onclick={preventReadonlyInteraction}
      onkeydown={preventReadonlyInteraction}
      onCheckedChange={handleCheckedChange}
      class={cn(
        'peer inline-flex h-6 w-11 shrink-0 items-center rounded-full border transition-all duration-200',
        hasError
          ? 'border-error-500 bg-transparent hover:border-error-500 hover:bg-error-50/30 dark:border-error-500 dark:hover:border-error-500 dark:hover:bg-error-500/10'
          : 'border-gray-300 bg-transparent hover:border-brand-500 hover:bg-black/3 dark:border-gray-700 dark:bg-transparent dark:hover:border-brand-500 dark:hover:bg-white/6',
        hasError
          ? 'focus-visible:ring-2 focus-visible:ring-error-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none dark:focus-visible:ring-offset-gray-950'
          : 'focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none dark:focus-visible:ring-offset-gray-950',
        hasError
          ? 'data-[state=checked]:border-error-500 data-[state=checked]:bg-error-500 data-[state=checked]:hover:bg-error-500 dark:data-[state=checked]:border-error-500 dark:data-[state=checked]:bg-error-500 dark:data-[state=checked]:hover:bg-error-500'
          : 'data-[state=checked]:border-brand-500 data-[state=checked]:bg-brand-500 data-[state=checked]:hover:bg-brand-500 dark:data-[state=checked]:border-brand-500 dark:data-[state=checked]:bg-brand-500 dark:data-[state=checked]:hover:bg-brand-500',
        isReadonly && !isDisabled && 'cursor-default',
        !isDisabled && !isReadonly && 'cursor-pointer',
        'disabled:cursor-not-allowed disabled:bg-gray-100 disabled:opacity-80 dark:disabled:bg-gray-800'
      )}
    >
      <Switch.Thumb
        class={cn(
          'pointer-events-none block h-5 w-5 rounded-full bg-white shadow-theme-xs ring-0 transition-transform duration-200',
          'data-[state=checked]:translate-x-5 data-[state=unchecked]:translate-x-0'
        )}
      />
    </Switch.Root>

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
