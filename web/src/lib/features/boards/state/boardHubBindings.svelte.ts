import type { SvelteMap } from 'svelte/reactivity';
import type { GetBoardByIdResponse, GetBoardDetailsResponse } from '../types/boards.api';
import type { BoardsHub } from '../hub/boards.hub';
import type {
  BoardSnapshotEventPayload,
  BoardsHubEvents,
  CardMovedEventPayload
} from '../types/boards.hub';
import * as tree from './boardTree';
import type { BoardPresence } from './boardPresence.svelte';
import type { ConnectionState } from './boardTypes';

export interface BoardHubDeps {
  board: GetBoardByIdResponse.BoardDto;
  members: GetBoardDetailsResponse.BoardMemberDto[];
  connectionState: ConnectionState;
  loaded: boolean;
  readonly viewerList: SvelteMap<number, GetBoardByIdResponse.UserDto>;
  readonly currentUserId: number | null;
  readonly presence: BoardPresence;
  leaveBoard(title: string, message: string): void;
}

/**
 * Applies what the hub reports onto the board. Events that arrive before the snapshot are held
 * back and replayed once it lands, so a late snapshot never overwrites a change that followed it.
 */
export function createBoardHubBindings(deps: BoardHubDeps) {
  const { markChanged, markNew, animateRemoteMove, animateRemoteDelete } = deps.presence;

  const sortSwimlanes = () => tree.sortSwimlanes(deps.board);
  const sortLists = tree.sortLists;
  const sortCards = tree.sortCards;
  const sortTags = () => tree.sortTags(deps.board);
  const sortAll = () => tree.sortAll(deps.board);
  const findCard = (cardId: number) => tree.findCard(deps.board, cardId);
  const forEachCardWithTag = (tagId: number, apply: (card: GetBoardByIdResponse.CardDto) => void) =>
    tree.forEachCardWithTag(deps.board, tagId, apply);

  let hub: BoardsHub | null = null;
  let awaitingSnapshot = true;
  let pendingEvents: (() => void)[] = [];

  function applySnapshot(snapshot: BoardSnapshotEventPayload) {
    deps.board = {
      id: snapshot.id,
      title: snapshot.title,
      description: snapshot.description,
      visibility: snapshot.visibility,
      swimlanes: snapshot.swimlanes,
      tags: snapshot.tags
    };
    deps.members = snapshot.members;
    deps.viewerList.clear();
    for (const viewer of snapshot.viewers) deps.viewerList.set(viewer.id, viewer);
    sortAll();
    deps.loaded = true;
    awaitingSnapshot = false;
    deps.connectionState = 'connected';

    const events = pendingEvents;
    pendingEvents = [];
    for (const apply of events) apply();
  }

  function reset(
    nextBoard: GetBoardByIdResponse.BoardDto,
    nextMembers: GetBoardDetailsResponse.BoardMemberDto[]
  ) {
    deps.board = nextBoard;
    deps.members = nextMembers;
    deps.viewerList.clear();
    deps.loaded = false;
    awaitingSnapshot = true;
    pendingEvents = [];
    deps.connectionState = 'connecting';
  }

  function registerHubEvents(h: BoardsHub) {
    hub = h;
    awaitingSnapshot = true;
    pendingEvents = [];

    function on<E extends keyof BoardsHubEvents>(event: E, callback: BoardsHubEvents[E]) {
      const run = callback as (...args: unknown[]) => void;
      h.on(event, ((...args: unknown[]) => {
        if (hub !== h) return;
        if (awaitingSnapshot) {
          pendingEvents.push(() => run(...args));
          return;
        }
        run(...args);
      }) as BoardsHubEvents[E]);
    }

    h.on('BoardSnapshot', (snapshot) => {
      if (hub === h) applySnapshot(snapshot);
    });

    on('ViewerJoined', (viewer) => {
      deps.viewerList.set(viewer.id, viewer);
    });

    on('ViewerLeft', (userId) => {
      deps.viewerList.delete(userId);
    });

    on('BoardUpdated', (payload) => {
      deps.board.title = payload.title;
      deps.board.description = payload.description;
    });

    on('SwimlaneCreated', (payload) => {
      const existing = deps.board.swimlanes.find((s) => s.id === payload.id);
      if (existing) {
        Object.assign(existing, payload);
        sortSwimlanes();
        return;
      }
      markNew('swimlane', payload.id);
      markChanged('swimlane', payload.id, payload.createdBy, 'added');
      deps.board.swimlanes.push({ ...payload, lists: [] });
      sortSwimlanes();
    });

    on('SwimlaneUpdated', (payload) => {
      const index = deps.board.swimlanes.findIndex((s) => s.id === payload.id);
      if (index !== -1) {
        deps.board.swimlanes[index].title = payload.title;
        deps.board.swimlanes[index].height = payload.height;
        markChanged('swimlane', payload.id, payload.updatedBy, 'edited');
      }
    });

    on('SwimlaneMoved', (payload) => {
      const index = deps.board.swimlanes.findIndex((s) => s.id === payload.id);
      if (index === -1 || payload.rank === deps.board.swimlanes[index].rank) return;

      animateRemoteMove('swimlane', payload.id, payload.movedBy, () => {
        const swimlane = deps.board.swimlanes.find((s) => s.id === payload.id);
        if (!swimlane) return false;
        swimlane.rank = payload.rank;
        sortSwimlanes();
        return true;
      });
    });

    on('SwimlaneDeleted', (payload) => {
      if (!deps.board.swimlanes.some((s) => s.id === payload.id)) return;

      animateRemoteDelete('swimlane', payload.id, payload.deletedBy, () => {
        const index = deps.board.swimlanes.findIndex((s) => s.id === payload.id);
        if (index !== -1) {
          deps.board.swimlanes.splice(index, 1);
          deps.board.swimlanes = [...deps.board.swimlanes];
        }
      });
    });

    on('ListCreated', (payload) => {
      const swimlane = deps.board.swimlanes.find((s) => s.id === payload.swimlaneId);
      if (swimlane) {
        const existing = swimlane.lists.find((l) => l.id === payload.id);
        if (existing) {
          Object.assign(existing, payload);
          sortLists(swimlane);
          return;
        }
        markNew('list', payload.id);
        markChanged('list', payload.id, payload.createdBy, 'added');
        swimlane.lists.push({ ...payload, cards: [] });
        sortLists(swimlane);
      }
    });

    on('ListUpdated', (payload) => {
      for (const s of deps.board.swimlanes) {
        const list = s.lists.find((l) => l.id === payload.id);
        if (list) {
          list.title = payload.title;
          list.width = payload.width;
          markChanged('list', payload.id, payload.updatedBy, 'edited');
          break;
        }
      }
    });

    on('ListMoved', (payload) => {
      animateRemoteMove('list', payload.id, payload.movedBy, () => {
        let movedList: GetBoardByIdResponse.ListDto | null = null;
        for (const s of deps.board.swimlanes) {
          const index = s.lists.findIndex((l) => l.id === payload.id);
          if (index !== -1) {
            [movedList] = s.lists.splice(index, 1);
            s.lists = [...s.lists];
            break;
          }
        }
        const targetSwimlane = deps.board.swimlanes.find((s) => s.id === payload.swimlaneId);
        if (!movedList || !targetSwimlane) return false;

        movedList.rank = payload.rank;
        targetSwimlane.lists.push(movedList);
        sortLists(targetSwimlane);
        return true;
      });
    });

    on('ListDeleted', (payload) => {
      if (!deps.board.swimlanes.some((s) => s.lists.some((l) => l.id === payload.id))) return;

      animateRemoteDelete('list', payload.id, payload.deletedBy, () => {
        for (const s of deps.board.swimlanes) {
          const index = s.lists.findIndex((l) => l.id === payload.id);
          if (index !== -1) {
            s.lists.splice(index, 1);
            s.lists = [...s.lists];
            break;
          }
        }
      });
    });

    on('CardCreated', (payload) => {
      for (const s of deps.board.swimlanes) {
        const list = s.lists.find((l) => l.id === payload.listId);
        if (list) {
          const existing = list.cards.find((c) => c.id === payload.id);
          if (existing) {
            Object.assign(existing, payload);
            sortCards(list);
            return;
          }
          markNew('card', payload.id);
          markChanged('card', payload.id, payload.createdBy, 'added');
          list.cards.push({ ...payload, updatedAt: null, updatedBy: null, tagIds: [] });
          sortCards(list);
          break;
        }
      }
    });

    const applyCardMove = (payload: CardMovedEventPayload): boolean => {
      let movedCard: GetBoardByIdResponse.CardDto | null = null;
      for (const s of deps.board.swimlanes) {
        for (const l of s.lists) {
          const index = l.cards.findIndex((c) => c.id === payload.id);
          if (index !== -1) {
            [movedCard] = l.cards.splice(index, 1);
            l.cards = [...l.cards];
            break;
          }
        }
        if (movedCard) break;
      }
      if (movedCard) {
        for (const s of deps.board.swimlanes) {
          const targetList = s.lists.find((l) => l.id === payload.listId);
          if (targetList) {
            movedCard.rank = payload.rank;
            targetList.cards.push(movedCard);
            sortCards(targetList);
            return true;
          }
        }
      }
      return false;
    };

    on('CardMoved', (payload) => {
      animateRemoteMove('card', payload.id, payload.movedBy, () => applyCardMove(payload));
    });

    on('CardUpdated', (payload) => {
      for (const s of deps.board.swimlanes) {
        for (const l of s.lists) {
          const card = l.cards.find((c) => c.id === payload.id);
          if (card) {
            card.title = payload.title;
            card.description = payload.description;
            markChanged('card', payload.id, payload.updatedBy, 'edited');
            return;
          }
        }
      }
    });

    on('CardDeleted', (payload) => {
      if (
        !deps.board.swimlanes.some((s) =>
          s.lists.some((l) => l.cards.some((c) => c.id === payload.id))
        )
      )
        return;

      animateRemoteDelete('card', payload.id, payload.deletedBy, () => {
        for (const s of deps.board.swimlanes) {
          for (const l of s.lists) {
            const index = l.cards.findIndex((c) => c.id === payload.id);
            if (index !== -1) {
              l.cards.splice(index, 1);
              l.cards = [...l.cards];
              return;
            }
          }
        }
      });
    });

    on('TagCreated', (payload) => {
      if (deps.board.tags.some((t) => t.id === payload.id)) return;
      deps.board.tags.push({ id: payload.id, title: payload.title, color: payload.color });
      sortTags();
    });

    on('TagUpdated', (payload) => {
      const tag = deps.board.tags.find((t) => t.id === payload.id);
      if (!tag) return;
      tag.title = payload.title;
      tag.color = payload.color;
      sortTags();
    });

    on('TagDeleted', (payload) => {
      const index = deps.board.tags.findIndex((t) => t.id === payload.id);
      if (index === -1) return;
      deps.board.tags.splice(index, 1);
      deps.board.tags = [...deps.board.tags];
      // A deleted tag leaves every card it was on, so no card keeps an id nothing resolves.
      forEachCardWithTag(payload.id, (card) => {
        card.tagIds = card.tagIds.filter((id) => id !== payload.id);
      });
    });

    on('CardTagAdded', (payload) => {
      const card = findCard(payload.cardId);
      if (!card || card.tagIds.includes(payload.tagId)) return;
      card.tagIds = [...card.tagIds, payload.tagId];
      markChanged('card', payload.cardId, payload.addedBy, 'edited');
    });

    on('CardTagRemoved', (payload) => {
      const card = findCard(payload.cardId);
      if (!card || !card.tagIds.includes(payload.tagId)) return;
      card.tagIds = card.tagIds.filter((id) => id !== payload.tagId);
      markChanged('card', payload.cardId, payload.removedBy, 'edited');
    });

    on('BoardVisibilityChanged', (payload) => {
      deps.board.visibility = payload.visibility;
    });

    on('BoardDeleted', () => {
      deps.leaveBoard(
        'Board deleted',
        'This deps.board was deleted, so it is no longer on your list.'
      );
    });

    // Not buffered behind the snapshot: losing access is worth acting on right away.
    h.on('RemovedFromBoard', () => {
      if (hub === h)
        deps.leaveBoard('Removed from deps.board', 'You no longer have access to this deps.board.');
    });

    on('YourRoleChanged', (_oldRole, newRole) => {
      const me = deps.members.find((m) => m.id === deps.currentUserId);
      if (me) me.role = newRole;
    });

    h.onClose(() => {
      if (hub === h) deps.connectionState = 'disconnected';
    });
    h.onReconnecting(() => {
      if (hub !== h) return;
      awaitingSnapshot = true;
      pendingEvents = [];
      deps.connectionState = 'reconnecting';
    });
  }

  return {
    get hub() {
      return hub;
    },
    registerHubEvents,
    reset
  };
}
