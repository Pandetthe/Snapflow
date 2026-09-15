const REM_PX = 16;

export function toRem(stored: number): number {
  return stored / REM_PX;
}

export function fromRem(rem: number): number {
  return Math.round(rem * REM_PX);
}

export function storedToCss(stored: number): string {
  return `${toRem(stored)}rem`;
}
