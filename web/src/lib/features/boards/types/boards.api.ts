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

export namespace GetBoardByIdResponse {
  export interface BoardDto {
    id: number;
    title: string;
    description: string;
    swimlanes: SwimlaneDto[];
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
    createdBy: UserDto;
    updatedAt: string | null;
    updatedBy: UserDto | null;
  }
}

export namespace GetBoardDetailsResponse {
  export interface BoardDto {
    id: number;
    title: string;
    description: string;
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
}

export interface IdResponse {
  id: number;
}
