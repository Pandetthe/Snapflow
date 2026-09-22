import type { ApiEvent } from '$lib/core/types/api';
import type { Result } from '$lib/core/types/app';
import { BaseService } from '$lib/core/base.service';
import { env } from '$env/dynamic/public';
import logger from '$lib/logger';
import type { PasskeyJson, PasskeyOptions } from '../passkeys';

export type ExternalProviderType =
  'google' | 'microsoft' | 'facebook' | 'github' | 'apple' | 'oidc' | 'saml';

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

const apiBaseUrl = () => (env.PUBLIC_API_BASE_URL || '').replace(/\/+$/, '');

export function externalSignInUrl(scheme: string, rememberMe: boolean): string {
  return `${apiBaseUrl()}/auth/external/${encodeURIComponent(scheme)}?rememberMe=${rememberMe}`;
}

export function externalLinkUrl(scheme: string): string {
  return `${apiBaseUrl()}/me/logins/${encodeURIComponent(scheme)}`;
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

export class AuthService extends BaseService {
  private json<TBody>(path: string, payload?: TBody): Promise<Response> {
    return this.apiClient.fetch(path, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload ?? {})
    });
  }

  private signInPath(path: string, rememberMe: boolean): string {
    return `${path}?useCookies=true&useSessionCookies=${!rememberMe}`;
  }

  signIn(data: SigninRequest): Promise<Result> {
    const { rememberMe, ...payload } = data;
    return this.handleResponse(this.json(this.signInPath('/auth/sign-in', rememberMe), payload));
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

  ldapSignIn(data: LdapSigninRequest): Promise<Result> {
    const { rememberMe, ...payload } = data;
    return this.handleResponse(
      this.json(this.signInPath('/auth/ldap/sign-in', rememberMe), payload)
    );
  }

  twoFactorSignIn(data: TwoFactorSigninRequest): Promise<Result> {
    const { rememberMe, ...payload } = data;
    return this.handleResponse(
      this.json(this.signInPath('/auth/sign-in/two-factor', rememberMe), payload)
    );
  }

  twoFactorPasskeyOptions(): Promise<Result<PasskeyOptions>> {
    return this.handleResponse(this.json('/auth/sign-in/two-factor/passkey/options'));
  }

  passkeySignInOptions(): Promise<Result<PasskeyOptions>> {
    return this.handleResponse(
      this.apiClient.fetch('/auth/sign-in/passkey/options', { method: 'POST' })
    );
  }

  passkeySignIn(data: PasskeySigninRequest): Promise<Result> {
    const { rememberMe, ...payload } = data;
    return this.handleResponse(
      this.json(this.signInPath('/auth/sign-in/passkey', rememberMe), payload)
    );
  }

  signOut(): Promise<Result> {
    return this.handleResponse(this.apiClient.fetch('/auth/sign-out', { method: 'POST' }));
  }

  signUp(data: SignupRequest): Promise<Result> {
    return this.handleResponse(this.json('/auth/sign-up', data));
  }

  forgotPassword(data: ForgotPasswordRequest): Promise<Result> {
    return this.handleResponse(this.json('/auth/forgot-password', data));
  }

  resetPassword(data: ResetPasswordRequest): Promise<Result> {
    return this.handleResponse(this.json('/auth/reset-password', data));
  }

  resendEmailConfirmation(data: ResendEmailConfirmationRequest): Promise<Result> {
    return this.handleResponse(this.json('/auth/resend-confirmation-email', data));
  }
}
