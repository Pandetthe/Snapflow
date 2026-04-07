import { error } from '@sveltejs/kit';
import type { PageServerLoad } from './$types';
import { BoardsService } from '$lib/features/boards/api/boards.api';
import { apiClient } from '$lib/server/api.server';

export const load: PageServerLoad = async (event) => {
  const boardId = parseInt(event.params.id);
  const result = await new BoardsService(apiClient).getBoard(boardId, event);

  function formatErrorMessage(title: string, detail?: string | null) {
    return detail?.trim() ? `${title}\n${detail}` : title;
  }

  function isHiddenBoardStatus(status?: number | null) {
    return status === 401 || status === 403 || status === 404;
  }

  if (!result.ok) {
    if (isHiddenBoardStatus(result.problem?.status)) {
      throw error(
        404,
        formatErrorMessage('Board not found', result.problem?.detail)
      );
    }

    throw error(
      result.problem?.status ?? 500,
      formatErrorMessage(result.problem?.title ?? 'Failed to load board', result.problem?.detail)
    );
  }

  return {
    board: result.value
  };
};
