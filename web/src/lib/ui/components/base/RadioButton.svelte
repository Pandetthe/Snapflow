<script lang="ts">
  import { RadioGroup } from 'bits-ui';
  import { cn } from '$lib/ui/utils';
  import { slide } from 'svelte/transition';

  interface Props {
    value: string;
    disabled?: boolean;
    readonly?: boolean | null;
    required?: boolean | null;
    label?: string;
    helperText?: string;
    class?: string;
    id?: string;
  }

  const generatedId = $props.id();

  let {
    value,
    disabled = false,
    readonly = false,
    required = false,
    label,
    helperText,
    id = generatedId,
    class: className
  }: Props = $props();

  const isDisabled = $derived.by(() => Boolean(disabled));
  const isReadonly = $derived.by(() => Boolean(readonly));
  const isRequired = $derived.by(() => Boolean(required));
  const helperTextId = $derived.by(() => (helperText ? `${id}-help` : undefined));

  function preventReadonlyInteraction(event: Event) {
    if (!isReadonly) {
      return;
    }

    event.preventDefault();
    event.stopPropagation();
  }
</script>

<div class="flex items-start gap-3">
  <div class="flex h-5 items-center">
    <RadioGroup.Item
      {value}
      {id}
      disabled={isDisabled}
      aria-readonly={isReadonly}
      onpointerdown={preventReadonlyInteraction}
      onclick={preventReadonlyInteraction}
      onkeydown={preventReadonlyInteraction}
      class={cn(
        'grid h-5 w-5 shrink-0 place-items-center rounded-full border-[1.25px] transition-all duration-200 focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none dark:focus-visible:ring-offset-gray-950',
        'border-gray-300 bg-transparent p-0 hover:border-brand-500 hover:bg-black/3 dark:border-gray-700 dark:hover:border-brand-500 dark:hover:bg-white/6',
        'data-[state=checked]:border-brand-500 data-[state=checked]:bg-brand-500 data-[state=checked]:hover:bg-brand-500 dark:data-[state=checked]:hover:bg-brand-500',
        isReadonly && !isDisabled && 'cursor-default',
        !isDisabled && !isReadonly && 'cursor-pointer',
        'peer disabled:cursor-not-allowed',
        isDisabled &&
          'border-gray-300 bg-gray-100 opacity-100 dark:border-gray-700 dark:bg-gray-900',
        isDisabled &&
          'data-[state=checked]:border-gray-400 data-[state=checked]:bg-gray-200 dark:data-[state=checked]:border-gray-600 dark:data-[state=checked]:bg-gray-800',
        className
      )}
    >
      {#snippet children({ checked })}
        <div
          class={cn(
            'h-1.5 w-1.5 rounded-full transition-all duration-200',
            isDisabled ? 'bg-gray-500 dark:bg-gray-400' : 'bg-white',
            checked ? 'scale-100 opacity-100' : 'scale-0 opacity-0'
          )}
        ></div>
      {/snippet}
    </RadioGroup.Item>
  </div>

  {#if label || helperText}
    <div class="flex flex-col gap-1">
      {#if label}
        <label
          for={id}
          class={cn(
            'text-sm font-medium select-none',
            isDisabled
              ? 'cursor-not-allowed text-gray-400 dark:text-gray-600'
              : isReadonly
                ? 'cursor-default text-gray-700 dark:text-gray-400'
                : 'cursor-pointer text-gray-700 dark:text-gray-400'
          )}
        >
          {label}
        </label>
      {/if}
      {#if helperText}
        <div transition:slide={{ axis: 'y', duration: 200 }}>
          <span id={helperTextId} class="text-xs text-gray-500 dark:text-gray-400">{helperText}</span>
        </div>
      {/if}
    </div>
  {/if}
</div>
