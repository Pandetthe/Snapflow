<script lang="ts">
  import { Dialog } from 'bits-ui';
  import { Button, InputTextField, ResponsiveDialog } from '$lib/ui/components';
  import { slide } from 'svelte/transition';
  import type { GetBoardByIdResponse } from '$lib/features/boards/types/boards.api';
  import type { Response } from '$lib/core/types/app';
  import { createForm, slideReveal } from '$lib/ui/utils';
  import { fromRem, toRem } from '$lib/features/boards/sizes';

  const MIN_HEIGHT_REM = 6.25;
  const MAX_HEIGHT_REM = 75;
  const DEFAULT_HEIGHT_REM = 25;

  let {
    open = $bindable(false),
    swimlane = $bindable(undefined),
    desktopMode = 'modal',
    mobileMode = 'drawer',
    desktopPlacement = 'center',
    mobilePlacement = 'center',
    desktopAnimation = 'fade-zoom',
    mobileAnimation = 'slide-up',
    mobileDrawerSide = 'bottom',
    triggerElement = undefined,
    onConfirm,
    onDelete
  }: {
    open: boolean;
    swimlane?: GetBoardByIdResponse.SwimlaneDto;
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
    onConfirm: (title: string, height: number | null) => Promise<Response<unknown>>;
    onDelete?: (id: number) => Promise<boolean>;
  } = $props();

  let isDeleting = $state(false);

  const form = createForm({
    initialValues: {
      title: '',
      height: null as number | null
    },
    validate: (values) => {
      const errors: Record<string, string> = {};

      if (!values.title.trim()) {
        errors.title = 'Title is required';
      } else if (values.title.trim().length < 3) {
        errors.title = 'Title must be at least 3 characters';
      } else if (values.title.trim().length > 100) {
        errors.title = 'Title must be less than 100 characters';
      }

      if (
        values.height !== null &&
        (values.height < MIN_HEIGHT_REM || values.height > MAX_HEIGHT_REM)
      ) {
        errors.height = `Height must be between ${MIN_HEIGHT_REM} and ${MAX_HEIGHT_REM} rem`;
      }

      return errors;
    },
    onSubmit: async (values) => {
      return onConfirm(values.title.trim(), values.height === null ? null : fromRem(values.height));
    },
    onSuccess: () => {
      open = false;
    }
  });

  // Filled in on open only. Clearing on close would empty the dialog while it animates away,
  // and every open overwrites the form anyway.
  $effect(() => {
    if (open) {
      form.reset({
        title: swimlane?.title ?? '',
        height: swimlane?.height ? toRem(swimlane.height) : null
      });
    }
  });

  async function handleDelete() {
    if (!swimlane || !onDelete || isDeleting) {
      return;
    }

    isDeleting = true;
    try {
      const deleted = await onDelete(swimlane.id);
      if (deleted) {
        open = false;
      }
    } finally {
      isDeleting = false;
    }
  }
</script>

<ResponsiveDialog
  bind:open
  size="lg"
  {desktopMode}
  {mobileMode}
  {mobileDrawerSide}
  {desktopPlacement}
  {mobilePlacement}
  {desktopAnimation}
  {mobileAnimation}
  {triggerElement}
  contentClass="sm:rounded-lg md:w-full"
>
  <Dialog.Title
    class="text-lg leading-none font-semibold tracking-tight text-gray-900 dark:text-gray-100"
  >
    {swimlane ? 'Edit Swimlane' : 'Create Swimlane'}
  </Dialog.Title>
  <form onsubmit={form.handleSubmit} novalidate class="mt-4 space-y-4">
    <InputTextField
      id="swimlane-title"
      name="title"
      label="Title"
      placeholder="Swimlane title"
      maxlength={100}
      required={true}
      bind:value={form.values.title}
      error={form.errors.title}
    />
    <div
      class="space-y-4 rounded-xl border border-gray-100 bg-gray-50/50 p-4 dark:border-gray-700 dark:bg-gray-900/30"
    >
      <div class="flex items-center justify-between">
        <label for="height" class="text-sm font-semibold text-gray-700 dark:text-gray-300">
          Height Mode
        </label>
        <div class="flex gap-1 rounded-lg bg-gray-100 p-1 dark:bg-gray-800">
          <button
            type="button"
            onclick={() => (form.values.height = null)}
            class="rounded-md px-3 py-1 text-xs font-medium transition-all {form.values.height ===
            null
              ? 'bg-white text-gray-900 shadow-sm dark:bg-gray-700 dark:text-white'
              : 'text-gray-500 hover:text-gray-700 dark:hover:text-gray-400'} focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500"
          >
            Auto
          </button>
          <button
            type="button"
            onclick={() => (form.values.height = form.values.height || DEFAULT_HEIGHT_REM)}
            class="rounded-md px-3 py-1 text-xs font-medium transition-all {form.values.height !==
            null
              ? 'bg-white text-gray-900 shadow-sm dark:bg-gray-700 dark:text-white'
              : 'text-gray-500 hover:text-gray-700 dark:hover:text-gray-400'} focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500"
          >
            Custom
          </button>
        </div>
      </div>

      {#if form.values.height !== null}
        <div transition:slide={slideReveal} class="space-y-4">
          <div class="flex items-center gap-4">
            <input
              id="height"
              type="range"
              bind:value={form.values.height}
              min={MIN_HEIGHT_REM}
              max={MAX_HEIGHT_REM}
              step="0.25"
              class="h-1.5 flex-1 cursor-pointer appearance-none rounded-lg bg-gray-200 accent-gray-900 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500 dark:bg-gray-700 dark:accent-gray-50"
            />
            <div
              class="flex items-center gap-1 rounded-md border border-gray-200 bg-white px-2 py-1 dark:border-gray-600 dark:bg-gray-700"
            >
              <input
                type="number"
                bind:value={form.values.height}
                min={MIN_HEIGHT_REM}
                max={MAX_HEIGHT_REM}
                step="0.25"
                class="w-14 border-none bg-transparent p-0 text-right text-sm font-medium focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500 dark:text-white"
              />
              <span class="text-xs text-gray-400">rem</span>
            </div>
          </div>
          {#if form.errors.height}
            <p class="text-sm text-error-600 dark:text-error-400">{form.errors.height}</p>
          {/if}
          <p class="text-[0.8rem] text-gray-500 dark:text-gray-400">
            Adjust the vertical space for this swimlane.
          </p>
        </div>
      {/if}
    </div>
    <div class="mt-6 flex flex-col-reverse gap-3 sm:flex-row sm:justify-end">
      {#if swimlane && onDelete}
        <Button
          type="button"
          onclick={handleDelete}
          variant="danger"
          disabled={form.isSubmitting || isDeleting}
          isLoading={isDeleting}
          loadingText="Deleting"
          class="w-full sm:mr-auto sm:min-w-32"
        >
          Delete
        </Button>
      {/if}
      <Button
        type="button"
        onclick={() => {
          open = false;
        }}
        variant="outline"
        disabled={form.isSubmitting || isDeleting}
        class="w-full sm:min-w-32"
      >
        Cancel
      </Button>
      <Button
        type="submit"
        variant="primary"
        disabled={!form.values.title.trim() || form.isSubmitting || isDeleting}
        isLoading={form.isSubmitting}
        loadingText={swimlane ? 'Saving' : 'Creating'}
        class="w-full sm:min-w-32"
      >
        {swimlane ? 'Save Changes' : 'Create'}
      </Button>
    </div>
  </form>
</ResponsiveDialog>
