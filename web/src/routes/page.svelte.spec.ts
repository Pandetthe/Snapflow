import { page } from 'vitest/browser';
import { describe, expect, it, vi } from 'vitest';
import { render } from 'vitest-browser-svelte';
import Page from './+page.svelte';

vi.mock('$env/dynamic/public', () => ({
  env: {
    PUBLIC_API_BASE_URL: 'http://localhost:5000'
  }
}));

const guestData = {
  isAuthenticated: false,
  user: null,
  boards: null,
  publicBoards: [{ id: 3, title: 'Open roadmap', description: '' }],
  refreshTime: '2026-01-01T00:00:00Z'
};

const signedInData = {
  isAuthenticated: true,
  user: { id: 1, userName: 'jane', email: 'jane@example.com', avatarUrl: null },
  boards: [
    {
      id: 7,
      title: 'Roadmap',
      description: '',
      createdAt: '2026-01-01T00:00:00Z',
      createdBy: { id: 1, userName: 'jane' },
      updatedAt: null,
      updatedBy: null,
      yourRole: 'owner'
    }
  ],
  publicBoards: [],
  refreshTime: '2026-01-01T00:00:00Z'
};

describe('/+page.svelte', () => {
  it('should render intro heading for guests', async () => {
    render(Page, { props: { data: guestData } as never });

    const heading = page.getByRole('heading', {
      level: 1,
      name: 'Plan work together and see every change live'
    });
    await expect.element(heading).toBeInTheDocument();
  });

  it('should render reconstruction notice for guests', async () => {
    render(Page, { props: { data: guestData } as never });

    const notice = page.getByRole('heading', { level: 2, name: 'Under reconstruction' });
    await expect.element(notice).toBeInTheDocument();
  });

  it('should list public boards for guests', async () => {
    render(Page, { props: { data: guestData } as never });

    await expect.element(page.getByRole('heading', { name: 'Public boards' })).toBeInTheDocument();
    await expect.element(page.getByRole('heading', { name: 'Open roadmap' })).toBeInTheDocument();
  });

  it('should render boards instead of the intro when signed in', async () => {
    render(Page, { props: { data: signedInData } as never });

    await expect
      .element(page.getByRole('heading', { level: 1, name: 'Hi jane!' }))
      .toBeInTheDocument();
    await expect.element(page.getByRole('heading', { name: 'Roadmap' })).toBeInTheDocument();
    await expect
      .element(page.getByRole('heading', { level: 2, name: 'Under reconstruction' }))
      .not.toBeInTheDocument();
  });
});
