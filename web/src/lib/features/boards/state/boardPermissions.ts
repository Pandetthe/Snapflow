import type { GetBoardDetailsResponse, MemberRole } from '../types/boards.api';

export interface BoardPermissions {
  isMember: boolean;
  role: MemberRole;
  canEditBoard: boolean;
  canManageSwimlanes: boolean;
  canManageLists: boolean;
  canManageCards: boolean;
  canManageTags: boolean;
  canAssignTags: boolean;
}

export function boardPermissions(
  members: GetBoardDetailsResponse.BoardMemberDto[],
  currentUserId: number | null
): BoardPermissions {
  const member = members.find((m) => m.id === currentUserId);
  const role: MemberRole = member?.role ?? 'viewer';
  const isAdmin = role === 'owner' || role === 'admin';

  return {
    isMember: member !== undefined,
    role,
    canEditBoard: isAdmin,
    canManageSwimlanes: isAdmin,
    canManageLists: isAdmin,
    canManageCards: role !== 'viewer',
    // Defining the board's tags is an admin job; putting one on a card is not.
    canManageTags: isAdmin,
    canAssignTags: role !== 'viewer'
  };
}
