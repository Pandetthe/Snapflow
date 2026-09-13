import type { ApiClient } from '$lib/core/types/api';
import type { Response, ProblemDetails, ValidationProblemDetails } from '$lib/core/types/app';

export abstract class BaseService {
  constructor(protected apiClient: ApiClient) {}

  protected async handleResponse<T = void>(
    promise: Promise<globalThis.Response>
  ): Promise<Response<T>> {
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

        if (body.trim().length > 0) {
          if (contentType.includes('json')) {
            const data = JSON.parse(body) as ValidationProblemDetails | ProblemDetails;
            if ('errors' in data && Array.isArray(data.errors)) {
              validationProblem = data;
            } else {
              problem = data;
            }
          } else {
            problem = { status: response.status, title: response.statusText || 'Error', detail: body.trim() };
          }
        } else {
          problem = { status: response.status, title: response.statusText || 'Error', detail: null };
        }
      } catch {
        problem = { status: response.status, title: response.statusText || 'Error', detail: 'Failed to parse error response' };
      }

      return { ok: false, problem, validationProblem };
    } catch (err) {
      return {
        ok: false,
        problem: {
          status: 500,
          title: 'Network Error',
          detail: err instanceof Error ? err.message : String(err)
        }
      };
    }
  }
}
