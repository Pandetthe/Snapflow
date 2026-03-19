<script lang="ts">
  import { cn } from '$lib/ui/utils';
  import { untrack } from 'svelte';

  interface Props {
    hourCycle?: 12 | 24;
    initialHour?: number;
    initialMinute?: number;
    onSelect?: (hour: number, minute: number) => void;
    onCancel?: () => void;
  }

  let props: Props = $props();

  let view = $state<'hours' | 'minutes'>('hours');
  let isDragging = $state(false);

  let isHourFocused = $state(false);
  let isMinuteFocused = $state(false);

  let hour = $state(0);
  let minute = $state(0);
  let period = $state<'AM' | 'PM'>('AM');

  $effect.pre(() => {
    const h = props.initialHour ?? 12;
    const m = props.initialMinute ?? 0;
    const cycle = props.hourCycle ?? 24;

    hour = cycle === 12 ? (h === 0 ? 12 : h > 12 ? h - 12 : h) : h;
    minute = m;
    period = h >= 12 ? 'PM' : 'AM';
  });

  const clockSize = 200;
  const center = clockSize / 2;
  const outerRadius = 75;
  const innerRadius = 45;

  function getPointerStyles(type: 'hours' | 'minutes', val: number) {
    let angle = 0;
    let radius = outerRadius;

    if (type === 'hours') {
      if (props.hourCycle === 24) {
        angle = val * 30;
        if (val === 0 || val > 12) radius = innerRadius;
      } else {
        angle = val * 30;
      }
    } else {
      angle = val * 6;
    }

    return {
      transform: `rotate(${angle - 90}deg)`,
      width: `${radius}px`
    };
  }

  let hourOptions = $derived.by(() => {
    const opts = [];
    if (props.hourCycle === 24) {
      for (let i = 1; i <= 12; i++) opts.push({ val: i, radius: outerRadius });
      for (let i = 13; i <= 23; i++) opts.push({ val: i, radius: innerRadius });
      opts.push({ val: 0, radius: innerRadius });
    } else {
      for (let i = 1; i <= 12; i++) opts.push({ val: i, radius: outerRadius });
    }
    return opts;
  });

  let minuteOptions = $derived.by(() => {
    const opts = [];
    for (let i = 0; i < 60; i += 5) {
      opts.push({ val: i, radius: outerRadius });
    }
    return opts;
  });

  function getPosition(val: number, isHour: boolean, radius: number) {
    let angle = isHour ? val * 30 - 90 : val * 6 - 90;
    let rad = (angle * Math.PI) / 180;
    return {
      x: center + radius * Math.cos(rad),
      y: center + radius * Math.sin(rad)
    };
  }

  function handlePointerDown(e: PointerEvent) {
    const target = e.currentTarget as HTMLElement;
    target.setPointerCapture(e.pointerId);
    isDragging = true;
    updateTimeFromEvent(e);
  }

  function handlePointerMove(e: PointerEvent) {
    if (!isDragging) return;
    updateTimeFromEvent(e);
  }

  function handlePointerUp(e: PointerEvent) {
    if (!isDragging) return;
    const target = e.currentTarget as HTMLElement;
    target.releasePointerCapture(e.pointerId);
    isDragging = false;

    if (view === 'hours') {
      setTimeout(() => {
        view = 'minutes';
      }, 200);
    }
  }

  function updateTimeFromEvent(e: PointerEvent) {
    const rect = (e.currentTarget as HTMLElement).getBoundingClientRect();
    const x = e.clientX - rect.left - center;
    const y = e.clientY - rect.top - center;

    let angle = (Math.atan2(y, x) * 180) / Math.PI + 90;
    if (angle < 0) angle += 360;
    const distance = Math.sqrt(x * x + y * y);

    if (view === 'hours') {
      let h = Math.round(angle / 30) % 12;

      if (props.hourCycle === 24) {
        const threshold = (outerRadius + innerRadius) / 2;
        if (distance < threshold) {
          if (h === 0) h = 0;
          else h += 12;
          hour = h;
        } else {
          if (h === 0) h = 12;
          hour = h;
        }
      } else {
        if (h === 0) h = 12;
        hour = h;
      }
    } else {
      let m = Math.round(angle / 6) % 60;
      minute = m;
    }
  }

  function formatNumber(n: number) {
    return n.toString().padStart(2, '0');
  }

  function handleOk() {
    let finalHour = hour;
    if (props.hourCycle === 12) {
      if (period === 'PM' && finalHour < 12) finalHour += 12;
      if (period === 'AM' && finalHour === 12) finalHour = 0;
    }
    props.onSelect?.(finalHour, minute);
  }
