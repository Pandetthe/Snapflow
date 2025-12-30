import type { PageServerLoad } from './$types';
import { BoardsService } from '$lib/services/boards.api';
import { apiClient } from '$lib/services/api.server.ts';
import { error } from '@sveltejs/kit';

export const load: PageServerLoad = async (event) => {
  event.depends('/api/boards');
  const refreshTime = new Date().toISOString();
  const result = await new BoardsService(apiClient).getBoards(event);
  if (!result.ok) {
    throw error(result.problem?.status ?? 500, result.problem?.title ?? 'Failed to load boards');
  }
  return {
    boards: result.value,
    refreshTime
  };
};
