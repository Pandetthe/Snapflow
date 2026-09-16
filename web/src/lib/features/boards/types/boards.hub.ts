import type {
  GetBoardByIdResponse,
  GetBoardDetailsResponse,
  MemberRole,
  TagColor
} from './boards.api';

export interface BoardsHubEvents {
  BoardSnapshot: (payload: BoardSnapshotEventPayload) => void;
  BoardUpdated: (payload: BoardUpdatedEventPayload) => void;
  BoardDeleted: () => void;
  RemovedFromBoard: () => void;
  ViewerJoined: (viewer: GetBoardByIdResponse.UserDto) => void;
  ViewerLeft: (userId: number) => void;
  YourRoleChanged: (oldRole: MemberRole, newRole: MemberRole) => void;
  SwimlaneCreated: (payload: SwimlaneCreatedEventPayload) => void;
  SwimlaneUpdated: (payload: SwimlaneUpdatedEventPayload) => void;
  SwimlaneMoved: (payload: SwimlaneMovedEventPayload) => void;
  SwimlaneDeleted: (payload: SwimlaneDeletedEventPayload) => void;
  ListCreated: (payload: ListCreatedEventPayload) => void;
  ListUpdated: (payload: ListUpdatedEventPayload) => void;
  ListMoved: (payload: ListMovedEventPayload) => void;
  ListDeleted: (payload: ListDeletedEventPayload) => void;
  CardCreated: (payload: CardCreatedEventPayload) => void;
  CardUpdated: (payload: CardUpdatedEventPayload) => void;
  CardMoved: (payload: CardMovedEventPayload) => void;
  CardLocked: (payload: CardLockedEventPayload) => void;
  CardUnlocked: (payload: CardUnlockedEventPayload) => void;
  CardDeleted: (payload: CardDeletedEventPayload) => void;
  TagCreated: (payload: TagCreatedEventPayload) => void;
  TagUpdated: (payload: TagUpdatedEventPayload) => void;
  TagDeleted: (payload: TagDeletedEventPayload) => void;
  CardTagAdded: (payload: CardTagAddedEventPayload) => void;
  CardTagRemoved: (payload: CardTagRemovedEventPayload) => void;
}

export interface BoardSnapshotEventPayload {
  id: number;
  title: string;
  description: string;
  swimlanes: GetBoardByIdResponse.SwimlaneDto[];
  tags: GetBoardByIdResponse.TagDto[];
  members: GetBoardDetailsResponse.BoardMemberDto[];
  viewers: GetBoardByIdResponse.UserDto[];
}

export interface BoardUpdatedEventPayload {
  title: string;
  description: string;
}

export interface SwimlaneCreatedEventPayload {
  id: number;
  title: string;
  rank: string;
  height: number | null;
  createdBy: GetBoardByIdResponse.UserDto;
}

export interface SwimlaneUpdatedEventPayload {
  id: number;
  title: string;
  height: number | null;
  updatedBy: GetBoardByIdResponse.UserDto;
}

export interface SwimlaneMovedEventPayload {
  id: number;
  rank: string;
  movedBy: GetBoardByIdResponse.UserDto;
}

export interface SwimlaneDeletedEventPayload {
  id: number;
  deletedBy: GetBoardByIdResponse.UserDto;
}

export interface ListCreatedEventPayload {
  id: number;
  swimlaneId: number;
  title: string;
  rank: string;
  width: number | null;
  createdBy: GetBoardByIdResponse.UserDto;
}

export interface ListUpdatedEventPayload {
  id: number;
  title: string;
  width: number | null;
  updatedBy: GetBoardByIdResponse.UserDto;
}

export interface ListMovedEventPayload {
  id: number;
  swimlaneId: number;
  rank: string;
  movedBy: GetBoardByIdResponse.UserDto;
}

export interface ListDeletedEventPayload {
  id: number;
  deletedBy: GetBoardByIdResponse.UserDto;
}

export interface CardCreatedEventPayload {
  id: number;
  listId: number;
  title: string;
  description: string;
  rank: string;
  createdAt: string;
  createdBy: { id: number; userName: string; avatarUrl: string | null };
}

