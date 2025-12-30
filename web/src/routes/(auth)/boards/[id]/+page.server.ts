import { error } from '@sveltejs/kit';
import type { PageServerLoad } from './$types';
import { BoardsService } from '$lib/services/boards.api';
import { apiClient } from '$lib/services/api.server';

export const load: PageServerLoad = async (event) => {
  const boardId = parseInt(event.params.id);
  const result = await new BoardsService(apiClient).getBoard(boardId, event);

  if (!result.ok) {
    if (result.problem?.status === 404) {
      throw error(404, result.problem?.title ?? 'Board not found');
    }
    throw error(result.problem?.status ?? 500, result.problem?.title ?? 'Failed to load board');
  }

  return {
    board: result.value
  };
};

