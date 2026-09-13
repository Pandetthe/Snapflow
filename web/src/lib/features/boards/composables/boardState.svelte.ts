import type { GetBoardByIdResponse, GetBoardDetailsResponse, MemberRole } from '../types/boards.api';
import type { Response } from '$lib/core/types/app';
import { BoardsHub } from '../hub/boards.hub';
import { errorStore } from '$lib/ui/stores/error.svelte';
import { triggerHaptic } from '$lib/ui/utils';
import { SvelteMap, SvelteSet } from 'svelte/reactivity';
import { tick } from 'svelte';
import type { CardMovedEventPayload } from '../types/boards.hub';
import { startElementFlight, type ElementFlight } from '../animations/elementFlight';

export type MovableKind = 'card' | 'list' | 'swimlane';

export interface RecentMove {
  user: GetBoardByIdResponse.UserDto;
  isCurrentUser: boolean;
  key: number;
  /** Whether the element has reached its new place and may play its arrival animation. */
  pop: boolean;
}

export type GetRecentMove = (kind: MovableKind, id: number) => RecentMove | undefined;

export type IsInFlight = (kind: MovableKind, id: number) => boolean;

const RECENT_MOVE_DURATION_MS = 2500;

export function createBoardState(
  initialBoard: GetBoardByIdResponse.BoardDto,
  initialMembers: GetBoardDetailsResponse.BoardMemberDto[],
  currentUserId: number
) {
  let board = $state(initialBoard);
  let members = $state(initialMembers);
  let connectionState = $state<'connecting' | 'connected' | 'reconnecting' | 'disconnected'>('connecting');
  let hub: BoardsHub | null = null;

  // Items recently moved from other connections (the moving connection never receives the event).
  const recentMoves = new SvelteMap<string, RecentMove>();
  let recentMoveKey = 0;

  function markMoved(kind: MovableKind, id: number, user: GetBoardByIdResponse.UserDto, pop = true) {
    const mapKey = `${kind}:${id}`;
    const key = ++recentMoveKey;
    recentMoves.set(mapKey, { user, isCurrentUser: user.id === currentUserId, key, pop });
    setTimeout(() => {
      if (recentMoves.get(mapKey)?.key === key) recentMoves.delete(mapKey);
    }, RECENT_MOVE_DURATION_MS);
  }

  const getRecentMove: GetRecentMove = (kind, id) => recentMoves.get(`${kind}:${id}`);

  // Same key keeps the label as is; only the arrival animation is enabled.
  function markArrived(kind: MovableKind, id: number) {
    const mapKey = `${kind}:${id}`;
    const move = recentMoves.get(mapKey);
    if (move && !move.pop) recentMoves.set(mapKey, { ...move, pop: true });
  }

  // Items whose ghost is still travelling to their new place after a remote move; hidden until it lands.
  const inFlight = new SvelteSet<string>();
  const activeFlights = new SvelteMap<string, ElementFlight>();

  const isInFlight: IsInFlight = (kind, id) => inFlight.has(`${kind}:${id}`);

  /**
   * Shows a move made on another connection: the label appears right away, a ghost flies from the old
   * place to the new one, and the real element settles in when the ghost lands.
   * `apply` changes the state and returns whether the element was moved.
   */
  async function animateRemoteMove(
    kind: MovableKind,
    id: number,
    user: GetBoardByIdResponse.UserDto,
    apply: () => boolean
  ) {
    const key = `${kind}:${id}`;

    // A newer move replaces a flight that has not landed yet.
    activeFlights.get(key)?.cancel();
    activeFlights.delete(key);
    inFlight.delete(key);

    // The ghost captures the label and carries it along; the arrival animation waits for the landing,
    // so the captured size is not mid-animation.
    markMoved(kind, id, user, false);
    await tick();

    // Capture the old position before the state (and DOM) change.
    const flight = startElementFlight(kind, id);
    if (!apply()) {
      flight?.cancel();
      return;
    }
    if (!flight) {
      markArrived(kind, id);
      return;
    }

    activeFlights.set(key, flight);
    inFlight.add(key);
    await tick();
    await flight.land();
    if (activeFlights.get(key) !== flight) return;

    activeFlights.delete(key);
    inFlight.delete(key);
    markArrived(kind, id);

    // Swap only after the real element has rendered in the landing pose, so no frame shows neither or both.
    // Background tabs get no animation frames, so the timeout makes sure the ghost is removed anyway.
    await tick();
    await new Promise((resolve) => {
      requestAnimationFrame(resolve);
      setTimeout(resolve, 100);
    });
    flight.release();
  }

  const role = $derived<MemberRole>(
    members.find((m) => m.id === currentUserId)?.role ?? 'viewer'
  );
  const canEditBoard = $derived(role === 'owner' || role === 'admin');
  const canManageSwimlanes = $derived(role === 'owner' || role === 'admin');
  const canManageLists = $derived(role === 'owner' || role === 'admin');
  const canManageCards = $derived(role !== 'viewer');

  function sortSwimlanes() {
    board.swimlanes.sort((a, b) => a.rank.localeCompare(b.rank));
    board.swimlanes = [...board.swimlanes];
  }

  function sortLists(swimlane: GetBoardByIdResponse.SwimlaneDto) {
    swimlane.lists.sort((a, b) => a.rank.localeCompare(b.rank));
    swimlane.lists = [...swimlane.lists];
  }

  function sortCards(list: GetBoardByIdResponse.ListDto) {
    list.cards.sort((a, b) => a.rank.localeCompare(b.rank));
    list.cards = [...list.cards];
  }

  function sortAll() {
    board.swimlanes.sort((a, b) => a.rank.localeCompare(b.rank));
    for (const s of board.swimlanes) {
      sortLists(s);
      for (const l of s.lists) {
        sortCards(l);
      }
    }
  }

  function registerHubEvents(h: BoardsHub) {
    hub = h;

    hub.on('BoardUpdated', (payload) => {
      board.title = payload.title;
      board.description = payload.description;
    });

    hub.on('SwimlaneCreated', (payload) => {
      const existing = board.swimlanes.find((s) => s.id === payload.id);
      if (existing) {
        Object.assign(existing, payload);
        sortSwimlanes();
        return;
      }
      board.swimlanes.push({ ...payload, lists: [] });
      sortSwimlanes();
    });

    hub.on('SwimlaneUpdated', (payload) => {
      const index = board.swimlanes.findIndex((s) => s.id === payload.id);
      if (index !== -1) {
        board.swimlanes[index].title = payload.title;
        board.swimlanes[index].height = payload.height;
      }
    });

    hub.on('SwimlaneMoved', (payload) => {
      const index = board.swimlanes.findIndex((s) => s.id === payload.id);
      if (index === -1 || payload.rank === board.swimlanes[index].rank) return;

      animateRemoteMove('swimlane', payload.id, payload.movedBy, () => {
        const swimlane = board.swimlanes.find((s) => s.id === payload.id);
        if (!swimlane) return false;
        swimlane.rank = payload.rank;
        sortSwimlanes();
        return true;
      });
    });

    hub.on('SwimlaneDeleted', (payload) => {
      const index = board.swimlanes.findIndex((s) => s.id === payload.id);
      if (index !== -1) {
        board.swimlanes.splice(index, 1);
        board.swimlanes = [...board.swimlanes];
      }
    });

    hub.on('ListCreated', (payload) => {
      const swimlane = board.swimlanes.find((s) => s.id === payload.swimlaneId);
      if (swimlane) {
        const existing = swimlane.lists.find((l) => l.id === payload.id);
        if (existing) {
          Object.assign(existing, payload);
          sortLists(swimlane);
          return;
        }
        swimlane.lists.push({ ...payload, cards: [] });
        sortLists(swimlane);
      }
    });

    hub.on('ListUpdated', (payload) => {
      for (const s of board.swimlanes) {
        const list = s.lists.find((l) => l.id === payload.id);
        if (list) {
          list.title = payload.title;
          break;
        }
      }
    });

    hub.on('ListMoved', (payload) => {
      animateRemoteMove('list', payload.id, payload.movedBy, () => {
        let movedList: GetBoardByIdResponse.ListDto | null = null;
        for (const s of board.swimlanes) {
          const index = s.lists.findIndex((l) => l.id === payload.id);
          if (index !== -1) {
            [movedList] = s.lists.splice(index, 1);
            s.lists = [...s.lists];
            break;
          }
        }
        const targetSwimlane = board.swimlanes.find((s) => s.id === payload.swimlaneId);
        if (!movedList || !targetSwimlane) return false;

        movedList.rank = payload.rank;
        targetSwimlane.lists.push(movedList);
        sortLists(targetSwimlane);
        return true;
      });
    });

    hub.on('ListDeleted', (payload) => {
      for (const s of board.swimlanes) {
        const index = s.lists.findIndex((l) => l.id === payload.id);
        if (index !== -1) {
          s.lists.splice(index, 1);
          s.lists = [...s.lists];
          break;
        }
      }
    });

    hub.on('CardCreated', (payload) => {
      for (const s of board.swimlanes) {
        const list = s.lists.find((l) => l.id === payload.listId);
        if (list) {
          const existing = list.cards.find((c) => c.id === payload.id);
          if (existing) {
            Object.assign(existing, payload);
            sortCards(list);
            return;
          }
          list.cards.push({ ...payload, updatedAt: null, updatedBy: null });
          sortCards(list);
          break;
        }
      }
    });

    const applyCardMove = (payload: CardMovedEventPayload): boolean => {
      let movedCard: GetBoardByIdResponse.CardDto | null = null;
      for (const s of board.swimlanes) {
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
        for (const s of board.swimlanes) {
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

    hub.on('CardMoved', (payload) => {
      animateRemoteMove('card', payload.id, payload.movedBy, () => applyCardMove(payload));
    });

    hub.on('CardUpdated', (payload) => {
      for (const s of board.swimlanes) {
        for (const l of s.lists) {
          const card = l.cards.find((c) => c.id === payload.id);
          if (card) {
            card.title = payload.title;
            card.description = payload.description;
            return;
          }
        }
      }
    });

    hub.on('CardDeleted', (payload) => {
      for (const s of board.swimlanes) {
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

    hub.on('BoardDeleted', () => {
      window.location.href = '/boards/';
    });

    hub.on('YourRoleChanged', (_oldRole, newRole) => {
      const me = members.find((m) => m.id === currentUserId);
      if (me) me.role = newRole;
    });

    hub.onClose(() => { connectionState = 'disconnected'; });
    hub.onReconnecting(() => { connectionState = 'reconnecting'; });
    hub.onReconnected(() => { connectionState = 'connected'; });
  }

  const hubUnavailable = { ok: false, problem: { title: 'Board hub unavailable', detail: 'Please try again.' } } as const;

  async function handleSwimlaneConfirm(
    editingSwimlane: GetBoardByIdResponse.SwimlaneDto | undefined,
    title: string,
    height: number | null
  ): Promise<Response<any>> {
    if (!hub) return hubUnavailable;
    if (editingSwimlane) {
      const res = await hub.updateSwimlane({ id: editingSwimlane.id, title, height });
      if (res?.ok) {
        editingSwimlane.title = title;
        editingSwimlane.height = height;
      }
      return res;
    } else {
      const res = await hub.createSwimlane({ title, height, beforeId: null });
      if (res?.ok) {
        if (!board.swimlanes.some((s) => s.id === res.value.id)) {
          board.swimlanes.push({ id: res.value.id, title, height, rank: res.value.rank, lists: [] });
          sortSwimlanes();
        }
      }
      return res;
    }
  }

  async function handleSwimlaneDelete(id: number): Promise<boolean> {
    if (!hub) {
      triggerHaptic('error');
      errorStore.addError('Web.BoardHubUnavailable', 'Board connection is unavailable');
      return false;
    }
    const res = await hub.deleteSwimlane({ id });
    if (!res.ok) {
      triggerHaptic('error');
      errorStore.addError('Web.DeleteSwimlaneFailed', 'Failed to delete swimlane');
      return false;
    }
    const index = board.swimlanes.findIndex((s) => s.id === id);
    if (index !== -1) {
      board.swimlanes.splice(index, 1);
      board.swimlanes = [...board.swimlanes];
    }
    triggerHaptic('success');
    return true;
  }

  async function handleListConfirm(
    editingList: GetBoardByIdResponse.ListDto | undefined,
    targetSwimlaneId: number | null,
    title: string,
    width: number | null
  ): Promise<Response<any>> {
    if (!hub) return hubUnavailable;
    if (!editingList && !targetSwimlaneId) return { ok: false, problem: { title: 'Invalid target swimlane', detail: 'Please choose a swimlane and try again.' } };
    if (editingList) {
      const res = await hub.updateList({ id: editingList.id, title, width });
      if (res?.ok) {
        editingList.title = title;
        editingList.width = width;
      }
      return res;
    } else {
      const res = await hub.createList({ swimlaneId: targetSwimlaneId!, title, width, beforeId: null });
      if (res?.ok) {
        const swimlane = board.swimlanes.find((s) => s.id === targetSwimlaneId);
        if (swimlane && !swimlane.lists.some((l) => l.id === res.value.id)) {
          swimlane.lists.push({ id: res.value.id, title, width, rank: res.value.rank, cards: [] });
          sortLists(swimlane);
        }
      }
      return res;
    }
  }

  async function handleListDelete(id: number): Promise<boolean> {
    if (!hub) {
      triggerHaptic('error');
      errorStore.addError('Web.BoardHubUnavailable', 'Board connection is unavailable');
      return false;
    }
    const res = await hub.deleteList({ id });
    if (!res.ok) {
      triggerHaptic('error');
      errorStore.addError('Web.DeleteListFailed', 'Failed to delete list');
      return false;
    }
    for (const swimlane of board.swimlanes) {
      const index = swimlane.lists.findIndex((l) => l.id === id);
      if (index !== -1) {
        swimlane.lists.splice(index, 1);
        swimlane.lists = [...swimlane.lists];
        break;
      }
    }
    triggerHaptic('success');
    return true;
  }

  async function handleCardConfirm(
    editingCard: GetBoardByIdResponse.CardDto | undefined,
    targetListId: number | null,
    title: string,
    description: string
  ): Promise<Response<any>> {
    if (!hub) return hubUnavailable;
    if (!editingCard && !targetListId) return { ok: false, problem: { title: 'Invalid target list', detail: 'Please choose a list and try again.' } };
    if (editingCard) {
      const res = await hub.updateCard({ id: editingCard.id, title, description });
      if (res?.ok) {
        editingCard.title = title;
        editingCard.description = description;
        if (res.value.updatedAt) editingCard.updatedAt = res.value.updatedAt;
        if (res.value.updatedBy) editingCard.updatedBy = res.value.updatedBy;
      }
      return res;
    } else {
      const res = await hub.createCard({ listId: targetListId!, title, description, beforeId: null });
      if (res?.ok) {
        for (const s of board.swimlanes) {
          const list = s.lists.find((l) => l.id === targetListId);
          if (list && !list.cards.some((c) => c.id === res.value.id)) {
            list.cards.push({
              id: res.value.id,
              title,
              description,
              rank: res.value.rank,
              createdAt: res.value.createdAt,
              createdBy: res.value.createdBy,
              updatedAt: null,
              updatedBy: null
            });
            sortCards(list);
            break;
          }
        }
      }
      return res;
    }
  }

  async function handleCardDelete(id: number): Promise<boolean> {
    if (!hub) {
      triggerHaptic('error');
      errorStore.addError('Web.BoardHubUnavailable', 'Board connection is unavailable');
      return false;
    }
    const res = await hub.deleteCard({ id });
    if (!res.ok) {
      triggerHaptic('error');
      errorStore.addError('Web.DeleteCardFailed', 'Failed to delete card');
      return false;
    }
    for (const swimlane of board.swimlanes) {
      for (const list of swimlane.lists) {
        const index = list.cards.findIndex((card) => card.id === id);
        if (index !== -1) {
          list.cards.splice(index, 1);
          list.cards = [...list.cards];
          triggerHaptic('success');
          return true;
        }
      }
    }
    triggerHaptic('success');
    return true;
  }

  return {
    get board() { return board; },
    set board(v) { board = v; },
    get members() { return members; },
    set members(v) { members = v; },
    get connectionState() { return connectionState; },
    set connectionState(v) { connectionState = v; },
    get role() { return role; },
    get canEditBoard() { return canEditBoard; },
    get canManageSwimlanes() { return canManageSwimlanes; },
    get canManageLists() { return canManageLists; },
    get canManageCards() { return canManageCards; },
    getRecentMove,
    isInFlight,
    sortAll,
    sortSwimlanes,
    registerHubEvents,
    handleSwimlaneConfirm,
    handleSwimlaneDelete,
    handleListConfirm,
    handleListDelete,
    handleCardConfirm,
    handleCardDelete
  };
}
