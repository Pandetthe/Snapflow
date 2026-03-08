// @vitest-environment jsdom
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { recentBoards } from './recent';
import { get } from 'svelte/store';

describe('recentBoards store', () => {
  beforeEach(() => {
    const storage: Record<string, string> = {};
    vi.stubGlobal('localStorage', {
      setItem: vi.fn((key, value) => {
        storage[key] = value;
      }),
      getItem: vi.fn((key) => storage[key] || null),
      removeItem: vi.fn((key) => {
        delete storage[key];
      })
    });

    recentBoards.clear();
  });

  it('should initialize with empty array by default', () => {
    expect(get(recentBoards)).toEqual([]);
  });

  it('should add a board to the list', () => {
    const board = { id: 1, title: 'Test Board' };
    recentBoards.add(board);

    expect(get(recentBoards)).toEqual([board]);
    expect(localStorage.setItem).toHaveBeenCalledWith('recent-boards', JSON.stringify([board]));
  });

  it('should move existing board to the top when added again', () => {
    const board1 = { id: 1, title: 'Board 1' };
    const board2 = { id: 2, title: 'Board 2' };

    recentBoards.add(board1);
    recentBoards.add(board2);
    expect(get(recentBoards)).toEqual([board2, board1]);

    recentBoards.add(board1);
    expect(get(recentBoards)).toEqual([board1, board2]);
  });

  it('should limit the list to 5 items', () => {
    for (let i = 1; i <= 6; i++) {
      recentBoards.add({ id: i, title: `Board ${i}` });
    }

    const currentRecent = get(recentBoards);
    expect(currentRecent).toHaveLength(5);
    expect(currentRecent[0].id).toBe(6);
    expect(currentRecent[4].id).toBe(2);
  });

  it('should clear the history', () => {
    recentBoards.add({ id: 1, title: 'Board 1' });
    recentBoards.clear();

    expect(get(recentBoards)).toEqual([]);
    expect(localStorage.removeItem).toHaveBeenCalledWith('recent-boards');
  });
});
