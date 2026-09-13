import type { Response as ApiResponseType } from '$lib/core/types/api';
import type { Response as AppResponse } from '$lib/core/types/app';
import { BaseService } from '$lib/core/base.service';
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

export class UsersService extends BaseService {

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
      } as ApiResponseType<{ user: User }>;
    }

    const user = (await response.json()) as User;
    return { ok: true, user };
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

  async requestEmailChange(body: { newEmail: string }): Promise<AppResponse<void>> {
    return this.handleResponse(
      this.apiClient.fetch('/me/email', {
        method: 'POST',
        body: JSON.stringify(body),
        headers: { 'Content-Type': 'application/json' }
      })
    );
  }

  async updateProfile(body: { userName: string }): Promise<AppResponse<void>> {
    return this.handleResponse(
      this.apiClient.fetch('/me', {
        method: 'PATCH',
        body: JSON.stringify(body),
        headers: { 'Content-Type': 'application/json' }
      })
    );
  }

  async changePassword(body: { currentPassword: string; newPassword: string }): Promise<AppResponse<void>> {
    return this.handleResponse(
      this.apiClient.fetch('/me/password', {
        method: 'PUT',
        body: JSON.stringify(body),
        headers: { 'Content-Type': 'application/json' }
      })
    );
  }

  async updateAvatar(formData: FormData): Promise<AppResponse<void>> {
    return this.handleResponse(
      this.apiClient.fetch('/me/avatar', { method: 'PUT', body: formData })
    );
  }

  async deleteAccount(): Promise<AppResponse<void>> {
    return this.handleResponse(
      this.apiClient.fetch('/me', { method: 'DELETE' })
    );
  }
}
