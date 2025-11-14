import type { Handle } from '@sveltejs/kit';

export const handle: Handle = async ({ event, resolve }) => {
    const session = event.cookies.get('Snapflow.Auth.Cookie');
    event.locals.session = session ?? null;

    return await resolve(event);
};
