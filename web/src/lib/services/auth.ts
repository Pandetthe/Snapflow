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

export interface PropertyValidationError {
	propertyName: string | null;
	code: string;
	description: string;
}

export interface ProblemDetails {
	type: string | null;
	title: string | null;
	status: number | null;
	detail: string | null;
	instance: string | null;
	traceId: string;
}

export interface ValidationProblemDetails extends ProblemDetails {
	errors: PropertyValidationError[];
}

export type ErrorResponse =
	| ({ ok: false } & ProblemDetails)
	| ({ ok: false } & ValidationProblemDetails)
	| ({ ok: false });

export type SuccessResponse<T> = [T] extends [void]
	? { ok: true }
	: { ok: true } & T;

export type Response<T = void> = SuccessResponse<T> | ErrorResponse;

class AuthService {
	private baseUrl = '/api/auth';

	async signin(credentials: SigninRequest): Promise<Response> {
		const response = await fetch(`${this.baseUrl}/sign-in?useCookies=true`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
			body: JSON.stringify(credentials),
			credentials: 'include',
		});

		if (!response.ok) {
			try {
				const error = await response.json();
				error.ok = false;
				return error;
			} catch (err) {
				console.log(err);
				return { ok: false };
			}
		}
		return { ok: true };
	}

	async signup(userData: SignupRequest): Promise<Response> {
		const response = await fetch(`${this.baseUrl}/sign-up`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
			body: JSON.stringify(userData),
		});

		if (!response.ok) {
			try {
				const error = await response.json();
				error.ok = false;
				return error;
			} catch (err) {
				console.log(err);
				return { ok: false };
			}
		}
		return { ok: true };
	}

	async forgotPassword(email: string): Promise<boolean> {
		const response = await fetch(`${this.baseUrl}/forgot-password`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
			body: JSON.stringify({ email }),
		});

		if (!response.ok) {
			const error = await response.json();
			throw new Error(error.message || 'Failed to send reset email');
		}

		return response.json();
	}
}

export const authService = new AuthService();
