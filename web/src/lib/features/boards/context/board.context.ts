import { getContext, setContext } from 'svelte';
import type { GetBoardByIdResponse } from '../types/boards.api';

export interface BoardUIContext {
  openSwimlaneModal(swimlane?: GetBoardByIdResponse.SwimlaneDto): void;
  openListModal(swimlaneId: number, list?: GetBoardByIdResponse.ListDto): void;
  openCardModal(listId: number, card?: GetBoardByIdResponse.CardDto): void;
}

const UI_KEY = Symbol('boardUI');

export const setBoardUI = (ctx: BoardUIContext) => setContext(UI_KEY, ctx);
export const getBoardUI = () => getContext<BoardUIContext>(UI_KEY);
