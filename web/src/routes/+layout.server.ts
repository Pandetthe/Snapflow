import type { LayoutServerLoad } from './$types';

export const load: LayoutServerLoad = async ({ locals }) => {
  return {
    isAuthenticated: !!locals.session && locals.user !== null,
    user: locals.user ?? null
  };
};
