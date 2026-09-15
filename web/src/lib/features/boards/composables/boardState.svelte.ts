import type {
  GetBoardByIdResponse,
  GetBoardDetailsResponse,
  MemberRole,
  TagColor
} from '../types/boards.api';
import type { Response } from '$lib/core/types/app';
import { BoardsHub } from '../hub/boards.hub';
import { errorStore } from '$lib/ui/stores/error.svelte';
import { triggerHaptic } from '$lib/ui/utils';
import { SvelteMap, SvelteSet } from 'svelte/reactivity';
import { tick } from 'svelte';
import type {
  BoardSnapshotEventPayload,
  BoardsHubEvents,
  CardMovedEventPayload
} from '../types/boards.hub';
import { startElementFlight, type ElementFlight } from '../animations/elementFlight';
import { goto } from '$app/navigation';
import { resolve } from '$app/paths';

export type MovableKind = 'card' | 'list' | 'swimlane';

export type BoardAction = 'moved' | 'added' | 'edited' | 'deleted';

export interface RecentMove {
  user: GetBoardByIdResponse.UserDto;
  action: BoardAction;
  isCurrentUser: boolean;
  key: number;
  /** Whether the element has reached its new place and may play its arrival animation. */
  pop: boolean;
}

export type GetRecentMove = (kind: MovableKind, id: number) => RecentMove | undefined;

export type IsInFlight = (kind: MovableKind, id: number) => boolean;

export type IsNew = (kind: MovableKind, id: number) => boolean;

export type IsLeaving = (kind: MovableKind, id: number) => boolean;

const RECENT_MOVE_DURATION_MS = 2500;

const NEW_ITEM_DURATION_MS = 700;

const LEAVING_ITEM_DURATION_MS = 1250;

