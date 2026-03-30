<script lang="ts">
  import { Label, Select } from 'bits-ui';
  import { Check, ChevronDown, ChevronsDown, ChevronsUp } from 'lucide-svelte';
  import { cn } from '$lib/ui/utils';
  import { slide } from 'svelte/transition';

  interface Option {
    value: string;
    label: string;
    disabled?: boolean;
  }

  interface Props {
    options: Option[];
    value?: string;
    values?: string[];
    onValueChange?: (value: string) => void;
    onValuesChange?: (values: string[]) => void;
    placeholder?: string;
    label?: string;
    helperText?: string;
    error?: string | boolean;
    disabled?: boolean;
    required?: boolean | null;
    readonly?: boolean | null;
    allowDeselect?: boolean;
    multiple?: boolean;
    id?: string;
    name?: string | null;
    class?: string;
  }

  const generatedId = $props.id();

  let {
    options,
    value = $bindable(''),
    values = $bindable([] as string[]),
    onValueChange,
    onValuesChange,
    placeholder = 'Select Option',
    label,
    helperText,
    error,
    disabled = false,
    required = false,
    readonly = false,
    allowDeselect = false,
    multiple = false,
    id = generatedId,
    name,
    class: className
  }: Props = $props();

  let open = $state(false);

  const normalizedName = $derived.by(() => (typeof name === 'string' ? name : undefined));
  const isDisabled = $derived.by(() => Boolean(disabled));
  const isRequired = $derived.by(() => Boolean(required));
  const isReadonly = $derived.by(() => Boolean(readonly));
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

  function handleSingleValueChange(nextValue: string) {
    if (isReadonly) {
      return;
    }

    onValueChange?.(nextValue);

    if (allowDeselect && nextValue === '') {
      open = false;
    }
  }

  function handleMultipleValueChange(nextValues: string[]) {
    if (isReadonly) {
      return;
    }

    onValuesChange?.(nextValues);
  }

  function preventReadonlyInteraction(event: Event) {
    if (!isReadonly) {
      return;
    }

    event.preventDefault();
    event.stopPropagation();
    open = false;
  }

  $effect(() => {
    if (isReadonly && open) {
      open = false;
    }
  });

  const selectedLabel = $derived.by(() => {
    if (!multiple) {
      return options.find((option) => option.value === value)?.label ?? placeholder;
    }

    if (values.length === 0) {
      return placeholder;
    }

    const labels = options
      .filter((option) => values.includes(option.value))
      .map((option) => option.label);

    if (labels.length <= 2) {
      return labels.join(', ');
    }

    return `${labels.slice(0, 2).join(', ')} +${labels.length - 2}`;
  });

  const selectedLabelTitle = $derived.by(() => {
    if (!multiple) {
      return selectedLabel;
    }

    if (values.length === 0) {
      return placeholder;
    }

    return options
      .filter((option) => values.includes(option.value))
      .map((option) => option.label)
      .join(', ');
  });

  const hasValue = $derived(multiple ? values.length > 0 : value !== '');
  const triggerTextClass = $derived(
    hasValue ? 'text-gray-800 dark:text-white/90' : 'text-gray-400 dark:text-white/30'
  );
  const triggerClass = $derived.by(() =>
    cn(
      'group relative flex h-11 w-full items-center rounded-lg border bg-transparent px-4 py-2.5 pr-24 text-left text-sm shadow-theme-xs transition-all duration-200',
      hasError
        ? 'border-error-500 hover:border-error-500 hover:bg-error-50/30 dark:border-error-500 dark:hover:border-error-500 dark:hover:bg-error-500/10'
        : 'border-gray-300 hover:border-brand-500 hover:bg-black/3 dark:border-gray-700 dark:hover:border-brand-500 dark:hover:bg-white/6',
      hasError
        ? 'focus-visible:border-error-500 focus-visible:ring-2 focus-visible:ring-error-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none dark:focus-visible:border-error-500 dark:focus-visible:ring-offset-gray-950'
        : 'focus-visible:border-brand-500 focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none dark:focus-visible:border-brand-500 dark:focus-visible:ring-offset-gray-950',
      'disabled:cursor-not-allowed disabled:bg-gray-50/50 disabled:opacity-80 dark:disabled:bg-gray-900/50 disabled:hover:border-gray-300 disabled:hover:bg-transparent dark:disabled:hover:border-gray-700 dark:disabled:hover:bg-transparent',
      isReadonly &&
        !isDisabled &&
        (hasError
          ? 'cursor-default hover:border-error-500 hover:bg-transparent dark:hover:border-error-500 dark:hover:bg-transparent'
          : 'cursor-default hover:border-gray-300 hover:bg-transparent dark:hover:border-gray-700 dark:hover:bg-transparent'),
      !isDisabled && !isReadonly && 'cursor-pointer',
      className
    )
  );
  const chevronClass = $derived.by(() =>
    cn(
      'pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 transition-transform duration-200 group-data-[state=open]:rotate-180',
      hasError ? 'text-error-500 dark:text-error-400' : 'text-gray-700 dark:text-gray-400'
    )
  );
  const itemClass = $derived.by(() =>
    cn(
      'group relative w-full cursor-pointer rounded-md px-3 py-2 text-sm text-gray-700 outline-none transition-colors hover:bg-gray-100 hover:text-gray-900 data-highlighted:bg-gray-100 data-highlighted:text-gray-900 data-disabled:pointer-events-none data-disabled:opacity-50 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-gray-100 dark:data-highlighted:bg-gray-800 dark:data-highlighted:text-gray-100',
      hasError
        ? 'data-selected:text-error-600 dark:data-selected:text-error-400'
        : 'data-selected:text-brand-600 dark:data-selected:text-brand-400'
    )
  );
  const checkIconClass = $derived.by(() =>
    hasError
      ? 'shrink-0 text-error-500 dark:text-error-400'
      : 'shrink-0 text-brand-500 dark:text-brand-400'
  );
  const contentClass =
    'z-50 mt-1 min-w-(--bits-select-anchor-width) max-h-[var(--bits-select-content-available-height)] origin-top overflow-hidden rounded-lg border border-gray-300 bg-white p-1 shadow-theme-lg will-change-[opacity,transform] data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=open]:zoom-in-95 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:zoom-out-95 dark:border-gray-700 dark:bg-gray-900';
  const viewportClass =
    'max-h-[min(20rem,var(--bits-select-content-available-height))] overflow-y-auto p-1';
  const scrollButtonSlotClass = 'h-6';
  const scrollUpButtonClass =
    'flex h-full w-full items-center justify-center rounded-md text-gray-500 transition-colors duration-150 ease-out animate-in fade-in-0 slide-in-from-bottom-1 hover:bg-gray-100 hover:text-gray-700 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-gray-100';
  const scrollDownButtonClass =
    'flex h-full w-full items-center justify-center rounded-md text-gray-500 transition-colors duration-150 ease-out animate-in fade-in-0 slide-in-from-top-1 hover:bg-gray-100 hover:text-gray-700 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-gray-100';
