import type { ProblemDetails, Response } from '$lib/types/api';

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

class AuthService {
	private baseUrl = '/api/auth';

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

	async signIn(data: SigninRequest, fetchFn: typeof fetch = fetch): Promise<Response> {
		const response = await fetchFn(`${this.baseUrl}/sign-in?useCookies=true`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
			body: JSON.stringify(data),
			credentials: 'include',
		});

		if (!response.ok) {
			return await this.#handleBadResponse(response);
		}
		return { ok: true };
	}

	async signUp(data: SignupRequest, fetchFn: typeof fetch = fetch): Promise<Response> {
		const response = await fetchFn(`${this.baseUrl}/sign-up`, {
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

	async forgotPassword(data: ForgotPasswordRequest, fetchFn: typeof fetch = fetch): Promise<Response> {
		const response = await fetchFn(`${this.baseUrl}/forgot-password`, {
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

	async resetPassword(data: ResetPasswordRequest, fetchFn: typeof fetch = fetch): Promise<Response> {
		const response = await fetchFn(`${this.baseUrl}/reset-password`, {
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

	async resendEmailConfirmation(data: ResendEmailConfirmationRequest, fetchFn: typeof fetch = fetch): Promise<Response> {
		const response = await fetchFn(`${this.baseUrl}/resend-confirmation-email`, {
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

export const authService = new AuthService();
