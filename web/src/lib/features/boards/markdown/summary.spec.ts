import { describe, expect, it } from 'vitest';
import { summarizeDescription } from './summary';

describe('summarizeDescription', () => {
  it('returns nothing for an empty description', () => {
    expect(summarizeDescription('  ')).toEqual({ text: '', tasksDone: 0, tasksTotal: 0 });
  });

  it('takes out the markdown syntax and joins the blocks into one line', () => {
    expect(
      summarizeDescription('# Plan\n\nShip **the** [docs](https://x.dev)\nnext `step`').text
    ).toBe('Plan Ship the docs next step');
  });

  it('reads plain text written before descriptions were markdown as it was typed', () => {
    expect(summarizeDescription('a < b & c\nsnake_case').text).toBe('a < b & c snake_case');
  });

  it('counts checklist items, nested ones included', () => {
    expect(summarizeDescription('- [x] one\n  - [ ] two\n- [x] three')).toMatchObject({
      tasksDone: 2,
      tasksTotal: 3
    });
  });
});
