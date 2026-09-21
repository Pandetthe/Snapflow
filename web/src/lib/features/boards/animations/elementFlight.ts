import type { MovableKind } from '../state/boardTypes';

export interface ElementFlight {
  /** Animates the ghost to the element's current (new) position; the ghost holds its final pose until released. */
  land: () => Promise<void>;
  /** Removes the ghost; call once the real element is visible at its new place. */
  release: () => void;
  /** Stops the flight and removes the ghost immediately. */
  cancel: () => void;
}

interface FlightMotion {
  /** Lift at the start of the flight, in px. */
  lift: number;
  liftScale: number;
  /** Tilt while lifted, in degrees. */
  rotate: number;
  /** Scale of the landing pose; must match --flight-landing-scale on the element (default 1). */
  landingScale: number;
}

// Big elements get gentler motion: a few percent of a swimlane's width is a lot of pixels.
const MOTION: Record<MovableKind, FlightMotion> = {
  card: { lift: 4, liftScale: 1.03, rotate: 0.8, landingScale: 1.015 },
  list: { lift: 4, liftScale: 1.012, rotate: 0.3, landingScale: 1 },
  swimlane: { lift: 3, liftScale: 1.006, rotate: 0.12, landingScale: 1 }
};

/** Landing pose the real element settles from (flight-settle in MovedByIndicator.svelte). */
const LANDING_LIFT_PX = 3;
const LANDING_SHADOW = '0 10px 20px -8px rgba(70, 95, 255, 0.3)';

const ID_ATTRIBUTES = ['data-card-id', 'data-list-id', 'data-swimlane-id'];

/** Rendered elements for an item, skipping ones Svelte keeps in the DOM while they transition out. */
const findElements = (kind: MovableKind, id: number) =>
  [...document.querySelectorAll<HTMLElement>(`[data-${kind}-id="${id}"]`)].filter(
    (el) => !el.closest('[inert]')
  );

/**
 * Where an element is laid out, ignoring `transform` on it and its ancestors.
 * Right after a reorder, animate:flip holds the element at its old place with a transform (first a
 * zero-length filled animation, then the real one), so the bounding rect alone points to the old spot.
 * Tailwind's translate/scale utilities use the separate `translate`/`scale` properties and are not affected.
 */
function layoutRect(el: HTMLElement): DOMRect {
  const rect = el.getBoundingClientRect();
  let offsetX = 0;
  let offsetY = 0;
  for (
    let node: HTMLElement | null = el;
    node && node !== document.body;
    node = node.parentElement
  ) {
    const transform = getComputedStyle(node).transform;
    if (transform && transform !== 'none') {
      const matrix = new DOMMatrixReadOnly(transform);
      offsetX += matrix.m41;
      offsetY += matrix.m42;
    }
  }
  return new DOMRect(rect.left - offsetX, rect.top - offsetY, rect.width, rect.height);
}

function createGhost(source: HTMLElement, from: DOMRect): HTMLElement {
  const ghost = source.cloneNode(true) as HTMLElement;
  ghost.classList.remove('flight-hidden', 'flight-settle');
  // Nested cards/lists in the clone must not be found as the real ones.
  for (const attribute of ID_ATTRIBUTES) {
    ghost.removeAttribute(attribute);
    ghost.querySelectorAll(`[${attribute}]`).forEach((el) => el.removeAttribute(attribute));
  }
  ghost.setAttribute('aria-hidden', 'true');
  Object.assign(ghost.style, {
    position: 'fixed',
    left: `${from.left}px`,
    top: `${from.top}px`,
    width: `${from.width}px`,
    height: `${from.height}px`,
    margin: '0',
    zIndex: '60',
    pointerEvents: 'none',
    transformOrigin: 'top left',
    boxSizing: 'border-box'
  });
  document.body.appendChild(ghost);
  return ghost;
}

function captureSource(
  kind: MovableKind,
  id: number
): { source: HTMLElement; from: DOMRect } | null {
  if (typeof document === 'undefined') return null;
  const source = findElements(kind, id)[0];
  if (!source) return null;
  const from = source.getBoundingClientRect();
  if (from.width === 0 || from.height === 0) return null;
  return { source, from };
}

/**
 * Captures a card, list or swimlane at its current position as a floating ghost, so a remote move
 * can be shown as the element travelling from its old place to the new one. Call before the move is applied.
 */
export function startElementFlight(kind: MovableKind, id: number): ElementFlight | null {
  if (
    typeof window !== 'undefined' &&
    window.matchMedia('(prefers-reduced-motion: reduce)').matches
  )
    return null;

  const captured = captureSource(kind, id);
  if (!captured) return null;
  const { source, from } = captured;

  const motion = MOTION[kind];
  const ghost = createGhost(source, from);
  // The highlight ring plays once, on the landed element.
  ghost.querySelectorAll('.moved-by-ring').forEach((el) => el.remove());

  let animation: Animation | null = null;

  return {
    async land() {
      // Moving to another parent renders a new element while the old one may still be in the DOM;
      // reordering within the same parent keeps (and moves) the original element.
      const candidates = findElements(kind, id);
      const target =
        candidates.find((el) => el !== source) ??
        (candidates.includes(source) ? source : undefined);
      const to = target ? layoutRect(target) : undefined;

      if (!to || to.width === 0) {
        animation = ghost.animate(
          [
            { opacity: 1, transform: 'scale(1)' },
            { opacity: 0, transform: 'scale(0.97)' }
          ],
          { duration: 220, easing: 'cubic-bezier(0.4, 0, 0.2, 1)', fill: 'forwards' }
        );
      } else {
        const dx = to.left - from.left;
        const dy = to.top - from.top;
        const sx = (to.width / from.width) * motion.landingScale;
        const sy = (to.height / from.height) * motion.landingScale;
        const duration = Math.min(750, Math.max(450, Math.hypot(dx, dy) * 0.6));

        // Two phases with their own easing: a soft lift, then an ease-in-out glide to the landing pose.
        animation = ghost.animate(
          [
            {
              offset: 0,
              transform: 'translate(0px, 0px) scale(1) rotate(0deg)',
              boxShadow: '0 1px 2px rgba(0, 0, 0, 0.05)',
              easing: 'cubic-bezier(0.33, 1, 0.68, 1)'
            },
            {
              offset: 0.22,
              transform: `translate(0px, -${motion.lift}px) scale(${motion.liftScale}) rotate(${motion.rotate}deg)`,
              boxShadow: '0 14px 26px -10px rgba(70, 95, 255, 0.3)',
              easing: 'cubic-bezier(0.65, 0, 0.35, 1)'
            },
            {
              offset: 1,
              transform: `translate(${dx}px, ${dy - LANDING_LIFT_PX}px) scale(${sx}, ${sy}) rotate(0deg)`,
              boxShadow: LANDING_SHADOW
            }
          ],
          { duration, fill: 'forwards' }
        );
      }

      // Browsers may pause animations in background tabs; never leave the real element hidden for long.
      const running = animation;
      const timeout = new Promise((resolve) =>
        setTimeout(resolve, Number(running.effect?.getTiming().duration ?? 0) + 150)
      );
      await Promise.race([running.finished.catch(() => {}), timeout]);
    },
    release() {
      ghost.remove();
    },
    cancel() {
      animation?.cancel();
      ghost.remove();
    }
  };
}
