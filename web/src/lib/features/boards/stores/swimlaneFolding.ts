import { writable } from 'svelte/store';

const STORAGE_KEY = 'swimlane-folding';

function getInitialFolding(): boolean {
  if (typeof window === 'undefined') return true;
  return localStorage.getItem(STORAGE_KEY) !== 'off';
}

function createSwimlaneFoldingStore() {
  const { subscribe, set } = writable<boolean>(getInitialFolding());

  return {
    subscribe,
    set: (enabled: boolean) => {
      set(enabled);
      if (typeof window !== 'undefined') {
        localStorage.setItem(STORAGE_KEY, enabled ? 'on' : 'off');
      }
    },
    init: () => {
      const initial = getInitialFolding();
      set(initial);
      return initial;
    }
  };
}

export const swimlaneFolding = createSwimlaneFoldingStore();
