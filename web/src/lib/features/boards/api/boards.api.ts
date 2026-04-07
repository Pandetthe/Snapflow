import type { ApiClient, ApiEvent } from '$lib/core/types/api';
import type { Response, ProblemDetails, ValidationProblemDetails } from '$lib/core/types/app';
import type {
  CreateBoardRequest,
  GetBoardByIdResponse,
  GetBoardDetailsResponse,
  GetBoardsResponse,
  IdResponse,
  UpdateBoardRequest
} from '$lib/features/boards/types/boards.api';

export class BoardsService {
  constructor(private apiClient: ApiClient) {
    this.apiClient = apiClient;
  }

  private buildProblem(status: number, statusText: string, detail?: string | null): ProblemDetails {
    return {
      status,
      title: statusText || 'Error',
      detail: detail?.trim() || null
    };
  }

  private async handleResponse<T = void>(
    promise: Promise<globalThis.Response>
  ): Promise<Response<T>> {
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
        const contentType = response.headers.get('content-type') ?? '';
        const body = await response.text();

        if (body.trim().length > 0) {
          if (contentType.includes('application/json')) {
            const data = JSON.parse(body) as ValidationProblemDetails | ProblemDetails;
            if ('errors' in data && Array.isArray(data.errors)) {
              validationProblem = data;
            } else {
              problem = data;
            }
          } else {
            problem = this.buildProblem(response.status, response.statusText, body);
          }
        } else {
          problem = this.buildProblem(response.status, response.statusText);
        }
      } catch (err) {
        problem = this.buildProblem(response.status, response.statusText, 'Failed to parse error response');
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

  getBoardDetails(id: number, event?: ApiEvent): Promise<Response<GetBoardDetailsResponse.BoardDto>> {
    return this.handleResponse(this.apiClient.fetch(`/boards/${id}/details`, { method: 'GET' }, event));
  }

  createBoard(request: CreateBoardRequest): Promise<Response<IdResponse>> {
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

  updateBoard(id: number, request: UpdateBoardRequest): Promise<Response> {
    return this.handleResponse(
      this.apiClient.fetch(`boards/${id}`, {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(request)
      })
    );
  }

  changeOwner(id: number, request: { userId: number }): Promise<Response> {
    return this.handleResponse(
      this.apiClient.fetch(`boards/${id}/change-owner`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(request)
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
