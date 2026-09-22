<script lang="ts">
  import type { LucideIcon } from '@lucide/svelte';
  import type { Snippet } from 'svelte';
  import { StatusBadge } from '$lib/ui/components';
  import type { StatusTone } from '$lib/ui/components/base/StatusBadge.svelte';

  let {
    icon: Icon,
    iconContent,
    label,
    value,
    badge,
    badgeTone = 'neutral',
    action
  }: {
    icon?: LucideIcon;
    iconContent?: Snippet;
    label: string;
    value?: string | Snippet;
    badge?: string;
    badgeTone?: StatusTone;
    action?: Snippet;
  } = $props();
</script>

<div class="flex items-center justify-between gap-4">
  <div class="flex min-w-0 flex-1 items-center gap-3">
    <div
      class="flex size-9 shrink-0 items-center justify-center rounded-lg border border-gray-200 text-gray-400 dark:border-gray-800 dark:text-gray-500"
    >
      {#if iconContent}
        {@render iconContent()}
      {:else if Icon}
        <Icon size={16} />
      {/if}
    </div>
    <div class="min-w-0 flex-1">
      <p class="truncate text-xs text-gray-400 dark:text-gray-500">{label}</p>
      <div class="flex min-h-5 min-w-0 items-center gap-2">
        {#if typeof value === 'string'}
          <p class="truncate text-sm font-medium text-gray-900 dark:text-white">{value}</p>
        {:else if value}
          {@render value()}
        {/if}
        {#if badge}
          <StatusBadge tone={badgeTone}>{badge}</StatusBadge>
        {/if}
      </div>
    </div>
  </div>
  {#if action}
    <div class="shrink-0">
      {@render action()}
    </div>
  {/if}
</div>
