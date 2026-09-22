import { error, redirect } from '@sveltejs/kit';
import type { PageServerLoad } from './$types';
import { BoardsService } from '$lib/features/boards/api/boards.api';
import { TagsService } from '$lib/features/boards/api/tags.api';
import { apiClient } from '$lib/server/api.server';

export const load: PageServerLoad = async (event) => {
  const boardId = Number(event.params.id);
  const result = await new BoardsService(apiClient).getBoardDetails(boardId, event);

  if (!result.ok) {
    if (result.problem?.status === 404) {
      throw error(
        404,
        `${result.problem?.title ?? 'Board not found'}\n${result.problem?.detail ?? ''}`
      );
    }

    throw error(
      result.problem?.status ?? 500,
      `${result.problem?.title ?? 'Failed to load board'}\n${result.problem?.detail ?? ''}`
    );
  }

  const role = result.value.members?.find((m) => m.id === event.locals.user?.id)?.role;

  if (role !== 'owner' && role !== 'admin') {
    throw redirect(302, `/boards/${boardId}`);
  }

  // The board's tags come from their own endpoint; an unreachable one leaves the section empty
  // rather than failing the whole page.
  const [tagsResult, visibilityOptionsResult] = await Promise.all([
    new TagsService(apiClient).getTags(boardId, event),
    new BoardsService(apiClient).getVisibilityOptions(event)
  ]);

  return {
    board: result.value,
    tags: tagsResult.ok ? tagsResult.value : [],
    visibilityOptions: visibilityOptionsResult.ok
      ? visibilityOptionsResult.value
      : { allowedVisibilities: [result.value.visibility] }
  };
};
