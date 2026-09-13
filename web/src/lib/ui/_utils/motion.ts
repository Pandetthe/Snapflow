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
