import { MarkdownManager } from '@tiptap/markdown';
import { describe, expect, it } from 'vitest';
import { descriptionSchema, MARKED_OPTIONS } from './extensions';
import { renderDescriptionHtml } from './render';

describe('renderDescriptionHtml', () => {
  it('renders nothing for an empty description', () => {
    expect(renderDescriptionHtml(' \n ')).toBe('');
  });

  it('renders markdown formatting, checklists included', () => {
    const html = renderDescriptionHtml('## Plan\n\n**bold** line\nnext\n\n- [x] done');
    expect(html).toContain('<h2>Plan</h2>');
    expect(html).toContain('<strong>bold</strong> line<br/>next');
    expect(html).toContain('data-checked="true"');
  });

  it('shows raw HTML as text', () => {
    const html = renderDescriptionHtml('<img src=x onerror=alert(1)> <script>alert(1)</script>');
    expect(html).not.toContain('<img');
    expect(html).not.toContain('<script');
  });

  it('keeps web links and drops the address of script links', () => {
    const html = renderDescriptionHtml('[ok](https://example.com) [bad](javascript:alert(1))');
    expect(html).toContain('href="https://example.com"');
    expect(html).not.toContain('javascript:');
    expect(html).toContain('rel="noopener noreferrer nofollow"');
  });

  it('renders images from web addresses only', () => {
    const html = renderDescriptionHtml(
      '![cat](https://example.com/cat.png)\n\n![bad](javascript:alert(1))'
    );
    expect(html).toContain('src="https://example.com/cat.png"');
    expect(html).not.toContain('javascript:');
  });

  it('plays a video saved as @[youtube](url) from youtube-nocookie.com', () => {
    const html = renderDescriptionHtml('@[youtube](https://www.youtube.com/watch?v=dQw4w9WgXcQ)');
    expect(html).toContain('<iframe');
    expect(html).toContain('https://www.youtube-nocookie.com/embed/dQw4w9WgXcQ');
  });

  it('renders GitHub tables', () => {
    const html = renderDescriptionHtml('| Task | Owner |\n| --- | --- |\n| Ship | Ann |');
    expect(html).toContain('<th');
    expect(html).toContain('Ship');
  });
});

describe('description markdown', () => {
  const manager = new MarkdownManager({
    extensions: descriptionSchema(),
    markedOptions: MARKED_OPTIONS
  });

  it.each([
    ['a YouTube video', '@[youtube](https://www.youtube.com/watch?v=dQw4w9WgXcQ)'],
    ['an image', '![cat](https://example.com/cat.png)'],
    // Tables are saved with their columns padded to line up; one written that way stays as it is.
    ['a table', '| Task | Owner |\n| ---- | ----- |\n| Ship | Ann   |']
  ])('saves %s as it was read', (_name, markdown) => {
    expect(manager.serialize(manager.parse(markdown)).trim()).toBe(markdown);
  });
});
