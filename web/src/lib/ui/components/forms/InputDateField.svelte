<script lang="ts">
  import { DatePicker } from 'bits-ui';
  import { parseDate, today, getLocalTimeZone } from '@internationalized/date';
  import { CalendarDays, ChevronLeft, ChevronRight, X, type Icon as IconType } from 'lucide-svelte';
  import { cn } from '$lib/ui/utils';

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
    dateOrder?: 'dmy' | 'mdy' | 'ymd';
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
    dateOrder = 'dmy',
    leftIcon: LeftIcon,
    rightIcon: RightIcon,
    leftIconDecorated = false,
    class: className
  }: Props = $props();

  type DateSegmentPart = 'day' | 'month' | 'year';

  const normalizedName = $derived.by(() => (typeof name === 'string' ? name : undefined));
  const isDisabled = $derived.by(() => Boolean(disabled));
  const isRequired = $derived.by(() => Boolean(required));
  const isReadonly = $derived.by(() => Boolean(readonly));
  const isTriggerDisabled = $derived.by(() => isDisabled || isReadonly);
  const DateTimeIcon = $derived.by(() => RightIcon ?? CalendarDays);
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
  const dateSegmentOrder = $derived.by(() => {
    switch (dateOrder) {
      case 'mdy':
        return ['month', 'day', 'year'] as const;
      case 'ymd':
        return ['year', 'month', 'day'] as const;
      default:
        return ['day', 'month', 'year'] as const;
    }
  });

  let datePickerValue = $state<ReturnType<typeof parseDate> | undefined>(undefined);
  let viewMode = $state<'days' | 'months' | 'years'>('days');
  let currentMonth = $state(today(getLocalTimeZone()));
  let yearPageOffset = $state(0);
  let startYear = $derived(Math.floor(currentMonth.year / 12) * 12 + yearPageOffset * 12);
  const monthNames = [
    'January',
    'February',
    'March',
    'April',
    'May',
    'June',
    'July',
    'August',
    'September',
    'October',
    'November',
    'December'
  ];

  const leadingPaddingClass = $derived.by(() => {
    if (!LeftIcon) {
      return '';
    }

    return leftIconDecorated ? 'pl-14' : 'pl-11';
  });

  function orderDateSegments(
    segments: Array<{ part: any; value: string }>,
    order: readonly DateSegmentPart[]
  ) {
    const segmentByPart = new Map<string, { part: any; value: string }>();

    for (const segment of segments) {
      if (segment.part !== 'literal') {
        segmentByPart.set(segment.part, segment);
      }
    }

    return order
      .map((part) => segmentByPart.get(part))
      .filter((segment): segment is { part: any; value: string } => Boolean(segment));
  }

  function toDateValue(rawValue: string) {
    if (!rawValue) {
      return undefined;
    }

    try {
      return parseDate(rawValue);
    } catch {
      return undefined;
    }
  }

  function focusFirstDateSegment() {
    if (typeof document === 'undefined') {
      return;
    }

    const inputRoot = document.getElementById(id);
    const firstSegment = inputRoot?.querySelector<HTMLElement>('[role="spinbutton"]');
    firstSegment?.focus();
  }

  function handleDateValueChange(nextValue: { toString: () => string } | undefined) {
    if (!nextValue) {
      if (allowDeselect) {
        value = '';
        datePickerValue = undefined;
      }
      return;
    }

    datePickerValue = nextValue as ReturnType<typeof parseDate>;
    value = nextValue.toString();
  }

  function handleInputClick(event: MouseEvent) {
    const target = event.target as HTMLElement | null;
    const currentTarget = event.currentTarget as HTMLElement | null;

    if (!target || !currentTarget) {
      return;
    }

    if (target.closest('[data-date-action]')) {
      return;
    }

    if (target.closest('[role="spinbutton"]')) {
      return;
    }

    if (isReadonly || isDisabled) {
      return;
    }

    focusFirstDateSegment();
  }

  function clearDate() {
    if (isTriggerDisabled || !allowDeselect) {
      return;
    }

    value = '';
    datePickerValue = undefined;
  }

  function handleInputKeyDown(event: KeyboardEvent) {
    if (isReadonly || isDisabled) {
      return;
    }

    if (event.key === 'Backspace' && (event.ctrlKey || event.metaKey) && allowDeselect) {
      event.preventDefault();
      value = '';
      datePickerValue = undefined;
      return;
    }

    if (event.key === 'ArrowRight') {
      const target = event.target as HTMLElement;
      const isLastSegment =
        !target.nextElementSibling || !target.nextElementSibling.hasAttribute('role');

      if (isLastSegment) {
        const inputRoot = document.getElementById(id);
        const firstAction = inputRoot?.querySelector<HTMLElement>('[data-date-action]');
        if (firstAction) {
          event.preventDefault();
          firstAction.focus();
        }
      }
    }
  }

  $effect(() => {
    if (!value) {
      if (datePickerValue !== undefined) {
        datePickerValue = undefined;
      }
      return;
    }

    const parsed = toDateValue(value);
    if (!parsed) {
      return;
    }

    const current = datePickerValue?.toString() ?? '';
    const next = parsed.toString();

    if (current !== next) {
      datePickerValue = parsed;
    }
  });
