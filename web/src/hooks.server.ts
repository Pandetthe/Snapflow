import type { Handle } from '@sveltejs/kit';
import { env } from '$env/dynamic/public'
import type { RefreshRequest, AuthResponse } from '$lib/types/api/auth';

const PUBLIC_ROUTES: string[] = [];
const UNAUTH_ONLY_ROUTES: string[] = ['/signin', '/signup'];

export const handle: Handle = async ({ event, resolve }) => {
	const { pathname, origin } = event.url;
	if (pathname.startsWith('/api')) {
		return resolve(event);
	}
	let accessToken = event.cookies.get('Snapflow.Auth.Cookie');
	event.locals.isAuthenticated = !!accessToken;
    if (event.locals.isAuthenticated && UNAUTH_ONLY_ROUTES.includes(pathname)) {
		return Response.redirect(`${origin}/`, 303);
	}

	if (!event.locals.isAuthenticated && !PUBLIC_ROUTES.includes(pathname) && !UNAUTH_ONLY_ROUTES.includes(pathname)) {
		return Response.redirect(`${origin}/signin`, 303);
	}
	return resolve(event);
};
