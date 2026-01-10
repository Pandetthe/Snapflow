import type { ApiClient, ProblemDetails, Response } from '$lib/types/api';

export interface SigninRequest {
  email: string;
  password: string;
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
      const error = await response.json() as ProblemDetails & { ok: false };
      error.ok = false;
      if ((error.status || 500) >= 500)
        console.log('Server error:', error);
      return error;
    } catch (err) {
      console.log(err);
      return { ok: false };
    }
  }

  async signIn(data: SigninRequest): Promise<Response> {
    const response = await this.apiClient.fetch(`/auth/sign-in?useCookies=true`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      return await this.#handleBadResponse(response);
    }
    return { ok: true };
  }

  async signOut(): Promise<Response> {
    const response = await this.apiClient.fetch(`/auth/sign-out`, {
      method: 'POST',
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
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
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
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
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
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
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
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      return await this.#handleBadResponse(response);
    }
    return { ok: true };
  }
}