</script>

<div class="flex w-full flex-col gap-1.5">
  <DatePicker.Root
    value={datePickerValue}
    bind:placeholder={currentMonth}
    disabled={isDisabled}
    required={isRequired}
    readonly={isReadonly}
    onValueChange={handleDateValueChange}
  >
    {#if label}
      <DatePicker.Label class="mb-1.5 block text-sm font-medium text-gray-700 dark:text-gray-400">
        {label}
      </DatePicker.Label>
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

      <DatePicker.Input
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
          {@const orderedSegments = orderDateSegments(segments, dateSegmentOrder)}
          {#each orderedSegments as { part, value: segmentValue }, index (`${part}-${index}`)}
            <div class="inline-flex items-center">
              <DatePicker.Segment
                {part}
                class={cn(
                  'rounded-md px-1 py-0.5 text-sm text-gray-800 transition-colors focus:bg-black/5 focus-visible:ring-1 focus-visible:outline-none aria-[valuetext=Empty]:text-gray-400 data-invalid:text-error-500 dark:text-white/90 dark:focus:bg-white/10 dark:aria-[valuetext=Empty]:text-white/30',
                  hasError ? 'focus-visible:ring-error-500 dark:focus-visible:ring-error-500' : 'focus-visible:ring-brand-500 dark:focus-visible:ring-brand-500',
                  !isReadonly && 'cursor-text hover:bg-black/5 dark:hover:bg-white/10',
                  isReadonly && 'cursor-default'
                )}
              >
                {segmentValue}
              </DatePicker.Segment>
              {#if index < orderedSegments.length - 1}
                <span class="px-0.5 text-sm text-gray-400 dark:text-white/30">/</span>
              {/if}
            </div>
          {/each}

          <div class="-mr-2 ml-auto inline-flex items-center">
            {#if allowDeselect && value}
              <button
                type="button"
                disabled={isTriggerDisabled}
                data-date-action="clear"
                class={cn(
                  'mr-1 inline-flex h-7 w-7 items-center justify-center rounded-md transition-transform duration-150 focus-visible:ring-1 focus-visible:outline-none',
                  hasError
                    ? 'text-error-500 focus-visible:ring-error-500'
                    : 'text-gray-500 focus-visible:ring-brand-500 dark:text-gray-400',
                  !isTriggerDisabled && (hasError
                    ? 'cursor-pointer hover:text-error-600 active:scale-95 dark:hover:text-error-300'
                    : 'cursor-pointer hover:text-brand-500 active:scale-95 dark:hover:text-brand-400'),
                  isTriggerDisabled && 'cursor-not-allowed opacity-50'
                )}
                onclick={clearDate}
                aria-label="Clear date"
              >
                <X size={16} />
              </button>
            {/if}

            <DatePicker.Trigger
              disabled={isTriggerDisabled}
              data-date-action="calendar"
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
              aria-label="Open calendar"
            >
              <DateTimeIcon size={18} />
            </DatePicker.Trigger>
          </div>
        {/snippet}
      </DatePicker.Input>
    </div>

    <DatePicker.Content
      align="end"
      trapFocus={true}
      class="z-50 origin-top-right rounded-xl border border-gray-200 bg-white p-0 shadow-theme-lg data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:zoom-out-95 data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=open]:zoom-in-95 dark:border-gray-800 dark:bg-gray-900"
    >
      <DatePicker.Calendar class="w-70 p-3 select-none">
        {#snippet children({ months, weekdays })}
          <DatePicker.Header class="mb-2 flex items-center justify-between gap-2">
            {#if viewMode === 'days'}
              <DatePicker.PrevButton
                class="inline-flex h-8 w-8 cursor-pointer items-center justify-center rounded-md text-gray-600 transition-all hover:bg-gray-100 hover:text-gray-800 active:scale-95 disabled:cursor-not-allowed disabled:opacity-40 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-gray-200"
              >
                <ChevronLeft size={16} />
              </DatePicker.PrevButton>
            {:else}
              <button
                type="button"
                class="inline-flex h-8 w-8 cursor-pointer items-center justify-center rounded-md text-gray-600 transition-all hover:bg-gray-100 hover:text-gray-800 active:scale-95 disabled:cursor-not-allowed disabled:opacity-40 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-gray-200"
                onclick={() => {
                  if (viewMode === 'years') yearPageOffset -= 1;
                  else if (viewMode === 'months')
                    currentMonth = currentMonth.set({ year: currentMonth.year - 1 });
                }}
              >
                <ChevronLeft size={16} />
              </button>
            {/if}

            <button
              type="button"
              class="cursor-pointer rounded-md px-2 py-1 text-sm font-semibold text-gray-700 transition-all hover:bg-gray-100 active:scale-95 dark:text-gray-200 dark:hover:bg-gray-800"
              onclick={() => {
                if (viewMode === 'days') viewMode = 'months';
                else if (viewMode === 'months') viewMode = 'years';
                else viewMode = 'days';
              }}
            >
              {#if viewMode === 'days'}
                {monthNames[currentMonth.month - 1]} {currentMonth.year}
              {:else if viewMode === 'months'}
                {currentMonth.year}
              {:else}
                {startYear} - {startYear + 11}
              {/if}
            </button>

            {#if viewMode === 'days'}
              <DatePicker.NextButton
                class="inline-flex h-8 w-8 cursor-pointer items-center justify-center rounded-md text-gray-600 transition-all hover:bg-gray-100 hover:text-gray-800 active:scale-95 disabled:cursor-not-allowed disabled:opacity-40 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-gray-200"
              >
                <ChevronRight size={16} />
              </DatePicker.NextButton>
            {:else}
              <button
                type="button"
                class="inline-flex h-8 w-8 cursor-pointer items-center justify-center rounded-md text-gray-600 transition-all hover:bg-gray-100 hover:text-gray-800 active:scale-95 disabled:cursor-not-allowed disabled:opacity-40 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-gray-200"
                onclick={() => {
                  if (viewMode === 'years') yearPageOffset += 1;
                  else if (viewMode === 'months')
                    currentMonth = currentMonth.set({ year: currentMonth.year + 1 });
                }}
              >
                <ChevronRight size={16} />
              </button>
            {/if}
          </DatePicker.Header>

          <div class="h-[238px] w-full">
            {#if viewMode === 'days'}
              {#each months as month (month.value.toString())}
                <DatePicker.Grid class="w-full border-collapse">
                  <DatePicker.GridHead>
                    <DatePicker.GridRow class="mb-1 grid grid-cols-7">
                      {#each weekdays as day, dayIndex (`${day}-${dayIndex}`)}
                        <DatePicker.HeadCell
                          class="text-center text-xs font-medium text-gray-500 dark:text-gray-400"
                        >
                          {day.slice(0, 2)}
                        </DatePicker.HeadCell>
                      {/each}
                    </DatePicker.GridRow>
                  </DatePicker.GridHead>

                  <DatePicker.GridBody>
                    {#each month.weeks as weekDates, weekIndex (`${month.value.toString()}-${weekIndex}`)}
                      <DatePicker.GridRow class="grid grid-cols-7">
                        {#each weekDates as date (date.toString())}
                          <DatePicker.Cell {date} month={month.value} class="p-0 text-center">
                            <DatePicker.Day
                              class="inline-flex h-9 w-9 cursor-pointer items-center justify-center rounded-md text-sm font-medium text-gray-700 transition-all hover:bg-gray-100 active:scale-95 data-disabled:pointer-events-none data-disabled:text-gray-300 data-outside-month:text-gray-300 data-selected:bg-brand-500 data-selected:text-white dark:text-gray-300 dark:hover:bg-gray-800 dark:data-disabled:text-gray-700 dark:data-outside-month:text-gray-600 dark:data-selected:bg-brand-500 dark:data-selected:text-white"
                            >
                              {date.day}
                            </DatePicker.Day>
                          </DatePicker.Cell>
                        {/each}
                      </DatePicker.GridRow>
                    {/each}
                  </DatePicker.GridBody>
                </DatePicker.Grid>
              {/each}
            {:else if viewMode === 'months'}
              <div class="grid h-full grid-cols-3 gap-2 py-2">
                {#each monthNames as monthName, i}
                  <button
                    type="button"
                    class={cn(
                      'flex cursor-pointer items-center justify-center rounded-md text-sm font-medium transition-all active:scale-95',
                      currentMonth.month === i + 1
                        ? 'bg-brand-500 text-white hover:bg-brand-600 dark:bg-brand-500 dark:hover:bg-brand-600'
                        : 'text-gray-700 hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-800'
                    )}
                    onclick={() => {
                      currentMonth = currentMonth.set({ month: i + 1 });
                      viewMode = 'days';
                    }}
                  >
                    {monthName.slice(0, 3)}
                  </button>
                {/each}
              </div>
            {:else if viewMode === 'years'}
              <div class="grid h-full grid-cols-3 gap-2 py-2">
                {#each Array(12) as _, i}
                  {@const y = startYear + i}
                  <button
                    type="button"
                    class={cn(
                      'flex cursor-pointer items-center justify-center rounded-md text-sm font-medium transition-all active:scale-95',
                      currentMonth.year === y
                        ? 'bg-brand-500 text-white hover:bg-brand-600 dark:bg-brand-500 dark:hover:bg-brand-600'
                        : 'text-gray-700 hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-800'
                    )}
                    onclick={() => {
                      currentMonth = currentMonth.set({ year: y });
                      viewMode = 'months';
                      yearPageOffset = 0;
                    }}
                  >
                    {y}
                  </button>
                {/each}
              </div>
            {/if}
          </div>
        {/snippet}
      </DatePicker.Calendar>
    </DatePicker.Content>
  </DatePicker.Root>

  {#if helperText && !hasError}
    <span id={helperTextId} class="text-xs text-gray-500 dark:text-gray-400">{helperText}</span>
  {/if}

  {#if hasError}
    <span id={errorTextId} class="text-xs font-medium text-error-500">{errorText}</span>
  {/if}
</div>
