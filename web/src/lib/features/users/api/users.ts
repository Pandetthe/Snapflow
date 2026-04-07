import type { ApiClient, Response as ApiResponseType } from '$lib/core/types/api';
import type { Response as AppResponse } from '$lib/core/types/app';
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
  avatarUrl: string | null;
  avatarType: AvatarType;
}

export interface SearchUserDto {
  id: number;
  userName: string;
  avatarUrl: string | null;
}

export class UsersService {
  constructor(private apiClient: ApiClient) {
    this.apiClient = apiClient;
  }

  async getMe(event?: RequestEvent | ServerLoadEvent): Promise<ApiResponseType<{ user: User }>> {
    const response = await this.apiClient.fetch('/me', { method: 'GET' }, event);

    if (!response.ok) {
      return {
        ok: false,
        problem: {
          status: response.status,
          title: response.statusText || 'Error',
          detail: null
        }
      } as any;
    }

    const email = (await response.json()) as any; // placeholder for user fetching
    const user = { ...email } as User;
    return { ok: true, user } as any;
  }

  async searchUsers(
    name: string,
    excludedIds: number[] = [],
    event?: RequestEvent | ServerLoadEvent
  ): Promise<AppResponse<SearchUserDto[]>> {
    const params = new URLSearchParams({ name });

    for (const excludedId of excludedIds) {
      params.append('excludedIds', excludedId.toString());
    }

    const response = await this.apiClient.fetch(
      `/users/search?${params.toString()}`,
      { method: 'GET' },
      event
    );

    if (!response.ok) {
      return {
        ok: false,
        problem: {
          status: response.status,
          title: response.statusText || 'Error'
        }
      };
    }

    const users = (await response.json()) as SearchUserDto[];
    return { ok: true, value: users };
  }
}
