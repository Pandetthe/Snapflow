<script module lang="ts">
  export type DialogTone = 'brand' | 'success' | 'info' | 'warning' | 'danger';
</script>

<script lang="ts">
  import { AlertDialog, Dialog } from 'bits-ui';
  import type { Snippet } from 'svelte';
  import type { Icon as IconType } from 'lucide-svelte';
  import { X } from 'lucide-svelte';
  import { cn } from '$lib/ui/utils';
  import ResponsiveDialog from './ResponsiveDialog.svelte';
  import ResponsiveAlertDialog from './ResponsiveAlertDialog.svelte';
  import type { DialogBehaviorOptions } from './dialogBehavior.svelte.js';

  const TONES: Record<DialogTone, { circle: string; icon: string }> = {
    brand: {
      circle: 'bg-brand-100 dark:bg-brand-900/30',
      icon: 'text-brand-600 dark:text-brand-400'
    },
    success: {
      circle: 'bg-success-100 dark:bg-success-900/40',
      icon: 'text-success-600 dark:text-success-400'
    },
    info: {
      circle: 'bg-sky-100 dark:bg-sky-900/30',
      icon: 'text-sky-600 dark:text-sky-400'
    },
    warning: {
      circle: 'bg-amber-100 dark:bg-amber-900/30',
      icon: 'text-amber-600 dark:text-amber-400'
    },
    danger: {
      circle: 'bg-error-50 dark:bg-error-500/10',
      icon: 'text-error-600 dark:text-error-400'
    }
  };

  let {
    open = $bindable(false),
    onOpenChange,
    alert = false,
    title,
    description,
    icon: Icon,
    tone = 'brand',
    closeButton = false,
    onsubmit,
    children,
    actions,
    size = 'md',
    contentClass,
    ...options
  }: DialogBehaviorOptions & {
    open: boolean;
    onOpenChange?: (open: boolean) => void;
    alert?: boolean;
    title: string;
    description?: string | Snippet;
    icon?: typeof IconType;
    tone?: DialogTone;
    closeButton?: boolean;
    onsubmit?: (event: SubmitEvent) => unknown;
    children?: Snippet;
    actions?: Snippet;
  } = $props();

  const Shell = $derived(alert ? ResponsiveAlertDialog : ResponsiveDialog);
  const Title = $derived(alert ? AlertDialog.Title : Dialog.Title);
  const Description = $derived(alert ? AlertDialog.Description : Dialog.Description);
</script>

{#snippet descriptionContent()}
  {#if typeof description === 'string'}
    {description}
  {:else if description}
    {@render description()}
  {/if}
{/snippet}

{#snippet body(centered: boolean)}
  {#if children}
    <div class={cn('empty:hidden', !centered && 'mt-5')}>
      {@render children()}
    </div>
  {/if}
  {#if actions}
    <div
      class={cn(
        'flex flex-col-reverse gap-3 *:w-full *:justify-center sm:flex-row sm:*:w-auto sm:*:min-w-32',
        centered ? 'sm:justify-center sm:*:flex-1' : 'mt-6 sm:justify-end'
      )}
    >
      {@render actions()}
    </div>
  {/if}
{/snippet}

<Shell
  bind:open
  {onOpenChange}
  {size}
  contentClass={cn(
    'border-white/10 bg-white/95 backdrop-blur-xl dark:bg-gray-900/95',
    contentClass
  )}
  {...options}
>
  {#if Icon}
    <div class="space-y-6 text-center">
      <div
        class={cn(
          'mx-auto flex size-16 items-center justify-center rounded-full shadow-inner',
          TONES[tone].circle
        )}
      >
        <Icon size={32} class={TONES[tone].icon} />
      </div>
      <div class="space-y-2">
        <Title class="text-xl font-bold tracking-tight text-gray-900 dark:text-white">
          {title}
        </Title>
        {#if description}
          <Description class="text-base leading-relaxed text-gray-500 dark:text-gray-400">
            {@render descriptionContent()}
          </Description>
        {/if}
      </div>
      {#if onsubmit}
        <form novalidate {onsubmit} class="space-y-6">
          {@render body(true)}
        </form>
      {:else}
        {@render body(true)}
      {/if}
    </div>
  {:else}
    <div class="flex items-start justify-between gap-3">
      <div class="min-w-0 space-y-1.5">
        <Title
          class="text-xl font-bold tracking-tight wrap-break-word text-gray-900 dark:text-white"
        >
          {title}
        </Title>
        {#if description}
          <Description class="text-sm text-gray-500 dark:text-gray-400">
            {@render descriptionContent()}
          </Description>
        {/if}
      </div>
      {#if closeButton}
        <button
          type="button"
          aria-label="Close"
          class="-mt-1 -mr-2 shrink-0 rounded-lg p-2 text-gray-500 transition-colors hover:bg-gray-100 hover:text-gray-800 dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-gray-100"
          onclick={() => {
            open = false;
          }}
        >
          <X size={18} />
        </button>
      {/if}
    </div>
    {#if onsubmit}
      <form novalidate {onsubmit}>
        {@render body(false)}
      </form>
    {:else}
      {@render body(false)}
    {/if}
  {/if}
</Shell>
