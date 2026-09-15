<script lang="ts">
  import type { BoardAction, RecentMove } from '$lib/features/boards/composables/boardState.svelte';
  import { cubicInOut, cubicOut } from 'svelte/easing';
  import type { TransitionConfig } from 'svelte/transition';
  import { UserAvatar } from '$lib/ui/components';
  import { Move, Pencil, Plus, Trash2 } from 'lucide-svelte';

  /**
   * Who added, edited, moved or deleted the element, with an icon for what they did.
   * The label sits on the element's top edge near its right corner, the same for cards, lists and swimlanes,
   * so the host element must not clip its overflow.
   */
  let {
    move,
    rounded = 'rounded-lg'
  }: {
    move: RecentMove | undefined;
    rounded?: string;
  } = $props();

  const ACTIONS: Record<BoardAction, { icon: typeof Move; label: string }> = {
    added: { icon: Plus, label: 'added this' },
    edited: { icon: Pencil, label: 'edited this' },
    moved: { icon: Move, label: 'moved this' },
    deleted: { icon: Trash2, label: 'deleted this' }
  };

  /** Rises onto the edge it is anchored to; the outro plays the same motion in reverse, a bit faster. */
  function pill(
    _node: Element,
    { duration, easing }: { duration: number; easing: (t: number) => number }
  ): TransitionConfig {
    const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    return {
      duration: reduceMotion ? 0 : duration,
      easing,
      css: (t, u) => `opacity: ${t}; transform: translateY(${u * 3}px) scale(${0.92 + 0.08 * t});`
    };
  }
</script>

<!--
  Keyed by move, so a new move replaces the label with a fresh animation.
  Transitions are local on purpose: a global outro would keep the whole host element
  (e.g. a card moved to another list) in the DOM until the fade finishes.
-->
{#each move ? [move] : [] as m (m.key)}
  {@const action = ACTIONS[m.action]}
  <span aria-hidden="true" class="moved-by-ring pointer-events-none absolute inset-0 {rounded}"
  ></span>
  <div
    class="pointer-events-none absolute -top-2 right-3 z-30 flex origin-bottom-right items-center"
    in:pill={{ duration: 280, easing: cubicOut }}
    out:pill={{ duration: 220, easing: cubicInOut }}
  >
    <span
      role="status"
      class="flex items-center gap-1 rounded-full bg-brand-500 py-0.5 pr-2 pl-0.5 text-[10px] leading-none font-medium text-white shadow-md shadow-brand-500/30"
    >
      <UserAvatar src={m.user.avatarUrl} name={m.user.userName} size={14} class="ring-white/70" />
      <span class="max-w-24 truncate">{m.isCurrentUser ? 'You' : m.user.userName}</span>
      <action.icon class="h-2.5 w-2.5 shrink-0 opacity-80" aria-hidden="true" />
      <span class="sr-only">{action.label}</span>
    </span>
  </div>
{/each}

<style>
  .moved-by-ring {
    animation: moved-by-ring 1800ms ease-in-out forwards;
  }

  /* Fades in instead of starting at full strength, then eases out */
  @keyframes moved-by-ring {
    0% {
      background-color: transparent;
      box-shadow:
        inset 0 0 0 1.5px transparent,
        0 0 0 0 transparent;
    }
    15% {
      background-color: color-mix(in srgb, var(--color-brand-500) 8%, transparent);
      box-shadow:
        inset 0 0 0 1.5px var(--color-brand-500),
        0 0 0 0 color-mix(in srgb, var(--color-brand-500) 35%, transparent);
    }
    60% {
      background-color: color-mix(in srgb, var(--color-brand-500) 4%, transparent);
      box-shadow:
        inset 0 0 0 1.5px color-mix(in srgb, var(--color-brand-400) 70%, transparent),
        0 0 0 6px transparent;
    }
    100% {
      background-color: transparent;
      box-shadow:
        inset 0 0 0 1.5px transparent,
        0 0 0 0 transparent;
    }
  }

  /*
    Remote move hand-over (animations/elementFlight.ts), shared by cards, lists and swimlanes:
    the element keeps its slot while the ghost flies, then settles from the ghost's landing pose.
  */
  :global(.flight-hidden) {
    visibility: hidden;
  }

  :global(.flight-settle) {
    transform-origin: top left;
    animation: flight-settle 420ms cubic-bezier(0.22, 1, 0.36, 1);
  }

  @keyframes -global-flight-settle {
    from {
      transform: translateY(-3px) scale(var(--flight-landing-scale, 1));
      box-shadow: 0 10px 20px -8px rgba(70, 95, 255, 0.3);
    }
    to {
      transform: none;
    }
  }

  @media (prefers-reduced-motion: reduce) {
    .moved-by-ring,
    :global(.flight-settle) {
      animation: none;
    }
  }
</style>
