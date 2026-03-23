import type { ApiClient, Response as ApiResponseType } from '$lib/core/types/api';
import type { RequestEvent, ServerLoadEvent } from '@sveltejs/kit';

export enum AvatarType {
  Gravatar = 'gravatar',
  Generated = 'generated',
  Uploaded = 'uploaded',
}

export interface User {
  id: number;
  userName: string;
  email: string;
  avatarUrl: string;
  avatarType: AvatarType;
}

export class UsersService {
  constructor(private apiClient: ApiClient) {
    this.apiClient = apiClient;
  }

  async getMe(event?: RequestEvent | ServerLoadEvent): Promise<ApiResponseType<{ user: User }>> {
    const response = await this.apiClient.fetch('/me', { method: 'GET' }, event);

    if (!response.ok) {
      return { ok: false };
    }

    const user = (await response.json()) as User;
    console.log('Fetched user:', user);
    return { ok: true, user };
  }
}
