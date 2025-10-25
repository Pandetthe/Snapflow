import tailwindcss from '@tailwindcss/vite';	
import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig } from 'vite';

export default defineConfig(() => {
	return {
		plugins: [tailwindcss(), sveltekit()],
		server: {
			proxy: {
				'/api': {
					target: 'https://localhost:7090',
					changeOrigin: true,
					secure: false,
					rewrite: (path) => path.replace(/^\/api/, '')
				}
			}
		}
	};
});

