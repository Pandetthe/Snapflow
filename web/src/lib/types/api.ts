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
