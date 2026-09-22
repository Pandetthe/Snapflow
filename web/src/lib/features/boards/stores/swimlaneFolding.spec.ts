// @vitest-environment jsdom
import { describe, it, expect, beforeEach, vi } from 'vitest';

describe('swimlane folding store', () => {
  beforeEach(() => {
    vi.resetModules();
    vi.stubGlobal('localStorage', {
      setItem: vi.fn(),
      getItem: vi.fn()
    });
  });

  it('should be on when nothing is stored', async () => {
    const { swimlaneFolding } = await import('./swimlaneFolding.svelte');

    expect(swimlaneFolding.init()).toBe(true);
    expect(swimlaneFolding.current).toBe(true);
  });

  it('should persist being turned off', async () => {
    const { swimlaneFolding } = await import('./swimlaneFolding.svelte');
    swimlaneFolding.set(false);

    expect(swimlaneFolding.current).toBe(false);
    expect(localStorage.setItem).toHaveBeenCalledWith('swimlane-folding', 'off');
  });

  it('should restore being turned off', async () => {
    vi.mocked(localStorage.getItem).mockReturnValue('off');
    const { swimlaneFolding } = await import('./swimlaneFolding.svelte');

    expect(swimlaneFolding.init()).toBe(false);
  });

  it('should stay on for anything other than off', async () => {
    vi.mocked(localStorage.getItem).mockReturnValue('nonsense');
    const { swimlaneFolding } = await import('./swimlaneFolding.svelte');

    expect(swimlaneFolding.init()).toBe(true);
  });
});
