import { WebHaptics } from 'web-haptics';
import { browser } from '$app/environment';

export type HapticPreset = 'success' | 'warning' | 'error' | 'light' | 'medium' | 'heavy' | 'soft' | 'rigid' | 'selection' | 'nudge' | 'buzz';
export const haptics = browser ? new WebHaptics({
  debug: true
}) : null;

/**
 * Triggers haptic feedback using the global singleton instance.
 * Safe to call even on server (does nothing).
 */
export function triggerHaptic(preset: HapticPreset | number | number[]) {
  if (haptics) {
    haptics.trigger(preset);
  }
}
