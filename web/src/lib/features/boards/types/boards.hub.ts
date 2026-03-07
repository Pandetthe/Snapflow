export interface BoardsHubEvents {
  BoardUpdated: (payload: BoardUpdatedEventPayload) => void;
  BoardDeleted: () => void;
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
}

export interface SwimlaneUpdatedEventPayload {
  id: number;
  title: string;
  height: number | null;
}

export interface SwimlaneMovedEventPayload {
  id: number;
  rank: string;
}

export interface SwimlaneDeletedEventPayload {
  id: number;
}

export interface ListCreatedEventPayload {
  id: number;
  swimlaneId: number;
  title: string;
  rank: string;
  width: number | null;
}

export interface ListUpdatedEventPayload {
  id: number;
  title: string;
  width: number | null;
}

export interface ListMovedEventPayload {
  id: number;
  swimlaneId: number;
  rank: string;
}

export interface ListDeletedEventPayload {
  id: number;
}

export interface CardCreatedEventPayload {
  id: number;
  listId: number;
  title: string;
  description: string;
  rank: string;
}

export interface CardUpdatedEventPayload {
  id: number;
  title: string;
  description: string;
}

export interface CardMovedEventPayload {
  id: number;
  listId: number;
  rank: string;
}

export interface CardLockedEventPayload {
  id: number;
}

export interface CardUnlockedEventPayload {
  id: number;
}

export interface CardDeletedEventPayload {
  id: number;
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
