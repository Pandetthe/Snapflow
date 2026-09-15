import type { JSONContent } from '@tiptap/core';
import { parseDescription } from './parse';

export interface DescriptionSummary {
  /** The description as one line of plain text, with the markdown syntax taken out. */
  text: string;
  tasksDone: number;
  tasksTotal: number;
}

/** What a card on the board shows of its description: a text excerpt and the checklist progress. */
export function summarizeDescription(markdown: string): DescriptionSummary {
  const summary: DescriptionSummary = { text: '', tasksDone: 0, tasksTotal: 0 };
  if (!markdown.trim()) return summary;

  const parts: string[] = [];
  const walk = (node: JSONContent) => {
    if (node.type === 'taskItem') {
      summary.tasksTotal++;
      if (node.attrs?.checked) summary.tasksDone++;
    }
    if (node.type === 'text' && node.text) parts.push(node.text);
    else if (node.type === 'hardBreak') parts.push(' ');
    node.content?.forEach(walk);
    if (node.type === 'paragraph' || node.type === 'heading' || node.type === 'codeBlock') {
      parts.push(' ');
    }
  };
  walk(parseDescription(markdown));

  summary.text = parts.join('').replace(/\s+/g, ' ').trim();
  return summary;
}
