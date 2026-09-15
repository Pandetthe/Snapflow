import { renderToHTMLString } from '@tiptap/static-renderer/pm/html-string';
import { descriptionSchema } from './extensions';
import { parseDescription } from './parse';

let extensions: ReturnType<typeof descriptionSchema> | undefined;

/**
 * A stored description as HTML, rendered without an editor (Tiptap's static renderer), on the server too.
 * The output holds only what the schema defines: text is escaped, raw HTML in the markdown comes out as text
 * and links with a scheme other than a web or mail one lose their address.
 */
export function renderDescriptionHtml(markdown: string): string {
  if (!markdown.trim()) return '';
  extensions ??= descriptionSchema({ openLinksOnClick: true });
  return renderToHTMLString({ extensions, content: parseDescription(markdown) });
}
