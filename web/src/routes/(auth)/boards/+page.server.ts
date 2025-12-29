import type { PageServerLoad } from './$types';
import { BoardsService } from '$lib/services/boards';
import { apiClient } from '$lib/services/api.server.ts';
import { error } from '@sveltejs/kit';

export const load: PageServerLoad = async (event) => {
  event.depends('/api/boards');
  const refreshTime = new Date().toISOString();
  const result = await new BoardsService(apiClient).getBoards(event);
  if (!result.ok) {
    if ('title' in result && result.title) {
      throw error(500, result.title as string);
    }
    throw error(500, 'Failed to load board');
  }
  return {
    boards: result.boards,
    refreshTime
  };
};
