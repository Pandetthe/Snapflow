import { mergeAttributes, type AnyExtension } from '@tiptap/core';
import Image from '@tiptap/extension-image';
import { TaskItem, TaskList } from '@tiptap/extension-list';
import { TableKit } from '@tiptap/extension-table';
import Youtube from '@tiptap/extension-youtube';
import { Markdown } from '@tiptap/markdown';
import StarterKit from '@tiptap/starter-kit';

/** GitHub-flavoured markdown, with a single line break kept as a break: that is what people typing in a card expect. */
export const MARKED_OPTIONS = { gfm: true, breaks: true };

/** Adds https:// to an address typed without a scheme; only web and mail links are accepted. */
export function normalizeLinkHref(input: string): string | null {
  const href = input.trim();
  if (!href) return null;
  const withScheme = /^[a-z][a-z\d+.-]*:/i.test(href) ? href : `https://${href}`;
  return /^(https?|mailto):/i.test(withScheme) ? withScheme : null;
}

/** Adds https:// to an address typed without a scheme; images and videos come from web addresses only. */
export function normalizeMediaUrl(input: string): string | null {
  const url = input.trim();
  if (!url) return null;
  const withScheme = /^[a-z][a-z\d+.-]*:/i.test(url) ? url : `https://${url}`;
  return /^https?:\/\//i.test(withScheme) ? withScheme : null;
}

/** An image from a web address; any other source (data:, javascript:) is left out when it renders. */
const DescriptionImage = Image.extend({
  renderHTML({ HTMLAttributes }) {
    const src = normalizeMediaUrl(String(HTMLAttributes.src ?? ''));
    return [
      'img',
      mergeAttributes(this.options.HTMLAttributes, HTMLAttributes, {
        src,
        loading: 'lazy',
        referrerpolicy: 'no-referrer'
      })
    ];
  }
});

// Markdown has no syntax for a video; this is the one markdown-it-video reads, and it stays readable as text.
const YOUTUBE_MARKDOWN = /^@\[youtube\]\(([^()\s]+)\)[ \t]*(?:\n|$)/;

/** A YouTube video, saved as @[youtube](url) on a line of its own and played from youtube-nocookie.com. */
const DescriptionYoutube = Youtube.extend({
  markdownTokenizer: {
    name: 'youtube',
    level: 'block',
    start: (src: string) => src.indexOf('@[youtube]('),
    tokenize: (src: string) => {
      const match = YOUTUBE_MARKDOWN.exec(src);
      if (!match) return undefined;
      return { type: 'youtube', raw: match[0], src: match[1] };
    }
  },
  parseMarkdown: (token, helpers) => helpers.createNode('youtube', { src: token.src }),
  renderMarkdown: (node) => (node.attrs?.src ? `@[youtube](${node.attrs.src})` : '')
});

/**
 * The nodes and marks a card description can hold, shared by the editor, the read-only view and the summary on the
 * board, so all three read a description the same way. Underline is left out, markdown has no syntax for it.
 */
export function descriptionSchema({ openLinksOnClick = false } = {}): AnyExtension[] {
  return [
    StarterKit.configure({
      underline: false,
      link: { openOnClick: openLinksOnClick, autolink: true, defaultProtocol: 'https' }
    }),
    TaskList,
    TaskItem.configure({ nested: true }),
    DescriptionImage.configure({ allowBase64: false }),
    TableKit.configure({ table: { resizable: false } }),
    DescriptionYoutube.configure({ nocookie: true, modestBranding: true, width: 640, height: 360 })
  ];
}

/** The schema plus markdown in and out, for editors that load and save a description. */
export function descriptionExtensions(options?: { openLinksOnClick?: boolean }): AnyExtension[] {
  return [...descriptionSchema(options), Markdown.configure({ markedOptions: MARKED_OPTIONS })];
}
