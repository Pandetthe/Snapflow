import { describe, it, expect } from 'vitest';
import { findCard, findList, forEachCardWithTag, sortAll, sortTags } from './boardTree';
import type { GetBoardByIdResponse } from '../types/boards.api';

function card(id: number, rank: string, tagIds: number[] = []): GetBoardByIdResponse.CardDto {
  return {
    id,
    title: `card${id}`,
    description: '',
    rank,
    tagIds,
    createdAt: '',
    createdBy: null,
    updatedAt: null,
    updatedBy: null
  };
}

function board(): GetBoardByIdResponse.BoardDto {
  return {
    id: 1,
    title: 'board',
    description: '',
    visibility: 'private',
    tags: [
      { id: 2, title: 'urgent', color: 'red' },
      { id: 1, title: 'bug', color: 'blue' }
    ],
    swimlanes: [
      {
        id: 20,
        title: 'second',
        height: null,
        rank: 'b',
        lists: [
          { id: 201, title: 'l201', width: null, rank: 'b', cards: [card(3, 'b', [1])] },
          { id: 200, title: 'l200', width: null, rank: 'a', cards: [] }
        ]
      },
      {
        id: 10,
        title: 'first',
        height: null,
        rank: 'a',
        lists: [
          {
            id: 100,
            title: 'l100',
            width: null,
            rank: 'a',
            cards: [card(2, 'b', [1, 2]), card(1, 'a')]
          }
        ]
      }
    ]
  };
}

describe('boardTree', () => {
  it('sortAll orders swimlanes, lists, cards and tags by their own key', () => {
    const b = board();
    sortAll(b);

    expect(b.swimlanes.map((s) => s.id)).toEqual([10, 20]);
    expect(b.swimlanes[1].lists.map((l) => l.id)).toEqual([200, 201]);
    expect(b.swimlanes[0].lists[0].cards.map((c) => c.id)).toEqual([1, 2]);
    expect(b.tags.map((t) => t.title)).toEqual(['bug', 'urgent']);
  });

  it('sortTags orders by title, not by id', () => {
    const b = board();
    sortTags(b);

    expect(b.tags.map((t) => t.id)).toEqual([1, 2]);
  });

  it('findCard reaches a card in any swimlane', () => {
    const b = board();

    expect(findCard(b, 3)?.title).toBe('card3');
    expect(findCard(b, 1)?.title).toBe('card1');
    expect(findCard(b, 404)).toBeUndefined();
  });

  it('findList reaches a list in any swimlane', () => {
    const b = board();

    expect(findList(b, 200)?.title).toBe('l200');
    expect(findList(b, 404)).toBeUndefined();
  });

  it('forEachCardWithTag visits every card carrying the tag and no other', () => {
    const b = board();
    const visited: number[] = [];

    forEachCardWithTag(b, 1, (c) => visited.push(c.id));

    expect(visited.sort()).toEqual([2, 3]);
  });

  it('forEachCardWithTag visits nothing when no card carries the tag', () => {
    const b = board();
    const visited: number[] = [];

    forEachCardWithTag(b, 99, (c) => visited.push(c.id));

    expect(visited).toEqual([]);
  });
});
