import type { ServerLoadEvent } from '@sveltejs/kit';
import { AuthService } from '$lib/features/auth/api/auth';
import { UsersService } from '$lib/features/users/api/users';
import { apiClient } from '$lib/server/api.server.ts';

export const load = async (event: ServerLoadEvent) => {
  const usersService = new UsersService(apiClient);
  const [authProviders, twoFactor, passkeys] = await Promise.all([
    new AuthService(apiClient).getProviders(event),
    usersService.getTwoFactor(event),
    usersService.getPasskeys(event)
  ]);

  return {
    user: event.locals.user,
    authProviders,
    twoFactor: twoFactor.ok ? twoFactor.value : null,
    passkeys: authProviders.passwordSignIn && passkeys.ok ? passkeys.value : null
  };
};
