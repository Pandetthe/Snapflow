import { error } from '@sveltejs/kit';
import type { PageServerLoad } from './$types';
import { boardsService } from '$lib/services/boards';

export const load: PageServerLoad = async ({ params, fetch }) => {
	const boardId = parseInt(params.id);
	
	if (isNaN(boardId)) {
		throw error(400, 'Invalid board ID');
	}

	const result = await boardsService.getBoard(boardId, fetch);
	
	if (!result.ok) {
		if ('status' in result && result.status === 404) {
			throw error(404, 'Board not found');
		}
		if ('title' in result && result.title) {
			throw error(500, result.title as string);
		}
		throw error(500, 'Failed to load board');
	}

	return {
		board: result.board
	};
};

