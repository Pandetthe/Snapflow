<script lang="ts">
  import { Button, InputTextField } from '$lib/ui/components';
  import { errorStore } from '$lib/ui/stores/error.svelte';
  import { TagsService } from '$lib/features/boards/api/tags.api';
  import { apiClient } from '$lib/core/api.client';
  import {
    TAG_COLORS,
    type GetBoardByIdResponse,
    type TagColor
  } from '$lib/features/boards/types/boards.api';
  import { tagChipClass, tagSwatchClass } from '$lib/features/boards/tagColors';
  import { slideReveal } from '$lib/ui/utils';
  import type { Response } from '$lib/core/types/app';
  import { slide } from 'svelte/transition';
  import { Check, Pencil, Plus, Tags as TagsIcon, Trash2, X } from 'lucide-svelte';

  const MAX_TITLE_LENGTH = 20;

  let { boardId, tags = $bindable([]) }: { boardId: number; tags: GetBoardByIdResponse.TagDto[] } =
    $props();

  const tagsService = new TagsService(apiClient);

  let draftTitle = $state('');
  let draftColor = $state<TagColor>('blue');
  let isSaving = $state(false);

  // The id of the tag being renamed, or null while the form adds a new one.
  let editingId = $state<number | null>(null);
  let busyId = $state<number | null>(null);

  const trimmedTitle = $derived(draftTitle.trim());
  const canSubmit = $derived(
    trimmedTitle.length > 0 && trimmedTitle.length <= MAX_TITLE_LENGTH && !isSaving
  );

  function reportFailure(
    fallbackTitle: string,
    fallbackDetail: string,
    response: Response<unknown>
  ) {
    errorStore.addError(
      response.problem?.title ?? fallbackTitle,
      response.problem?.detail ?? fallbackDetail
    );
  }

  function startEdit(tag: GetBoardByIdResponse.TagDto) {
    editingId = tag.id;
    draftTitle = tag.title;
    draftColor = tag.color;
  }

  function cancelEdit() {
    editingId = null;
    draftTitle = '';
    draftColor = 'blue';
  }

  async function submit() {
    if (!canSubmit) return;

    isSaving = true;
    try {
      if (editingId !== null) {
        const id = editingId;
        const res = await tagsService.updateTag(boardId, id, {
          title: trimmedTitle,
          color: draftColor
        });
        if (!res.ok) {
          reportFailure('Web.UpdateTagFailed', 'Failed to update the tag', res);
          return;
        }
        tags = tags.map((t) =>
          t.id === id ? { ...t, title: trimmedTitle, color: draftColor } : t
        );
      } else {
        const res = await tagsService.createTag(boardId, {
          title: trimmedTitle,
          color: draftColor
        });
        if (!res.ok) {
          reportFailure('Web.CreateTagFailed', 'Failed to create the tag', res);
          return;
        }
        tags = [...tags, { id: res.value.id, title: trimmedTitle, color: draftColor }];
      }
      tags = [...tags].sort((a, b) => a.title.localeCompare(b.title));
      cancelEdit();
    } finally {
      isSaving = false;
    }
  }

  async function remove(tag: GetBoardByIdResponse.TagDto) {
    busyId = tag.id;
    try {
      const res = await tagsService.deleteTag(boardId, tag.id);
      if (!res.ok) {
        reportFailure('Web.DeleteTagFailed', 'Failed to delete the tag', res);
        return;
      }
      tags = tags.filter((t) => t.id !== tag.id);
      if (editingId === tag.id) cancelEdit();
    } finally {
      busyId = null;
    }
  }
</script>

<section
  class="rounded-2xl border border-gray-200 bg-white p-5 shadow-sm transition-all duration-200 sm:rounded-3xl sm:p-6 dark:border-gray-800 dark:bg-gray-900/50"
>
  <div class="mb-6 flex items-start justify-between">
    <div>
      <h2 class="text-lg font-bold text-gray-900 dark:text-white">Tags</h2>
      <p class="text-sm text-gray-600 dark:text-gray-400">
        {tags.length} tag{tags.length === 1 ? '' : 's'} on this board
      </p>
    </div>
    <TagsIcon class="h-5 w-5 text-gray-500" />
  </div>

  <div class="mb-6 space-y-3">
    <InputTextField
      id="tag-title"
      name="tag-title"
      label={editingId === null ? 'New tag' : 'Rename tag'}
      placeholder="e.g. Bug"
      maxlength={MAX_TITLE_LENGTH}
      bind:value={draftTitle}
      helperText={`${trimmedTitle.length}/${MAX_TITLE_LENGTH}`}
    />

    <div class="flex flex-wrap gap-1.5">
      {#each TAG_COLORS as color (color)}
        <button
          type="button"
          onclick={() => (draftColor = color)}
          aria-label={`Use the ${color} colour`}
          aria-pressed={draftColor === color}
          class="h-7 w-7 cursor-pointer rounded-full transition-transform focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-brand-500 {tagSwatchClass(
            color
          )} {draftColor === color
            ? 'ring-2 ring-gray-900 ring-offset-2 dark:ring-white dark:ring-offset-gray-900'
            : 'hover:scale-110'}"
        ></button>
      {/each}
    </div>

    <div class="flex gap-2">
      <Button
        type="button"
        variant="primary"
        size="sm"
        startIcon={editingId === null ? Plus : Check}
        disabled={!canSubmit}
        isLoading={isSaving}
        loadingText="Saving"
        onclick={submit}
      >
        {editingId === null ? 'Add tag' : 'Save tag'}
      </Button>
      {#if editingId !== null}
        <Button type="button" variant="outline" size="sm" onclick={cancelEdit}>Cancel</Button>
      {/if}
    </div>
  </div>

  {#if tags.length === 0}
    <p class="text-sm text-gray-500 dark:text-gray-400">
      No tags yet. Add one above and it becomes available on every card.
    </p>
  {:else}
    <ul class="space-y-2">
      {#each tags as tag (tag.id)}
        <li
          class="flex items-center justify-between gap-2 rounded-2xl border border-gray-100 bg-gray-50/50 p-2.5 dark:border-gray-800 dark:bg-white/2"
          transition:slide={slideReveal}
        >
          <span
            class="inline-flex min-w-0 items-center rounded-full px-2 py-0.5 text-xs font-medium {tagChipClass(
              tag.color
            )}"
          >
            <span class="truncate">{tag.title}</span>
          </span>

          <div class="flex shrink-0 items-center gap-1">
            <Button
              variant="ghost"
              size="xs"
              class="h-8 w-8 rounded-full p-0 text-gray-400 hover:text-gray-700 dark:hover:text-gray-200"
              onclick={() => (editingId === tag.id ? cancelEdit() : startEdit(tag))}
              aria-label={editingId === tag.id ? 'Stop editing tag' : `Edit tag ${tag.title}`}
              haptic="light"
            >
              {#if editingId === tag.id}
                <X size={16} />
              {:else}
                <Pencil size={16} />
              {/if}
            </Button>
            <Button
              variant="ghost"
              size="xs"
              class="h-8 w-8 rounded-full p-0 text-gray-400 hover:text-error-600 dark:hover:text-error-400"
              onclick={() => remove(tag)}
              disabled={busyId === tag.id}
              aria-label={`Delete tag ${tag.title}`}
              haptic="light"
            >
              <Trash2 size={16} />
            </Button>
          </div>
        </li>
      {/each}
    </ul>
  {/if}
</section>
