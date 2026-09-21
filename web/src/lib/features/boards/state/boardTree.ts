import type { GetBoardByIdResponse } from '../types/boards.api';

type Board = GetBoardByIdResponse.BoardDto;
type Swimlane = GetBoardByIdResponse.SwimlaneDto;
type List = GetBoardByIdResponse.ListDto;
type Card = GetBoardByIdResponse.CardDto;

export function sortSwimlanes(board: Board) {
  board.swimlanes.sort((a, b) => a.rank.localeCompare(b.rank));
  board.swimlanes = [...board.swimlanes];
}

export function sortLists(swimlane: Swimlane) {
  swimlane.lists.sort((a, b) => a.rank.localeCompare(b.rank));
  swimlane.lists = [...swimlane.lists];
}

export function sortCards(list: List) {
  list.cards.sort((a, b) => a.rank.localeCompare(b.rank));
  list.cards = [...list.cards];
}

export function sortTags(board: Board) {
  board.tags.sort((a, b) => a.title.localeCompare(b.title));
  board.tags = [...board.tags];
}

export function sortAll(board: Board) {
  board.swimlanes.sort((a, b) => a.rank.localeCompare(b.rank));
  for (const swimlane of board.swimlanes) {
    sortLists(swimlane);
    for (const list of swimlane.lists) {
      sortCards(list);
    }
  }
  board.tags.sort((a, b) => a.title.localeCompare(b.title));
}

export function findCard(board: Board, cardId: number): Card | undefined {
  for (const swimlane of board.swimlanes)
    for (const list of swimlane.lists) {
      const card = list.cards.find((c) => c.id === cardId);
      if (card) return card;
    }
  return undefined;
}

export function findList(board: Board, listId: number): List | undefined {
  for (const swimlane of board.swimlanes) {
    const list = swimlane.lists.find((l) => l.id === listId);
    if (list) return list;
  }
  return undefined;
}

/** Every card that carries the tag, so an edit or a delete reaches all of them. */
export function forEachCardWithTag(board: Board, tagId: number, apply: (card: Card) => void) {
  for (const swimlane of board.swimlanes)
    for (const list of swimlane.lists)
      for (const card of list.cards) if (card.tagIds.includes(tagId)) apply(card);
}
