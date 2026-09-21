import { HubConnectionBuilder, type HubConnection } from '@microsoft/signalr';
import { env } from '$env/dynamic/public';
import logger from '$lib/logger';
import type {
  Response,
  ProblemDetails,
  ValidationProblemDetails,
  RankResponse
} from '$lib/core/types/app';
import type {
  BoardsHubEvents,
  CreateSwimlaneRequest,
  UpdateSwimlaneRequest,
  MoveSwimlaneRequest,
  DeleteSwimlaneRequest,
  CreateListRequest,
  UpdateListRequest,
  MoveListRequest,
  DeleteListRequest,
  CreateCardRequest,
  UpdateCardRequest,
  MoveCardRequest,
  DeleteCardRequest,
  CreateSwimlaneResponse,
  UpdateSwimlaneResponse,
  CreateListResponse,
  UpdateListResponse,
  CreateCardResponse,
  UpdateCardResponse,
  CreateTagHubRequest,
  UpdateTagHubRequest,
  DeleteTagRequest,
  AddTagToCardRequest,
  RemoveTagFromCardRequest,
  CreateTagHubResponse,
  UpdateTagHubResponse
} from '$lib/features/boards/types/boards.hub';

interface HubEnvelope<T> {
  statusCode: number;
  value?: T;
  problemDetails?: ProblemDetails | ValidationProblemDetails;
}

export class BoardsHub {
  private connection: HubConnection;

  constructor(boardId: number) {
    const url = `${(env.PUBLIC_API_BASE_URL ?? '').replace(/\/+$/, '')}/boards/${boardId}/hub`;
    this.connection = new HubConnectionBuilder()
      .withUrl(url, { withCredentials: true })
      .withAutomaticReconnect()
      .build();
  }

  async start() {
    logger.debug('BoardsHub: Starting connection...');
    await this.connection.start();
    logger.info('BoardsHub: Connection started.');
  }

  async stop() {
    await this.connection.stop();
  }

  on<E extends keyof BoardsHubEvents>(event: E, callback: BoardsHubEvents[E]) {
    this.connection.on(event, callback);
    return () => this.connection.off(event, callback);
  }

  onClose(callback: (error?: Error) => void) {
    this.connection.onclose(callback);
  }

  onReconnecting(callback: (error?: Error) => void) {
    this.connection.onreconnecting(callback);
  }

  onReconnected(callback: (connectionId?: string) => void) {
    this.connection.onreconnected(callback);
  }

  private async handleResponse<T = void>(
    method: string,
    promise: Promise<HubEnvelope<T> | null | undefined>
  ): Promise<Response<T>> {
    logger.debug({ method }, 'BoardsHub: Awaiting response');
    try {
      const res = await promise;
      logger.debug({ method, res }, 'BoardsHub: Received response');
      if (res === null || res === undefined) {
        return { ok: true } as Response<T>;
      }

      const ok = res.statusCode >= 200 && res.statusCode < 300;

      if (ok) {
        return { ok: true, value: res.value as T };
      }

      let problem: ProblemDetails | undefined;
      let validationProblem: ValidationProblemDetails | undefined;

      if (res.problemDetails) {
        if ('errors' in res.problemDetails && Array.isArray(res.problemDetails.errors)) {
          validationProblem = res.problemDetails;
        } else {
          problem = res.problemDetails;
        }
      }

      if (!ok) {
        logger.warn({ method, problem, validationProblem }, 'BoardsHub: Error response');
      }

      return {
        ok: false,
        problem,
        validationProblem
      };
    } catch (err) {
      logger.error({ method, err }, 'BoardsHub: Connection error');
      return {
        ok: false,
        problem: {
          status: 500,
          title: 'Connection Error',
          detail: err instanceof Error ? err.message : String(err)
        }
      };
    }
  }

  // Commands
  createSwimlane(request: CreateSwimlaneRequest): Promise<Response<CreateSwimlaneResponse>> {
    return this.handleResponse<CreateSwimlaneResponse>(
      'CreateSwimlane',
      this.connection.invoke('CreateSwimlane', request)
    );
  }

  updateSwimlane(request: UpdateSwimlaneRequest): Promise<Response<UpdateSwimlaneResponse>> {
    return this.handleResponse<UpdateSwimlaneResponse>(
      'UpdateSwimlane',
      this.connection.invoke('UpdateSwimlane', request)
    );
  }

  moveSwimlane(request: MoveSwimlaneRequest): Promise<Response<RankResponse>> {
    logger.debug({ request }, 'BoardsHub: Invoking MoveSwimlane');
    return this.handleResponse<RankResponse>(
      'MoveSwimlane',
      this.connection.invoke('MoveSwimlane', request)
    );
  }

  deleteSwimlane(request: DeleteSwimlaneRequest): Promise<Response> {
    return this.handleResponse('DeleteSwimlane', this.connection.invoke('DeleteSwimlane', request));
  }

  createList(request: CreateListRequest): Promise<Response<CreateListResponse>> {
    return this.handleResponse<CreateListResponse>(
      'CreateList',
      this.connection.invoke('CreateList', request)
    );
  }

  updateList(request: UpdateListRequest): Promise<Response<UpdateListResponse>> {
    return this.handleResponse<UpdateListResponse>(
      'UpdateList',
      this.connection.invoke('UpdateList', request)
    );
  }

  moveList(request: MoveListRequest): Promise<Response<RankResponse>> {
    return this.handleResponse<RankResponse>(
      'MoveList',
      this.connection.invoke('MoveList', request)
    );
  }

  deleteList(request: DeleteListRequest): Promise<Response> {
    return this.handleResponse('DeleteList', this.connection.invoke('DeleteList', request));
  }

  createCard(request: CreateCardRequest): Promise<Response<CreateCardResponse>> {
    return this.handleResponse<CreateCardResponse>(
      'CreateCard',
      this.connection.invoke('CreateCard', request)
    );
  }

  updateCard(request: UpdateCardRequest): Promise<Response<UpdateCardResponse>> {
    return this.handleResponse<UpdateCardResponse>(
      'UpdateCard',
      this.connection.invoke('UpdateCard', request)
    );
  }

  moveCard(request: MoveCardRequest): Promise<Response<RankResponse>> {
    return this.handleResponse<RankResponse>(
      'MoveCard',
      this.connection.invoke('MoveCard', request)
    );
  }

  deleteCard(request: DeleteCardRequest): Promise<Response> {
    return this.handleResponse('DeleteCard', this.connection.invoke('DeleteCard', request));
  }

  createTag(request: CreateTagHubRequest): Promise<Response<CreateTagHubResponse>> {
    return this.handleResponse<CreateTagHubResponse>(
      'CreateTag',
      this.connection.invoke('CreateTag', request)
    );
  }

  updateTag(request: UpdateTagHubRequest): Promise<Response<UpdateTagHubResponse>> {
    return this.handleResponse<UpdateTagHubResponse>(
      'UpdateTag',
      this.connection.invoke('UpdateTag', request)
    );
  }

  deleteTag(request: DeleteTagRequest): Promise<Response> {
    return this.handleResponse('DeleteTag', this.connection.invoke('DeleteTag', request));
  }

  addTagToCard(request: AddTagToCardRequest): Promise<Response> {
    return this.handleResponse('AddTagToCard', this.connection.invoke('AddTagToCard', request));
  }

  removeTagFromCard(request: RemoveTagFromCardRequest): Promise<Response> {
    return this.handleResponse(
      'RemoveTagFromCard',
      this.connection.invoke('RemoveTagFromCard', request)
    );
  }
}
