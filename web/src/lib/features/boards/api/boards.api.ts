import type { ApiEvent } from '$lib/core/types/api';
import type { Response } from '$lib/core/types/app';
import { BaseService } from '$lib/core/base.service';
import type {
  BoardVisibilityOptionsResponse,
  ChangeBoardVisibilityRequest,
  CreateBoardRequest,
  GetBoardByIdResponse,
  GetBoardDetailsResponse,
  GetBoardsResponse,
  IdResponse,
  PublicBoardDto,
  UpdateBoardRequest
} from '$lib/features/boards/types/boards.api';

export class BoardsService extends BaseService {

  getBoards(event?: ApiEvent): Promise<Response<GetBoardsResponse.BoardDto[]>> {
    return this.handleResponse(this.apiClient.fetch('boards', { method: 'GET' }, event));
  }

  getPublicBoards(event?: ApiEvent): Promise<Response<PublicBoardDto[]>> {
    return this.handleResponse(this.apiClient.fetch('boards/public', { method: 'GET' }, event));
  }

  getVisibilityOptions(event?: ApiEvent): Promise<Response<BoardVisibilityOptionsResponse>> {
    return this.handleResponse(
      this.apiClient.fetch('boards/visibility-options', { method: 'GET' }, event)
    );
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

  changeVisibility(id: number, request: ChangeBoardVisibilityRequest): Promise<Response> {
    return this.handleResponse(
      this.apiClient.fetch(`boards/${id}/visibility`, {
        method: 'PUT',
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
