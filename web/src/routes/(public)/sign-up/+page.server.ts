import { redirect } from '@sveltejs/kit';
import { externalSignInUrl } from '$lib/features/auth/api/auth';
import type { PageServerLoad } from './$types';

export const load: PageServerLoad = async ({ parent }) => {
  const { authProviders } = await parent();

  if (authProviders.autoRedirectScheme) {
    throw redirect(303, externalSignInUrl(authProviders.autoRedirectScheme, false));
  }
};
