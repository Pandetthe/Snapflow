<script lang="ts">
  import { AppDialog, Button, InputTextField, UserAvatar } from '$lib/ui/components';
  import type { GetBoardByIdResponse } from '$lib/features/boards/types/boards.api';
  import type { Response } from '$lib/core/types/app';
  import { createForm } from '$lib/ui/utils';
  import { getContext, untrack } from 'svelte';
  import { Check, Pencil } from 'lucide-svelte';
  import { tagChipClass } from '$lib/features/boards/tagColors';
  import MarkdownView from './MarkdownView.svelte';
  import TagChip from './TagChip.svelte';

  // Must match CardOptions.MaxDescriptionLength on the server.
  const MAX_DESCRIPTION_LENGTH = 10000;

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
  const getBoardState = getContext<() => string>('boardState');
  const boardState = $derived(getBoardState());
  const getCanManageCards = getContext<() => boolean>('canManageCards');
  const canManageCards = $derived(getCanManageCards());
  const getCanAssignTags = getContext<() => boolean>('canAssignTags');
  const canAssignTags = $derived(getCanAssignTags());

  // An existing card opens for reading; whoever may change it goes on to edit from there.
  let mode = $state<'view' | 'edit'>('view');
  // Bumped each time editing starts, so the editor mounts again with the values it should load.
  let editSession = $state(0);

  const viewing = $derived(mode === 'view' && card !== undefined);

  const cardTags = $derived(
    (card?.tagIds ?? [])
      .map((id) => boardTags.find((t) => t.id === id))
      .filter((t) => t !== undefined)
  );

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

      if (values.description.length > MAX_DESCRIPTION_LENGTH) {
        errors.description = `Description must be at most ${MAX_DESCRIPTION_LENGTH} characters`;
      }

      return errors;
    },
    onSubmit: async (values) => {
      return onConfirm(values.title.trim(), values.description.trim(), selectedTagIds);
    },
    onSuccess: () => {
      if (card) {
        mode = 'view';
      } else {
        open = false;
      }
    }
  });

  function startEditing() {
    form.reset({
      title: card?.title ?? '',
      description: card?.description ?? ''
    });
    selectedTagIds = [...(card?.tagIds ?? [])];
    editSession++;
    mode = 'edit';
  }

  function stopEditing() {
    if (card) {
      mode = 'view';
    } else {
      open = false;
    }
  }

  // Set up on open only. Clearing on close would empty the dialog while it animates away, and reading the card
  // here would restart editing whenever someone else changes it.
  $effect(() => {
    if (!open) return;
    untrack(() => {
      if (card) {
        mode = 'view';
      } else {
        startEditing();
      }
    });
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

  function formatDate(value: string) {
    return new Date(value).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' });
  }
</script>

<AppDialog
  bind:open
  size="xl"
  sizeClass="max-w-3xl"
  {desktopMode}
  {mobileMode}
  {mobileDrawerSide}
  {desktopPlacement}
  {mobilePlacement}
  {desktopAnimation}
  {mobileAnimation}
  {triggerElement}
  title={viewing && card ? card.title : card ? 'Edit card' : 'Create card'}
  onsubmit={viewing ? undefined : form.handleSubmit}
>
  {#if viewing && card}
    <!-- The card's content, with who made and changed it in a panel beside it (under it on narrow screens) -->
    <div class="flex flex-col gap-5 md:flex-row md:gap-6">
      <div class="min-w-0 flex-1 space-y-5">
        {#if cardTags.length > 0}
          <div class="flex flex-wrap gap-1.5">
            {#each cardTags as tag (tag.id)}
              <TagChip {tag} />
            {/each}
          </div>
        {/if}

        {#if card.description.trim()}
          <MarkdownView markdown={card.description} />
        {:else}
          <p class="text-sm text-gray-400 italic dark:text-gray-500">No description</p>
        {/if}
      </div>

      <aside
        aria-label="Card details"
        class="shrink-0 border-t border-gray-200 pt-4 md:w-52 md:border-t-0 md:border-l md:pt-0 md:pl-6 dark:border-gray-800"
      >
        <dl class="space-y-4 text-xs">
          <div>
            <dt class="font-medium text-gray-500 dark:text-gray-400">Created</dt>
            <dd class="mt-1.5 flex min-w-0 items-center gap-2 text-gray-800 dark:text-gray-200">
              <UserAvatar src={card.createdBy.avatarUrl} name={card.createdBy.userName} size={20} />
              <span class="truncate">{card.createdBy.userName}</span>
            </dd>
            <dd class="mt-1 text-gray-500 tabular-nums dark:text-gray-400">
              <time datetime={card.createdAt}>{formatDate(card.createdAt)}</time>
            </dd>
          </div>

          {#if card.updatedAt}
            <div>
              <dt class="font-medium text-gray-500 dark:text-gray-400">Updated</dt>
              {#if card.updatedBy}
                <dd class="mt-1.5 flex min-w-0 items-center gap-2 text-gray-800 dark:text-gray-200">
                  <UserAvatar
                    src={card.updatedBy.avatarUrl}
                    name={card.updatedBy.userName}
                    size={20}
                  />
                  <span class="truncate">{card.updatedBy.userName}</span>
                </dd>
              {/if}
              <dd class="mt-1 text-gray-500 tabular-nums dark:text-gray-400">
                <time datetime={card.updatedAt}>{formatDate(card.updatedAt)}</time>
              </dd>
            </div>
          {/if}
        </dl>
      </aside>
    </div>
  {:else}
    <div class="space-y-4">
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

      <!-- Loaded when editing starts: the editor brings the emoji list, which the board itself never needs -->
      {#key editSession}
        {#await import('./MarkdownEditor.svelte') then { default: MarkdownEditor }}
          <MarkdownEditor
            label="Description"
            placeholder="Add more detail…"
            maxLength={MAX_DESCRIPTION_LENGTH}
            disabled={form.isSubmitting || isDeleting}
            bind:value={form.values.description}
            error={form.errors.description}
          />
        {/await}
      {/key}

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
    </div>
  {/if}

  {#snippet actions()}
    {#if viewing}
      <Button
        type="button"
        onclick={() => {
          open = false;
        }}
        variant="outline"
      >
        Close
      </Button>
      {#if canManageCards}
        <Button
          type="button"
          onclick={startEditing}
          startIcon={Pencil}
          disabled={boardState !== 'connected'}
        >
          Edit
        </Button>
      {/if}
    {:else}
      {#if card && onDelete}
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
        onclick={stopEditing}
        variant="outline"
        disabled={form.isSubmitting || isDeleting}
      >
        Cancel
      </Button>
      <Button
        type="submit"
        disabled={!form.values.title.trim() || form.isSubmitting || isDeleting}
        isLoading={form.isSubmitting}
        loadingText={card ? 'Saving' : 'Creating'}
      >
        {card ? 'Save changes' : 'Create'}
      </Button>
    {/if}
  {/snippet}
</AppDialog>
