<script lang="ts">
  import type { AppError } from '$lib/core/types/app';
  import { AppDialog, Button } from '$lib/ui/components';
  import { TriangleAlert } from 'lucide-svelte';

  let {
    isOpen = $bindable(false),
    errors = $bindable([]),
    desktopMode = 'modal',
    mobileMode = 'drawer',
    desktopPlacement = 'center',
    mobilePlacement = 'center',
    desktopAnimation = 'fade-zoom',
    mobileAnimation = 'slide-up',
    mobileDrawerSide = 'bottom',
    triggerElement = undefined
  }: {
    isOpen?: boolean;
    errors?: AppError[];
    desktopMode?: 'modal' | 'drawer';
    mobileMode?: 'modal' | 'drawer';
    desktopPlacement?: 'center' | 'trigger';
    mobilePlacement?: 'center' | 'trigger';
    desktopAnimation?:
      'fade-zoom' | 'slide-up' | 'slide-down' | 'slide-left' | 'slide-right' | 'none';
    mobileAnimation?:
      'fade-zoom' | 'slide-up' | 'slide-down' | 'slide-left' | 'slide-right' | 'none';
    mobileDrawerSide?: 'top' | 'right' | 'bottom' | 'left';
    triggerElement?: HTMLElement | null;
  } = $props();

  function hasValue(v: string | null): v is string {
    return typeof v === 'string' && v.trim().length > 0;
  }
</script>

<AppDialog
  alert
  bind:open={isOpen}
  size="md"
  {desktopMode}
  {mobileMode}
  {mobileDrawerSide}
  {desktopPlacement}
  {mobilePlacement}
  {desktopAnimation}
  {mobileAnimation}
  {triggerElement}
  icon={TriangleAlert}
  tone="danger"
  title="Something went wrong"
  description="We apologize for the inconvenience. Please try again in a moment."
>
  {#if errors && errors.length > 0}
    <div class="space-y-3">
      {#each errors as err, i (i)}
        <div class="rounded-lg bg-gray-50 p-3 text-left dark:bg-gray-700/50">
          {#if hasValue(err.code) || hasValue(err.description)}
            {#if hasValue(err.code)}
              <div class="mb-2">
                <div class="text-xxs font-semibold text-gray-400 dark:text-gray-400">Code</div>
                <div
                  class="mt-1 font-mono text-xs wrap-break-word text-gray-700 dark:text-gray-200"
                >
                  {err.code}
                </div>
              </div>
            {/if}

            {#if hasValue(err.description)}
              <div>
                <div class="text-xxs font-semibold text-gray-400 dark:text-gray-400">
                  Description
                </div>
                <div
                  class="mt-1 font-mono text-xs wrap-break-word whitespace-pre-wrap text-gray-700 dark:text-gray-200"
                >
                  {err.description}
                </div>
              </div>
            {/if}
          {:else}
            <div class="font-mono text-xs text-gray-500 dark:text-gray-300">Unknown Error</div>
          {/if}
        </div>
      {/each}
    </div>
  {/if}

  {#snippet actions()}
    <Button
      onclick={() => {
        isOpen = false;
      }}
      haptic="light"
    >
      Close
    </Button>
  {/snippet}
</AppDialog>
