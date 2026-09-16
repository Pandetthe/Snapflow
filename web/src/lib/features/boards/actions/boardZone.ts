import type { ActionReturn } from 'svelte/action';
import {
  dndzone,
  dragHandleZone,
  type DndZoneAttributes,
  type Item,
  type Options
} from 'svelte-dnd-action';

export type BoardZoneOptions<T extends Item> = Options<T> & { useHandle: boolean };

function zoneOptionsOf<T extends Item>(options: BoardZoneOptions<T>): Options<T> {
  const zoneOptions: Partial<BoardZoneOptions<T>> = { ...options };
  delete zoneOptions.useHandle;
  return zoneOptions as Options<T>;
}

function createZone<T extends Item>(node: HTMLElement, options: BoardZoneOptions<T>) {
  return options.useHandle
    ? dragHandleZone(node, zoneOptionsOf(options))
    : dndzone(node, zoneOptionsOf(options));
}

export function boardZone<T extends Item>(
  node: HTMLElement,
  options: BoardZoneOptions<T>
): ActionReturn<BoardZoneOptions<T>, DndZoneAttributes<T>> {
  let useHandle = options.useHandle;
  let zone = createZone(node, options);

  return {
    update(next: BoardZoneOptions<T>) {
      if (next.useHandle === useHandle) {
        zone.update?.(zoneOptionsOf(next));
        return;
      }
      useHandle = next.useHandle;
      zone.destroy?.();
      zone = createZone(node, next);
    },
    destroy() {
      zone.destroy?.();
    }
  };
}
