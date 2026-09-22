import type { GetBoardByIdResponse, TagColor } from '../types/boards.api';
import type { Result } from '$lib/core/types/app';
import type { BoardsHub } from '../hub/boards.hub';
import { errorStore } from '$lib/ui/stores/error.svelte';
import { triggerHaptic } from '$lib/ui/utils';
import * as tree from './boardTree';
import type { BoardPresence } from './boardPresence.svelte';

export interface BoardCommandDeps {
  readonly hub: BoardsHub | null;
  readonly board: GetBoardByIdResponse.BoardDto;
  readonly presence: BoardPresence;
}

/** Every change the user makes on the board: create, edit and delete for each kind of element. */
export function createBoardCommands(deps: BoardCommandDeps) {
  const markNew = deps.presence.markNew;
  const sortSwimlanes = () => tree.sortSwimlanes(deps.board);
  const sortLists = tree.sortLists;
  const sortCards = tree.sortCards;
  const sortTags = () => tree.sortTags(deps.board);
  const findCard = (cardId: number) => tree.findCard(deps.board, cardId);
  const forEachCardWithTag = (tagId: number, apply: (card: GetBoardByIdResponse.CardDto) => void) =>
    tree.forEachCardWithTag(deps.board, tagId, apply);

  const hubUnavailable = {
    ok: false,
    problem: { title: 'Board hub unavailable', detail: 'Please try again.' }
  } as const;

  async function handleSwimlaneConfirm(
    editingSwimlane: GetBoardByIdResponse.SwimlaneDto | undefined,
    title: string,
    height: number | null
  ): Promise<Result<unknown>> {
    if (!deps.hub) return hubUnavailable;
    if (editingSwimlane) {
      const res = await deps.hub.updateSwimlane({ id: editingSwimlane.id, title, height });
      if (res?.ok) {
        editingSwimlane.title = title;
        editingSwimlane.height = height;
      }
      return res;
    } else {
      const res = await deps.hub.createSwimlane({ title, height, beforeId: null });
      if (res?.ok) {
        if (!deps.board.swimlanes.some((s) => s.id === res.value.id)) {
          markNew('swimlane', res.value.id);
          deps.board.swimlanes.push({
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
    if (!deps.hub) {
      triggerHaptic('error');
      errorStore.addError('Web.BoardHubUnavailable', 'Board connection is unavailable');
      return false;
    }
    const res = await deps.hub.deleteSwimlane({ id });
    if (!res.ok) {
      triggerHaptic('error');
      errorStore.addError('Web.DeleteSwimlaneFailed', 'Failed to delete swimlane');
      return false;
    }
    const index = deps.board.swimlanes.findIndex((s) => s.id === id);
    if (index !== -1) {
      deps.board.swimlanes.splice(index, 1);
      deps.board.swimlanes = [...deps.board.swimlanes];
    }
    triggerHaptic('success');
    return true;
  }

  async function handleListConfirm(
    editingList: GetBoardByIdResponse.ListDto | undefined,
    targetSwimlaneId: number | null,
    title: string,
    width: number | null
  ): Promise<Result<unknown>> {
    if (!deps.hub) return hubUnavailable;
    if (!editingList && !targetSwimlaneId)
      return {
        ok: false,
        problem: {
          title: 'Invalid target swimlane',
          detail: 'Please choose a swimlane and try again.'
        }
      };
    if (editingList) {
      const res = await deps.hub.updateList({ id: editingList.id, title, width });
      if (res?.ok) {
        editingList.title = title;
        editingList.width = width;
      }
      return res;
    } else {
      const res = await deps.hub.createList({
        swimlaneId: targetSwimlaneId!,
        title,
        width,
        beforeId: null
      });
      if (res?.ok) {
        const swimlane = deps.board.swimlanes.find((s) => s.id === targetSwimlaneId);
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
    if (!deps.hub) {
      triggerHaptic('error');
      errorStore.addError('Web.BoardHubUnavailable', 'Board connection is unavailable');
      return false;
    }
    const res = await deps.hub.deleteList({ id });
    if (!res.ok) {
      triggerHaptic('error');
      errorStore.addError('Web.DeleteListFailed', 'Failed to delete list');
      return false;
    }
    for (const swimlane of deps.board.swimlanes) {
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
  ): Promise<Result<unknown>> {
    if (!deps.hub) return hubUnavailable;
    if (!editingCard && !targetListId)
      return {
        ok: false,
        problem: { title: 'Invalid target list', detail: 'Please choose a list and try again.' }
      };
    if (editingCard) {
      const res = await deps.hub.updateCard({ id: editingCard.id, title, description });
      if (res?.ok) {
        editingCard.title = title;
        editingCard.description = description;
        if (res.value.updatedAt) editingCard.updatedAt = res.value.updatedAt;
        if (res.value.updatedBy) editingCard.updatedBy = res.value.updatedBy;
        await applyCardTags(editingCard.id, tagIds);
      }
      return res;
    } else {
      const res = await deps.hub.createCard({
        listId: targetListId!,
        title,
        description,
        beforeId: null
      });
      if (res?.ok) {
        for (const s of deps.board.swimlanes) {
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
    if (!deps.hub) {
      triggerHaptic('error');
      errorStore.addError('Web.BoardHubUnavailable', 'Board connection is unavailable');
      return false;
    }
    const res = await deps.hub.deleteCard({ id });
    if (!res.ok) {
      triggerHaptic('error');
      errorStore.addError('Web.DeleteCardFailed', 'Failed to delete card');
      return false;
    }
    for (const swimlane of deps.board.swimlanes) {
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
  ): Promise<Result<unknown>> {
    if (!deps.hub) return hubUnavailable;
    if (editingTag) {
      const res = await deps.hub.updateTag({ id: editingTag.id, title, color });
      if (res?.ok) {
        editingTag.title = title;
        editingTag.color = color;
        sortTags();
      }
      return res;
    }
    const res = await deps.hub.createTag({ title, color });
    if (res?.ok && !deps.board.tags.some((t) => t.id === res.value.id)) {
      deps.board.tags.push({ id: res.value.id, title, color });
      sortTags();
    }
    return res;
  }

  async function handleTagDelete(id: number): Promise<boolean> {
    if (!deps.hub) {
      triggerHaptic('error');
      errorStore.addError('Web.BoardHubUnavailable', 'Board connection is unavailable');
      return false;
    }
    const res = await deps.hub.deleteTag({ id });
    if (!res.ok) {
      triggerHaptic('error');
      errorStore.addError('Web.DeleteTagFailed', 'Failed to delete tag');
      return false;
    }
    const index = deps.board.tags.findIndex((t) => t.id === id);
    if (index !== -1) {
      deps.board.tags.splice(index, 1);
      deps.board.tags = [...deps.board.tags];
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

    if (!deps.hub) {
      triggerHaptic('error');
      errorStore.addError('Web.BoardHubUnavailable', 'Board connection is unavailable');
      return false;
    }

    const previous = card.tagIds;
    card.tagIds = add ? [...previous, tagId] : previous.filter((id) => id !== tagId);

    const res = add
      ? await deps.hub.addTagToCard({ cardId, tagId })
      : await deps.hub.removeTagFromCard({ cardId, tagId });

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
