const STORAGE_KEY = 'swimlane-folding';

function getInitialFolding(): boolean {
  if (typeof window === 'undefined') return true;
  return localStorage.getItem(STORAGE_KEY) !== 'off';
}

class SwimlaneFoldingState {
  #enabled = $state(getInitialFolding());

  get current() {
    return this.#enabled;
  }

  set(enabled: boolean) {
    this.#enabled = enabled;
    if (typeof window !== 'undefined') {
      localStorage.setItem(STORAGE_KEY, enabled ? 'on' : 'off');
    }
  }

  init() {
    const initial = getInitialFolding();
    this.#enabled = initial;
    return initial;
  }
}

export const swimlaneFolding = new SwimlaneFoldingState();
