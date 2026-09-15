/** A placeholder and the loaded element that takes its place, if any. */
export type MorphPair = [placeholder: HTMLElement | null, target: HTMLElement | null | undefined];

/**
 * Morphs a skeleton into the loaded page: each placeholder moves and scales onto the element that replaces
 * it, placeholders with nothing to replace (missing or hidden) fade out. Call with the loaded page rendered
 * but still invisible in the same place; resolves when the placeholders have arrived, then show the page.
 */
export async function morphPlaceholders(pairs: MorphPair[], durationMs: number): Promise<void> {
  const animations = pairs.flatMap(([placeholder, target]) => {
    if (!placeholder) return [];

    const from = placeholder.getBoundingClientRect();
    const to = target?.getBoundingClientRect();
    placeholder.style.transformOrigin = 'top left';

    if (!to || to.width === 0 || to.height === 0 || from.width === 0 || from.height === 0) {
      return [
        placeholder.animate([{ opacity: 1 }, { opacity: 0, transform: 'scale(0.97)' }], {
          duration: durationMs * 0.6,
          easing: 'cubic-bezier(0.4, 0, 1, 1)',
          fill: 'forwards'
        })
      ];
    }

    const dx = to.left - from.left;
    const dy = to.top - from.top;
    return [
      placeholder.animate(
        [
          { transform: 'none' },
          {
            transform: `translate(${dx}px, ${dy}px) scale(${to.width / from.width}, ${to.height / from.height})`
          }
        ],
        { duration: durationMs, easing: 'cubic-bezier(0.22, 1, 0.36, 1)', fill: 'forwards' }
      )
    ];
  });

  // Browsers may pause animations in background tabs; the page must still show.
  const timeout = new Promise((resolve) => setTimeout(resolve, durationMs + 100));
  await Promise.race([Promise.all(animations.map((a) => a.finished.catch(() => {}))), timeout]);
}
