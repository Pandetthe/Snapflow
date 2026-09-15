import type { JSONContent } from '@tiptap/core';
import { MarkdownManager } from '@tiptap/markdown';
import { descriptionSchema, MARKED_OPTIONS } from './extensions';

let manager: MarkdownManager | undefined;

/** Reads a stored description into a Tiptap document. Needs no DOM, so it also runs on the server. */
export function parseDescription(markdown: string): JSONContent {
  manager ??= new MarkdownManager({
    extensions: descriptionSchema(),
    markedOptions: MARKED_OPTIONS
  });
  return manager.parse(markdown);
}
