/*
  Swimlanes resizing for a dragged list or card (board-dnd.css). A swimlane's height follows its content and
  cannot transition, so the list zone carries it: while an item is dragged every list zone holds the height
  measured here at drag start, only the zone receiving the item grows to fit it, and after the drop the zones
  ease back to their content height.
*/
const ZONE_REST_HEIGHT_VAR = '--board-zone-rest-height';

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
