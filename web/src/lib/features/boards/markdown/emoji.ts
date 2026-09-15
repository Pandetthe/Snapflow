import Emoji, { gitHubEmojis, shortcodeToEmoji, type EmojiItem } from '@tiptap/extension-emoji';
import type { SuggestionOptions } from '@tiptap/suggestion';

const MAX_SUGGESTIONS = 8;

// GitHub's custom emoji are images without a character, which markdown could not keep.
const EMOJIS = gitHubEmojis.filter((item) => item.emoji);

/** Emoji whose shortcode starts with the query come first, then those whose shortcode or tags contain it. */
export function searchEmoji(query: string): EmojiItem[] {
  const q = query.trim().toLowerCase();
  if (!q) return EMOJIS.slice(0, MAX_SUGGESTIONS);

  const starting: EmojiItem[] = [];
  const containing: EmojiItem[] = [];
  for (const item of EMOJIS) {
    if (item.shortcodes.some((s) => s.startsWith(q))) {
      starting.push(item);
      if (starting.length === MAX_SUGGESTIONS) break;
    } else if (
      item.shortcodes.some((s) => s.includes(q)) ||
      item.tags.some((t) => t.startsWith(q))
    ) {
      containing.push(item);
    }
  }
  return [...starting, ...containing].slice(0, MAX_SUGGESTIONS);
}

/**
 * Emoji for the description editor only: ":" opens suggestions, and ":smile:" or ":)" typed out become one.
 * An emoji is saved as its character, so the stored markdown, and the board that never loads this list, show it as is.
 */
export function descriptionEmoji(render: SuggestionOptions<EmojiItem>['render']) {
  return Emoji.extend({
    renderMarkdown: (node) => {
      const name = String(node.attrs?.name ?? '');
      return shortcodeToEmoji(name, EMOJIS)?.emoji ?? `:${name}:`;
    }
  }).configure({
    emojis: EMOJIS,
    enableEmoticons: true,
    suggestion: {
      items: ({ query }) => searchEmoji(query),
      // A character rather than the emoji node, like the markdown it is saved as.
      command: ({ editor, range, props }) => {
        editor.chain().focus().insertContentAt(range, `${props.emoji} `).run();
      },
      render
    }
  });
}
