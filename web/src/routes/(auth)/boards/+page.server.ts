import type { PageServerLoad } from './$types';
import type { Board } from '$lib/services/boards';
import { BoardsService } from '$lib/services/boards';
import { apiClient } from '$lib/services/api.server.ts';

export const load: PageServerLoad = async (event) => {
	event.depends('/api/boards');
	const initialRefreshTime = new Date().toISOString();
	const result = await new BoardsService(apiClient).getBoards(event);
	if (!result.ok) {
		if ('title' in result && result.title) {
			return {
				boards: [] as Board[],
				error: result.title as string,
				initialRefreshTime
			};
		}
		return {
			boards: [] as Board[],
			error: 'Failed to load boards.',
			initialRefreshTime
		};
	}
	return {
		boards: result.boards,
		error: null as string | null,
		initialRefreshTime
	};
};
