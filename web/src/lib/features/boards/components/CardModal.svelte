<script lang="ts">
  import { Dialog } from 'bits-ui';
  import { Button, InputTextField, Textarea, ResponsiveDialog } from '$lib/ui/components';
  import type { GetBoardByIdResponse } from '$lib/features/boards/types/boards.api';
  import type { Response } from '$lib/core/types/app';
  import { createForm } from '$lib/ui/utils';
  import { getContext } from 'svelte';
  import { Check } from 'lucide-svelte';
  import { tagChipClass } from '$lib/features/boards/tagColors';
  let {
    open = $bindable(false),
    card = $bindable(undefined),
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
    card?: GetBoardByIdResponse.CardDto;
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
    onConfirm: (title: string, description: string, tagIds: number[]) => Promise<Response<unknown>>;
    onDelete?: (id: number) => Promise<boolean>;
  } = $props();

  let isDeleting = $state(false);

  const getBoard = getContext<() => GetBoardByIdResponse.BoardDto>('board');
  const boardTags = $derived(getBoard().tags);
  const getCanAssignTags = getContext<() => boolean>('canAssignTags');
  const canAssignTags = $derived(getCanAssignTags());

  // Held here until the card is saved, so Cancel leaves the card's tags as they were.
  let selectedTagIds = $state<number[]>([]);

  function toggleTag(id: number) {
    selectedTagIds = selectedTagIds.includes(id)
      ? selectedTagIds.filter((tagId) => tagId !== id)
      : [...selectedTagIds, id];
  }

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
      } else if (values.title.trim().length > 50) {
        errors.title = 'Title must be less than 50 characters';
      }

      if (values.description.length > 1000) {
        errors.description = 'Description must be less than 1000 characters';
      }

      return errors;
    },
    onSubmit: async (values) => {
      return onConfirm(values.title.trim(), values.description.trim(), selectedTagIds);
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
        title: card?.title ?? '',
        description: card?.description ?? ''
      });
      selectedTagIds = [...(card?.tagIds ?? [])];
    }
  });

  async function handleDelete() {
    if (!card || !onDelete || isDeleting) {
      return;
    }

    isDeleting = true;
    try {
      const deleted = await onDelete(card.id);
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
    {card ? 'Edit Card' : 'Create Card'}
  </Dialog.Title>
  <form onsubmit={form.handleSubmit} novalidate class="mt-4 space-y-4">
    <InputTextField
      id="card-title"
      name="title"
      label="Title"
      placeholder="Card title"
      maxlength={50}
      required={true}
      bind:value={form.values.title}
      error={form.errors.title}
    />

    <Textarea
      id="card-description"
      name="description"
      label="Description"
      placeholder="Card description"
      maxlength={1000}
      rows={4}
      bind:value={form.values.description}
      error={form.errors.description}
      helperText={`${form.values.description.length}/1000`}
    />

    {#if canAssignTags && boardTags.length > 0}
      <div class="space-y-2">
        <span class="block text-sm font-medium text-gray-700 dark:text-gray-300">Tags</span>
        <div class="flex flex-wrap gap-1.5">
          {#each boardTags as tag (tag.id)}
            {@const selected = selectedTagIds.includes(tag.id)}
            <button
              type="button"
              onclick={() => toggleTag(tag.id)}
              aria-pressed={selected}
              class="inline-flex cursor-pointer items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium transition-opacity focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500 {tagChipClass(
                tag.color
              )} {selected ? '' : 'opacity-40 hover:opacity-70'}"
            >
              {#if selected}
                <Check class="h-3 w-3" />
              {/if}
              <span class="max-w-40 truncate">{tag.title}</span>
            </button>
          {/each}
        </div>
      </div>
    {/if}

    <div class="mt-6 flex flex-col-reverse gap-3 sm:flex-row sm:justify-end">
      {#if card && onDelete}
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
        loadingText={card ? 'Saving' : 'Creating'}
        class="w-full sm:min-w-32"
      >
        {card ? 'Save Changes' : 'Create'}
      </Button>
    </div>
  </form>
</ResponsiveDialog>
