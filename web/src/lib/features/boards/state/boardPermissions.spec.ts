import { describe, it, expect } from 'vitest';
import { boardPermissions } from './boardPermissions';
import type { GetBoardDetailsResponse, MemberRole } from '../types/boards.api';

function members(
  ...entries: [id: number, role: MemberRole][]
): GetBoardDetailsResponse.BoardMemberDto[] {
  return entries.map(([id, role]) => ({ id, role, userName: `user${id}`, avatarUrl: null }));
}

describe('boardPermissions', () => {
  it('treats someone absent from the member list as a viewer', () => {
    const permissions = boardPermissions(members([1, 'owner']), 99);

    expect(permissions.isMember).toBe(false);
    expect(permissions.role).toBe('viewer');
    expect(permissions.canManageCards).toBe(false);
    expect(permissions.canAssignTags).toBe(false);
  });

  it('treats a signed-out visitor as a viewer', () => {
    const permissions = boardPermissions(members([1, 'owner']), null);

    expect(permissions.isMember).toBe(false);
    expect(permissions.role).toBe('viewer');
  });

  it('gives an owner every permission', () => {
    const permissions = boardPermissions(members([1, 'owner']), 1);

    expect(permissions).toEqual({
      isMember: true,
      role: 'owner',
      canEditBoard: true,
      canManageSwimlanes: true,
      canManageLists: true,
      canManageCards: true,
      canManageTags: true,
      canAssignTags: true
    });
  });

  it('gives an admin the same permissions as an owner', () => {
    const owner = boardPermissions(members([1, 'owner']), 1);
    const admin = boardPermissions(members([2, 'admin']), 2);

    expect({ ...admin, role: 'owner' }).toEqual(owner);
  });

  it('lets a member work on cards and tags but not on the board layout', () => {
    const permissions = boardPermissions(members([3, 'member']), 3);

    expect(permissions.canManageCards).toBe(true);
    expect(permissions.canAssignTags).toBe(true);
    expect(permissions.canEditBoard).toBe(false);
    expect(permissions.canManageSwimlanes).toBe(false);
    expect(permissions.canManageLists).toBe(false);
    expect(permissions.canManageTags).toBe(false);
  });

  it('lets a viewer change nothing', () => {
    const permissions = boardPermissions(members([4, 'viewer']), 4);

    expect(permissions.isMember).toBe(true);
    expect(permissions.canEditBoard).toBe(false);
    expect(permissions.canManageSwimlanes).toBe(false);
    expect(permissions.canManageLists).toBe(false);
    expect(permissions.canManageCards).toBe(false);
    expect(permissions.canManageTags).toBe(false);
    expect(permissions.canAssignTags).toBe(false);
  });

  it('separates defining a tag from putting one on a card', () => {
    const member = boardPermissions(members([3, 'member']), 3);

    expect(member.canAssignTags).toBe(true);
    expect(member.canManageTags).toBe(false);
  });

  it('picks the current user out of a board with several members', () => {
    const list = members([1, 'owner'], [2, 'admin'], [3, 'member'], [4, 'viewer']);

    expect(boardPermissions(list, 3).role).toBe('member');
    expect(boardPermissions(list, 2).role).toBe('admin');
  });
});
