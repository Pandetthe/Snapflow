import { error, redirect } from '@sveltejs/kit';
import type { PageServerLoadEvent } from './$types';

export async function load({ url, parent }: PageServerLoadEvent) {
  const { authProviders } = await parent();
  if (!authProviders.passwordSignIn) {
    throw redirect(303, '/sign-in');
  }

  const email = url.searchParams.get('email');
  const code = url.searchParams.get('resetCode');

  if (!email || !code) {
    return error(404, 'Page Not Found');
  }

  return {
    email,
    code
  };
}
