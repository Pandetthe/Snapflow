import type { Handle } from '@sveltejs/kit';
import { usersService } from '$lib/services/users';

export const handle: Handle = async ({ event, resolve }) => {
	if (event.url.pathname.startsWith('/api'))
		return await resolve(event);
    const session = event.cookies.get('Snapflow.Auth.Cookie');
    event.locals.session = session ?? null;
	event.locals.user = null;
	if (session) {
		try {
			const result = await usersService.getMe(event.fetch);
			if (result.ok && 'user' in result) {
				event.locals.user = result.user;
			}
		} catch (error) {
			console.error('Failed to fetch user data:', error);
		}
	}

    return await resolve(event);
};
