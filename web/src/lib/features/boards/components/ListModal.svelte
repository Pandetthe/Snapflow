<script lang="ts">
  import { AppDialog, Button, InputTextField } from '$lib/ui/components';
  import { slide } from 'svelte/transition';
  import type { GetBoardByIdResponse } from '$lib/features/boards/types/boards.api';
  import type { Response } from '$lib/core/types/app';
  import { createForm, slideReveal } from '$lib/ui/utils';
  import { fromRem, toRem } from '$lib/features/boards/sizes';

  const MIN_WIDTH_REM = 6.25;
  const MAX_WIDTH_REM = 37.5;
  const DEFAULT_WIDTH_REM = 18.75;

  let {
    open = $bindable(false),
    list = $bindable(undefined),
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
    list?: GetBoardByIdResponse.ListDto;
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
    onConfirm: (title: string, width: number | null) => Promise<Response<unknown>>;
    onDelete?: (id: number) => Promise<boolean>;
  } = $props();

  let isDeleting = $state(false);

  const form = createForm({
    initialValues: {
      title: '',
      width: null as number | null
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

      if (values.width !== null && (values.width < MIN_WIDTH_REM || values.width > MAX_WIDTH_REM)) {
        errors.width = `Width must be between ${MIN_WIDTH_REM} and ${MAX_WIDTH_REM} rem`;
      }

      return errors;
    },
    onSubmit: async (values) => {
      return onConfirm(values.title.trim(), values.width === null ? null : fromRem(values.width));
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
        title: list?.title ?? '',
        width: list?.width ? toRem(list.width) : null
      });
    }
  });

  async function handleDelete() {
    if (!list || !onDelete || isDeleting) {
      return;
    }

    isDeleting = true;
    try {
      const deleted = await onDelete(list.id);
      if (deleted) {
        open = false;
      }
    } finally {
      isDeleting = false;
    }
  }
</script>

<AppDialog
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
  title={list ? 'Edit list' : 'Create list'}
  onsubmit={form.handleSubmit}
>
  <div class="space-y-6">
    <InputTextField
      id="list-title"
      name="title"
      label="Title"
      placeholder="List title"
      maxlength={100}
      required={true}
      bind:value={form.values.title}
      error={form.errors.title}
    />

    <div
      class="space-y-4 rounded-xl border border-gray-100 bg-gray-50/50 p-4 dark:border-gray-700 dark:bg-gray-900/30"
    >
      <div class="flex items-center justify-between">
        <label for="width" class="text-sm font-semibold text-gray-700 dark:text-gray-300">
          Width Mode
        </label>
        <div class="flex gap-1 rounded-lg bg-gray-100 p-1 dark:bg-gray-800">
          <button
            type="button"
            onclick={() => (form.values.width = null)}
            class="rounded-md px-3 py-1 text-xs font-medium transition-all {form.values.width ===
            null
              ? 'bg-white text-gray-900 shadow-sm dark:bg-gray-700 dark:text-white'
              : 'text-gray-500 hover:text-gray-700 dark:hover:text-gray-400'} focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500"
          >
            Auto
          </button>
          <button
            type="button"
            onclick={() => (form.values.width = form.values.width || DEFAULT_WIDTH_REM)}
            class="rounded-md px-3 py-1 text-xs font-medium transition-all {form.values.width !==
            null
              ? 'bg-white text-gray-900 shadow-sm dark:bg-gray-700 dark:text-white'
              : 'text-gray-500 hover:text-gray-700 dark:hover:text-gray-400'} focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500"
          >
            Custom
          </button>
        </div>
      </div>

      {#if form.values.width !== null}
        <div transition:slide={slideReveal} class="space-y-4">
          <div class="flex items-center gap-4">
            <input
              id="width"
              type="range"
              bind:value={form.values.width}
              min={MIN_WIDTH_REM}
              max={MAX_WIDTH_REM}
              step="0.25"
              class="h-1.5 flex-1 cursor-pointer appearance-none rounded-lg bg-gray-200 accent-gray-900 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500 dark:bg-gray-700 dark:accent-gray-50"
            />
            <div
              class="flex items-center gap-1 rounded-md border border-gray-200 bg-white px-2 py-1 dark:border-gray-600 dark:bg-gray-700"
            >
              <input
                type="number"
                bind:value={form.values.width}
                min={MIN_WIDTH_REM}
                max={MAX_WIDTH_REM}
                step="0.25"
                class="w-14 border-none bg-transparent p-0 text-right text-sm font-medium focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500 dark:text-white"
              />
              <span class="text-xs text-gray-400">rem</span>
            </div>
          </div>
          {#if form.errors.width}
            <p class="text-sm text-error-600 dark:text-error-400">{form.errors.width}</p>
          {/if}
          <p class="text-[0.8rem] text-gray-500 dark:text-gray-400">
            Adjust the width of this list.
          </p>
        </div>
      {/if}
    </div>
  </div>

  {#snippet actions()}
    {#if list && onDelete}
      <Button
        type="button"
        onclick={handleDelete}
        variant="danger"
        disabled={form.isSubmitting || isDeleting}
        isLoading={isDeleting}
        loadingText="Deleting"
        class="sm:mr-auto"
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
    >
      Cancel
    </Button>
    <Button
      type="submit"
      disabled={!form.values.title.trim() || form.isSubmitting || isDeleting}
      isLoading={form.isSubmitting}
      loadingText={list ? 'Saving' : 'Creating'}
    >
      {list ? 'Save changes' : 'Create'}
    </Button>
  {/snippet}
</AppDialog>
