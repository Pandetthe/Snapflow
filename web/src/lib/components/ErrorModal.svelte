<script lang="ts">
	import type { AppError } from '$lib/types/app';
	import { AlertDialog } from 'bits-ui';

  let { 
    isOpen = $bindable(false), 
    errors = $bindable([]), 
  }: { 
    isOpen?: boolean;
    errors?: AppError[]; 
  } = $props();

	function hasValue(v: string | null): v is string {
		return typeof v === 'string' && v.trim().length > 0;
	}

  function onOpenChange(open: boolean) {
    isOpen = open;
    if (!open) {
      errors = [];
    }
  }
</script>

<AlertDialog.Root bind:open={isOpen} onOpenChangeComplete={onOpenChange}>
	<AlertDialog.Portal>
		<AlertDialog.Overlay class="data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0 fixed inset-0 z-50 bg-black/80" />
		<AlertDialog.Content interactOutsideBehavior="close" escapeKeydownBehavior="close" class="fixed left-[50%] top-[50%] translate-x-[-50%] translate-y-[-50%] w-full max-w-lg rounded-lg bg-white dark:bg-gray-800 p-8 shadow-lg data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0 data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95 duration-500 ease-out z-50">
			<div class="text-center">
				<div class="mx-auto flex items-center justify-center h-16 w-16 rounded-full bg-red-100 dark:bg-red-900/20 mb-4 animate-pulse">
					<svg class="h-8 w-8 text-red-600 dark:text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
						<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"/>
					</svg>
				</div>

				<AlertDialog.Title class="text-lg font-semibold text-gray-900 dark:text-white mb-2 animate-fade-in">
					Something went wrong
				</AlertDialog.Title>
				<AlertDialog.Description class="text-sm text-gray-600 dark:text-gray-400 mb-6 animate-fade-in" style="animation-delay: 100ms">
					We apologize for the inconvenience. Please try again in a moment.
				</AlertDialog.Description>

        {#if errors && errors.length > 0}
          <div class="space-y-3 animate-fade-in" style="animation-delay: 200ms">
            {#each errors as err, i (i)}
              <div class="p-3 bg-gray-50 dark:bg-gray-700/50 rounded-lg text-left">
                {#if hasValue(err.code) || hasValue(err.description)}
                  {#if hasValue(err.code)}
                    <div class="mb-2">
                      <div class="text-xxs text-gray-400 dark:text-gray-400 font-semibold">Code</div>
                      <div class="text-xs text-gray-700 dark:text-gray-200 font-mono wrap-break-word mt-1">{err.code}</div>
                    </div>
                  {/if}

                  {#if hasValue(err.description)}
                    <div>
                      <div class="text-xxs text-gray-400 dark:text-gray-400 font-semibold">Description</div>
                      <div class="text-xs text-gray-700 dark:text-gray-200 font-mono whitespace-pre-wrap wrap-break-word mt-1">{err.description}</div>
                    </div>
                  {/if}
                {:else}
                  <div class="text-xs text-gray-500 dark:text-gray-300 font-mono">Unknown Error</div>
                {/if}
              </div>
            {/each}
          </div>
        {/if}
        <AlertDialog.Cancel class="mt-6 inline-flex h-9 items-center justify-center rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700">
          Close
        </AlertDialog.Cancel>
      </div>
		</AlertDialog.Content>
	</AlertDialog.Portal>
</AlertDialog.Root>