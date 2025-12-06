import { error } from '@sveltejs/kit';

export async function load({ url }) {
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