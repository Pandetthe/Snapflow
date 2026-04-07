<script lang="ts">
  import { Dialog } from 'bits-ui';
  import { Button, InputTextField, Textarea } from '$lib/ui/components';
  import { createForm } from '$lib/ui/utils';
  import { X } from 'lucide-svelte';
  import type { Response } from '$lib/core/types/api';
  import type { GetBoardsResponse } from '$lib/features/boards/types/boards.api';

  let {
    open = $bindable(false),
    board = $bindable(undefined),
    onConfirm,
    onDelete
  }: {
    open: boolean;
    board?: GetBoardsResponse.BoardDto;
    onConfirm: (title: string, description: string) => Promise<Response<any>>;
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

<Dialog.Root bind:open>
  <Dialog.Portal>
    <Dialog.Overlay
      class="fixed inset-0 z-50 bg-black/80 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:animate-in data-[state=open]:fade-in-0"
    />
    <Dialog.Content
      class="fixed top-[50%] left-[50%] z-50 max-h-[88vh] w-[calc(100%-1.25rem)] max-w-lg translate-x-[-50%] translate-y-[-50%] overflow-y-auto rounded-2xl border border-gray-200 bg-white p-5 shadow-2xl duration-200 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:zoom-out-95 data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=open]:zoom-in-95 sm:w-full sm:p-6 dark:border-gray-700 dark:bg-gray-900"
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
          >
            Cancel
          </Button>

          <Button
            type="submit"
            variant="primary"
            disabled={!form.values.title.trim() || form.isSubmitting}
            isLoading={form.isSubmitting}
            loadingText={board ? 'Saving' : 'Creating'}
          >
            {board ? 'Save changes' : 'Create board'}
          </Button>
        </div>
      </form>
    </Dialog.Content>
  </Dialog.Portal>
</Dialog.Root>
