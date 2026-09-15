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
