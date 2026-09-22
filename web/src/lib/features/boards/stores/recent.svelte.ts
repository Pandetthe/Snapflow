import { untrack } from 'svelte';

const STORAGE_KEY = 'recent-boards';
const MAX_RECENT = 5;

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

class RecentBoardsState {
  #ids = $state<number[]>([]);
  #storageKey = STORAGE_KEY;

  get current() {
    return this.#ids;
  }

  #persist(ids: number[]) {
    if (typeof window !== 'undefined') {
      localStorage.setItem(this.#storageKey, JSON.stringify(ids));
    }
  }

  configure(userId?: number | string | null) {
    this.#storageKey = getStorageKey(userId);
    this.#ids = getStoredRecent(this.#storageKey);
  }

  add(boardId: number) {
    const current = untrack(() => this.#ids);
    const next = [boardId, ...current.filter((id) => id !== boardId)].slice(0, MAX_RECENT);
    this.#ids = next;
    this.#persist(next);
  }

  clear() {
    this.#ids = [];
    if (typeof window !== 'undefined') {
      localStorage.removeItem(this.#storageKey);
    }
  }
}

export const recentBoards = new RecentBoardsState();
