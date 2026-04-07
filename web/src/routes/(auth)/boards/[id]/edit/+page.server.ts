import { error, redirect } from '@sveltejs/kit';
import type { PageServerLoad } from './$types';
import { BoardsService } from '$lib/features/boards/api/boards.api';
import { apiClient } from '$lib/server/api.server';

export const load: PageServerLoad = async (event) => {
  const boardId = Number(event.params.id);
  const result = await new BoardsService(apiClient).getBoardDetails(boardId, event);

  if (!result.ok) {
    if (result.problem?.status === 404) {
      throw error(404, `${result.problem?.title ?? 'Board not found'}\n${result.problem?.detail ?? ''}`);
    }

    throw error(
      result.problem?.status ?? 500,
      `${result.problem?.title ?? 'Failed to load board'}\n${result.problem?.detail ?? ''}`
    );
  }

  const userId = event.locals.user?.id;
  const owner = result.value.members?.find((m) => m.role === 'owner');
  const ownerId = owner ? (owner.id ?? (owner as any).userId) : null;

  if (!ownerId || ownerId !== userId) {
    throw redirect(302, `/boards/${boardId}`);
  }

  return {
    board: result.value
  };
};