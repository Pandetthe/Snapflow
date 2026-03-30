<script lang="ts">
  import { TimeField, Popover } from 'bits-ui';
  import { parseTime } from '@internationalized/date';
  import { Clock3, X, type Icon as IconType } from 'lucide-svelte';
  import { cn } from '$lib/ui/utils';
  import { ClockPicker } from '$lib/ui/components';
  import { slide } from 'svelte/transition';

  interface Props {
    value?: string;
    label?: string;
    helperText?: string;
    error?: string;
    id?: string;
    name?: string | null;
    disabled?: boolean | null;
    required?: boolean | null;
    readonly?: boolean | null;
    allowDeselect?: boolean;
    hourCycle?: 12 | 24;
    leftIcon?: typeof IconType;
    rightIcon?: typeof IconType;
    leftIconDecorated?: boolean;
    class?: string;
  }

  const generatedId = $props.id();

  let {
    value = $bindable(''),
    label,
    helperText,
    error,
    id = generatedId,
    name,
    disabled = false,
    required = false,
    readonly = false,
    allowDeselect = false,
    hourCycle = 24,
    leftIcon: LeftIcon,
    rightIcon: RightIcon,
    leftIconDecorated = false,
    class: className
  }: Props = $props();

  const normalizedName = $derived.by(() => (typeof name === 'string' ? name : undefined));
  const isDisabled = $derived.by(() => Boolean(disabled));
  const isRequired = $derived.by(() => Boolean(required));
  const isReadonly = $derived.by(() => Boolean(readonly));
  const isTriggerDisabled = $derived.by(() => isDisabled || isReadonly);
  const DateTimeIcon = $derived.by(() => RightIcon ?? Clock3);
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

  let timeFieldValue = $state<ReturnType<typeof parseTime> | undefined>(undefined);
  let isClockOpen = $state(false);

  const leadingPaddingClass = $derived.by(() => {
    if (!LeftIcon) return '';
    return leftIconDecorated ? 'pl-14' : 'pl-11';
  });

  function toTimeValue(rawValue: string) {
    if (!rawValue) return undefined;
    try {
      return parseTime(rawValue);
    } catch {
      return undefined;
    }
  }

  function focusFirstTimeSegment() {
    if (typeof document === 'undefined') return;
    const inputRoot = document.getElementById(id);
    const firstSegment = inputRoot?.querySelector<HTMLElement>('[role="spinbutton"]');
    firstSegment?.focus();
  }

  function handleTimeValueChange(nextValue: { toString: () => string } | undefined) {
    if (!nextValue) {
      if (allowDeselect) {
        value = '';
        timeFieldValue = undefined;
      }
      return;
    }

    timeFieldValue = nextValue as ReturnType<typeof parseTime>;
    value = nextValue.toString();
  }

  function handleInputClick(event: MouseEvent) {
    const target = event.target as HTMLElement | null;
    const currentTarget = event.currentTarget as HTMLElement | null;
    if (!target || !currentTarget) return;

    const segment = target.closest('[role="spinbutton"]');
    const isDayPeriod =
      segment?.getAttribute('data-bits-day-period') !== null ||
      segment?.getAttribute('data-segment') === 'dayPeriod';

    if (isDayPeriod && segment instanceof HTMLElement) {
      const upArrowEvent = new KeyboardEvent('keydown', {
        key: 'ArrowUp',
        bubbles: true,
        cancelable: true
      });
      segment.dispatchEvent(upArrowEvent);
      return;
    }

    if (target.closest('[data-time-action]')) return;
    if (target.closest('[role="spinbutton"]')) return;
    if (isReadonly || isDisabled) return;
    focusFirstTimeSegment();
  }

  function handleInputKeyDown(e: KeyboardEvent) {
    if (isReadonly || isDisabled) return;

    const target = e.target as HTMLElement;
    const isDayPeriod =
      target.getAttribute('data-bits-day-period') !== null ||
      target.getAttribute('data-segment') === 'dayPeriod';

    if (isDayPeriod && (e.key === 'Enter' || e.key === ' ')) {
      e.preventDefault();
      const upArrowEvent = new KeyboardEvent('keydown', {
        key: 'ArrowUp',
        bubbles: true,
        cancelable: true
      });
      target.dispatchEvent(upArrowEvent);
      return;
    }

    if (e.key === 'Backspace' && (e.ctrlKey || e.metaKey) && allowDeselect) {
      e.preventDefault();
      value = '';
      timeFieldValue = undefined;
      return;
    }

    if (e.key === 'ArrowRight') {
      const target = e.target as HTMLElement;
      const isLastSegment =
        !target.nextElementSibling || !target.nextElementSibling.hasAttribute('role');
      if (isLastSegment) {
        const inputRoot = document.getElementById(id);
        const firstAction = inputRoot?.querySelector<HTMLElement>('[data-time-action]');
        if (firstAction) {
          e.preventDefault();
          firstAction.focus();
        }
      }
    }
  }

  function selectQuickTime(nextValue: string) {
    const parsed = toTimeValue(nextValue);
    if (parsed) {
      handleTimeValueChange(parsed);
      isClockOpen = false;
    }
  }

  function clearTime() {
    if (isTriggerDisabled || !allowDeselect) {
      return;
    }

    value = '';
    timeFieldValue = undefined;
    isClockOpen = false;
  }

  $effect(() => {
    if (!value) {
      timeFieldValue = undefined;
      return;
    }
    const parsed = toTimeValue(value);
    if (parsed && parsed.toString() !== timeFieldValue?.toString()) {
      timeFieldValue = parsed;
    }
  });
