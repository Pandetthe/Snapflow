import type { PageServerLoad } from './$types';
import { BoardsService } from '$lib/features/boards/api/boards.api';
import { apiClient } from '$lib/server/api.server';

export const load: PageServerLoad = async (event) => {
  const result = await new BoardsService(apiClient).getVisibilityOptions(event);

  return {
    visibilityOptions: result.ok ? result.value : { allowedVisibilities: ['private' as const] }
  };
};
