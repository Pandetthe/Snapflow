import { LAYOUT_FLIP_MS } from './motion';

const ZONE_REST_HEIGHT_VAR = '--board-zone-rest-height';
const DRAGGED_LIST_HEIGHT_VAR = '--board-dragged-list-height';
const DRAGGED_EL_ID = 'dnd-action-dragged-el';

function listZones() {
  return document.querySelectorAll<HTMLElement>('section[data-board-zone="lists"]');
}

export function holdListZoneHeights(): void {
  for (const zone of listZones()) {
    zone.style.setProperty(
      ZONE_REST_HEIGHT_VAR,
      `${Math.round(zone.getBoundingClientRect().height)}px`
    );
  }
}

export function releaseListZoneHeights(): void {
  for (const zone of listZones()) zone.style.removeProperty(ZONE_REST_HEIGHT_VAR);
}

export function measureDraggedList(listId: number): void {
  const listEl = document.querySelector<HTMLElement>(`[data-list-id="${listId}"]`);
  const zone = listEl?.querySelector<HTMLElement>('section[data-board-zone="cards"]');
  if (!listEl || !zone) return;

  const style = getComputedStyle(zone);
  const padding = parseFloat(style.paddingTop) + parseFloat(style.paddingBottom);
  const gap = parseFloat(style.rowGap) || 0;
  const slots = [...zone.querySelectorAll<HTMLElement>(':scope > [data-board-slot="card"]')];
  const cardsHeight =
    slots.reduce((sum, slot) => sum + slot.offsetHeight, 0) + gap * Math.max(slots.length - 1, 0);
  const chrome = listEl.offsetHeight - (zone.offsetHeight - padding);
  const minContent = Math.max(parseFloat(style.minHeight) - padding, 0);

  document.documentElement.style.setProperty(
    DRAGGED_LIST_HEIGHT_VAR,
    `${Math.round(chrome + Math.max(cardsHeight, minContent))}px`
  );
}

export function forgetDraggedList(): void {
  document.documentElement.style.removeProperty(DRAGGED_LIST_HEIGHT_VAR);
}

export function sizeDraggedList(zone: HTMLElement): void {
  const dragged = document.getElementById(DRAGGED_EL_ID);
  if (!dragged) return;

  const rest = parseFloat(zone.style.getPropertyValue(ZONE_REST_HEIGHT_VAR)) || 0;
  const content =
    parseFloat(document.documentElement.style.getPropertyValue(DRAGGED_LIST_HEIGHT_VAR)) || 0;
  const height = Math.max(rest, content);
  if (!height) return;

  if (dragged.dataset.listSized === undefined) {
    dragged.dataset.listSized = '';
    dragged.style.transition += `, height ${LAYOUT_FLIP_MS}ms cubic-bezier(0.22, 1, 0.36, 1)`;
  }
  dragged.style.height = `${height}px`;
}
