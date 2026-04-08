<script lang="ts">
  import { Dialog } from 'bits-ui';
  import { Button, InputTextField, ResponsiveDialog } from '$lib/ui/components';
  import { slide } from 'svelte/transition';
  import type { GetBoardByIdResponse } from '$lib/features/boards/types/boards.api';
  import type { Response } from '$lib/core/types/app';
  import { createForm } from '$lib/ui/utils';

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
    desktopAnimation?: 'fade-zoom' | 'slide-up' | 'slide-down' | 'slide-left' | 'slide-right' | 'none';
    mobileAnimation?: 'fade-zoom' | 'slide-up' | 'slide-down' | 'slide-left' | 'slide-right' | 'none';
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

      if (values.width !== null && (values.width < 100 || values.width > 600)) {
        errors.width = 'Width must be between 100 and 600';
      }

      return errors;
    },
    onSubmit: async (values) => {
      return onConfirm(values.title.trim(), values.width);
    },
    onSuccess: () => {
      open = false;
    }
  });

  $effect(() => {
    if (open) {
      form.reset({
        title: list?.title ?? '',
        width: list?.width ?? null
      });
    } else {
      form.reset();
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
        {list ? 'Edit List' : 'Create List'}
      </Dialog.Title>
      <form onsubmit={form.handleSubmit} novalidate class="mt-4 space-y-6">
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
                class="rounded-md px-3 py-1 text-xs font-medium transition-all {form.values.width === null
                  ? 'bg-white text-gray-900 shadow-sm dark:bg-gray-700 dark:text-white'
                  : 'text-gray-500 hover:text-gray-700 dark:hover:text-gray-400'} focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none dark:focus-visible:ring-offset-gray-900"
              >
                Auto
              </button>
              <button
                type="button"
                onclick={() => (form.values.width = form.values.width || 300)}
                class="rounded-md px-3 py-1 text-xs font-medium transition-all {form.values.width !== null
                  ? 'bg-white text-gray-900 shadow-sm dark:bg-gray-700 dark:text-white'
                  : 'text-gray-500 hover:text-gray-700 dark:hover:text-gray-400'} focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none dark:focus-visible:ring-offset-gray-900"
              >
                Custom
              </button>
            </div>
          </div>

          {#if form.values.width !== null}
            <div transition:slide={{ duration: 200 }} class="space-y-4">
              <div class="flex items-center gap-4">
                <input
                  id="width"
                  type="range"
                  bind:value={form.values.width}
                  min="100"
                  max="600"
                  step="10"
                  class="h-1.5 flex-1 cursor-pointer appearance-none rounded-lg bg-gray-200 accent-gray-900 focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none dark:bg-gray-700 dark:accent-gray-50 dark:focus-visible:ring-offset-gray-900"
                />
                <div
                  class="flex items-center gap-1 rounded-md border border-gray-200 bg-white px-2 py-1 dark:border-gray-600 dark:bg-gray-700"
                >
                  <input
                    type="number"
                    bind:value={form.values.width}
                    min="100"
                    class="w-12 border-none bg-transparent p-0 text-right text-sm font-medium focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:ring-offset-white focus-visible:outline-none dark:text-white dark:focus-visible:ring-offset-gray-900"
                  />
                  <span class="text-xs text-gray-400">px</span>
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

        <div class="mt-6 flex flex-col-reverse gap-3 sm:flex-row sm:justify-end">
          {#if list && onDelete}
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
            loadingText={list ? 'Saving' : 'Creating'}
            class="w-full sm:min-w-32"
          >
            {list ? 'Save Changes' : 'Create'}
          </Button>
        </div>
      </form>
    </ResponsiveDialog>