</script>

<div
  class="flex w-full max-w-xs min-w-64 flex-col items-center rounded-xl bg-transparent px-4 pt-4 pb-2 select-none"
>
  <div class="mb-3 flex items-end justify-center gap-1 text-gray-400">
    <input
      type="text"
      inputmode="numeric"
      maxlength="2"
      class={cn(
        'h-14 w-16 cursor-text rounded-lg bg-transparent text-center text-4xl font-light transition-colors outline-none focus:ring-2 focus:ring-brand-500',
        view === 'hours'
          ? 'bg-brand-50 text-brand-600 dark:bg-brand-900/30 dark:text-brand-400'
          : 'text-gray-400 hover:bg-gray-50 dark:hover:bg-gray-800'
      )}
      value={props.hourCycle === 12
        ? String(hour === 0 ? 12 : hour > 12 ? hour - 12 : hour)
        : isHourFocused
          ? String(hour)
          : formatNumber(hour)}
      onfocus={(e) => {
        view = 'hours';
        isHourFocused = true;
        e.currentTarget.select();
      }}
      onblur={() => {
        isHourFocused = false;
      }}
      oninput={(e) => {
        let val = parseInt(e.currentTarget.value, 10);
        if (isNaN(val)) return;
        if (props.hourCycle === 12) {
          if (val > 12) {
            val = 12;
            e.currentTarget.value = '12';
          }
          if (period === 'PM' && val < 12) hour = val + 12;
          else if (period === 'AM' && val === 12) hour = 0;
          else hour = val;
        } else {
          if (val > 23) {
            val = 23;
            e.currentTarget.value = '23';
          }
          hour = val;
        }
      }}
      onkeydown={(e) => {
        if (e.key === 'Enter') {
          e.preventDefault();
          handleOk();
        }
      }}
    />
    <span class="pb-1 text-3xl font-light opacity-50">:</span>
    <input
      type="text"
      inputmode="numeric"
      maxlength="2"
      class={cn(
        'h-14 w-16 cursor-text rounded-lg bg-transparent text-center text-4xl font-light transition-colors outline-none focus:ring-2 focus:ring-brand-500',
        view === 'minutes'
          ? 'bg-brand-50 text-brand-600 dark:bg-brand-900/30 dark:text-brand-400'
          : 'text-gray-400 hover:bg-gray-50 dark:hover:bg-gray-800'
      )}
      value={isMinuteFocused ? String(minute) : formatNumber(minute)}
      onfocus={(e) => {
        view = 'minutes';
        isMinuteFocused = true;
        e.currentTarget.select();
      }}
      onblur={() => {
        isMinuteFocused = false;
      }}
      oninput={(e) => {
        let val = parseInt(e.currentTarget.value, 10);
        if (isNaN(val)) return;
        if (val > 59) {
          val = 59;
          e.currentTarget.value = '59';
        }
        minute = val;
      }}
      onkeydown={(e) => {
        if (e.key === 'Enter') {
          e.preventDefault();
          handleOk();
        }
      }}
    />

    {#if props.hourCycle === 12}
      <div class="mb-1 ml-2 flex flex-col justify-end space-y-0.5">
        <button
          type="button"
          class={cn(
            'cursor-pointer rounded border px-2 py-0.5 text-xs font-semibold transition-all select-none active:scale-95',
            period === 'AM'
              ? 'border-brand-200 bg-brand-100 text-brand-700 dark:border-brand-800 dark:bg-brand-900/30 dark:text-brand-300'
              : 'border-transparent text-gray-500'
          )}
          onclick={() => (period = 'AM')}>AM</button
        >
        <button
          type="button"
          class={cn(
            'cursor-pointer rounded border px-2 py-0.5 text-xs font-semibold transition-all select-none active:scale-95',
            period === 'PM'
              ? 'border-brand-200 bg-brand-100 text-brand-700 dark:border-brand-800 dark:bg-brand-900/30 dark:text-brand-300'
              : 'border-transparent text-gray-500'
          )}
          onclick={() => (period = 'PM')}>PM</button
        >
      </div>
    {/if}
  </div>

  <div
    role="timer"
    tabindex="-1"
    class="relative flex cursor-pointer touch-none items-center justify-center rounded-full bg-gray-100 dark:bg-gray-800"
    style={`width: ${clockSize}px; height: ${clockSize}px;`}
    onpointerdown={handlePointerDown}
    onpointermove={handlePointerMove}
    onpointerup={handlePointerUp}
  >
    <div class="absolute z-10 h-2 w-2 rounded-full bg-brand-500 dark:bg-brand-400"></div>

    <div
      class="absolute z-0 origin-left bg-brand-500 dark:bg-brand-400"
      style={`height: 2px; left: ${center}px; top: ${center - 1}px; transform: ${getPointerStyles(view, view === 'hours' ? hour : minute).transform}; width: ${getPointerStyles(view, view === 'hours' ? hour : minute).width}; ${isDragging ? 'transition: none;' : 'transition: all 250ms cubic-bezier(0.4, 0, 0.2, 1);'}`}
    >
      <div
        class="absolute top-1/2 -right-4 h-8 w-8 -translate-y-1/2 rounded-full bg-brand-500 shadow-sm dark:bg-brand-400"
      ></div>
    </div>

    {#if view === 'hours'}
      {#each hourOptions as { val, radius }}
        {@const pos = getPosition(val, true, radius)}
        <div
          class={cn(
            'pointer-events-none absolute z-10 flex h-8 w-8 -translate-x-1/2 -translate-y-1/2 items-center justify-center rounded-full text-sm font-medium transition-colors',
            hour === val ? 'text-white' : 'text-gray-700 dark:text-gray-300'
          )}
          style={`left: ${pos.x}px; top: ${pos.y}px;`}
        >
          {val === 0 ? '00' : val}
        </div>
      {/each}
    {:else}
      {#each minuteOptions as { val, radius }}
        {@const pos = getPosition(val, false, radius)}
        <div
          class={cn(
            'pointer-events-none absolute z-10 flex h-8 w-8 -translate-x-1/2 -translate-y-1/2 items-center justify-center rounded-full text-sm font-medium transition-colors',
            minute === val ? 'text-white' : 'text-gray-700 dark:text-gray-300'
          )}
          style={`left: ${pos.x}px; top: ${pos.y}px;`}
        >
          {formatNumber(val)}
        </div>
      {/each}
      {#if minute % 5 !== 0}
        {@const pos = getPosition(minute, false, outerRadius)}
        <div
          class="pointer-events-none absolute z-10 flex h-8 w-8 -translate-x-1/2 -translate-y-1/2 items-center justify-center rounded-full"
          style={`left: ${pos.x}px; top: ${pos.y}px;`}
        >
          <div class="h-1.5 w-1.5 rounded-full bg-white"></div>
        </div>
      {/if}
    {/if}
  </div>

  <div class="mt-3 flex w-full justify-between gap-2">
    <button
      type="button"
      class="cursor-pointer rounded-lg px-3 py-2 text-sm font-medium text-brand-600 transition-all hover:bg-brand-50 active:scale-95 dark:text-brand-400 dark:hover:bg-brand-900/20"
      onclick={() => {
        const now = new Date();
        props.onSelect?.(now.getHours(), now.getMinutes());
      }}
    >
      Now
    </button>
    <div class="flex gap-2">
      <button
        type="button"
        class="cursor-pointer rounded-lg px-3 py-2 text-sm font-medium text-gray-600 transition-all hover:bg-gray-100 active:scale-95 dark:text-gray-400 dark:hover:bg-gray-800"
        onclick={props.onCancel}
      >
        Cancel
      </button>
      <button
        type="button"
        class="cursor-pointer rounded-lg px-3 py-2 text-sm font-medium text-brand-600 transition-all hover:bg-brand-50 active:scale-95 dark:text-brand-400 dark:hover:bg-brand-900/20"
        onclick={handleOk}
      >
        OK
      </button>
    </div>
  </div>
</div>
