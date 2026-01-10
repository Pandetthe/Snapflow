import type { ApiClient, ApiEvent } from '$lib/types/api';
import type { Response, ProblemDetails, ValidationProblemDetails } from '$lib/types/app';
import type { GetBoardByIdResponse, GetBoardsResponse, IdResponse } from '$lib/types/boards.api';

export class BoardsService {
  constructor(private apiClient: ApiClient) {
    this.apiClient = apiClient;
  }

  private async handleResponse<T = void>(promise: Promise<globalThis.Response>): Promise<Response<T>> {
    try {
      const response = await promise;
      const ok = response.ok;

      if (ok) {
        if (response.status === 204) {
          return { ok: true, value: undefined as any };
        }

        const text = await response.text();
        const value = text ? (JSON.parse(text) as T) : (undefined as any);
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

  createBoard(request: { title: string; description: string }): Promise<Response<IdResponse>> {
    return this.handleResponse(
      this.apiClient.fetch('boards', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(request)
      })
    );
  }

  updateBoard(request: { id: number; title: string; description: string }): Promise<Response> {
    return this.handleResponse(
      this.apiClient.fetch(`boards/${request.id}`, {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          title: request.title,
          description: request.description
        })
      })
    );
  }

  deleteBoard(id: number): Promise<Response> {
    return this.handleResponse(
      this.apiClient.fetch(`boards/${id}`, {
        method: 'DELETE'
      })
    );
  }
}
