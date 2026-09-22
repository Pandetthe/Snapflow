<script lang="ts">
  import type { GetBoardByIdResponse } from '$lib/features/boards/types/boards.api';
  import { tagChipClass } from '$lib/features/boards/tagColors';
  import { X } from '@lucide/svelte';

  let {
    tag,
    size = 'sm',
    onRemove = undefined
  }: {
    tag: GetBoardByIdResponse.TagDto;
    size?: 'xs' | 'sm';
    onRemove?: () => void;
  } = $props();
</script>

<span
  class="inline-flex max-w-full items-center gap-1 rounded-full font-medium {tagChipClass(
    tag.color
  )} {size === 'xs' ? 'px-1.5 py-px text-[10px]' : 'px-2 py-0.5 text-xs'}"
>
  <span class="truncate">{tag.title}</span>
  {#if onRemove}
    <button
      type="button"
      class="-mr-0.5 shrink-0 cursor-pointer rounded-full opacity-60 transition-opacity hover:opacity-100 focus-visible:opacity-100 focus-visible:outline-2 focus-visible:outline-offset-1 focus-visible:outline-current"
      onclick={onRemove}
      aria-label={`Remove tag ${tag.title}`}
    >
      <X class={size === 'xs' ? 'h-2.5 w-2.5' : 'h-3 w-3'} />
    </button>
  {/if}
</span>
