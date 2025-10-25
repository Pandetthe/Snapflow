import { parseBoard, type Board, type BoardResponse } from '$lib/types/api/boards';
import type { MeResponse, User } from '$lib/types/api/users';
import type { PageServerLoad } from './$types';

export const load = (async ({ cookies, depends, fetch }) => {
    depends('api:boards');
    const accessToken = cookies.get('Snapflow.Auth.Cookie');
    if (!accessToken)
        throw new Error('Unauthorized: No access token found');
    try {
        const res = await fetch('/api/boards', {
            method: 'GET',
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json',
            },
        });

        const user = await fetch("/api/users/me", {
            method: 'GET',
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json',
            },
        });
        if (res.ok) {
            const rawBoards = await res.json() as BoardResponse[];
            const rawUser = await user.json() as MeResponse;
            return { boards: rawBoards.map(parseBoard) satisfies Board[], user: rawUser satisfies User };
        }
    } catch (error) {
        console.error('Error while fetching boards:', error);
    }
    return { boards: [] satisfies Board[], user: null };
}) satisfies PageServerLoad;