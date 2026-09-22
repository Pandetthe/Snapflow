export type Theme = 'light' | 'dark' | 'system';

function getInitialTheme(): Theme {
  if (typeof window === 'undefined') return 'system';

  const stored = localStorage.getItem('theme') as Theme | null;
  if (stored) return stored;

  return 'system';
}

function applyTheme(theme: Theme) {
  if (typeof window === 'undefined') return;

  document.documentElement.setAttribute('data-theme', theme);

  if (theme === 'system') {
    const systemDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    document.documentElement.classList.toggle('dark', systemDark);
  } else {
    document.documentElement.classList.toggle('dark', theme === 'dark');
  }
}

class ThemeState {
  #theme = $state<Theme>(getInitialTheme());

  constructor() {
    if (typeof window === 'undefined') return;

    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
      if (this.#theme === 'system') {
        document.documentElement.classList.toggle('dark', e.matches);
      }
    });
  }

  get current() {
    return this.#theme;
  }

  set(theme: Theme) {
    this.#theme = theme;
    if (typeof window !== 'undefined') {
      localStorage.setItem('theme', theme);
      applyTheme(theme);
    }
  }

  toggle() {
    const next = this.#theme === 'light' ? 'dark' : this.#theme === 'dark' ? 'system' : 'light';
    this.set(next);
  }

  init() {
    const initial = getInitialTheme();
    this.#theme = initial;
    applyTheme(initial);
    return initial;
  }
}

export const theme = new ThemeState();
