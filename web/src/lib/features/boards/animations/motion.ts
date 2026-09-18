import { cubicOut } from 'svelte/easing';

const reduceMotion = typeof window !== 'undefined' && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

/**
 * Siblings making room for, or closing the gap after, a moved swimlane, list or card.
 * Longer and softer than the default flip, in the style of the remote-move flight (elementFlight.ts).
 * svelte-dnd-action's `flipDurationMs` must be equal to the flip duration.
 */
export const LAYOUT_FLIP_MS = reduceMotion ? 0 : 280;
export const layoutFlip = { duration: LAYOUT_FLIP_MS, easing: cubicOut };

/**
 * How long an element shows that someone added, edited, moved or deleted it: the label, the ring around
 * the element, and the wait before a deleted element leaves. The same for every action, so each one reads
 * for the same time. `--board-change-feedback` in styles/board-dnd.css must be equal to it.
 * Not shortened for reduced motion: the label is information, not movement.
 */
export const CHANGE_FEEDBACK_MS = 2500;

/**
 * The leave that follows a deleted element's feedback: the element fades and its slot closes.
 * Must be equal to the board-leave animations in styles/board-dnd.css, delays included.
 */
export const ITEM_LEAVE_MS = 480;
