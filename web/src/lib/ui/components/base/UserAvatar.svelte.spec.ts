import { page } from 'vitest/browser';
import { describe, expect, it } from 'vitest';
import { render } from 'vitest-browser-svelte';
import UserAvatar from './UserAvatar.svelte';

const IMAGE =
  'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=';
const BROKEN = 'data:image/png;base64,AAAA';

const skeletonIn = (root: Element) => root.querySelector('.absolute.inset-0');
const imageIn = (root: Element) => root.querySelector('img');

describe('UserAvatar', () => {
  it('shows the image once it has loaded and removes the skeleton', async () => {
    const { container } = await render(UserAvatar, { props: { src: IMAGE, name: 'Ann Lee' } });

    await expect.poll(() => imageIn(container)?.classList.contains('opacity-0')).toBe(false);
    await expect.poll(() => skeletonIn(container)).toBeNull();
  });

  it('shows an avatar it has loaded before without the skeleton', async () => {
    const first = await render(UserAvatar, { props: { src: IMAGE, name: 'Ann Lee' } });
    await expect.poll(() => skeletonIn(first.container)).toBeNull();

    const { container } = await render(UserAvatar, { props: { src: IMAGE, name: 'Ann Lee' } });

    expect(skeletonIn(container)).toBeNull();
    expect(imageIn(container)?.classList.contains('opacity-0')).toBe(false);
  });

  it('falls back to the initials when the image is broken', async () => {
    const { container } = await render(UserAvatar, { props: { src: BROKEN, name: 'Ann Lee' } });

    await expect.element(page.getByText('AL')).toBeInTheDocument();
    await expect.poll(() => skeletonIn(container)).toBeNull();
  });

  it('shows the initials at once when there is no image', async () => {
    const { container } = await render(UserAvatar, { props: { src: null, name: 'Ann Lee' } });

    await expect.element(page.getByText('AL')).toBeInTheDocument();
    expect(skeletonIn(container)).toBeNull();
  });
});
