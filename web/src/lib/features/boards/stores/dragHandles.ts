import { writable } from 'svelte/store';

export type DragHandleVisibility = 'always' | 'hover' | 'hidden';

const STORAGE_KEY = 'drag-handles';
const VALUES: DragHandleVisibility[] = ['always', 'hover', 'hidden'];

function getInitialVisibility(): DragHandleVisibility {
  if (typeof window === 'undefined') return 'always';

  const stored = localStorage.getItem(STORAGE_KEY) as DragHandleVisibility | null;
  return stored && VALUES.includes(stored) ? stored : 'always';
}

function applyVisibility(visibility: DragHandleVisibility) {
  if (typeof window === 'undefined') return;
  document.documentElement.setAttribute('data-drag-handles', visibility);
}

function createDragHandlesStore() {
  const { subscribe, set } = writable<DragHandleVisibility>(getInitialVisibility());

  return {
    subscribe,
    set: (visibility: DragHandleVisibility) => {
      set(visibility);
      if (typeof window !== 'undefined') {
        localStorage.setItem(STORAGE_KEY, visibility);
        applyVisibility(visibility);
      }
    },
    init: () => {
      const initial = getInitialVisibility();
      set(initial);
      applyVisibility(initial);
      return initial;
    }
  };
}

export const dragHandles = createDragHandlesStore();