</script>

<div class="flex w-full flex-col gap-1.5">
  <TimeField.Root
    value={timeFieldValue}
    disabled={isDisabled}
    required={isRequired}
    readonly={isReadonly}
    onValueChange={handleTimeValueChange}
    {hourCycle}
  >
    {#if label}
      <TimeField.Label class="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-400">
        {label}
      </TimeField.Label>
    {/if}

    <div class="group relative">
      {#if LeftIcon}
        {#if leftIconDecorated}
          <span
            class="absolute top-1/2 left-0 z-10 flex h-11 w-[46px] -translate-y-1/2 items-center justify-center border-r border-gray-200 text-gray-500 dark:border-gray-800 dark:text-gray-400"
          >
            <LeftIcon size={18} />
          </span>
        {:else}
          <div class="absolute top-1/2 left-4 z-10 -translate-y-1/2 text-gray-400">
            <LeftIcon size={18} />
          </div>
        {/if}
      {/if}

      <TimeField.Input
        {id}
        name={normalizedName}
        onclick={handleInputClick}
        onkeydown={handleInputKeyDown}
        aria-invalid={hasError}
        aria-describedby={describedBy}
        aria-errormessage={hasError ? errorTextId : undefined}
        class={cn(
          'flex h-11 w-full cursor-text items-center rounded-lg border bg-transparent px-4 py-2.5 text-sm shadow-theme-xs transition-all duration-200 select-none',
          hasError
            ? 'border-error-500 text-gray-800 hover:border-error-500 hover:bg-error-50/30 dark:border-error-500 dark:text-white/90 dark:hover:border-error-500 dark:hover:bg-error-500/10'
            : 'border-gray-300 text-gray-800 hover:border-brand-500 hover:bg-black/3 dark:border-gray-700 dark:text-white/90 dark:hover:border-brand-500 dark:hover:bg-white/6',
          isReadonly && 'cursor-default text-gray-500 dark:text-gray-400',
          hasError
            ? 'focus-within:border-error-500 focus-within:ring-2 focus-within:ring-error-500 focus-within:ring-offset-2 focus-within:ring-offset-white dark:focus-within:border-error-500 dark:focus-within:ring-offset-gray-950'
            : 'focus-within:border-brand-500 focus-within:ring-2 focus-within:ring-brand-500 focus-within:ring-offset-2 focus-within:ring-offset-white dark:focus-within:border-brand-500 dark:focus-within:ring-offset-gray-950',
          'data-disabled:cursor-not-allowed data-disabled:bg-gray-50/50 data-disabled:opacity-80 dark:data-disabled:bg-gray-900/50',
          leadingPaddingClass,
          className
        )}
      >
        {#snippet children({ segments })}
          {#each segments as { part, value: segmentValue }, index (part + index)}
            <div class="inline-flex items-center">
              <TimeField.Segment
                {part}
                class={cn(
                  'rounded-md px-1 py-0.5 text-sm text-gray-800 transition-all focus:bg-black/5 focus-visible:ring-1 focus-visible:outline-none aria-[valuetext=Empty]:text-gray-400 data-invalid:text-error-500 dark:text-white/90 dark:focus:bg-white/10 dark:aria-[valuetext=Empty]:text-white/30',
                  hasError
                    ? 'focus-visible:ring-error-500 dark:focus-visible:ring-error-500'
                    : 'focus-visible:ring-brand-500 dark:focus-visible:ring-brand-500',
                  !isReadonly &&
                    part !== 'dayPeriod' &&
                    'cursor-text hover:bg-black/5 dark:hover:bg-white/10',
                  !isReadonly &&
                    part === 'dayPeriod' &&
                    'cursor-pointer caret-transparent select-none selection:bg-transparent selection:text-gray-800 hover:bg-black/5 active:scale-95 dark:selection:text-white/90 dark:hover:bg-white/10',
                  isReadonly && 'cursor-default'
                )}
              >
                {segmentValue}
              </TimeField.Segment>
            </div>
          {/each}
        {/snippet}
      </TimeField.Input>

      <div class="absolute top-1/2 right-2 flex -translate-y-1/2 items-center gap-0.5 pr-1">
        {#if allowDeselect && value}
          <button
            type="button"
            disabled={isTriggerDisabled}
            data-time-action="clear"
            class={cn(
              'inline-flex h-7 w-7 items-center justify-center rounded-md transition-transform duration-150 focus-visible:ring-1 focus-visible:outline-none',
              hasError
                ? 'text-error-500 focus-visible:ring-error-500'
                : 'text-gray-500 focus-visible:ring-brand-500 dark:text-gray-400',
              !isTriggerDisabled &&
                (hasError
                  ? 'cursor-pointer hover:text-error-600 active:scale-95 dark:hover:text-error-300'
                  : 'cursor-pointer hover:text-brand-500 active:scale-95 dark:hover:text-brand-400'),
              isTriggerDisabled && 'cursor-not-allowed opacity-50'
            )}
            onclick={clearTime}
            aria-label="Clear time"
          >
            <X size={16} />
          </button>
        {/if}

        <Popover.Root bind:open={isClockOpen}>
          <Popover.Trigger
            disabled={isTriggerDisabled}
            data-time-action="clock"
            class={cn(
              'inline-flex h-7 w-7 items-center justify-center rounded-md transition-transform duration-150 focus-visible:ring-1 focus-visible:outline-none',
              hasError
                ? 'text-error-500 focus-visible:ring-error-500 dark:text-error-400 dark:focus-visible:ring-error-500'
                : 'text-gray-500 focus-visible:ring-brand-500 dark:text-gray-400',
              !isTriggerDisabled &&
                (hasError
                  ? 'cursor-pointer hover:text-error-600 active:scale-95 dark:hover:text-error-300'
                  : 'cursor-pointer hover:text-brand-500 active:scale-95 dark:hover:text-brand-400'),
              isTriggerDisabled && 'cursor-not-allowed opacity-50'
            )}
          >
            <DateTimeIcon size={18} />
          </Popover.Trigger>
          <Popover.Portal>
            <Popover.Content
              sideOffset={8}
              align="end"
              class="z-50 origin-top-right rounded-xl border border-gray-200 bg-white p-0 shadow-theme-lg data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:zoom-out-95 data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=open]:zoom-in-95 dark:border-gray-800 dark:bg-gray-900"
            >
              <ClockPicker
                {hourCycle}
                initialHour={timeFieldValue ? timeFieldValue.hour : new Date().getHours()}
                initialMinute={timeFieldValue ? timeFieldValue.minute : new Date().getMinutes()}
                onSelect={(h, m) => {
                  selectQuickTime(`${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}`);
                }}
                onCancel={() => {
                  isClockOpen = false;
                }}
              />
            </Popover.Content>
          </Popover.Portal>
        </Popover.Root>
      </div>
    </div>
  </TimeField.Root>

  {#if helperText && !hasError}
    <div transition:slide={{ axis: 'y', duration: 200 }}>
      <span id={helperTextId} class="text-xs text-gray-500 dark:text-gray-400">{helperText}</span>
    </div>
  {/if}

  {#if hasError}
    <div transition:slide={{ axis: 'y', duration: 200 }}>
      <span id={errorTextId} class="text-xs font-medium text-error-500">{errorText}</span>
    </div>
  {/if}
</div>
