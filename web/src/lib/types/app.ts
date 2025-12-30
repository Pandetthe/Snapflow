export interface AppError {
  code: string | null;
  description: string | null;
}

export interface ValidationError {
  code: string;
  description: string;
  propertyName: string | null;
}

export interface ProblemDetails {
  type?: string | null;
  title?: string | null;
  status?: number | null;
  detail?: string | null;
  instance?: string | null;
}

export interface ValidationProblemDetails extends ProblemDetails {
  errors: ValidationError[];
}

export type Response<T = void> =
  | { ok: true; value: T; problem?: never; validationProblem?: never }
  | { ok: false; value?: never; problem?: ProblemDetails; validationProblem?: ValidationProblemDetails };

export interface IdResponse<T = number> {
  id: T;
}

export interface RankResponse {
  rank: string;
}

