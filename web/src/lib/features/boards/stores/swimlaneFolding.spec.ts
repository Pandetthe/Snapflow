// @vitest-environment jsdom
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { get } from 'svelte/store';

describe('swimlane folding store', () => {
  beforeEach(() => {
    vi.resetModules();
    vi.stubGlobal('localStorage', {
      setItem: vi.fn(),
      getItem: vi.fn()
    });
  });

  it('should be on when nothing is stored', async () => {
    const { swimlaneFolding } = await import('./swimlaneFolding');

    expect(swimlaneFolding.init()).toBe(true);
    expect(get(swimlaneFolding)).toBe(true);
  });

  it('should persist being turned off', async () => {
    const { swimlaneFolding } = await import('./swimlaneFolding');
    swimlaneFolding.set(false);

    expect(get(swimlaneFolding)).toBe(false);
    expect(localStorage.setItem).toHaveBeenCalledWith('swimlane-folding', 'off');
  });

  it('should restore being turned off', async () => {
    vi.mocked(localStorage.getItem).mockReturnValue('off');
    const { swimlaneFolding } = await import('./swimlaneFolding');

    expect(swimlaneFolding.init()).toBe(false);
  });

  it('should stay on for anything other than off', async () => {
    vi.mocked(localStorage.getItem).mockReturnValue('nonsense');
    const { swimlaneFolding } = await import('./swimlaneFolding');

    expect(swimlaneFolding.init()).toBe(true);
  });
});
