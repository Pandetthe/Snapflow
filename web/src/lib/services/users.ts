import type { Response } from '$lib/types/api';

export interface User {
	id: number;
	userName: string;
	email: string;
}

class UsersService {
	private baseUrl = '/api';

	async getMe(fetchFn: typeof fetch = fetch): Promise<Response<{ user: User }>> {
		const response = await fetchFn(`${this.baseUrl}/me`, {
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

		const user = (await response.json()) as User;
		return { ok: true, user };
	}
}

export const usersService = new UsersService();