export interface CardUpdatedEventPayload {
  id: number;
  title: string;
  description: string;
  updatedBy: GetBoardByIdResponse.UserDto;
}

export interface CardMovedEventPayload {
  id: number;
  listId: number;
  rank: string;
  movedBy: GetBoardByIdResponse.UserDto;
}

export interface CardLockedEventPayload {
  id: number;
}

export interface CardUnlockedEventPayload {
  id: number;
}

export interface CardDeletedEventPayload {
  id: number;
  deletedBy: GetBoardByIdResponse.UserDto;
}

export interface TagCreatedEventPayload {
  id: number;
  title: string;
  color: TagColor;
  createdBy: GetBoardByIdResponse.UserDto;
}

export interface TagUpdatedEventPayload {
  id: number;
  title: string;
  color: TagColor;
  updatedBy: GetBoardByIdResponse.UserDto;
}

export interface TagDeletedEventPayload {
  id: number;
  deletedBy: GetBoardByIdResponse.UserDto;
}

export interface CardTagAddedEventPayload {
  cardId: number;
  tagId: number;
  addedBy: GetBoardByIdResponse.UserDto;
}

export interface CardTagRemovedEventPayload {
  cardId: number;
  tagId: number;
  removedBy: GetBoardByIdResponse.UserDto;
}

export interface MoveSwimlaneRequest {
  id: number;
  beforeId: number | null;
}

export interface MoveListRequest {
  id: number;
  swimlaneId: number;
  beforeId: number | null;
}

export interface CreateSwimlaneRequest {
  title: string;
  height: number | null;
  beforeId: number | null;
}

export interface UpdateSwimlaneRequest {
  id: number;
  title: string;
  height: number | null;
}

export interface DeleteSwimlaneRequest {
  id: number;
}

export interface CreateListRequest {
  swimlaneId: number;
  title: string;
  width: number | null;
  beforeId: number | null;
}

export interface UpdateListRequest {
  id: number;
  title: string;
  width: number | null;
}

export interface DeleteListRequest {
  id: number;
}

export interface CreateCardRequest {
  listId: number;
  title: string;
  description: string;
  beforeId: number | null;
}

export interface UpdateCardRequest {
  id: number;
  title: string;
  description: string;
}

export interface MoveCardRequest {
  id: number;
  listId: number;
  beforeId: number | null;
}

export interface DeleteCardRequest {
  id: number;
}

export interface CreateSwimlaneResponse {
  id: number;
  rank: string;
  createdAt: string;
  createdBy: {
    id: number;
    userName: string;
    avatarUrl: string | null;
  };
}

export interface UpdateSwimlaneResponse {
  updatedAt: string;
  updatedBy: {
    id: number;
    userName: string;
    avatarUrl: string | null;
  };
}

export interface CreateListResponse {
  id: number;
  rank: string;
  createdAt: string;
  createdBy: {
    id: number;
    userName: string;
    avatarUrl: string | null;
  };
}

export interface UpdateListResponse {
  updatedAt: string;
  updatedBy: {
    id: number;
    userName: string;
    avatarUrl: string | null;
  };
}

export interface CreateCardResponse {
  id: number;
  rank: string;
  createdAt: string;
  createdBy: {
    id: number;
    userName: string;
    avatarUrl: string | null;
  };
}

export interface UpdateCardResponse {
  updatedAt: string;
  updatedBy: {
    id: number;
    userName: string;
    avatarUrl: string | null;
  };
}

export interface CreateTagHubRequest {
  title: string;
  color: TagColor;
}

export interface UpdateTagHubRequest {
  id: number;
  title: string;
  color: TagColor;
}

export interface DeleteTagRequest {
  id: number;
}

export interface AddTagToCardRequest {
  cardId: number;
  tagId: number;
}

export interface RemoveTagFromCardRequest {
  cardId: number;
  tagId: number;
}

export interface CreateTagHubResponse {
  id: number;
  createdAt: string;
  createdBy: {
    id: number;
    userName: string;
  };
}

export interface UpdateTagHubResponse {
  updatedAt: string;
  updatedBy: {
    id: number;
    userName: string;
  };
}
