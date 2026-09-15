import type { ApiEvent } from '$lib/core/types/api';
import type { Response } from '$lib/core/types/app';
import { BaseService } from '$lib/core/base.service';
import type {
  CreateTagRequest,
  CreateTagResponse,
  GetBoardByIdResponse,
  UpdateTagRequest,
  UpdateTagResponse
} from '$lib/features/boards/types/boards.api';

export class TagsService extends BaseService {
  getTags(boardId: number, event?: ApiEvent): Promise<Response<GetBoardByIdResponse.TagDto[]>> {
    return this.handleResponse(
      this.apiClient.fetch(`boards/${boardId}/tags`, { method: 'GET' }, event)
    );
  }

  createTag(boardId: number, request: CreateTagRequest): Promise<Response<CreateTagResponse>> {
    return this.handleResponse(
      this.apiClient.fetch(`boards/${boardId}/tags`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(request)
      })
    );
  }

  updateTag(
    boardId: number,
    tagId: number,
    request: UpdateTagRequest
  ): Promise<Response<UpdateTagResponse>> {
    return this.handleResponse(
      this.apiClient.fetch(`boards/${boardId}/tags/${tagId}`, {
        method: 'PATCH',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(request)
      })
    );
  }

  deleteTag(boardId: number, tagId: number): Promise<Response> {
    return this.handleResponse(
      this.apiClient.fetch(`boards/${boardId}/tags/${tagId}`, { method: 'DELETE' })
    );
  }
}
