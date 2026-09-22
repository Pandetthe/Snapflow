import { describe, it, expect, beforeEach } from 'vitest';
import { flushSync } from 'svelte';
import { recentBoards } from './recent.svelte';

describe('recentBoards used from an effect', () => {
  beforeEach(() => {
    recentBoards.configure(undefined);
    recentBoards.clear();
  });

  it('settles instead of re-running itself forever', () => {
    const cleanup = $effect.root(() => {
      $effect(() => {
        recentBoards.add(7);
      });
    });

    expect(() => flushSync()).not.toThrow();
    expect(recentBoards.current).toEqual([7]);

    cleanup();
  });
});
