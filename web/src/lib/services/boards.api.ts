import type { ApiClient, ApiEvent } from '$lib/types/api';
import type { Response, ProblemDetails, ValidationProblemDetails } from '$lib/types/app';
import type { GetBoardByIdResponse, GetBoardsResponse } from '$lib/types/boards.api';

export class BoardsService {
  constructor(private apiClient: ApiClient) {
    this.apiClient = apiClient;
  }

  private async handleResponse<T = void>(promise: Promise<globalThis.Response>): Promise<Response<T>> {
    try {
      const response = await promise;
      const ok = response.ok;

      if (ok) {
        const value = (await response.json()) as T;
        return { ok: true, value };
      }

      let problem: ProblemDetails | undefined;
      let validationProblem: ValidationProblemDetails | undefined;

      try {
        const data = await response.json();
        if (data.errors && Array.isArray(data.errors)) {
          validationProblem = data as ValidationProblemDetails;
        } else {
          problem = data as ProblemDetails;
        }
      } catch (err) {
        problem = {
          status: response.status,
          title: response.statusText,
          detail: 'Failed to parse error response'
        };
      }

      return {
        ok: false,
        problem,
        validationProblem
      };
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

  getBoards(event?: ApiEvent): Promise<Response<GetBoardsResponse.BoardDto[]>> {
    return this.handleResponse(this.apiClient.fetch('boards', { method: 'GET' }, event));
  }

  getBoard(id: number, event?: ApiEvent): Promise<Response<GetBoardByIdResponse.BoardDto>> {
    return this.handleResponse(this.apiClient.fetch(`/boards/${id}`, { method: 'GET' }, event));
  }
}
