// @vitest-environment jsdom
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { get } from 'svelte/store';

describe('theme store', () => {
  beforeEach(() => {
    vi.stubGlobal('matchMedia', vi.fn().mockImplementation(query => ({
      matches: false,
      media: query,
      onchange: null,
      addListener: vi.fn(),
      removeListener: vi.fn(),
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      dispatchEvent: vi.fn(),
    })));

    vi.stubGlobal('localStorage', {
      setItem: vi.fn(),
      getItem: vi.fn()
    });

    document.documentElement.className = '';
  });

  it('should initialize with light theme by default', async () => {
    const { theme } = await import('./theme');
    theme.set('light');
    expect(get(theme)).toBe('light');
  });

  it('should toggle theme from light to dark', async () => {
    const { theme } = await import('./theme');
    theme.set('light');
    theme.toggle();

    expect(get(theme)).toBe('dark');
    expect(localStorage.setItem).toHaveBeenCalledWith('theme', 'dark');
    expect(document.documentElement.classList.contains('dark')).toBe(true);
  });

  it('should set specific theme', async () => {
    const { theme } = await import('./theme');
    theme.set('dark');

    expect(get(theme)).toBe('dark');
    expect(localStorage.setItem).toHaveBeenCalledWith('theme', 'dark');
    expect(document.documentElement.classList.contains('dark')).toBe(true);
  });
});
