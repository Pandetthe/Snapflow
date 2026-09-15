import { LAYOUT_FLIP_MS } from './motion';

/*
  While a swimlane is dragged every swimlane folds down to its header bar, so the whole board fits on a few screens
  and the swimlane can be carried without scrolling. Heights are held at their measured size first, so the fold has
  a definite height to ease from (styles/board-dnd.css).
*/

const REST_HEIGHT_VAR = '--swimlane-rest-height';
const DRAGGED_EL_ID = 'dnd-action-dragged-el';
// Header (h-11) plus the swimlane's bottom border
const FOLDED_HEIGHT = 'calc(2.75rem + 1px)';

let unfoldTimer: number | undefined;
let anchorFrame: number | undefined;

function swimlaneZone() {
  return document.querySelector<HTMLElement>('section[data-board-zone="swimlanes"]');
}

function swimlaneItems(zone: HTMLElement) {
  return zone.querySelectorAll<HTMLElement>(
    `:scope > [data-board-slot="swimlane"]:not(#${DRAGGED_EL_ID}) > [data-board-item="swimlane"]`
  );
}

/**
 * Keeps the dragged swimlane's slot where it was on screen while the swimlanes above it fold,
 * so the pointer stays over it instead of landing on another swimlane and moving it right away.
 */
function anchorWhileFolding(slot: HTMLElement | null) {
  if (anchorFrame !== undefined) cancelAnimationFrame(anchorFrame);
  anchorFrame = undefined;
  if (!slot) return;

  const top = slot.getBoundingClientRect().top;
  const until = performance.now() + LAYOUT_FLIP_MS + 50;
  const follow = () => {
    const drift = slot.getBoundingClientRect().top - top;
    if (Math.abs(drift) >= 1) window.scrollBy(0, drift);
    anchorFrame = performance.now() < until ? requestAnimationFrame(follow) : undefined;
  };
  follow();
}

export function foldSwimlanes(draggedId: number): void {
  const zone = swimlaneZone();
  if (!zone) return;

  window.clearTimeout(unfoldTimer);
  if (!zone.hasAttribute('data-folded')) {
    for (const item of swimlaneItems(zone)) {
      item.style.setProperty(REST_HEIGHT_VAR, `${item.getBoundingClientRect().height}px`);
    }
    zone.setAttribute('data-fold-held', '');
    void zone.offsetHeight;
  }
  zone.setAttribute('data-folded', '');

  const dragged = document.getElementById(DRAGGED_EL_ID);
  if (dragged) {
    dragged.style.transition += `, height ${LAYOUT_FLIP_MS}ms cubic-bezier(0.22, 1, 0.36, 1)`;
    dragged.style.height = FOLDED_HEIGHT;
  }

  anchorWhileFolding(
    zone.querySelector(`:scope > [data-board-slot="swimlane"] > [data-swimlane-id="${draggedId}"]`)
      ?.parentElement ?? null
  );
}

export function unfoldSwimlanes(): void {
  const zone = swimlaneZone();
  if (anchorFrame !== undefined) cancelAnimationFrame(anchorFrame);
  anchorFrame = undefined;
  if (!zone?.hasAttribute('data-folded')) return;

  zone.removeAttribute('data-folded');
  window.clearTimeout(unfoldTimer);
  // Heights keep easing towards the held size, then go back to following their content
  unfoldTimer = window.setTimeout(() => {
    zone.removeAttribute('data-fold-held');
    for (const item of swimlaneItems(zone)) item.style.removeProperty(REST_HEIGHT_VAR);
  }, LAYOUT_FLIP_MS);
}
