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

    recentBoards.configure(undefined);
    recentBoards.clear();
  });

  it('should scope recent boards by user id', () => {
    recentBoards.configure(1);
    recentBoards.add(1);

    recentBoards.configure(2);
    expect(get(recentBoards)).toEqual([]);

    recentBoards.add(2);

    recentBoards.configure(1);
    expect(get(recentBoards)).toEqual([1]);
  });

  it('should initialize with empty array by default', () => {
    expect(get(recentBoards)).toEqual([]);
  });

  it('should add a board to the list', () => {
    const boardId = 1;
    recentBoards.add(boardId);

    expect(get(recentBoards)).toEqual([boardId]);
    expect(localStorage.setItem).toHaveBeenCalledWith('recent-boards', JSON.stringify([boardId]));
  });

  it('should move existing board to the top when added again', () => {
    const board1 = 1;
    const board2 = 2;

    recentBoards.add(board1);
    recentBoards.add(board2);
    expect(get(recentBoards)).toEqual([board2, board1]);

    recentBoards.add(board1);
    expect(get(recentBoards)).toEqual([board1, board2]);
  });

  it('should limit the list to 5 items', () => {
    for (let i = 1; i <= 6; i++) {
      recentBoards.add(i);
    }

    const currentRecent = get(recentBoards);
    expect(currentRecent).toHaveLength(5);
    expect(currentRecent[0]).toBe(6);
    expect(currentRecent[4]).toBe(2);
  });

  it('should clear the history', () => {
    recentBoards.add(1);
    recentBoards.clear();

    expect(get(recentBoards)).toEqual([]);
    expect(localStorage.removeItem).toHaveBeenCalledWith('recent-boards');
  });
});