</script>

<div class="flex w-full flex-col gap-1.5">
  {#if label}
    <Label.Root for={id} class="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-400">
      {label}
    </Label.Root>
  {/if}

  {#if normalizedName}
    {#if multiple}
      {#if values.length > 0}
        {#each values as selectedValue (`${normalizedName}-${selectedValue}`)}
          <input
            type="text"
            class="sr-only"
            tabindex="-1"
            aria-hidden="true"
            name={normalizedName}
            value={selectedValue}
            disabled={isDisabled}
          />
        {/each}
      {:else}
        <input
          type="text"
          class="sr-only"
          tabindex="-1"
          aria-hidden="true"
          name={normalizedName}
          value=""
          required={isRequired}
          disabled={isDisabled}
        />
      {/if}
    {:else}
      <input
        type="text"
        class="sr-only"
        tabindex="-1"
        aria-hidden="true"
        name={normalizedName}
        {value}
        required={isRequired}
        disabled={isDisabled}
      />
    {/if}
  {/if}

  <div class="relative z-20 bg-transparent">
    {#if multiple}
      <Select.Root
        type="multiple"
        bind:value={values}
        bind:open
        disabled={isDisabled}
        items={options}
        onValueChange={handleMultipleValueChange}
      >
        <Select.Trigger
          {id}
          disabled={isDisabled}
          aria-invalid={hasError}
          aria-describedby={describedBy}
          aria-errormessage={hasError && errorText ? errorTextId : undefined}
          aria-required={isRequired}
          aria-readonly={isReadonly}
          onclick={preventReadonlyInteraction}
          onpointerdown={preventReadonlyInteraction}
          onkeydown={preventReadonlyInteraction}
          class={triggerClass}
        >
          <span
            class={cn('block min-w-0 flex-1 truncate', triggerTextClass)}
            title={selectedLabelTitle}
          >
            {selectedLabel}
          </span>
          <ChevronDown size={20} class={chevronClass} />
        </Select.Trigger>

        <Select.Portal>
          <Select.Content class={contentClass} side="bottom" sideOffset={6} avoidCollisions={false}>
            <div class={scrollButtonSlotClass}>
              <Select.ScrollUpButton class={scrollUpButtonClass}>
                <ChevronsUp size={14} />
              </Select.ScrollUpButton>
            </div>
            <Select.Viewport class={viewportClass}>
              {#each options as option}
                <Select.Item
                  value={option.value}
                  label={option.label}
                  disabled={option.disabled}
                  class={itemClass}
                >
                  {#snippet children({ selected })}
                    <div class="flex items-center justify-between gap-2">
                      <span>{option.label}</span>
                      {#if selected}
                        <Check size={16} class={checkIconClass} />
                      {/if}
                    </div>
                  {/snippet}
                </Select.Item>
              {/each}
            </Select.Viewport>
            <div class={scrollButtonSlotClass}>
              <Select.ScrollDownButton class={scrollDownButtonClass}>
                <ChevronsDown size={14} />
              </Select.ScrollDownButton>
            </div>
          </Select.Content>
        </Select.Portal>
      </Select.Root>
    {:else}
      <Select.Root
        type="single"
        bind:value
        bind:open
        disabled={isDisabled}
        {allowDeselect}
        items={options}
        onValueChange={handleSingleValueChange}
      >
        <Select.Trigger
          {id}
          disabled={isDisabled}
          aria-invalid={hasError}
          aria-describedby={describedBy}
          aria-errormessage={hasError && errorText ? errorTextId : undefined}
          aria-required={isRequired}
          aria-readonly={isReadonly}
          onclick={preventReadonlyInteraction}
          onpointerdown={preventReadonlyInteraction}
          onkeydown={preventReadonlyInteraction}
          class={triggerClass}
        >
          <span
            class={cn('block min-w-0 flex-1 truncate', triggerTextClass)}
            title={selectedLabelTitle}
          >
            {selectedLabel}
          </span>
          <ChevronDown size={20} class={chevronClass} />
        </Select.Trigger>

        <Select.Portal>
          <Select.Content class={contentClass} side="bottom" sideOffset={6} avoidCollisions={false}>
            <div class={scrollButtonSlotClass}>
              <Select.ScrollUpButton class={scrollUpButtonClass}>
                <ChevronsUp size={14} />
              </Select.ScrollUpButton>
            </div>
            <Select.Viewport class={viewportClass}>
              {#each options as option}
                <Select.Item
                  value={option.value}
                  label={option.label}
                  disabled={option.disabled}
                  class={itemClass}
                >
                  {#snippet children({ selected })}
                    <div class="flex items-center justify-between gap-2">
                      <span>{option.label}</span>
                      {#if selected}
                        <Check size={16} class={checkIconClass} />
                      {/if}
                    </div>
                  {/snippet}
                </Select.Item>
              {/each}
            </Select.Viewport>
            <div class={scrollButtonSlotClass}>
              <Select.ScrollDownButton class={scrollDownButtonClass}>
                <ChevronsDown size={14} />
              </Select.ScrollDownButton>
            </div>
          </Select.Content>
        </Select.Portal>
      </Select.Root>
    {/if}
  </div>

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
