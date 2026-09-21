import type { Response } from '$lib/core/types/app';
import { BaseService } from '$lib/core/base.service';
import type { RequestEvent, ServerLoadEvent } from '@sveltejs/kit';
import type { PasskeyJson, PasskeyOptions } from '$lib/features/auth/passkeys';

export enum AvatarType {
  Gravatar = 'gravatar',
  Generated = 'generated',
  Uploaded = 'uploaded'
}

export interface User {
  id: number;
  userName: string;
  email: string;
  emailConfirmed: boolean;
  avatarUrl: string | null;
  avatarType: AvatarType;
}

export interface SearchUserDto {
  id: number;
  userName: string;
  avatarUrl: string | null;
}

export interface TwoFactorStatus {
  isEnabled: boolean;
  recoveryCodesLeft: number;
}

export interface AuthenticatorSetup {
  sharedKey: string;
  authenticatorUri: string;
}

export interface RecoveryCodesResponse {
  recoveryCodes: string[];
}

export interface Passkey {
  id: string;
  name: string;
  createdAt: string;
  isBackedUp: boolean;
}

export interface ExternalLogin {
  provider: string;
  displayName: string;
}

export class UsersService extends BaseService {
  getMe(event?: RequestEvent | ServerLoadEvent): Promise<Response<User>> {
    return this.handleResponse(this.apiClient.fetch('/me', { method: 'GET' }, event));
  }

  async searchUsers(
    name: string,
    excludedIds: number[] = [],
    event?: RequestEvent | ServerLoadEvent
  ): Promise<Response<SearchUserDto[]>> {
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

  async requestEmailChange(body: { newEmail: string }): Promise<Response<void>> {
    return this.handleResponse(
      this.apiClient.fetch('/me/email', {
        method: 'POST',
        body: JSON.stringify(body),
        headers: { 'Content-Type': 'application/json' }
      })
    );
  }

  async updateProfile(body: { userName: string }): Promise<Response<void>> {
    return this.handleResponse(
      this.apiClient.fetch('/me', {
        method: 'PATCH',
        body: JSON.stringify(body),
        headers: { 'Content-Type': 'application/json' }
      })
    );
  }

  async changePassword(body: {
    currentPassword: string;
    newPassword: string;
  }): Promise<Response<void>> {
    return this.handleResponse(
      this.apiClient.fetch('/me/password', {
        method: 'PUT',
        body: JSON.stringify(body),
        headers: { 'Content-Type': 'application/json' }
      })
    );
  }

  async getPasswordStatus(
    event?: RequestEvent | ServerLoadEvent
  ): Promise<Response<{ hasPassword: boolean }>> {
    return this.handleResponse<{ hasPassword: boolean }>(
      this.apiClient.fetch('/me/password', { method: 'GET' }, event)
    );
  }

  async setPassword(body: { newPassword: string }): Promise<Response<void>> {
    return this.handleResponse(
      this.apiClient.fetch('/me/password', {
        method: 'POST',
        body: JSON.stringify(body),
        headers: { 'Content-Type': 'application/json' }
      })
    );
  }

  async updateAvatar(formData: FormData): Promise<Response<void>> {
    return this.handleResponse(
      this.apiClient.fetch('/me/avatar', { method: 'PUT', body: formData })
    );
  }

  async getTwoFactor(event?: RequestEvent | ServerLoadEvent): Promise<Response<TwoFactorStatus>> {
    return this.handleResponse<TwoFactorStatus>(
      this.apiClient.fetch('/me/two-factor', { method: 'GET' }, event)
    );
  }

  async setupAuthenticator(): Promise<Response<AuthenticatorSetup>> {
    return this.handleResponse<AuthenticatorSetup>(
      this.apiClient.fetch('/me/two-factor/authenticator', { method: 'POST' })
    );
  }

  async enableTwoFactor(body: { code: string }): Promise<Response<RecoveryCodesResponse>> {
    return this.handleResponse<RecoveryCodesResponse>(
      this.apiClient.fetch('/me/two-factor/enable', {
        method: 'POST',
        body: JSON.stringify(body),
        headers: { 'Content-Type': 'application/json' }
      })
    );
  }

  async disableTwoFactor(body: { code?: string; recoveryCode?: string }): Promise<Response<void>> {
    return this.handleResponse(
      this.apiClient.fetch('/me/two-factor/disable', {
        method: 'POST',
        body: JSON.stringify(body),
        headers: { 'Content-Type': 'application/json' }
      })
    );
  }

  async regenerateRecoveryCodes(body: { code: string }): Promise<Response<RecoveryCodesResponse>> {
    return this.handleResponse<RecoveryCodesResponse>(
      this.apiClient.fetch('/me/two-factor/recovery-codes', {
        method: 'POST',
        body: JSON.stringify(body),
        headers: { 'Content-Type': 'application/json' }
      })
    );
  }

  async getPasskeys(event?: RequestEvent | ServerLoadEvent): Promise<Response<Passkey[]>> {
    return this.handleResponse<Passkey[]>(
      this.apiClient.fetch('/me/passkeys', { method: 'GET' }, event)
    );
  }

  async createPasskeyOptions(): Promise<Response<PasskeyOptions>> {
    return this.handleResponse<PasskeyOptions>(
      this.apiClient.fetch('/me/passkeys/options', { method: 'POST' })
    );
  }

  async addPasskey(body: {
    credential: PasskeyJson;
    state: string;
    name?: string;
  }): Promise<Response<Passkey>> {
    return this.handleResponse<Passkey>(
      this.apiClient.fetch('/me/passkeys', {
        method: 'POST',
        body: JSON.stringify(body),
        headers: { 'Content-Type': 'application/json' }
      })
    );
  }

  async renamePasskey(id: string, body: { name: string }): Promise<Response<void>> {
    return this.handleResponse(
      this.apiClient.fetch(`/me/passkeys/${encodeURIComponent(id)}`, {
        method: 'PATCH',
        body: JSON.stringify(body),
        headers: { 'Content-Type': 'application/json' }
      })
    );
  }

  async removePasskey(id: string): Promise<Response<void>> {
    return this.handleResponse(
      this.apiClient.fetch(`/me/passkeys/${encodeURIComponent(id)}`, { method: 'DELETE' })
    );
  }

  async getLogins(event?: RequestEvent | ServerLoadEvent): Promise<Response<ExternalLogin[]>> {
    return this.handleResponse<ExternalLogin[]>(
      this.apiClient.fetch('/me/logins', { method: 'GET' }, event)
    );
  }

  async removeLogin(provider: string): Promise<Response<void>> {
    return this.handleResponse(
      this.apiClient.fetch(`/me/logins/${encodeURIComponent(provider)}`, { method: 'DELETE' })
    );
  }

  async deleteAccount(): Promise<Response<void>> {
    return this.handleResponse(this.apiClient.fetch('/me', { method: 'DELETE' }));
  }
}
