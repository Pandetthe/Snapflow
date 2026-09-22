import { getContext, setContext } from 'svelte';
import type { GetBoardByIdResponse } from '../types/boards.api';
import type { BoardsHub } from '../hub/boards.hub';
import type { ConnectionState } from '../state/boardTypes';
import type { GetRecentMove, IsInFlight, IsLeaving, IsNew } from '../state/boardPresence.svelte';

export interface BoardUIContext {
  openSwimlaneModal(swimlane?: GetBoardByIdResponse.SwimlaneDto): void;
  openListModal(swimlaneId: number, list?: GetBoardByIdResponse.ListDto): void;
  openCardModal(listId: number, card?: GetBoardByIdResponse.CardDto): void;
}

export interface BoardContext {
  readonly hub: BoardsHub | null;
  readonly board: GetBoardByIdResponse.BoardDto;
  readonly connectionState: ConnectionState;
  readonly canManageSwimlanes: boolean;
  readonly canManageLists: boolean;
  readonly canManageCards: boolean;
  readonly canAssignTags: boolean;
  readonly getRecentMove: GetRecentMove;
  readonly isInFlight: IsInFlight;
  readonly isNew: IsNew;
  readonly isLeaving: IsLeaving;
}

const UI_KEY = Symbol('boardUI');
const BOARD_KEY = Symbol('board');

export const setBoardUI = (ctx: BoardUIContext) => setContext(UI_KEY, ctx);
export const getBoardUI = () => getContext<BoardUIContext>(UI_KEY);

export const setBoardContext = (ctx: BoardContext) => setContext(BOARD_KEY, ctx);
export const getBoardContext = () => getContext<BoardContext>(BOARD_KEY);
