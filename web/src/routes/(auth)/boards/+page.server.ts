import type { PageServerLoad } from './$types';
import type { Board } from '$lib/services/boards';
import { boardsService } from '$lib/services/boards';

export const load: PageServerLoad = async ({ fetch }) => {
	const initialRefreshTime = new Date().toISOString();
	const result = await boardsService.getBoards(fetch);
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
