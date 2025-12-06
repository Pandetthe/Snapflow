import type { AppError } from '$lib/types/app';
import { writable } from 'svelte/store';


function createErrorStore() {

  const { subscribe, set, update } = writable<AppError[]>([]);
  return {
    subscribe,
    addError: (code: string | null, description: string | null) => {
      update((errors) => [...errors, { code, description }]);
    },
    addErrors: (errors: AppError[]) => {
      update((currentErrors) => [...currentErrors, ...errors]);
    },
    clearError: () => set([]),
    reset: () => set([]),
  };
}

export const errorStore = createErrorStore();