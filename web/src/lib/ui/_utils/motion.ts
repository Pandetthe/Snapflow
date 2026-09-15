import { cubicIn, quintOut } from 'svelte/easing';

/**
 * Enter/exit animation for floating menus (dropdown menus, selects, popovers): in the rhythm of dialogs
 * but quicker — a small zoom and slide from the trigger side with the ease-flow curve, and a faster
 * ease-in exit. bits-ui sets --bits-floating-transform-origin on the floating wrapper.
 * Written as full class names so Tailwind generates them.
 */
export const floatingMotionClass = [
  'origin-(--bits-floating-transform-origin) will-change-[opacity,transform]',
  'data-[state=open]:animate-in data-[state=open]:fade-in-0 data-[state=open]:zoom-in-95 data-[state=open]:duration-200 data-[state=open]:ease-flow',
  'data-[side=bottom]:slide-in-from-top-1 data-[side=top]:slide-in-from-bottom-1 data-[side=left]:slide-in-from-right-1 data-[side=right]:slide-in-from-left-1',
  'data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=closed]:zoom-out-95 data-[state=closed]:duration-150 data-[state=closed]:ease-in'
].join(' ');

// Svelte transitions do not follow prefers-reduced-motion on their own.
const reduceMotion =
  typeof window !== 'undefined' && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
const ms = (duration: number) => (reduceMotion ? 0 : duration);

/** Inline content opening and closing (field messages, expandable sections); quintOut matches ease-flow. */
export const slideReveal = { axis: 'y', duration: ms(240), easing: quintOut } as const;

/** An item entering a list. */
export const itemIn = { y: 8, duration: ms(280), easing: quintOut };

/** An item leaving a list, quicker than it came in. */
export const itemOut = { duration: ms(160), easing: cubicIn };

export const placeholderOut = { duration: ms(120), easing: cubicIn };
