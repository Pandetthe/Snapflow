import { writable } from 'svelte/store';

const STORAGE_KEY = 'recent-boards';
const MAX_RECENT = 5;

let currentStorageKey = STORAGE_KEY;

function getStorageKey(userId?: number | string | null) {
  return userId ? `${STORAGE_KEY}:${userId}` : STORAGE_KEY;
}

function getStoredRecent(storageKey: string): number[] {
  if (typeof window === 'undefined') return [];
  const stored = localStorage.getItem(storageKey);
  if (!stored) return [];
  try {
    const parsed = JSON.parse(stored);
    if (!Array.isArray(parsed)) return [];
    return parsed.map(Number).filter((id) => !isNaN(id));
  } catch {
    return [];
  }
}

function createRecentBoardsStore() {
  const { subscribe, set, update } = writable<number[]>([]);

  function persist(ids: number[]) {
    if (typeof window !== 'undefined') {
      localStorage.setItem(currentStorageKey, JSON.stringify(ids));
    }
  }

  return {
    subscribe,
    configure: (userId?: number | string | null) => {
      currentStorageKey = getStorageKey(userId);
      set(getStoredRecent(currentStorageKey));
    },
    add: (boardId: number) => {
      update((ids) => {
        const filtered = ids.filter((id) => id !== boardId);
        const newIds = [boardId, ...filtered].slice(0, MAX_RECENT);
        persist(newIds);
        return newIds;
      });
    },
    clear: () => {
      set([]);
      if (typeof window !== 'undefined') {
        localStorage.removeItem(currentStorageKey);
      }
    }
  };
}

export const recentBoards = createRecentBoardsStore();
