import { LAYOUT_FLIP_MS } from './motion';

const ZONE_HELD_HEIGHT_VAR = '--board-zone-held-height';
const DRAGGED_LIST_HEIGHT_VAR = '--board-dragged-list-height';
const DRAGGED_EL_ID = 'dnd-action-dragged-el';

export type DraggedItem = { list: number } | { card: number };

function listZones() {
  return document.querySelectorAll<HTMLElement>('section[data-board-zone="lists"]');
}

function slotOf(dragged: DraggedItem) {
  const selector =
    'list' in dragged
      ? `section[data-board-zone="lists"] [data-list-id="${dragged.list}"]`
      : `section[data-board-zone="cards"] [data-card-id="${dragged.card}"]`;
  return document.querySelector(selector)?.closest<HTMLElement>('[data-board-slot]') ?? null;
}

/*
  Measures what each list zone needs with the dragged item out of it, by taking the item's slot out of the
  layout for the measurement. Reading the heights instead of adding the item's own up keeps whatever else
  decides them, from a swimlane's fixed height to the row stretching into space no list fills.

  svelte-dnd-action pins the zone the item came from to its size for the whole drag (preventShrinking), which
  the height below takes over: it holds the row open where the drag can still put the item back, and gives way
  where it cannot. The pin is inline and set in this same task, so dropping it here is not something the page
  has shown yet, and the library restoring it on drop writes back the same empty value.

  Nothing may read the layout between restoring the slot and the browser's own next pass, or the zones would
  transition from the height measured without the item to the one they show.
*/
export function holdListZoneHeights(dragged: DraggedItem): void {
  const zones = [...listZones()];
  for (const zone of zones) zone.style.removeProperty(ZONE_HELD_HEIGHT_VAR);

  const slot = slotOf(dragged);
  slot?.parentElement?.style.removeProperty('min-height');

  const display = slot?.style.display ?? '';
  if (slot) slot.style.display = 'none';
  const heights = zones.map((zone) => Math.round(zone.getBoundingClientRect().height));
  if (slot) slot.style.display = display;

  zones.forEach((zone, index) => {
    zone.style.setProperty(ZONE_HELD_HEIGHT_VAR, `${heights[index]}px`);
  });
}

export function releaseListZoneHeights(): void {
  for (const zone of listZones()) zone.style.removeProperty(ZONE_HELD_HEIGHT_VAR);
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

  const held = parseFloat(zone.style.getPropertyValue(ZONE_HELD_HEIGHT_VAR)) || 0;
  const content =
    parseFloat(document.documentElement.style.getPropertyValue(DRAGGED_LIST_HEIGHT_VAR)) || 0;
  const height = Math.max(held, content);
  if (!height) return;

  if (dragged.dataset.listSized === undefined) {
    dragged.dataset.listSized = '';
    dragged.style.transition += `, height ${LAYOUT_FLIP_MS}ms cubic-bezier(0.22, 1, 0.36, 1)`;
  }
  dragged.style.height = `${height}px`;
}
