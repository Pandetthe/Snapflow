import type { PageServerLoad } from './$types';
import { BoardsService } from '$lib/features/boards/api/boards.api';
import { apiClient } from '$lib/server/api.server.ts';
import { error } from '@sveltejs/kit';

export const load: PageServerLoad = async (event) => {
  event.depends('/api/boards');
  const boardsService = new BoardsService(apiClient);
  const refreshTime = new Date().toISOString();

  const [boardsResult, publicBoardsResult] = await Promise.all([
    event.locals.user ? boardsService.getBoards(event) : null,
    boardsService.getPublicBoards(event)
  ]);

  if (boardsResult && !boardsResult.ok) {
    throw error(
      boardsResult.problem?.status ?? 500,
      boardsResult.problem?.title ?? 'Failed to load boards'
    );
  }

  return {
    boards: boardsResult?.value ?? null,
    publicBoards: publicBoardsResult.ok ? publicBoardsResult.value : [],
    refreshTime
  };
};
