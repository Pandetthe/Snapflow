import { describe, it, expect } from 'vitest';
import { flushSync } from 'svelte';
import { createBoardState } from './boardState.svelte';
import type { GetBoardByIdResponse, GetBoardDetailsResponse } from '../types/boards.api';

function board(): GetBoardByIdResponse.BoardDto {
  return {
    id: 1,
    title: 'board',
    description: '',
    visibility: 'private',
    swimlanes: [],
    tags: []
  };
}

function member(
  id: number,
  role: GetBoardDetailsResponse.BoardMemberDto['role']
): GetBoardDetailsResponse.BoardMemberDto {
  return { id, role, userName: `user${id}`, avatarUrl: null };
}

describe('createBoardState permissions', () => {
  it('does not wake readers when a member change leaves the permission alone', () => {
    const bs = createBoardState(board(), [member(1, 'admin')], 1);

    let reads = 0;
    const cleanup = $effect.root(() => {
      $effect(() => {
        void bs.canManageCards;
        reads += 1;
      });
    });
    flushSync();
    expect(reads).toBe(1);

    bs.members = [member(1, 'admin'), member(2, 'viewer')];
    flushSync();

    expect(reads).toBe(1);
    cleanup();
  });

  it('wakes readers when the permission actually changes', () => {
    const bs = createBoardState(board(), [member(1, 'viewer')], 1);

    let latest: boolean | undefined;
    const cleanup = $effect.root(() => {
      $effect(() => {
        latest = bs.canManageCards;
      });
    });
    flushSync();
    expect(latest).toBe(false);

    bs.members = [member(1, 'admin')];
    flushSync();

    expect(latest).toBe(true);
    cleanup();
  });
});
