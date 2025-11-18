import type { Response } from '$lib/types/api';

export interface BoardUser {
	id: number;
	userName: string;
}

export interface Board {
	id: number;
	title: string;
	description: string;
	createdAt: string;
	createdBy: BoardUser;
	updatedAt: string | null;
	updatedBy: BoardUser | null;
}

class BoardsService {
	private baseUrl = '/api/boards';

	async getBoards(fetchFn: typeof fetch = fetch): Promise<Response<{ boards: Board[] }>> {
		const response = await fetchFn(this.baseUrl, {
			method: 'GET',
			credentials: 'include'
		});

		if (!response.ok) {
			try {
				const error = await response.json();
				(error as any).ok = false;
				return error;
			} catch (err) {
				console.error(err);
				return { ok: false };
			}
		}

		const boards = (await response.json()) as Board[];
		return { ok: true, boards };
	}

	async getBoard(id: number, fetchFn: typeof fetch = fetch): Promise<Response<{ board: Board }>> {
		const response = await fetchFn(`${this.baseUrl}/${id}`, {
			method: 'GET',
			credentials: 'include'
		});

		if (!response.ok) {
			try {
				const error = await response.json();
				(error as any).ok = false;
				return error;
			} catch (err) {
				console.error(err);
				return { ok: false };
			}
		}

		const board = (await response.json()) as Board;
		return { ok: true, board };
	}
}

export const boardsService = new BoardsService();
