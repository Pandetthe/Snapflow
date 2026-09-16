export namespace GetBoardsResponse {
  export interface UserDto {
    id: number;
    userName: string;
  }

  export interface BoardDto {
    id: number;
    title: string;
    description: string;
    createdAt: string;
    createdBy: UserDto;
    updatedAt: string | null;
    updatedBy: UserDto | null;
    yourRole?: string;
  }
}

export interface PublicBoardDto {
  id: number;
  title: string;
  description: string;
}

export type TagColor =
  'red' | 'green' | 'blue' | 'yellow' | 'purple' | 'orange' | 'pink' | 'teal' | 'gray';

export const TAG_COLORS: TagColor[] = [
  'red',
  'orange',
  'yellow',
  'green',
  'teal',
  'blue',
  'purple',
  'pink',
  'gray'
];

export type BoardVisibility = 'private' | 'unlisted' | 'public';

export namespace GetBoardByIdResponse {
  export interface BoardDto {
    id: number;
    title: string;
    description: string;
    visibility: BoardVisibility;
    swimlanes: SwimlaneDto[];
    tags: TagDto[];
  }

  /** A tag definition on the board; cards carry only the ids. */
  export interface TagDto {
    id: number;
    title: string;
    color: TagColor;
  }

  export interface UserDto {
    id: number;
    userName: string;
    avatarUrl: string | null;
  }

  export interface BoardMemberDto extends UserDto {
    role: MemberRole;
  }

  export interface SwimlaneDto {
    id: number;
    title: string;
    height: number | null;
    rank: string;
    lists: ListDto[];
  }

  export interface ListDto {
    id: number;
    title: string;
    width: number | null;
    rank: string;
    cards: CardDto[];
  }

  export interface CardDto {
    id: number;
    title: string;
    description: string;
    rank: string;
    createdAt: string;
    createdBy: UserDto | null;
    updatedAt: string | null;
    updatedBy: UserDto | null;
    tagIds: number[];
  }
}

export namespace GetBoardDetailsResponse {
  export interface BoardDto {
    id: number;
    title: string;
    description: string;
    visibility: BoardVisibility;
    members: BoardMemberDto[];
  }

  export interface UserDto {
    id: number;
    userName: string;
    avatarUrl: string | null;
  }

  export interface BoardMemberDto extends UserDto {
    role: MemberRole;
  }
}

export interface CreateBoardRequest {
  title: string;
  description: string;
  members?: CreateBoardMemberRequest[] | null;
  visibility: BoardVisibility;
}

export type MemberRole = 'owner' | 'admin' | 'member' | 'viewer';

export interface CreateBoardMemberRequest {
  userId: number;
  role: MemberRole;
}

export interface UpdateBoardRequest {
  title: string;
  description: string;
  members: CreateBoardMemberRequest[];
  visibility: BoardVisibility;
}

export interface BoardVisibilityOptionsResponse {
  allowedVisibilities: BoardVisibility[];
}

export interface ChangeBoardVisibilityRequest {
  visibility: BoardVisibility;
}

export interface IdResponse {
  id: number;
}

export interface CreateTagRequest {
  title: string;
  color: TagColor;
}

export interface UpdateTagRequest {
  title: string;
  color: TagColor;
}

export interface CreateTagResponse {
  id: number;
  createdAt: string;
  createdBy: {
    id: number;
    userName: string;
  };
}

export interface UpdateTagResponse {
  updatedAt: string;
  updatedBy: {
    id: number;
    userName: string;
  };
}
