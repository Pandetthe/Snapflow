import type { ApiClient, Response as ApiResponseType } from '$lib/core/types/api';
import type { Response as AppResponse, ProblemDetails, ValidationProblemDetails } from '$lib/core/types/app';
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

  private async handleResponse<T = void>(
    promise: Promise<globalThis.Response>
  ): Promise<AppResponse<T>> {
    try {
      const response = await promise;

      if (response.ok) {
        if (response.status === 204) return { ok: true, value: undefined as any };
        const text = await response.text();
        const value = text ? (JSON.parse(text) as T) : (undefined as any);
        return { ok: true, value };
      }

      let problem: ProblemDetails | undefined;
      let validationProblem: ValidationProblemDetails | undefined;

      try {
        const contentType = response.headers.get('content-type') ?? '';
        const body = await response.text();
        if (body.trim()) {
          if (contentType.includes('application/json')) {
            const data = JSON.parse(body) as ValidationProblemDetails | ProblemDetails;
            if ('errors' in data && Array.isArray(data.errors)) {
              validationProblem = data;
            } else {
              problem = data;
            }
          } else {
            problem = { status: response.status, title: response.statusText || 'Error', detail: body };
          }
        } else {
          problem = { status: response.status, title: response.statusText || 'Error' };
        }
      } catch {
        problem = { status: response.status, title: response.statusText || 'Error' };
      }

      return { ok: false, problem, validationProblem };
    } catch (err) {
      return {
        ok: false,
        problem: { status: 500, title: 'Network Error', detail: err instanceof Error ? err.message : String(err) }
      };
    }
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
