import { test, expect } from '@playwright/test';

test('home page has expected title and heading', async ({ page }) => {
	await page.goto('/');

	await expect(page).toHaveTitle(/Snapflow/);

	const heading = page.getByRole('heading', { level: 1, name: 'Snapflow' });
	await expect(heading).toBeVisible();
});

test('home page shows sign in links for unauthenticated users', async ({ page }) => {
	await page.goto('/');

	await expect(page.getByRole('link', { name: 'Sign in' })).toBeVisible();
	await expect(page.getByRole('link', { name: 'Create test account' })).toBeVisible();
});
