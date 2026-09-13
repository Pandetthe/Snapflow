import { error } from '@sveltejs/kit';
import type { PageServerLoadEvent } from './$types';

export async function load({ url }: PageServerLoadEvent) {
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
