import type { ApiClient, Response } from '$lib/types/api';
import type { RequestEvent } from '../../routes/(public)/$types';
import type { ServerLoadEvent } from '@sveltejs/kit';

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

export interface BoardDetailed {
	id: number;
	title: string;
	description: string;
	swimlanes: Swimlane[];
	createdAt: string;
	createdBy: BoardUser;
	updatedAt: string | null;
	updatedBy: BoardUser | null;
}

export interface Swimlane {
	id: number;
	title: string;
	lists: List[];
	createdAt: string;
	createdBy: BoardUser;
	updatedAt: string | null;
	updatedBy: BoardUser | null;
}

export interface List {
	id: number;
	title: string;
	cards: Card[];
	createdAt: string;
	createdBy: BoardUser;
	updatedAt: string | null;
	updatedBy: BoardUser | null;
}

export interface Card {
	id: number;
	title: string;
	description: string;
	createdAt: string;
	createdBy: BoardUser;
	updatedAt: string | null;
	updatedBy: BoardUser | null;
}

export class BoardsService {
	constructor(private apiClient: ApiClient) {
		this.apiClient = apiClient;
	}

	async getBoards(event?: RequestEvent | ServerLoadEvent): Promise<Response<{ boards: Board[] }>> {
		const response = await this.apiClient.fetch('boards', { method: 'GET' }, event);


		if (!response.ok) {
			return { ok: false };
		}

		const boards = (await response.json()) as Board[];
		return { ok: true, boards };
	}

	async getBoard(id: number, event?: RequestEvent | ServerLoadEvent): Promise<Response<{ board: BoardDetailed }>> {
		const response = await this.apiClient.fetch(`/boards/${id}`, { method: 'GET' }, event);

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

		const board = await response.json() as BoardDetailed;
		return { ok: true, board };
	}
}
