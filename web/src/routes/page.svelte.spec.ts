import { page } from 'vitest/browser';
import { describe, expect, it } from 'vitest';
import { render } from 'vitest-browser-svelte';
import Page from './+page.svelte';

describe('/+page.svelte', () => {
	it('should render main heading', async () => {
		render(Page, { props: { data: { isAuthenticated: false, user: null } } });

		const heading = page.getByRole('heading', { level: 1, name: 'Snapflow' });
		await expect.element(heading).toBeInTheDocument();
	});

	it('should render reconstruction warning', async () => {
		render(Page, { props: { data: { isAuthenticated: false, user: null } } });

		const warning = page.getByRole('heading', { level: 2, name: 'Under Reconstruction' });
		await expect.element(warning).toBeInTheDocument();
	});

	it('should render sign in buttons when not authenticated', async () => {
		render(Page, { props: { data: { isAuthenticated: false, user: null } } });

		const createAccountBtn = page.getByRole('link', { name: 'Create test account' });
		const signInBtn = page.getByRole('link', { name: 'Sign in' });

		await expect.element(createAccountBtn).toBeInTheDocument();
		await expect.element(signInBtn).toBeInTheDocument();
	});

	it('should render boards button when authenticated', async () => {
		render(Page, { props: { data: { isAuthenticated: true, user: null } } });

		const boardsBtn = page.getByRole('link', { name: 'Check my boards' });
		await expect.element(boardsBtn).toBeInTheDocument();
	});
});
