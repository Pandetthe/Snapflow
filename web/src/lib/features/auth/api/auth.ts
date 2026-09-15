import type { ApiClient, ApiEvent, ProblemDetails, Response } from '$lib/core/types/api';
import { env } from '$env/dynamic/public';
import logger from '$lib/logger';
import type { PasskeyJson, PasskeyOptions } from '../passkeys';

export type ExternalProviderType = 'google' | 'microsoft' | 'facebook' | 'github' | 'oidc' | 'saml';

export interface ExternalProvider {
  scheme: string;
  displayName: string;
  type: ExternalProviderType;
}

export interface AuthProviders {
  passwordSignIn: boolean;
  externalSignUp: boolean;
  ldap: { displayName: string } | null;
  providers: ExternalProvider[];
  autoRedirectScheme: string | null;
}

export const localOnlyProviders: AuthProviders = {
  passwordSignIn: true,
  externalSignUp: false,
  ldap: null,
  providers: [],
  autoRedirectScheme: null
};

export interface TwoFactorSigninRequest {
  code?: string;
  recoveryCode?: string;
  passkeyCredential?: PasskeyJson;
  passkeyState?: string;
  rememberMe: boolean;
  rememberDevice: boolean;
}

export interface PasskeySigninRequest {
  credential: PasskeyJson;
  state: string;
  rememberMe: boolean;
}

export interface LdapSigninRequest {
  userName: string;
  password: string;
  rememberMe: boolean;
}

export function externalSignInUrl(scheme: string, rememberMe: boolean): string {
  const base = (env.PUBLIC_API_BASE_URL || '').replace(/\/+$/, '');
  return `${base}/auth/external/${encodeURIComponent(scheme)}?rememberMe=${rememberMe}`;
}

export interface SigninRequest {
  email: string;
  password: string;
  rememberMe: boolean;
}

export interface SignupRequest {
  email: string;
  userName: string;
  password: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResendEmailConfirmationRequest {
  email: string;
}

export interface ResetPasswordRequest {
  email: string;
  resetCode: string;
  newPassword: string;
}

export class AuthService {
  constructor(private apiClient: ApiClient) {
    this.apiClient = apiClient;
  }

  async #handleBadResponse<T>(response: globalThis.Response): Promise<Response<T>> {
    try {
      const error = (await response.json()) as ProblemDetails & { ok: false };
      error.ok = false;
      if ((error.status || 500) >= 500) logger.error({ error }, 'Server error');
      return error;
    } catch (err) {
      logger.error({ err }, 'Error parsing response');
      return { ok: false };
    }
  }

  async signIn(data: SigninRequest): Promise<Response> {
    const { rememberMe, ...payload } = data;
    const response = await this.apiClient.fetch(
      `/auth/sign-in?useCookies=true&useSessionCookies=${!rememberMe}`,
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload)
      });

    if (!response.ok) {
      return await this.#handleBadResponse(response);
    }
    return { ok: true };
  }

  async getProviders(event?: ApiEvent): Promise<AuthProviders> {
    try {
      const response = await this.apiClient.fetch('/auth/providers', { method: 'GET' }, event);
      if (response.ok) {
        return (await response.json()) as AuthProviders;
      }
      logger.error({ status: response.status }, 'Failed to fetch authentication providers');
    } catch (err) {
      logger.error({ err }, 'Failed to fetch authentication providers');
    }
    return localOnlyProviders;
  }

  async ldapSignIn(data: LdapSigninRequest): Promise<Response> {
    const { rememberMe, ...payload } = data;
    const response = await this.apiClient.fetch(
      `/auth/ldap/sign-in?useCookies=true&useSessionCookies=${!rememberMe}`,
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload)
      });

    if (!response.ok) {
      return await this.#handleBadResponse(response);
    }
    return { ok: true };
  }

  async twoFactorSignIn(data: TwoFactorSigninRequest): Promise<Response> {
    const { rememberMe, ...payload } = data;
    const response = await this.apiClient.fetch(
      `/auth/sign-in/two-factor?useCookies=true&useSessionCookies=${!rememberMe}`,
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload)
      });

    if (!response.ok) {
      return await this.#handleBadResponse(response);
    }
    return { ok: true };
  }

  async twoFactorPasskeyOptions(): Promise<Response<PasskeyOptions>> {
    const response = await this.apiClient.fetch('/auth/sign-in/two-factor/passkey/options', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: '{}'
    });

    if (!response.ok) {
      return await this.#handleBadResponse(response);
    }
    return { ok: true, ...((await response.json()) as PasskeyOptions) };
  }

  async passkeySignInOptions(): Promise<Response<PasskeyOptions>> {
    const response = await this.apiClient.fetch('/auth/sign-in/passkey/options', {
      method: 'POST'
    });

    if (!response.ok) {
      return await this.#handleBadResponse(response);
    }
    return { ok: true, ...((await response.json()) as PasskeyOptions) };
  }

  async passkeySignIn(data: PasskeySigninRequest): Promise<Response> {
    const { rememberMe, ...payload } = data;
    const response = await this.apiClient.fetch(
      `/auth/sign-in/passkey?useCookies=true&useSessionCookies=${!rememberMe}`,
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload)
      });

    if (!response.ok) {
      return await this.#handleBadResponse(response);
    }
    return { ok: true };
  }

  async signOut(): Promise<Response> {
    const response = await this.apiClient.fetch(`/auth/sign-out`, {
      method: 'POST'
    });

    if (!response.ok) {
      return await this.#handleBadResponse(response);
    }
    return { ok: true };
  }

  async signUp(data: SignupRequest): Promise<Response> {
    const response = await this.apiClient.fetch(`/auth/sign-up`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data)
    });

    if (!response.ok) {
      return await this.#handleBadResponse(response);
    }
    return { ok: true };
  }

  async forgotPassword(data: ForgotPasswordRequest): Promise<Response> {
    const response = await this.apiClient.fetch(`/auth/forgot-password`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data)
    });

    if (!response.ok) {
      return await this.#handleBadResponse(response);
    }

    return { ok: true };
  }

  async resetPassword(data: ResetPasswordRequest): Promise<Response> {
    const response = await this.apiClient.fetch(`/auth/reset-password`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data)
    });

    if (!response.ok) {
      return await this.#handleBadResponse(response);
    }
    return { ok: true };
  }

  async resendEmailConfirmation(data: ResendEmailConfirmationRequest): Promise<Response> {
    const response = await this.apiClient.fetch(`/auth/resend-confirmation-email`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data)
    });

    if (!response.ok) {
      return await this.#handleBadResponse(response);
    }
    return { ok: true };
  }
}
