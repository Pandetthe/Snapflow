import type { SearchUserDto } from '$lib/features/users/api/users';
import type { MemberRole } from './boards.api';

export type SelectedMember = SearchUserDto & { role: MemberRole };

export type OwnerMember = SearchUserDto & { role: 'owner' };
