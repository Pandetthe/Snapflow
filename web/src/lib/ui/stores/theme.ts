import { writable } from 'svelte/store';

type Theme = 'light' | 'dark' | 'system';

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

function createThemeStore() {
  const { subscribe, set, update } = writable<Theme>(getInitialTheme());

  if (typeof window !== 'undefined') {
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
      update((currentTheme) => {
        if (currentTheme === 'system') {
          document.documentElement.classList.toggle('dark', e.matches);
        }
        return currentTheme;
      });
    });
  }

  return {
    subscribe,
    toggle: () => {
      update((theme) => {
        const newTheme = theme === 'light' ? 'dark' : theme === 'dark' ? 'system' : 'light';
        if (typeof window !== 'undefined') {
          localStorage.setItem('theme', newTheme);
          applyTheme(newTheme);
        }
        return newTheme;
      });
    },
    set: (theme: Theme) => {
      set(theme);
      if (typeof window !== 'undefined') {
        localStorage.setItem('theme', theme);
        applyTheme(theme);
      }
    },
    init: () => {
      const initial = getInitialTheme();
      set(initial);
      applyTheme(initial);
      return initial;
    }
  };
}

export const theme = createThemeStore();
