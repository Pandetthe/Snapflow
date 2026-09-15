<script lang="ts">
  import { cn } from '$lib/ui/utils';
  import { renderDescriptionHtml } from '$lib/features/boards/markdown/render';
  import '$lib/features/boards/styles/markdown.css';

  /** A card description rendered read-only, the way the editor shows it, without an editor instance. */
  let { markdown, class: className }: { markdown: string; class?: string } = $props();

  const html = $derived(renderDescriptionHtml(markdown));
</script>

<div class={cn('markdown-content markdown-view', className)}>
  <!-- eslint-disable-next-line svelte/no-at-html-tags -- Tiptap's static renderer escapes text and emits only schema nodes (markdown/render.ts) -->
  {@html html}
</div>
