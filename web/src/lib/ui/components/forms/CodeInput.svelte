<script lang="ts">
  import { Label, PinInput } from 'bits-ui';
  import { slide } from 'svelte/transition';
  import { cn, slideReveal } from '$lib/ui/utils';

  const PATTERNS = {
    numeric: '^\\d+$',
    alphanumeric: '^[a-zA-Z0-9]+$'
  };

  const generatedId = $props.id();

  let {
    value = $bindable(''),
    length = 6,
    kind = 'numeric',
    label,
    error,
    id = generatedId,
    name,
    disabled = false,
    autofocus = false,
    onValueChange,
    onComplete
  }: {
    value?: string;
    length?: number;
    kind?: 'numeric' | 'alphanumeric';
    label?: string;
    error?: string;
    id?: string;
    name?: string;
    disabled?: boolean;
    autofocus?: boolean;
    onValueChange?: (value: string) => void;
    onComplete?: (value: string) => void;
  } = $props();

  let inputRef = $state<HTMLInputElement | null>(null);

  const numeric = $derived(kind === 'numeric');
  const fluid = $derived(length > 6);
  const errorId = $derived(error ? `${id}-error` : undefined);
  const groupSize = $derived(Math.ceil(length / 2));

  function transformPaste(text: string) {
    return numeric ? text.replace(/\D/g, '') : text.replace(/[^a-zA-Z0-9]/g, '').toUpperCase();
  }

  $effect(() => {
    if (autofocus && inputRef) {
      inputRef.focus();
    }
  });
</script>

{#snippet cellView(cell: PinInput.CellProps['cell'])}
  <PinInput.Cell
    {cell}
    class={cn(
      'relative flex items-center justify-center rounded-lg border bg-transparent font-medium text-gray-800 tabular-nums shadow-theme-xs transition-all duration-200 dark:text-white/90',
      fluid ? 'h-11 min-w-0 flex-1 text-base' : 'h-12 w-11 text-lg',
      !numeric && 'uppercase',
      error ? 'border-error-500 dark:border-error-500' : 'border-gray-300 dark:border-gray-700',
      cell.isActive && 'border-transparent outline-2 outline-offset-2 dark:border-transparent',
      cell.isActive && (error ? 'outline-error-500' : 'outline-brand-500')
    )}
  >
    {cell.char ?? ''}
    {#if cell.hasFakeCaret}
      <div class="pointer-events-none absolute inset-0 flex items-center justify-center">
        <div class="h-5 w-px bg-gray-800 motion-safe:animate-pulse dark:bg-white"></div>
      </div>
    {/if}
  </PinInput.Cell>
{/snippet}

<div class="flex w-full flex-col gap-1.5">
  {#if label}
    <Label.Root for={id} class="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-400">
      {label}
    </Label.Root>
  {/if}

  <PinInput.Root
    bind:value
    bind:inputRef
    inputId={id}
    maxlength={length}
    pattern={PATTERNS[kind]}
    inputmode={numeric ? 'numeric' : 'text'}
    autocomplete={numeric ? 'one-time-code' : 'off'}
    autocapitalize={numeric ? undefined : 'characters'}
    spellcheck={false}
    pasteTransformer={transformPaste}
    pushPasswordManagerStrategy="none"
    {name}
    {disabled}
    aria-invalid={error ? true : undefined}
    aria-describedby={errorId}
    onValueChange={(next: string) => onValueChange?.(next)}
    onComplete={(code: string) => onComplete?.(code)}
    class={cn(
      'flex items-center has-disabled:opacity-60',
      fluid ? 'w-full gap-1.5' : 'w-fit gap-2'
    )}
  >
    {#snippet children({ cells })}
      <div class={cn('flex', fluid ? 'min-w-0 flex-1 gap-1.5' : 'gap-2')}>
        {#each cells.slice(0, groupSize) as cell, index (index)}
          {@render cellView(cell)}
        {/each}
      </div>
      {#if cells.length > groupSize}
        <div
          class="h-0.5 w-3 shrink-0 rounded-full bg-gray-300 dark:bg-gray-600"
          aria-hidden="true"
        ></div>
        <div class={cn('flex', fluid ? 'min-w-0 flex-1 gap-1.5' : 'gap-2')}>
          {#each cells.slice(groupSize) as cell, index (index)}
            {@render cellView(cell)}
          {/each}
        </div>
      {/if}
    {/snippet}
  </PinInput.Root>

  {#if error}
    <div transition:slide={slideReveal}>
      <span id={errorId} class="text-xs font-medium text-error-500">{error}</span>
    </div>
  {/if}
</div>
