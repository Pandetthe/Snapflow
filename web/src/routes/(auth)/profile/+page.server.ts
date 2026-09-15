import type { ServerLoadEvent } from '@sveltejs/kit';
import { AuthService } from '$lib/features/auth/api/auth';
import { apiClient } from '$lib/server/api.server.ts';

export const load = async (event: ServerLoadEvent) => {
  return {
    user: event.locals.user,
    authProviders: await new AuthService(apiClient).getProviders(event)
  };
};
