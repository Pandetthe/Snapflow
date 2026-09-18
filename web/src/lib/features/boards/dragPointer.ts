/**
 * Where the pointer is while a list is dragged, kept from the moment the drag starts.
 * The drop zone on a swimlane's header bar holds no lists of its own, so it reads the pointer to work out
 * which list of that swimlane the drop landed next to (components/Swimlane.svelte).
 */
let pointerX: number | null = null;

function remember(event: PointerEvent) {
  pointerX = event.clientX;
}

export function trackDragPointer(): void {
  window.addEventListener('pointermove', remember, { passive: true });
}

export function forgetDragPointer(): void {
  window.removeEventListener('pointermove', remember);
  pointerX = null;
}

export function dragPointerX(): number | null {
  return pointerX;
}