export function createBoardState(
  initialBoard: GetBoardByIdResponse.BoardDto,
  initialMembers: GetBoardDetailsResponse.BoardMemberDto[],
  currentUserId: number
) {
  let board = $state(initialBoard);
  let members = $state(initialMembers);
  let connectionState = $state<'connecting' | 'connected' | 'reconnecting' | 'disconnected'>(
    'connecting'
  );
  let hub: BoardsHub | null = null;

  const recentMoves = new SvelteMap<string, RecentMove>();
  let recentMoveKey = 0;

  function markChanged(
    kind: MovableKind,
    id: number,
    user: GetBoardByIdResponse.UserDto,
    action: BoardAction
  ) {
    const mapKey = `${kind}:${id}`;
    const key = ++recentMoveKey;
    recentMoves.set(mapKey, {
      user,
      action,
      isCurrentUser: user.id === currentUserId,
      key,
      pop: false
    });
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

  const newItems = new SvelteSet<string>();

  function markNew(kind: MovableKind, id: number) {
    const key = `${kind}:${id}`;
    newItems.add(key);
    setTimeout(() => newItems.delete(key), NEW_ITEM_DURATION_MS);
  }

  const isNew: IsNew = (kind, id) => newItems.has(`${kind}:${id}`);

  const leavingItems = new SvelteSet<string>();

  const isLeaving: IsLeaving = (kind, id) => leavingItems.has(`${kind}:${id}`);

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
    markChanged(kind, id, user, 'moved');
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

  async function animateRemoteDelete(
    kind: MovableKind,
    id: number,
    user: GetBoardByIdResponse.UserDto,
    apply: () => void
  ) {
    const key = `${kind}:${id}`;
    activeFlights.get(key)?.cancel();
    activeFlights.delete(key);
    inFlight.delete(key);

    markChanged(kind, id, user, 'deleted');
    leavingItems.add(key);
    await new Promise((resolve) => setTimeout(resolve, LEAVING_ITEM_DURATION_MS));
    leavingItems.delete(key);
    apply();
  }

  const role = $derived<MemberRole>(members.find((m) => m.id === currentUserId)?.role ?? 'viewer');
  const canEditBoard = $derived(role === 'owner' || role === 'admin');
  const canManageSwimlanes = $derived(role === 'owner' || role === 'admin');
  const canManageLists = $derived(role === 'owner' || role === 'admin');
  const canManageCards = $derived(role !== 'viewer');
  // Defining the board's tags is an admin job; putting one on a card is not.
  const canManageTags = $derived(role === 'owner' || role === 'admin');
  const canAssignTags = $derived(role !== 'viewer');

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

  function sortTags() {
    board.tags.sort((a, b) => a.title.localeCompare(b.title));
    board.tags = [...board.tags];
  }

  /** Every card that carries the tag, so an edit or a delete reaches all of them. */
  function forEachCardWithTag(tagId: number, apply: (card: GetBoardByIdResponse.CardDto) => void) {
    for (const s of board.swimlanes)
      for (const l of s.lists) for (const c of l.cards) if (c.tagIds.includes(tagId)) apply(c);
  }

  function findCard(cardId: number): GetBoardByIdResponse.CardDto | undefined {
    for (const s of board.swimlanes)
      for (const l of s.lists) {
        const card = l.cards.find((c) => c.id === cardId);
        if (card) return card;
      }
    return undefined;
  }

  function sortAll() {
    board.swimlanes.sort((a, b) => a.rank.localeCompare(b.rank));
    for (const s of board.swimlanes) {
      sortLists(s);
      for (const l of s.lists) {
        sortCards(l);
      }
    }
    board.tags.sort((a, b) => a.title.localeCompare(b.title));
  }

  let loaded = $state(false);
  let awaitingSnapshot = true;
  let pendingEvents: (() => void)[] = [];

  function applySnapshot(snapshot: BoardSnapshotEventPayload) {
    board = {
      id: snapshot.id,
      title: snapshot.title,
      description: snapshot.description,
      swimlanes: snapshot.swimlanes,
      tags: snapshot.tags
    };
    members = snapshot.members;
    sortAll();
    loaded = true;
    awaitingSnapshot = false;
    connectionState = 'connected';

    const events = pendingEvents;
    pendingEvents = [];
    for (const apply of events) apply();
  }

  function reset(
    nextBoard: GetBoardByIdResponse.BoardDto,
    nextMembers: GetBoardDetailsResponse.BoardMemberDto[]
  ) {
    board = nextBoard;
    members = nextMembers;
    loaded = false;
    awaitingSnapshot = true;
    pendingEvents = [];
    connectionState = 'connecting';
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

    on('BoardUpdated', (payload) => {
      board.title = payload.title;
      board.description = payload.description;
    });

    on('SwimlaneCreated', (payload) => {
      const existing = board.swimlanes.find((s) => s.id === payload.id);
      if (existing) {
        Object.assign(existing, payload);
        sortSwimlanes();
        return;
      }
      markNew('swimlane', payload.id);
      markChanged('swimlane', payload.id, payload.createdBy, 'added');
      board.swimlanes.push({ ...payload, lists: [] });
      sortSwimlanes();
    });

    on('SwimlaneUpdated', (payload) => {
      const index = board.swimlanes.findIndex((s) => s.id === payload.id);
      if (index !== -1) {
        board.swimlanes[index].title = payload.title;
        board.swimlanes[index].height = payload.height;
        markChanged('swimlane', payload.id, payload.updatedBy, 'edited');
      }
    });

    on('SwimlaneMoved', (payload) => {
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

    on('SwimlaneDeleted', (payload) => {
      if (!board.swimlanes.some((s) => s.id === payload.id)) return;

      animateRemoteDelete('swimlane', payload.id, payload.deletedBy, () => {
        const index = board.swimlanes.findIndex((s) => s.id === payload.id);
        if (index !== -1) {
          board.swimlanes.splice(index, 1);
          board.swimlanes = [...board.swimlanes];
        }
      });
    });

    on('ListCreated', (payload) => {
      const swimlane = board.swimlanes.find((s) => s.id === payload.swimlaneId);
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
      for (const s of board.swimlanes) {
        const list = s.lists.find((l) => l.id === payload.id);
        if (list) {
          list.title = payload.title;
          markChanged('list', payload.id, payload.updatedBy, 'edited');
          break;
        }
      }
    });

    on('ListMoved', (payload) => {
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

    on('ListDeleted', (payload) => {
      if (!board.swimlanes.some((s) => s.lists.some((l) => l.id === payload.id))) return;

      animateRemoteDelete('list', payload.id, payload.deletedBy, () => {
        for (const s of board.swimlanes) {
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
      for (const s of board.swimlanes) {
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

    on('CardMoved', (payload) => {
      animateRemoteMove('card', payload.id, payload.movedBy, () => applyCardMove(payload));
    });

    on('CardUpdated', (payload) => {
      for (const s of board.swimlanes) {
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
        !board.swimlanes.some((s) => s.lists.some((l) => l.cards.some((c) => c.id === payload.id)))
      )
        return;

      animateRemoteDelete('card', payload.id, payload.deletedBy, () => {
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
    });

    on('TagCreated', (payload) => {
      if (board.tags.some((t) => t.id === payload.id)) return;
      board.tags.push({ id: payload.id, title: payload.title, color: payload.color });
      sortTags();
    });

    on('TagUpdated', (payload) => {
      const tag = board.tags.find((t) => t.id === payload.id);
      if (!tag) return;
      tag.title = payload.title;
      tag.color = payload.color;
      sortTags();
    });

    on('TagDeleted', (payload) => {
      const index = board.tags.findIndex((t) => t.id === payload.id);
      if (index === -1) return;
      board.tags.splice(index, 1);
      board.tags = [...board.tags];
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

    on('BoardDeleted', () => {
      leaveBoard('Web.BoardDeleted', 'This board was deleted.');
    });

    // Not buffered behind the snapshot: losing access is worth acting on right away.
    h.on('RemovedFromBoard', () => {
      if (hub === h) leaveBoard('Web.RemovedFromBoard', 'You were removed from this board.');
    });

    on('YourRoleChanged', (_oldRole, newRole) => {
      const me = members.find((m) => m.id === currentUserId);
      if (me) me.role = newRole;
    });

    h.onClose(() => {
      if (hub === h) connectionState = 'disconnected';
    });
    h.onReconnecting(() => {
      if (hub !== h) return;
      awaitingSnapshot = true;
      pendingEvents = [];
      connectionState = 'reconnecting';
    });
  }

  /** Sends the user back to their boards, telling them why the board went away. */
  function leaveBoard(code: string, message: string) {
    errorStore.addError(code, message);
    // invalidateAll so the boards page is loaded again without this board.
    goto(resolve('/boards'), { invalidateAll: true });
  }

  const hubUnavailable = {
    ok: false,
    problem: { title: 'Board hub unavailable', detail: 'Please try again.' }
  } as const;

  async function handleSwimlaneConfirm(
    editingSwimlane: GetBoardByIdResponse.SwimlaneDto | undefined,
    title: string,
    height: number | null
  ): Promise<Response<unknown>> {
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
          markNew('swimlane', res.value.id);
          board.swimlanes.push({
            id: res.value.id,
            title,
            height,
            rank: res.value.rank,
            lists: []
          });
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
  ): Promise<Response<unknown>> {
    if (!hub) return hubUnavailable;
    if (!editingList && !targetSwimlaneId)
      return {
        ok: false,
        problem: {
          title: 'Invalid target swimlane',
          detail: 'Please choose a swimlane and try again.'
        }
      };
    if (editingList) {
      const res = await hub.updateList({ id: editingList.id, title, width });
      if (res?.ok) {
        editingList.title = title;
        editingList.width = width;
      }
      return res;
    } else {
      const res = await hub.createList({
        swimlaneId: targetSwimlaneId!,
        title,
        width,
        beforeId: null
      });
      if (res?.ok) {
        const swimlane = board.swimlanes.find((s) => s.id === targetSwimlaneId);
        if (swimlane && !swimlane.lists.some((l) => l.id === res.value.id)) {
          markNew('list', res.value.id);
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

  /**
   * Brings a card's tags in line with `tagIds`, one call per difference. A card is tagged through
   * its own endpoints rather than as part of the card itself, so this runs after the card is saved.
   */
  async function applyCardTags(cardId: number, tagIds: number[]) {
    const card = findCard(cardId);
    const current = card?.tagIds ?? [];
    const added = tagIds.filter((id) => !current.includes(id));
    const removed = current.filter((id) => !tagIds.includes(id));
    for (const tagId of added) await handleCardTagToggle(cardId, tagId, true);
    for (const tagId of removed) await handleCardTagToggle(cardId, tagId, false);
  }

  async function handleCardConfirm(
    editingCard: GetBoardByIdResponse.CardDto | undefined,
    targetListId: number | null,
    title: string,
    description: string,
    tagIds: number[] = []
  ): Promise<Response<unknown>> {
    if (!hub) return hubUnavailable;
    if (!editingCard && !targetListId)
      return {
        ok: false,
        problem: { title: 'Invalid target list', detail: 'Please choose a list and try again.' }
      };
    if (editingCard) {
      const res = await hub.updateCard({ id: editingCard.id, title, description });
      if (res?.ok) {
        editingCard.title = title;
        editingCard.description = description;
        if (res.value.updatedAt) editingCard.updatedAt = res.value.updatedAt;
        if (res.value.updatedBy) editingCard.updatedBy = res.value.updatedBy;
        await applyCardTags(editingCard.id, tagIds);
      }
      return res;
    } else {
      const res = await hub.createCard({
        listId: targetListId!,
        title,
        description,
        beforeId: null
      });
      if (res?.ok) {
        for (const s of board.swimlanes) {
          const list = s.lists.find((l) => l.id === targetListId);
          if (list && !list.cards.some((c) => c.id === res.value.id)) {
            markNew('card', res.value.id);
            list.cards.push({
              id: res.value.id,
              title,
              description,
              rank: res.value.rank,
              createdAt: res.value.createdAt,
              createdBy: res.value.createdBy,
              updatedAt: null,
              updatedBy: null,
              tagIds: []
            });
            sortCards(list);
            break;
          }
        }
        await applyCardTags(res.value.id, tagIds);
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

  async function handleTagConfirm(
    editingTag: GetBoardByIdResponse.TagDto | undefined,
    title: string,
    color: TagColor
  ): Promise<Response<unknown>> {
    if (!hub) return hubUnavailable;
    if (editingTag) {
      const res = await hub.updateTag({ id: editingTag.id, title, color });
      if (res?.ok) {
        editingTag.title = title;
        editingTag.color = color;
        sortTags();
      }
      return res;
    }
    const res = await hub.createTag({ title, color });
    if (res?.ok && !board.tags.some((t) => t.id === res.value.id)) {
      board.tags.push({ id: res.value.id, title, color });
      sortTags();
    }
    return res;
  }

  async function handleTagDelete(id: number): Promise<boolean> {
    if (!hub) {
      triggerHaptic('error');
      errorStore.addError('Web.BoardHubUnavailable', 'Board connection is unavailable');
      return false;
    }
    const res = await hub.deleteTag({ id });
    if (!res.ok) {
      triggerHaptic('error');
      errorStore.addError('Web.DeleteTagFailed', 'Failed to delete tag');
      return false;
    }
    const index = board.tags.findIndex((t) => t.id === id);
    if (index !== -1) {
      board.tags.splice(index, 1);
      board.tags = [...board.tags];
    }
    forEachCardWithTag(id, (card) => {
      card.tagIds = card.tagIds.filter((tagId) => tagId !== id);
    });
    triggerHaptic('success');
    return true;
  }

  /**
   * Puts a tag on a card or takes it off. The card is changed straight away and put back
   * the way it was if the server refuses, so the picker never waits on a round trip.
   */
  async function handleCardTagToggle(
    cardId: number,
    tagId: number,
    add: boolean
  ): Promise<boolean> {
    const card = findCard(cardId);
    if (!card) return false;
    if (card.tagIds.includes(tagId) === add) return true;

    if (!hub) {
      triggerHaptic('error');
      errorStore.addError('Web.BoardHubUnavailable', 'Board connection is unavailable');
      return false;
    }

    const previous = card.tagIds;
    card.tagIds = add ? [...previous, tagId] : previous.filter((id) => id !== tagId);

    const res = add
      ? await hub.addTagToCard({ cardId, tagId })
      : await hub.removeTagFromCard({ cardId, tagId });

    if (!res.ok) {
      card.tagIds = previous;
      triggerHaptic('error');
      errorStore.addError(
        add ? 'Web.AddTagToCardFailed' : 'Web.RemoveTagFromCardFailed',
        add ? 'Failed to add the tag to the card' : 'Failed to remove the tag from the card'
      );
      return false;
    }

    triggerHaptic('success');
    return true;
  }

  return {
    get board() {
      return board;
    },
    set board(v) {
      board = v;
    },
    get members() {
      return members;
    },
    set members(v) {
      members = v;
    },
    get connectionState() {
      return connectionState;
    },
    get hasSnapshot() {
      return loaded;
    },
    set connectionState(v) {
      connectionState = v;
    },
    get role() {
      return role;
    },
    get canEditBoard() {
      return canEditBoard;
    },
    get canManageSwimlanes() {
      return canManageSwimlanes;
    },
    get canManageLists() {
      return canManageLists;
    },
    get canManageCards() {
      return canManageCards;
    },
    get canManageTags() {
      return canManageTags;
    },
    get canAssignTags() {
      return canAssignTags;
    },
    getRecentMove,
    isInFlight,
    isNew,
    isLeaving,
    sortAll,
    sortSwimlanes,
    registerHubEvents,
    reset,
    handleSwimlaneConfirm,
    handleSwimlaneDelete,
    handleListConfirm,
    handleListDelete,
    handleCardConfirm,
    handleCardDelete,
    handleTagConfirm,
    handleTagDelete,
    handleCardTagToggle
  };
}
