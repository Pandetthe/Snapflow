<script lang="ts">
  import { Dialog } from 'bits-ui';
  import { Button, InputTextField, Textarea, ResponsiveDialog } from '$lib/ui/components';
  import { createForm } from '$lib/ui/utils';
  import { X } from 'lucide-svelte';
  import type { Response } from '$lib/core/types/api';
  import type { GetBoardsResponse } from '$lib/features/boards/types/boards.api';

  let {
    open = $bindable(false),
    board = $bindable(undefined),
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
    board?: GetBoardsResponse.BoardDto;
    desktopMode?: 'modal' | 'drawer';
    mobileMode?: 'modal' | 'drawer';
    desktopPlacement?: 'center' | 'trigger';
    mobilePlacement?: 'center' | 'trigger';
    desktopAnimation?: 'fade-zoom' | 'slide-up' | 'slide-down' | 'slide-left' | 'slide-right' | 'none';
    mobileAnimation?: 'fade-zoom' | 'slide-up' | 'slide-down' | 'slide-left' | 'slide-right' | 'none';
    mobileDrawerSide?: 'top' | 'right' | 'bottom' | 'left';
    triggerElement?: HTMLElement | null;
    onConfirm: (title: string, description: string) => Promise<Response<unknown>>;
    onDelete?: (id: number) => void;
  } = $props();

  const form = createForm({
    initialValues: {
      title: '',
      description: ''
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

      if (values.description.length > 500) {
        errors.description = 'Description must be less than 500 characters';
      }

      return errors;
    },
    onSubmit: async (values) => {
      return onConfirm(values.title.trim(), values.description.trim());
    },
    onSuccess: () => {
      open = false;
    }
  });

  $effect(() => {
    if (open) {
      form.reset({
        title: board?.title ?? '',
        description: board?.description ?? ''
      });
    } else {
      form.reset();
    }
  });
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
  contentClass="sm:w-full"
>
      <div class="mb-4 flex items-start justify-between gap-3">
        <div>
          <Dialog.Title class="text-xl font-semibold text-gray-900 dark:text-white">
            {board ? 'Edit board' : 'Create new board'}
          </Dialog.Title>
          <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
            {board
              ? 'Update board details for your team.'
              : 'Give it a name and optional description.'}
          </p>
        </div>
        <button
          type="button"
          aria-label="Close modal"
          class="rounded-lg p-2 text-gray-500 transition-colors hover:bg-gray-100 hover:text-gray-800 focus-visible:ring-2 focus-visible:ring-brand-500 focus-visible:ring-offset-2 focus-visible:outline-none dark:text-gray-400 dark:hover:bg-gray-800 dark:hover:text-gray-100 dark:focus-visible:ring-offset-gray-950"
          onclick={() => (open = false)}
        >
          <X size={18} />
        </button>
      </div>

      <form onsubmit={form.handleSubmit} novalidate class="space-y-4 sm:space-y-5">
        <InputTextField
          id="board-title"
          name="title"
          label="Board title"
          placeholder="e.g. Product roadmap"
          maxlength={100}
          required={true}
          bind:value={form.values.title}
          error={form.errors.title}
        />

        <Textarea
          id="board-description"
          name="description"
          label="Description"
          placeholder="Optional context for this board"
          maxlength={500}
          rows={4}
          bind:value={form.values.description}
          error={form.errors.description}
          helperText={`${form.values.description.length}/500`}
          class="resize-y"
        />

        <div class="flex flex-col-reverse gap-2 pt-1 sm:flex-row sm:justify-end">
          {#if board && onDelete}
            <Button
              type="button"
              variant="danger"
              class="sm:mr-auto"
              onclick={() => {
                onDelete(board.id);
                open = false;
              }}
            >
              Delete
            </Button>
          {/if}

          <Button
            type="button"
            variant="outline"
            onclick={() => {
              open = false;
            }}
            class="w-full sm:w-auto sm:min-w-32"
          >
            Cancel
          </Button>

          <Button
            type="submit"
            variant="primary"
            disabled={!form.values.title.trim() || form.isSubmitting}
            isLoading={form.isSubmitting}
            loadingText={board ? 'Saving' : 'Creating'}
            class="w-full sm:w-auto sm:min-w-32"
          >
            {board ? 'Save changes' : 'Create board'}
          </Button>
        </div>
      </form>
    </ResponsiveDialog>
