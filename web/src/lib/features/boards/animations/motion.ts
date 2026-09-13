import { cubicOut } from 'svelte/easing';

const reduceMotion = typeof window !== 'undefined' && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

/**
 * Siblings making room for, or closing the gap after, a moved swimlane, list or card.
 * Longer and softer than the default flip, in the style of the remote-move flight (elementFlight.ts).
 * svelte-dnd-action's `flipDurationMs` must be equal to the flip duration.
 */
export const LAYOUT_FLIP_MS = reduceMotion ? 0 : 280;
export const layoutFlip = { duration: LAYOUT_FLIP_MS, easing: cubicOut };
