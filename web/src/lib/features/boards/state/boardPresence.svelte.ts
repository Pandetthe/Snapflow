import { SvelteMap, SvelteSet } from 'svelte/reactivity';
import { tick } from 'svelte';
import type { GetBoardByIdResponse } from '../types/boards.api';
import { startElementFlight, type ElementFlight } from '../animations/elementFlight';
import { CHANGE_FEEDBACK_MS, ITEM_LEAVE_MS } from '../animations/motion';
import type { MovableKind } from './boardTypes';

export type { MovableKind };

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

const NEW_ITEM_DURATION_MS = 700;

// A deleted element shows who deleted it for as long as any other change reads, then leaves.
const LEAVING_ITEM_DURATION_MS = CHANGE_FEEDBACK_MS + ITEM_LEAVE_MS;

/**
 * Tracks what changed on the board and who changed it, and plays the arrival and departure
 * animations for changes that arrived over the hub.
 */
export function createBoardPresence(currentUserId: number | null) {
  const recentMoves = new SvelteMap<string, RecentMove>();
  let recentMoveKey = 0;

  function markChanged(
    kind: MovableKind,
    id: number,
    user: GetBoardByIdResponse.UserDto | null,
    action: BoardAction
  ) {
    if (!user) return;
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
    }, CHANGE_FEEDBACK_MS);
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
    user: GetBoardByIdResponse.UserDto | null,
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
    user: GetBoardByIdResponse.UserDto | null,
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

  return {
    markChanged,
    markNew,
    getRecentMove,
    isInFlight,
    isNew,
    isLeaving,
    animateRemoteMove,
    animateRemoteDelete
  };
}

export type BoardPresence = ReturnType<typeof createBoardPresence>;
