import { writable } from 'svelte/store';

export interface RecentBoard {
  id: number;
  title: string;
}

const STORAGE_KEY = 'recent-boards';
const MAX_RECENT = 5;

function getStoredRecent(): RecentBoard[] {
  if (typeof window === 'undefined') return [];
  const stored = localStorage.getItem(STORAGE_KEY);
  if (!stored) return [];
  try {
    return JSON.parse(stored);
  } catch {
    return [];
  }
}

function createRecentBoardsStore() {
  const { subscribe, set, update } = writable<RecentBoard[]>(getStoredRecent());

  return {
    subscribe,
    add: (board: RecentBoard) => {
      update((boards) => {
        const filtered = boards.filter((b) => b.id !== board.id);
        const newBoards = [board, ...filtered].slice(0, MAX_RECENT);

        if (typeof window !== 'undefined') {
          localStorage.setItem(STORAGE_KEY, JSON.stringify(newBoards));
        }
        return newBoards;
      });
    },
    clear: () => {
      set([]);
      if (typeof window !== 'undefined') {
        localStorage.removeItem(STORAGE_KEY);
      }
    }
  };
}

export const recentBoards = createRecentBoardsStore();
