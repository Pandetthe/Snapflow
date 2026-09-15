// @vitest-environment jsdom
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { get } from 'svelte/store';

describe('drag handles store', () => {
  beforeEach(() => {
    vi.resetModules();
    vi.stubGlobal('localStorage', {
      setItem: vi.fn(),
      getItem: vi.fn()
    });
    document.documentElement.removeAttribute('data-drag-handles');
  });

  it('should default to always when nothing is stored', async () => {
    const { dragHandles } = await import('./dragHandles');

    expect(dragHandles.init()).toBe('always');
    expect(get(dragHandles)).toBe('always');
    expect(document.documentElement.getAttribute('data-drag-handles')).toBe('always');
  });

  it('should persist and apply the chosen visibility', async () => {
    const { dragHandles } = await import('./dragHandles');
    dragHandles.set('hidden');

    expect(get(dragHandles)).toBe('hidden');
    expect(localStorage.setItem).toHaveBeenCalledWith('drag-handles', 'hidden');
    expect(document.documentElement.getAttribute('data-drag-handles')).toBe('hidden');
  });

  it('should restore a stored visibility', async () => {
    vi.mocked(localStorage.getItem).mockReturnValue('hover');
    const { dragHandles } = await import('./dragHandles');

    expect(dragHandles.init()).toBe('hover');
    expect(document.documentElement.getAttribute('data-drag-handles')).toBe('hover');
  });

  it('should fall back to always when the stored value is not a known one', async () => {
    vi.mocked(localStorage.getItem).mockReturnValue('sideways');
    const { dragHandles } = await import('./dragHandles');

    expect(dragHandles.init()).toBe('always');
  });
});
