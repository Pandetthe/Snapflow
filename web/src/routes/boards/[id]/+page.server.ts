import { error, redirect } from '@sveltejs/kit';
import type { PageServerLoad } from './$types';
import { BoardsService } from '$lib/features/boards/api/boards.api';
import { apiClient } from '$lib/server/api.server';
import type { GetBoardByIdResponse } from '$lib/features/boards/types/boards.api';

export const load: PageServerLoad = async (event) => {
  const boardId = parseInt(event.params.id);
  const boardsService = new BoardsService(apiClient);
  const detailsResult = await boardsService.getBoardDetails(boardId, event);

  function formatErrorMessage(title: string, detail?: string | null) {
    return detail?.trim() ? `${title}\n${detail}` : title;
  }

  function isHiddenBoardStatus(status?: number | null) {
    return status === 401 || status === 403 || status === 404;
  }

  if (!detailsResult.ok) {
    if (isHiddenBoardStatus(detailsResult.problem?.status)) {
      if (!event.locals.user) {
        throw redirect(303, '/sign-in');
      }

      throw error(404, formatErrorMessage('Board not found', detailsResult.problem?.detail));
    }

    throw error(
      detailsResult.problem?.status ?? 500,
      formatErrorMessage(
        detailsResult.problem?.title ?? 'Failed to load board',
        detailsResult.problem?.detail
      )
    );
  }

  const { id, title, description, visibility, members } = detailsResult.value;
  const board: GetBoardByIdResponse.BoardDto = {
    id,
    title,
    description,
    visibility,
    swimlanes: [],
    tags: []
  };

  return { board, members, user: event.locals.user };
};
