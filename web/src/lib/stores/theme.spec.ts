// @vitest-environment jsdom
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { theme } from './theme';
import { get } from 'svelte/store';

describe('theme store', () => {
	beforeEach(() => {
		vi.stubGlobal('localStorage', {
			setItem: vi.fn(),
			getItem: vi.fn()
		});

		document.documentElement.className = '';
		theme.set('light');
	});

	it('should initialize with light theme by default', () => {
		expect(get(theme)).toBe('light');
	});

	it('should toggle theme from light to dark', () => {
		theme.toggle();

		expect(get(theme)).toBe('dark');
		expect(localStorage.setItem).toHaveBeenCalledWith('theme', 'dark');
		expect(document.documentElement.classList.contains('dark')).toBe(true);
	});

	it('should set specific theme', () => {
		theme.set('dark');

		expect(get(theme)).toBe('dark');
		expect(localStorage.setItem).toHaveBeenCalledWith('theme', 'dark');
		expect(document.documentElement.classList.contains('dark')).toBe(true);
	});
});
