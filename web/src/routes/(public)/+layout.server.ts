import { redirect } from '@sveltejs/kit';
import { AuthService } from '$lib/features/auth/api/auth';
import { apiClient } from '$lib/server/api.server.ts';
import type { LayoutServerLoad } from './$types';

export const load: LayoutServerLoad = async (event) => {
  if (event.locals.session) {
    throw redirect(303, '/');
  }

  return {
    authProviders: await new AuthService(apiClient).getProviders(event)
  };
};
