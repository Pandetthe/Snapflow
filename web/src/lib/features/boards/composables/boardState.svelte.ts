import type { GetBoardByIdResponse, GetBoardDetailsResponse } from '../types/boards.api';
import { noticeStore } from '$lib/ui/stores/notice.svelte';
import { SvelteMap } from 'svelte/reactivity';
import { goto } from '$app/navigation';
import { resolve } from '$app/paths';
import { boardPermissions } from '../state/boardPermissions';
import * as tree from '../state/boardTree';
import { createBoardPresence } from '../state/boardPresence.svelte';
import { createBoardCommands } from '../state/boardCommands.svelte';
import { createBoardHubBindings } from '../state/boardHubBindings.svelte';
import type { ConnectionState } from '../state/boardTypes';

export function createBoardState(
  initialBoard: GetBoardByIdResponse.BoardDto,
  initialMembers: GetBoardDetailsResponse.BoardMemberDto[],
  currentUserId: number | null
) {
  let board = $state(initialBoard);
  let members = $state(initialMembers);
  let connectionState = $state<ConnectionState>('connecting');

  const presence = createBoardPresence(currentUserId);
  const { getRecentMove, isInFlight, isNew, isLeaving } = presence;

  const viewerList = new SvelteMap<number, GetBoardByIdResponse.UserDto>();

  const viewers = $derived.by(() => {
    const all = [...viewerList.values()];
    all.sort((a, b) => a.userName.localeCompare(b.userName));
    return [
      ...all.filter((v) => v.id === currentUserId),
      ...all.filter((v) => v.id !== currentUserId)
    ];
  });

  const permissions = $derived(boardPermissions(members, currentUserId));

  const sortAll = () => tree.sortAll(board);
  const sortSwimlanes = () => tree.sortSwimlanes(board);

  let loaded = $state(false);

  const bindings = createBoardHubBindings({
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
    set connectionState(v) {
      connectionState = v;
    },
    get loaded() {
      return loaded;
    },
    set loaded(v) {
      loaded = v;
    },
    viewerList,
    currentUserId,
    presence,
    leaveBoard
  });

  const { registerHubEvents, reset } = bindings;

  /**
   * Sends the user back to their boards, telling them why the board went away. Nothing failed
   * here, so this is a notice rather than an error; the notice outlives the navigation.
   */
  function leaveBoard(title: string, message: string) {
    noticeStore.add(title, message);
    // invalidateAll so the boards page is loaded again without this board.
    goto(resolve('/'), { invalidateAll: true });
  }

  const commands = createBoardCommands({
    get hub() {
      return bindings.hub;
    },
    get board() {
      return board;
    },
    presence
  });

  const {
    handleSwimlaneConfirm,
    handleSwimlaneDelete,
    handleListConfirm,
    handleListDelete,
    handleCardConfirm,
    handleCardDelete,
    handleTagConfirm,
    handleTagDelete,
    handleCardTagToggle
  } = commands;
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
    get viewers() {
      return viewers;
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
    get isMember() {
      return permissions.isMember;
    },
    get role() {
      return permissions.role;
    },
    get canEditBoard() {
      return permissions.canEditBoard;
    },
    get canManageSwimlanes() {
      return permissions.canManageSwimlanes;
    },
    get canManageLists() {
      return permissions.canManageLists;
    },
    get canManageCards() {
      return permissions.canManageCards;
    },
    get canManageTags() {
      return permissions.canManageTags;
    },
    get canAssignTags() {
      return permissions.canAssignTags;
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
