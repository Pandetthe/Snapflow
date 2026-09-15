import type { TagColor } from './types/boards.api';

/**
 * Tailwind needs the class names spelled out, so each colour keeps its own literal strings:
 * `chip` for a tag shown on a card or in a picker, `swatch` for the colour buttons in the editor.
 */
const styles: Record<TagColor, { chip: string; swatch: string }> = {
  red: {
    chip: 'bg-red-100 text-red-800 dark:bg-red-500/15 dark:text-red-300',
    swatch: 'bg-red-500'
  },
  orange: {
    chip: 'bg-orange-100 text-orange-800 dark:bg-orange-500/15 dark:text-orange-300',
    swatch: 'bg-orange-500'
  },
  yellow: {
    chip: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-500/15 dark:text-yellow-300',
    swatch: 'bg-yellow-500'
  },
  green: {
    chip: 'bg-green-100 text-green-800 dark:bg-green-500/15 dark:text-green-300',
    swatch: 'bg-green-500'
  },
  teal: {
    chip: 'bg-teal-100 text-teal-800 dark:bg-teal-500/15 dark:text-teal-300',
    swatch: 'bg-teal-500'
  },
  blue: {
    chip: 'bg-blue-100 text-blue-800 dark:bg-blue-500/15 dark:text-blue-300',
    swatch: 'bg-blue-500'
  },
  purple: {
    chip: 'bg-purple-100 text-purple-800 dark:bg-purple-500/15 dark:text-purple-300',
    swatch: 'bg-purple-500'
  },
  pink: {
    chip: 'bg-pink-100 text-pink-800 dark:bg-pink-500/15 dark:text-pink-300',
    swatch: 'bg-pink-500'
  },
  gray: {
    chip: 'bg-gray-200 text-gray-800 dark:bg-gray-500/20 dark:text-gray-300',
    swatch: 'bg-gray-500'
  }
};

const fallback = styles.gray;

export function tagChipClass(color: TagColor): string {
  return (styles[color] ?? fallback).chip;
}

export function tagSwatchClass(color: TagColor): string {
  return (styles[color] ?? fallback).swatch;
}
