<script lang="ts">
  import type { AppError } from '$lib/core/types/app';
  import { AlertDialog } from 'bits-ui';
  import { Button, ResponsiveAlertDialog } from '$lib/ui/components';

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
    desktopAnimation?: 'fade-zoom' | 'slide-up' | 'slide-down' | 'slide-left' | 'slide-right' | 'none';
    mobileAnimation?: 'fade-zoom' | 'slide-up' | 'slide-down' | 'slide-left' | 'slide-right' | 'none';
    mobileDrawerSide?: 'top' | 'right' | 'bottom' | 'left';
    triggerElement?: HTMLElement | null;
  } = $props();

  function hasValue(v: string | null): v is string {
    return typeof v === 'string' && v.trim().length > 0;
  }

</script>

<ResponsiveAlertDialog
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
  contentClass="border-white/10 bg-white/95 backdrop-blur-xl dark:bg-gray-900/95"
>
      <div class="text-center">
        <div
          class="animate-fade-in mx-auto mb-6 flex h-16 w-16 items-center justify-center rounded-full shadow-inner bg-rose-50 motion-reduce:animate-none dark:bg-rose-900/20"
        >
          <svg
            class="h-8 w-8 text-red-600 dark:text-red-400"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
            aria-hidden="true"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
            />
          </svg>
        </div>

        <AlertDialog.Title
          class="animate-fade-in mb-2 text-xl font-bold tracking-tight text-gray-900 motion-reduce:animate-none dark:text-white"
          style="animation-delay: 100ms"
        >
          Something went wrong
        </AlertDialog.Title>
        <AlertDialog.Description
          class="animate-fade-in mb-6 text-base leading-relaxed text-gray-500 motion-reduce:animate-none dark:text-gray-400"
          style="animation-delay: 200ms"
        >
          We apologize for the inconvenience. Please try again in a moment.
        </AlertDialog.Description>

        {#if errors && errors.length > 0}
          <div class="animate-fade-in space-y-3 motion-reduce:animate-none" style="animation-delay: 300ms">
            {#each errors as err, i (i)}
              <div class="rounded-lg bg-gray-50 p-3 text-left dark:bg-gray-700/50">
                {#if hasValue(err.code) || hasValue(err.description)}
                  {#if hasValue(err.code)}
                    <div class="mb-2">
                      <div class="text-xxs font-semibold text-gray-400 dark:text-gray-400">
                        Code
                      </div>
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
                  <div class="font-mono text-xs text-gray-500 dark:text-gray-300">
                    Unknown Error
                  </div>
                {/if}
              </div>
            {/each}
          </div>
        {/if}
        <AlertDialog.Cancel>
          {#snippet children()}
            <Button
              variant="primary"
              size="md"
              class="mt-6 w-full sm:min-w-35 justify-center"
              haptic="light"
            >
              Close
            </Button>
          {/snippet}
        </AlertDialog.Cancel>
      </div>
    </ResponsiveAlertDialog>


