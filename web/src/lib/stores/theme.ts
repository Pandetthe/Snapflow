import { writable } from 'svelte/store';

type Theme = 'light' | 'dark';

function createThemeStore() {
	const { subscribe, set, update } = writable<Theme>('light');

	return {
		subscribe,
		toggle: () => {
			update(theme => {
				const newTheme = theme === 'light' ? 'dark' : 'light';
				if (typeof window !== 'undefined') {
					localStorage.setItem('theme', newTheme);
					if (newTheme === 'dark') {
						document.documentElement.classList.add('dark');
					} else {
						document.documentElement.classList.remove('dark');
					}
				}
				return newTheme;
			});
		},
		set: (theme: Theme) => {
			set(theme);
			if (typeof window !== 'undefined') {
				localStorage.setItem('theme', theme);
				if (theme === 'dark') {
					document.documentElement.classList.add('dark');
				} else {
					document.documentElement.classList.remove('dark');
				}
			}
		},
		init: () => {
			let initial: Theme = 'light';
			if (typeof window !== 'undefined') {
				const isDark = document.documentElement.classList.contains('dark');
				initial = isDark ? 'dark' : 'light';
				set(initial);
				localStorage.setItem('theme', initial);
			} else {
				set(initial);
			}
			return initial;
		}
	};
}

export const theme = createThemeStore();
