import type { Handle } from '@sveltejs/kit';
import { UsersService } from '$lib/services/users';
import { apiClient } from '$lib/services/api.server.ts';

export const handle: Handle = async ({ event, resolve }) => {
  const session = event.cookies.get('Snapflow.Auth.Cookie');
  event.locals.session = session ?? null;
  event.locals.user = null;

  if (session) {
    try {
      const result = await new UsersService(apiClient).getMe(event);
      if (result.ok) {
        event.locals.user = result.user;
      }
    } catch (err) {
      console.error('Failed to fetch user data:', err);
    }
  }
  return await resolve(event);
}